using Microsoft.Build.Framework;
using System.IO;
using System.Linq;

namespace Acklann.TSBuild.MSBuild
{
    public class CompileTypescript : ITask
    {
        public ITaskItem ConfigurationFile { get; set; }

        public bool? Minify { get; set; }

        public bool? GenerateSourceMaps { get; set; }

        public bool Execute()
        {
            NodeJS.Install((msg, _, __) => { Log(msg); });

            string projectFolder = Path.GetDirectoryName(BuildEngine.ProjectFileOfTaskNode);
            string configFilePath = ConfigurationFile?.GetMetadata("FullPath") ?? Compiler.FindConfigurationFile(projectFolder);

            var options = new Configuration.CompilerOptions(
                configFilePath,
                Minify,
                GenerateSourceMaps);

            CompilerResult result = Compiler.Run(options, projectFolder);
            foreach (CompilerError err in result.Errors) Log(err);
            Log(result);

            return result.HasErrors == false;
        }

        #region ITask

        public IBuildEngine BuildEngine { get; set; }
        public ITaskHost HostObject { get; set; }

        #endregion ITask

        #region Backing Members

        public const string FullPath = "FullPath";

        private void Log(CompilerResult result)
        {
            if (result.Success == false) return;

            string cwd = Path.GetDirectoryName(BuildEngine.ProjectFileOfTaskNode);
            string rel(string x) => (x == null ? null : string.Format("{0}\\{1}", Path.GetDirectoryName(x).Replace(cwd, string.Empty), Path.GetFileName(x)));
            string merge(string[] l) => string.Concat('[', string.Join(" + ", l.Take(3).Select(x => rel(x))), ']');

            BuildEngine.LogMessageEvent(new BuildMessageEventArgs(
                string.Format(
                    "tsc -> in:{0}  out:{1}  elapse:{2}",

                    merge(result.SourceFiles),
                    merge(result.GeneratedFiles),
                    result.Elapse.ToString("hh\\:mm\\:ss\\.fff")
                    ),
                null,
                nameof(CompileTypescript),
                MessageImportance.Normal
                ));
        }

        private void Log(CompilerError error)
        {
            switch (error.Severity)
            {
                case ErrorSeverity.Error:
                    BuildEngine.LogErrorEvent(new BuildErrorEventArgs(
                        string.Empty,
                        $"{error.StatusCode}",
                        error.File,
                        error.Line,
                        error.Column,
                        0, 0,
                        error.Message,
                        string.Empty,
                        nameof(CompileTypescript)));
                    break;

                case ErrorSeverity.Warning:
                    BuildEngine.LogWarningEvent(new BuildWarningEventArgs(
                        string.Empty,
                        $"{error.StatusCode}",
                        error.File,
                        error.Line,
                        error.Column,
                        0, 0,
                        error.Message,
                        string.Empty,
                        nameof(CompileTypescript)));
                    break;
            }
        }

        private void Log(string message, MessageImportance importance = MessageImportance.Normal)
        {
            BuildEngine.LogMessageEvent(new BuildMessageEventArgs(message, null, nameof(CompileTypescript), importance));
        }

        #endregion Backing Members
    }
}