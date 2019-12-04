using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Acklann.TSBuild
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "VSTHRD110:Observe result of async calls", Justification = "<Pending>")]
    public class TypescriptWatcher : IVsRunningDocTableEvents3
    {
        public TypescriptWatcher(VSPackage package, IVsOutputWindowPane pane)
        {
            if (package == null) throw new ArgumentNullException(nameof(package));

            _vsOutWindow = pane ?? throw new ArgumentNullException(nameof(pane));
            _loadedProjects = new Dictionary<string, Microsoft.Build.Evaluation.Project>();

            _runningDocumentTable = new RunningDocumentTable(package);
            _runningDocumentTable.Advise(this);

            _errorList = new ErrorListProvider(package)
            {
                ProviderName = $"{Symbol.Name} Error List",
                ProviderGuid = new Guid("e18abe6a-7968-4bd6-9266-0684b1d50e7a")
            };
        }

        private async void RunCompiler(string sourceFile, IVsHierarchy hierarchy)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            string projectFolder = Path.GetDirectoryName(hierarchy?.ToProject()?.FullName);

            var options = new Configuration.CompilerOptions(
                Compiler.FindConfigurationFile(Path.GetDirectoryName(projectFolder)),
                ConfigurationPage.ShouldMinify,
                ConfigurationPage.ShouldGenerateSourceMaps
                );

            CompilerResult result = await Compiler.RunAsync(options);
            string summary = GetSummary(result, projectFolder);
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            _vsOutWindow.OutputStringThreadSafe(summary);
            foreach (CompilerError error in result.Errors) HandleError(error, hierarchy);
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

        #region IVsRunningDocTableEvents3

        public int OnAfterSave(uint docCookie)
        {
            if (!ConfigurationPage.ShouldCompileOnSave) return VSConstants.S_OK;

            ThreadHelper.ThrowIfNotOnUIThread();
            RunningDocumentInfo document = _runningDocumentTable.GetDocumentInfo(docCookie);

            if (string.Equals(Path.GetExtension(document.Moniker), ".ts", StringComparison.OrdinalIgnoreCase) && document.Hierarchy != null)
            {
                RunCompiler(document.Moniker, document.Hierarchy);
            }

            System.Diagnostics.Debug.WriteLine($"{nameof(OnAfterSave)} -> {document.Moniker}");
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

        private string GetSummary(CompilerResult result, string cwd)
        {
            const int n = 75;
            string rel(string x) => (x == null ? null : string.Format("{0}\\{1}", Path.GetDirectoryName(x).Replace(cwd, string.Empty), Path.GetFileName(x)).Trim('\\'));
            string header(string title = "", char c = '=')
            {
                string spacer = string.Concat(Enumerable.Repeat(c, n));
                return spacer.Insert(5, $" {title} ").Remove(n);
            }

            var builder = new StringBuilder();
            builder.AppendLine(header($"Build started: {Path.GetFileName(cwd)}"));
            builder.AppendLine("Source Files:\n");
            for (int i = 0; i < result.SourceFiles.Length; i++)
                builder.AppendLine($"  in: {rel(result.SourceFiles[i])}");

            builder.AppendLine("Ouput:");
            for (int i = 0; i < result.GeneratedFiles.Length; i++)
                builder.AppendLine($"  out: {rel(result.GeneratedFiles[i])}");

            builder.AppendLine(header($"errors:{result.Errors.Length} elapse:{result.Elapse.ToString("hh\\:mm\\:ss\\.fff")}"));

            return builder.ToString();
        }

        #endregion Backing Members
    }
}