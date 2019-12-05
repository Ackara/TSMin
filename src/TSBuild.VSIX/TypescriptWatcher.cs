using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.IO;
using System.Linq;
using System.Text;

namespace Acklann.TSBuild
{
    public class TypescriptWatcher : IVsRunningDocTableEvents3
    {
        public TypescriptWatcher(VSPackage package, IVsOutputWindowPane pane)
        {
            if (package == null) throw new ArgumentNullException(nameof(package));
            _vsOutWindow = pane ?? throw new ArgumentNullException(nameof(pane));

            _runningDocumentTable = new RunningDocumentTable(package);
            _runningDocumentTable.Advise(this);

            _errorList = new ErrorListProvider(package)
            {
                ProviderName = $"{Symbol.Name} Error List",
                ProviderGuid = new Guid("e18abe6a-7968-4bd6-9266-0684b1d50e7a")
            };
        }

        public void Start()
        {
            _stillLoading = false;
        }

        private async void RunCompiler(string activeFile, IVsHierarchy hierarchy)
        {
            string projectFolder = Path.GetDirectoryName(hierarchy.ToProject()?.FullName);

            var options = new Configuration.CompilerOptions(
                Compiler.FindConfigurationFile(projectFolder),
                ConfigurationPage.ShouldMinify,
                ConfigurationPage.ShouldGenerateSourceMaps
                );

            CompilerResult result = await Compiler.RunAsync(options, projectFolder);
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            ShowErrors(activeFile, hierarchy, result.Errors);
            _vsOutWindow.Writeline(GetSummary(result, projectFolder));
        }

        private void ShowErrors(string activeFile, IVsHierarchy hierarchy, CompilerError[] errors)
        {
            if (ConfigurationPage.ShouldShowCompilerErrors == false) return;

            TaskProvider.TaskCollection list = _errorList.Tasks;
            ErrorTask item;

            int nErrors = list.Count;
            for (int i = 0; i < nErrors; i++)
            {
                item = (ErrorTask)list[i];
                if (Helper.AreSame(item.Document, activeFile))
                {
                    item.Navigate -= GotoLine;
                    list.RemoveAt(i);
                    nErrors--; i--;
                }
            }

            CompilerError error;
            nErrors = errors.Length;
            for (int i = 0; i < nErrors; i++)
            {
                list.Add(ToTask(errors[i], hierarchy));
            }
        }

        private string GetSummary(CompilerResult result, string cwd)
        {
            const int n = 75;
            string rel(string x) => (x == null ? null : string.Format("{0}\\{1}", Path.GetDirectoryName(x).Replace(cwd, string.Empty), Path.GetFileName(x)).Trim('\\'));
            string header(string title = "", char c = '-')
            {
                string spacer = string.Concat(Enumerable.Repeat(c, n));
                return spacer.Insert(5, $" {title} ").Remove(n);
            }

            var builder = new StringBuilder();
            builder.AppendLine(header($"Build started: {Path.GetFileName(cwd)}"));
            builder.AppendLine("  Source Files:");
            for (int i = 0; i < result.SourceFiles.Length; i++)
                builder.AppendLine($"    in: {rel(result.SourceFiles[i])}");

            builder.AppendLine();
            builder.AppendLine("  Compiled Files:");
            for (int i = 0; i < result.GeneratedFiles.Length; i++)
                builder.AppendLine($"    out: {rel(result.GeneratedFiles[i])}");

            return builder
                .AppendLine(header($"errors:{result.Errors.Length} elapse:{result.Elapse.ToString("hh\\:mm\\:ss\\.fff")}", '='))
                .AppendLine()
                .ToString();
        }

        #region IVsRunningDocTableEvents3

        public int OnAfterSave(uint docCookie)
        {
            if (_stillLoading) return VSConstants.S_OK;
            if (!ConfigurationPage.ShouldCompileOnSave) return VSConstants.S_OK;
            ThreadHelper.ThrowIfNotOnUIThread();

            _vsOutWindow?.Clear();
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

        private readonly Guid _textViewGuid = new Guid("{7651A703-06E5-11D1-8EBD-00A0C90F26EA}");
        private readonly RunningDocumentTable _runningDocumentTable;
        private readonly IVsOutputWindowPane _vsOutWindow;
        private readonly ErrorListProvider _errorList;
        private bool _stillLoading = true;

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

        private ErrorTask ToTask(CompilerError error, IVsHierarchy hierarchy)
        {
            var task = new ErrorTask
            {
                CanDelete = true,
                Text = error.Message,
                Document = error.File,
                Line = error.Line,
                Column = error.Column,
                HierarchyItem = hierarchy,
               
                Category = TaskCategory.BuildCompile,
                ErrorCategory = ToCatetory(error.Severity)
            };
            task.Navigate += GotoLine;
            return task;
        }

        private void GotoLine(object sender, EventArgs e)
        {
            if (sender is ErrorTask task)
            {
                task.Line++;
                _errorList.Navigate(task, _textViewGuid);
                task.Line--;
            }
        }

        #endregion Backing Members
    }
}