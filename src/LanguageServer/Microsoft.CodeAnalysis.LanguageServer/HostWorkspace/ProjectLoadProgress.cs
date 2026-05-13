// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Microsoft.CodeAnalysis.LanguageServer.HostWorkspace;

/// <summary>
/// Reports the progress of a project load operation, including how many
/// projects have completed loading out of the total.
/// </summary>
/// <param name="CompletedCount">The number of projects that have finished loading so far.</param>
/// <param name="TotalCount">The total number of projects being loaded.</param>
/// <param name="ProjectPath">The file path of the project that just completed loading.</param>
internal readonly record struct ProjectLoadProgress(int CompletedCount, int TotalCount, string ProjectPath)
{
    /// <summary>
    /// Gets the loading percentage as an integer in the range [0, 100].
    /// Clamped to 100 to handle edge cases where the completed count
    /// doesn't exactly match the total.
    /// </summary>
    public int Percentage => TotalCount > 0
        ? Math.Min(100, (int)((long)CompletedCount * 100 / TotalCount))
        : 0;
}

/// <summary>
/// Per-operation progress state that tracks how many projects have completed loading.
/// Attached to each <see cref="ProjectToLoad"/> item so that only projects from the
/// originating operation increment this counter — unrelated reloads or concurrent
/// load operations get their own state (or null).
/// </summary>
/// <remarks>
/// Thread-safe: <see cref="ReportCompleted"/> serializes increment + report under a lock
/// to guarantee monotonically increasing progress notifications to the client.
/// </remarks>
internal sealed class LoadProgressState
{
    private readonly IProgress<ProjectLoadProgress> _reporter;
    private readonly int _totalCount;
    private readonly object _gate = new();
    private int _completedCount;

    public LoadProgressState(IProgress<ProjectLoadProgress> reporter, int totalCount)
    {
        _reporter = reporter;
        _totalCount = totalCount;
    }

    /// <summary>
    /// Atomically increments the completed count and reports progress.
    /// The lock ensures that increment + report is atomic, so the client
    /// never sees a non-monotonic sequence of progress updates.
    /// </summary>
    public void ReportCompleted(string projectPath)
    {
        lock (_gate)
        {
            var completed = ++_completedCount;
            _reporter.Report(new ProjectLoadProgress(completed, _totalCount, projectPath));
        }
    }
}
