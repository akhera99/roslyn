﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Editor.EditorConfigSettings;
using Microsoft.CodeAnalysis.Editor.Shared.Utilities;
using Microsoft.Internal.VisualStudio.Shell.TableControl;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell.TableManager;
using Microsoft.VisualStudio.TextManager.Interop;
using IOleServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;

namespace Microsoft.VisualStudio.LanguageServices.EditorConfigSettings;

[Guid(SettingsEditorFactoryGuidString)]
internal sealed class SettingsEditorFactory() : IVsEditorFactory, IVsEditorFactory4
{
    private static SettingsEditorFactory? s_instance;

    public static readonly Guid SettingsEditorFactoryGuid = new(SettingsEditorFactoryGuidString);
    public const string SettingsEditorFactoryGuidString = "68b46364-d378-42f2-9e72-37d86c5f4468";
    public const string Extension = ".editorconfig";

    private ServiceProvider? _vsServiceProvider;

    public static SettingsEditorFactory GetInstance()
    {
        s_instance ??= new SettingsEditorFactory();

        return s_instance;
    }

    public int CreateEditorInstance(uint grfCreateDoc,
                                    string filePath,
                                    string pszPhysicalView,
                                    IVsHierarchy pvHier,
                                    uint itemid,
                                    IntPtr punkDocDataExisting,
                                    out IntPtr ppunkDocView,
                                    out IntPtr ppunkDocData,
                                    out string? pbstrEditorCaption,
                                    out Guid pguidCmdUI,
                                    out int pgrfCDW)
    {
        // Initialize to null
        ppunkDocView = IntPtr.Zero;
        ppunkDocData = IntPtr.Zero;
        pguidCmdUI = SettingsEditorFactoryGuid;
        pgrfCDW = 0;
        pbstrEditorCaption = null;

        Assumes.NotNull(_vsServiceProvider);

        var componentModel = (IComponentModel)_vsServiceProvider.GetService(typeof(SComponentModel));
        var workspace = componentModel.GetService<VisualStudioWorkspace>();
        if (!workspace.CurrentSolution.Projects.Any(p => p.Language is LanguageNames.CSharp or LanguageNames.VisualBasic))
        {
            // If there are no VB or C# projects loaded in the solution (so an editorconfig file in a C++ project) then we want their
            // editorfactory to present the file instead of use showing ours
            return VSConstants.VS_E_UNSUPPORTEDFORMAT;
        }

        if (!workspace.CurrentSolution.Projects.Any(p => p.AnalyzerConfigDocuments.Any(editorconfig => StringComparer.OrdinalIgnoreCase.Equals(editorconfig.FilePath, filePath))))
        {
            // If the user is simply opening an editorconfig file that does not apply to the current solution we just want to show the text view
            return VSConstants.VS_E_UNSUPPORTEDFORMAT;
        }

        // Validate inputs
        if ((grfCreateDoc & (VSConstants.CEF_OPENFILE | VSConstants.CEF_SILENT)) == 0)
        {
            return VSConstants.E_INVALIDARG;
        }

        var threadingContext = componentModel.GetService<IThreadingContext>();
        IVsTextLines? textBuffer = null;
        if (punkDocDataExisting == IntPtr.Zero)
        {
            if (_vsServiceProvider.TryGetService<SLocalRegistry, ILocalRegistry>(threadingContext.JoinableTaskFactory, out var localRegistry))
            {
                var textLinesGuid = typeof(IVsTextLines).GUID;
                _ = localRegistry.CreateInstance(typeof(VsTextBufferClass).GUID, null, ref textLinesGuid, 1 /*CLSCTX_INPROC_SERVER*/, out var ptr);
                try
                {
                    textBuffer = Marshal.GetObjectForIUnknown(ptr) as IVsTextLines;
                }
                finally
                {
                    _ = Marshal.Release(ptr); // Release RefCount from CreateInstance call
                }

                if (textBuffer is IObjectWithSite objectWithSite)
                {
                    var oleServiceProvider = _vsServiceProvider.GetService<IOleServiceProvider>(threadingContext.JoinableTaskFactory);
                    objectWithSite.SetSite(oleServiceProvider);
                }
            }
        }
        else
        {
            textBuffer = Marshal.GetObjectForIUnknown(punkDocDataExisting) as IVsTextLines;
            if (textBuffer == null)
            {
                return VSConstants.VS_E_INCOMPATIBLEDOCDATA;
            }
        }

        if (textBuffer is null)
        {
            throw new InvalidOperationException("unable to acquire text buffer");
        }

        var settingsDataProviderFactory = workspace.Services.GetRequiredService<ISettingsAggregator>();
        var tableManagerProvider = componentModel.GetService<ITableManagerProvider>();
        var controlProvider = componentModel.GetService<IWpfTableControlProvider>();
        var vsEditorAdaptersFactoryService = componentModel.GetService<IVsEditorAdaptersFactoryService>();

        // Create the editor
        var newEditor = new SettingsEditorPane(vsEditorAdaptersFactoryService,
                                               threadingContext,
                                               settingsDataProviderFactory,
                                               controlProvider,
                                               tableManagerProvider,
                                               filePath,
                                               textBuffer,
                                               workspace);
        ppunkDocView = Marshal.GetIUnknownForObject(newEditor);
        ppunkDocData = Marshal.GetIUnknownForObject(textBuffer);
        pbstrEditorCaption = "";
        return VSConstants.S_OK;
    }

    public int SetSite(IOleServiceProvider psp)
    {
        // We don't dispose this (as we aren't disposable), but that's fine as it's disposer only clears a field.
        _vsServiceProvider = new ServiceProvider(psp);
        return VSConstants.S_OK;
    }

    public int Close() => VSConstants.S_OK;

    public int MapLogicalView(ref Guid rguidLogicalView, out string? pbstrPhysicalView)
    {
        pbstrPhysicalView = null;    // initialize out parameter

        // we support only a single physical view
        if (VSConstants.LOGVIEWID_Primary == rguidLogicalView)
        {
            return VSConstants.S_OK;        // primary view uses NULL as pbstrPhysicalView
        }
        else
        {
            return VSConstants.E_NOTIMPL;   // you must return E_NOTIMPL for any unrecognized rguidLogicalView values
        }
    }

    public object? GetDocumentData(uint grfCreate, string pszMkDocument, IVsHierarchy pHier, uint itemid)
        => null;

    public object? GetDocumentView(uint grfCreate, string pszPhysicalView, IVsHierarchy pHier, IntPtr punkDocData, uint itemid)
        => null;

    public string? GetEditorCaption(string pszMkDocument, string pszPhysicalView, IVsHierarchy pHier, IntPtr punkDocData, out Guid pguidCmdUI)
        => throw new NotImplementedException();

    public bool ShouldDeferUntilIntellisenseIsReady(uint grfCreate, string pszMkDocument, string pszPhysicalView)
        => true;
}
