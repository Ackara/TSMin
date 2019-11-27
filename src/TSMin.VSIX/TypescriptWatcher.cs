using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

namespace Acklann.TSMin
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

        private void CompileTypescriptFiles(string documentPath, IVsHierarchy hierarchy, Microsoft.Build.Evaluation.Project config)
        {
            XmlDocument xml = GetDocument(config.FullPath);

            foreach (XmlElement item in xml.SelectNodes($"//{nameof(MSBuild.CompileTypescript)}"))
            {
                GetArguments(item, out string source, out CompilerOptions options, out string condition);
                options.OutputFile = ExpandPath(options.OutputFile, config);
                string[] sourceFiles = ExpandPaths(source, config);

                if (sourceFiles.Contains(documentPath) == false) continue;

                CompilerResult result = Compiler.Compile(options, sourceFiles);
                foreach (var err in result.Errors) HandleError(err, hierarchy);
                if (result.HasErrors == false) Log(result, config.DirectoryPath);
            }
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

        private static void GetArguments(XmlElement element, out string source, out CompilerOptions options, out string condition)
        {
            condition = element.GetAttribute(nameof(Microsoft.Build.Execution.ProjectTaskInstance.Condition));
            source = element.GetAttribute(nameof(MSBuild.CompileTypescript.SourceFiles));
            bool.TryParse(element.GetAttribute(nameof(MSBuild.CompileTypescript.Minify)), out bool shouldMinify);
            bool.TryParse(element.GetAttribute(nameof(MSBuild.CompileTypescript.GenerateSourceMap)), out bool shouldGenerateSourceMap);

            options = new CompilerOptions
            {
                OutputFile = element.GetAttribute(nameof(MSBuild.CompileTypescript.OutFile)),
                GenerateSourceMaps = shouldGenerateSourceMap,
                Minify = shouldMinify
            };
        }

        #region IVsRunningDocTableEvents3

        public int OnAfterSave(uint docCookie)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            RunningDocumentInfo document = _runningDocumentTable.GetDocumentInfo(docCookie);
            Microsoft.Build.Evaluation.Project config = GetMSBuildProject(document.Hierarchy);

            if (Path.GetFileName(document.Moniker).EndsWith(".ts", StringComparison.OrdinalIgnoreCase))
            {
                if (config != null)
                    System.Threading.Tasks.Task.Run(() => { CompileTypescriptFiles(document.Moniker, document.Hierarchy, config); });
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

        private static string ExpandPath(string text, Microsoft.Build.Evaluation.Project config)
        {
            text = config.ExpandString(text);
            if (!Path.IsPathRooted(text)) text = Path.Combine(config.DirectoryPath, text);

            return text;
        }

        private static string[] ExpandPaths(string text, Microsoft.Build.Evaluation.Project config)
        {
            string[] paths = config.ExpandString(text).Split(';');
            for (int i = 0; i < paths.Length; i++)
            {
                if (!Path.IsPathRooted(paths[i]))
                    paths[i] = Path.Combine(config.DirectoryPath, paths[i]);
            }

            return paths;
        }

        private static XmlDocument GetDocument(string fullPath)
        {
            var doc = new XmlDocument();
            using (var stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                doc.Load(stream);
            }

            return doc;
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

        private void Log(CompilerResult result, string cwd)
        {
#pragma warning disable VSTHRD010 // Invoke single-threaded types on Main thread
            string rel(string x) => (x == null ? null : string.Format("{0}\\{1}", Path.GetDirectoryName(x).Replace(cwd, string.Empty), Path.GetFileName(x)).Trim('\\'));

            _vsOutWindow.OutputStringThreadSafe(string.Format(
                ("tsc -> in:[{0}] out:[{1}] elapse:{2}" + Environment.NewLine),

                string.Join(" + ", result.SourceFiles.Select(x => rel(x))),
                string.Join(" + ", result.GeneratedFiles.Select(x => rel(x))),
                result.Elapse.ToString("hh\\:mm\\:ss\\.fff"))
                );
#pragma warning restore VSTHRD010 // Invoke single-threaded types on Main thread
        }

        #endregion Backing Members
    }
}