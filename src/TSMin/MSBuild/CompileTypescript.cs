using Microsoft.Build.Framework;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Acklann.TSMin.MSBuild
{
    public class CompileTypescript : ITask
    {
        [Required]
        public ITaskItem[] SourceFiles { get; set; }

        public string OutputFile { get; set; }

        public bool GenerateSourceMap { get; set; }

        public bool Minify { get; set; }

        public bool Execute()
        {
            NodeJS.Install((msg, _, __) => { Log(msg); });

            var options = new CompilerOptions
            {
                Minify = Minify,
                GenerateSourceMaps = GenerateSourceMap,
                OutputFile = GetFullPath(OutputFile)
            };

            CompilerResult result = Compiler.Compile(options, GetFullPaths(SourceFiles).ToArray());
            foreach (CompilerError item in result.Errors) Log(item);
            if (!result.HasErrors) Log(result);

            return result.HasErrors == false;
        }

        private static IEnumerable<string> GetFullPaths(ITaskItem[] items)
        {
            string fullPath;
            int n = items.Length;
            for (int i = 0; i < n; i++)
            {
                fullPath = items[i].GetMetadata(FullPath);
                if (string.IsNullOrEmpty(fullPath)) continue;
                yield return fullPath;
            }
        }

        private string GetFullPath(string path)
        {
            if (Path.IsPathRooted(path)) return path;
            else return Path.Combine(Path.GetDirectoryName(BuildEngine.ProjectFileOfTaskNode), path);
        }

        #region ITask

        public IBuildEngine BuildEngine { get; set; }
        public ITaskHost HostObject { get; set; }

        #endregion ITask

        #region Backing Members

        public const string FullPath = "FullPath";

        private void Log(CompilerResult result)
        {
            string cwd = Path.GetDirectoryName(BuildEngine.ProjectFileOfTaskNode);
            string rel(string x) => (x == null ? null : string.Format("{0}\\{1}", Path.GetDirectoryName(x).Replace(cwd, string.Empty), Path.GetFileName(x)));
            string merge(string[] l) => l.Length > 1 ? string.Concat('[', string.Join(" + ", l.Select(x => rel(x)).Take(3)), "...]") : string.Join(string.Empty, l.Select(x => rel(x)));

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