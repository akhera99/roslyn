// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Composition;
using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis.ErrorReporting;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.CodeAnalysis.LanguageServer.Handler;
using Microsoft.CodeAnalysis.Options;
using Microsoft.CodeAnalysis.PooledObjects;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.Threading;
using Roslyn.LanguageServer.Protocol;

namespace Microsoft.CodeAnalysis.LanguageServer.HostWorkspace;

[Shared]
[ExportCSharpVisualBasicStatelessLspService(typeof(AutoLoadProjectsInitializer))]
[method: ImportingConstructor]
[method: Obsolete(MefConstruction.ImportingConstructorMessage, error: true)]
internal sealed class AutoLoadProjectsInitializer(
    LanguageServerProjectSystem projectSystem,
    ILoggerFactory loggerFactory,
    ServerConfiguration serverConfiguration,
    IGlobalOptionService globalOptionService) : ILspService, IOnInitialized
{
    private readonly ILogger _logger = loggerFactory.CreateLogger<AutoLoadProjectsInitializer>();

    public async Task OnInitializedAsync(ClientCapabilities clientCapabilities, RequestContext context, CancellationToken cancellationToken)
    {
        if (!serverConfiguration.AutoLoadProjects)
        {
            return;
        }

        var isUsingDevKit = globalOptionService.GetOption(LspOptionsStorage.LspUsingDevkitFeatures);
        Contract.ThrowIfTrue(isUsingDevKit, "Auto load projects is not supported when using DevKit.");

        var initializeParams = context.GetRequiredService<IInitializeManager>().TryGetInitializeParams();
        Contract.ThrowIfNull(initializeParams, "Initialize params should be set during initialization.");

        var workspaceFolders = initializeParams.WorkspaceFolders;
        if (workspaceFolders is null || workspaceFolders.Length == 0)
        {
            _logger.LogWarning("No workspace folders provided during initialization; could not auto load projects.");
            return;
        }

        var (isLoadingDisabled, solutionPath) = TryGetVSCodeSolutionSettings(workspaceFolders, _logger);
        if (isLoadingDisabled)
        {
            _logger.LogInformation("Using VS Code settings to disable auto loading solution on startup.");
            return;
        }
        else if (solutionPath is not null)
        {
            _logger.LogInformation("Using VS Code settings to auto load solution {SolutionFile}", solutionPath);
            await StartAndReportProgressAsync(progress => projectSystem.OpenSolutionAsync(solutionPath, progress));
            return;
        }

        // If there's a single workspace folder with a single solution file at the root, load that solution.
        if (workspaceFolders.Length == 1)
        {
            var folder = workspaceFolders[0];
            if (TryGetFolderPath(folder, _logger, out var folderPath))
            {
                var solutionFiles = Directory.EnumerateFiles(folderPath, "*.sln", SearchOption.TopDirectoryOnly)
                    .Concat(Directory.EnumerateFiles(folderPath, "*.slnx", SearchOption.TopDirectoryOnly))
                    .ToArray();

                if (solutionFiles.Length == 1)
                {
                    _logger.LogInformation("Found single solution file {SolutionFile} to auto load", solutionFiles[0]);
                    await StartAndReportProgressAsync(progress => projectSystem.OpenSolutionAsync(solutionFiles[0], progress));
                    return;
                }
            }
        }

        using var _ = ArrayBuilder<string>.GetInstance(out var projectFiles);
        foreach (var folder in workspaceFolders)
        {
            _logger.LogTrace("Searching for projects to load in workspace folder: {FolderUri}", folder.DocumentUri);
            if (TryGetFolderPath(folder, _logger, out var folderPath))
            {
                projectFiles.AddRange(Directory.EnumerateFiles(folderPath, "*.csproj", SearchOption.AllDirectories));
            }
        }

        _logger.LogInformation("Discovered {count} projects to auto load", projectFiles.Count);

        // Capture immutable snapshot before passing to fire-and-forget lambda
        // to avoid using the pooled ArrayBuilder after it's returned to the pool.
        var immutableProjectFiles = projectFiles.ToImmutable();
        await StartAndReportProgressAsync(progress => projectSystem.OpenProjectsAsync(immutableProjectFiles, progress));

        async Task StartAndReportProgressAsync(Func<IProgress<ProjectLoadProgress>, Task> loadOperation)
        {
            var workDoneProgressManager = context.GetRequiredLspService<WorkDoneProgressManager>();

            // We will await for the client to know that we are starting work...
            var progressReporter = await workDoneProgressManager.CreateWorkDoneProgressAsync(reportProgressToClient: true, cancellationToken);

            // ...but we'll fire-and-forget for the actual loading. Pass CancellationToken.None since we want to ensure the progressReporter is always disposed.
            Task.Run(async () =>
                {
                    try
                    {
                        progressReporter.Report(new WorkDoneProgressBegin
                        {
                            Title = "Loading projects",
                            Percentage = 0,
                        });

                        // Use a synchronous IProgress<T> implementation instead of System.Progress<T>.
                        // System.Progress<T> posts callbacks to the captured SynchronizationContext (or the
                        // thread pool when null), which can cause progress reports to arrive out-of-order
                        // or after WorkDoneProgressEnd.
                        var progress = new SynchronousProgress<ProjectLoadProgress>(p =>
                        {
                            progressReporter.Report(new WorkDoneProgressReport
                            {
                                Message = $"{p.CompletedCount}/{p.TotalCount} projects",
                                Percentage = p.Percentage,
                            });
                        });

                        await loadOperation(progress);

                        progressReporter.Report(new WorkDoneProgressEnd
                        {
                            Message = "Projects loaded",
                        });
                    }
                    catch (Exception ex) when (FatalError.ReportAndCatch(ex))
                    {
                        // Ensure the client receives an end notification even on failure,
                        // otherwise it will hang waiting for the progress to complete.
                        progressReporter.Report(new WorkDoneProgressEnd
                        {
                            Message = "Failed to load projects",
                        });
                    }
                    finally
                    {
                        progressReporter.Dispose();
                    }
                }, CancellationToken.None).Forget();
        }
    }

    /// <summary>
    /// An <see cref="IProgress{T}"/> implementation that invokes the callback synchronously
    /// on the calling thread, unlike <see cref="Progress{T}"/> which posts to the
    /// captured <see cref="SynchronizationContext"/>. Exceptions from the handler are
    /// caught and reported to avoid crashing the parallel project load pipeline.
    /// </summary>
    private sealed class SynchronousProgress<T>(Action<T> handler) : IProgress<T>
    {
        public void Report(T value)
        {
            try
            {
                handler(value);
            }
            catch (Exception ex) when (FatalError.ReportAndCatch(ex))
            {
            }
        }
    }

    internal static bool TryGetFolderPath(WorkspaceFolder folder, ILogger logger, [NotNullWhen(returnValue: true)] out string? folderPath)
    {
        if (folder.DocumentUri.ParsedUri is null || folder.DocumentUri.ParsedUri.Scheme != Uri.UriSchemeFile)
        {
            logger.LogWarning("Workspace folder {FolderUri} is not a file URI, skipping.", folder.DocumentUri);
            folderPath = null;
            return false;
        }

        folderPath = folder.DocumentUri.GetDocumentFilePathFromUri();
        if (!Directory.Exists(folderPath))
        {
            logger.LogWarning("Workspace folder path {FolderPath} does not exist, skipping.", folderPath);
            return false;
        }

        return true;
    }

    internal static (bool isLoadingDisabled, string? solutionPath) TryGetVSCodeSolutionSettings(WorkspaceFolder[] workspaceFolders, ILogger logger)
    {
        foreach (var folder in workspaceFolders)
        {
            logger.LogTrace("Searching for VS Code settings to load in workspace folder: {FolderUri}", folder.DocumentUri);

            if (TryGetFolderPath(folder, logger, out var folderPath)
                && VSCodeSettings.TryRead(Path.Combine(folderPath, ".vscode", "settings.json"), logger, out var settings)
                && settings.TryGetStringSetting(VSCodeSettings.Names.DefaultSolution) is { Length: > 0 } defaultSolution)
            {
                var isLoadingDisabled = string.Equals(defaultSolution, "disable", StringComparison.Ordinal);
                if (isLoadingDisabled)
                {
                    if (workspaceFolders.Length == 1)
                        return (isLoadingDisabled: true, solutionPath: null);

                    continue;
                }

                var solutionPath = Path.IsPathRooted(defaultSolution)
                    ? defaultSolution
                    : Path.GetFullPath(Path.Combine(folderPath, defaultSolution));

                if (!File.Exists(solutionPath))
                {
                    logger.LogWarning("Default solution path {SolutionPath} does not exist, skipping.", solutionPath);
                    continue;
                }

                return (isLoadingDisabled: false, solutionPath);
            }
        }

        return (isLoadingDisabled: false, solutionPath: null);
    }
}
