using Microsoft.Build.Framework;
using Newtonsoft.Json;
using System.IO;
using System.Linq;

namespace Acklann.TSMin.MSBuild
{
    public class CompileTypescript : ITask
    {
        public const string MetaElement = "OutFile";

        [Required]
        public ITaskItem[] SourceFiles { get; set; }

        public string TypescriptCompilerOptions { get; set; }

        public string UglifyjsOptions { get; set; }

        public bool Minify { get; set; }

        public bool GenerateSourceMap { get; set; }

        public bool Execute()
        {
            GetData(SourceFiles, out string[] inputFiles, out string outFile);
            System.Console.WriteLine("out: " + outFile);

            var options = new CompilerOptions
            {
                Minify = Minify,
                OutputFile = outFile,
                GenerateSourceMaps = GenerateSourceMap
            };

            bool success = true;
            CompilerResult result = Compiler.Compile(options, inputFiles);
            foreach (CompilerError error in result.Errors)
            {
                Log(error);
                if (error.Severity == ErrorSeverity.Error) success = false;
            }

            if (success) Log(result);
            return success;
        }

        private void GetData(ITaskItem[] items, out string[] sourceFiles, out string outFile)
        {
            outFile = null;
            sourceFiles = new string[items.Length];

            int n = items.Length;
            for (int i = 0; i < n; i++)
            {
                if (string.IsNullOrEmpty(outFile))
                {
                    outFile = items[i].GetMetadata(MetaElement);
                    if (!Path.IsPathRooted(outFile)) outFile = Path.Combine(Path.GetDirectoryName(BuildEngine.ProjectFileOfTaskNode), outFile);
                }

                sourceFiles[i] = items[i].ItemSpec;
            }
        }

        #region ITask

        public IBuildEngine BuildEngine { get; set; }
        public ITaskHost HostObject { get; set; }

        #endregion ITask

        #region Backing Members

        private void Log(CompilerResult result)
        {
            string cwd = Path.GetDirectoryName(BuildEngine.ProjectFileOfTaskNode);
            string rel(string x) => string.Format("{0}\\{1}", Path.GetDirectoryName(x).Replace(cwd, string.Empty), Path.GetFileName(x));
            string merge(string[] l) => l.Length > 1 ? string.Concat('[', string.Join(" + ", l.Select(x => rel(x)).Take(3)), ']') : rel(l[0]);

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

        #endregion Backing Members
    }
}