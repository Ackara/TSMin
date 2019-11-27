using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace Acklann.TSMin
{
    public class TypescriptWatcher : IVsRunningDocTableEvents3
    {
        public TypescriptWatcher(VSPackage package, IVsOutputWindowPane pane, EnvDTE.StatusBar statusBar)
        {
            if (package == null) throw new ArgumentNullException(nameof(package));

            _vsOutWindow = pane ?? throw new ArgumentNullException(nameof(pane));
            _statusBar = statusBar ?? throw new ArgumentNullException(nameof(statusBar));
            _loadedProjects = new Dictionary<string, Microsoft.Build.Evaluation.Project>();

            _runningDocumentTable = new RunningDocumentTable(package);
            _runningDocumentTable.Advise(this);

            _errorList = new ErrorListProvider(package)
            {
                ProviderName = $"{Symbol.Name} Error List",
                ProviderGuid = new Guid("e18abe6a-7968-4bd6-9266-0684b1d50e7a")
            };
        }

        private void CompileTypescript(string documentPath, IVsHierarchy hierarchy)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (hierarchy == null) throw new ArgumentNullException(nameof(hierarchy));
            Microsoft.Build.Evaluation.Project config = GetMSBuildProject(hierarchy);
            if (config == null) return;

            var xml = new XmlDocument();
            xml.LoadXml(config.Xml.RawXml);

            foreach (XmlElement item in xml.SelectNodes($"//{nameof(MSBuild.CompileTypescript)}"))
            {
                TryGetOptions(item, documentPath, out string[] sourceFiles, out CompilerOptions options);
                CompilerResult result = Compiler.Compile(options, sourceFiles);
                foreach (var err in result.Errors) HandleError(err, hierarchy);
                if (result.HasErrors == false) Log(result);
            }
        }

        private bool TryGetOptions(XmlElement element, string documentPath, out string[] sourceFiles, out CompilerOptions options)
        {
            throw new System.NotImplementedException();
        }

        private void HandleError(CompilerError error, IVsHierarchy hierarchy)
        {
            ErrorTask task;
            bool shouldAdd = true;
            int n = _errorList.Tasks.Count;

            for (int i = 0; i < n; i++)
            {
                task = (ErrorTask)_errorList.Tasks[i];

                if (task.Document == error.File)
                {
                    _errorList.Tasks.RemoveAt(i);
                    n--;
                }

                if (task.Text == error.Message && task.Document == error.File && task.Line == error.Line && task.Column == error.Column)
                {
                    shouldAdd = false;
                }
            }

            if (shouldAdd) _errorList.Tasks.Add(new ErrorTask
            {
                Text = error.Message,
                HierarchyItem = hierarchy,
                Document = error.File,
                Line = (error.Line - 1),
                Column = error.Column,
                Category = TaskCategory.BuildCompile,
                ErrorCategory = ToCatetory(error.Severity)
            });
        }

        private Microsoft.Build.Evaluation.Project GetMSBuildProject(IVsHierarchy hierarchy)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            hierarchy.GetProperty(VSConstants.VSITEMID_ROOT, (int)__VSHPROPID.VSHPROPID_ExtObject, out object objProj);
            string projectFilePath = (objProj as EnvDTE.Project)?.FullName;
            if (!File.Exists(projectFilePath)) return null;

            Microsoft.Build.Evaluation.Project config;

            if (_loadedProjects.ContainsKey(projectFilePath))
                config = _loadedProjects[projectFilePath];
            else
                _loadedProjects.Add(projectFilePath, config = new Microsoft.Build.Evaluation.Project(projectFilePath));

            return config;
        }

        #region IVsRunningDocTableEvents3

        public int OnAfterSave(uint docCookie)
        {
            RunningDocumentInfo document = _runningDocumentTable.GetDocumentInfo(docCookie);
            string fileName = Path.GetFileName(document.Moniker);
            System.Diagnostics.Debug.WriteLine($"{nameof(OnAfterSave)} -> {document.Moniker}");

            if (fileName.EndsWith(".ts", StringComparison.OrdinalIgnoreCase))
            {
                CompileTypescript(document.Moniker, document.Hierarchy);
            }
            else if (Path.GetExtension(document.Moniker).EndsWith("proj"))
            {
            }

            return VSConstants.S_OK;
        }

        public int OnAfterFirstDocumentLock(uint docCookie, uint dwRDTLockType, uint dwReadLocksRemaining, uint dwEditLocksRemaining) => VSConstants.S_OK;

        public int OnBeforeLastDocumentUnlock(uint docCookie, uint dwRDTLockType, uint dwReadLocksRemaining, uint dwEditLocksRemaining) => VSConstants.S_OK;

        public int OnAfterAttributeChange(uint docCookie, uint grfAttribs) => VSConstants.S_OK;

        public int OnBeforeDocumentWindowShow(uint docCookie, int fFirstShow, IVsWindowFrame pFrame) => VSConstants.S_OK;

        public int OnAfterDocumentWindowHide(uint docCookie, IVsWindowFrame pFrame) => VSConstants.S_OK;

        public int OnAfterAttributeChangeEx(uint docCookie, uint grfAttribs, IVsHierarchy pHierOld, uint itemidOld, string pszMkDocumentOld, IVsHierarchy pHierNew, uint itemidNew, string pszMkDocumentNew) => VSConstants.S_OK;

        public int OnBeforeSave(uint docCookie) => VSConstants.S_OK;

        #endregion IVsRunningDocTableEvents3

        #region Backing Members

        private readonly IDictionary<string, Microsoft.Build.Evaluation.Project> _loadedProjects;
        private readonly RunningDocumentTable _runningDocumentTable;
        private readonly ErrorListProvider _errorList;
        private readonly IVsOutputWindowPane _vsOutWindow;
        private readonly EnvDTE.StatusBar _statusBar;

        private static TaskErrorCategory ToCatetory(ErrorSeverity severity)
        {
            switch (severity)
            {
                default:
                case ErrorSeverity.Debug:
                    return TaskErrorCategory.Message;

                case ErrorSeverity.Warning:
                    return TaskErrorCategory.Warning;

                case ErrorSeverity.Error:
                    return TaskErrorCategory.Error;
            }
        }

        private void Log(CompilerResult result)
        {

        }

        #endregion Backing Members
    }
}