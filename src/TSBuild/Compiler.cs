using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Acklann.TSBuild
{
    public class Compiler
    {
        public static CompilerResult Run(Configuration.CompilerOptions options, string currentDirectory = default)
        {
            string scriptPath = Path.Combine(NodeJS.InstallationDirectory, "compiler.js");
            if (!File.Exists(scriptPath)) throw new FileNotFoundException($"Could not find file at '{scriptPath}'.");

            long start = System.DateTime.Now.Ticks;
            string cwd = (Path.GetDirectoryName(options.ConfigurationFile)?? currentDirectory);

            using (Process node = NodeJS.Execute($"/c node \"{scriptPath}\" {options.ToArgs()}", cwd))
            {
                GetOutput(node.StandardOutput, cwd, out string[] sourceFiles, out string[] generatedFiles);

                return new CompilerResult(
                    (node.ExitCode == 0),
                    GetErrors(node.StandardError).ToArray(),
                    sourceFiles,
                    generatedFiles,
                    System.TimeSpan.FromTicks(System.DateTime.Now.Ticks - start)
                    );
            }
        }

        public static   Task<CompilerResult> RunAsync(Configuration.CompilerOptions options, string currentDirectory = default)
            => Task.Run(() => { return Run(options, currentDirectory); });

        public static string FindConfigurationFile(string currentDirectory = default)
        {
            return Directory.EnumerateFiles(
                currentDirectory ?? Directory.GetCurrentDirectory(),
                Configuration.CompilerOptions.DEFAULT_FILE_NAME,
                SearchOption.TopDirectoryOnly
                ).FirstOrDefault();
        }

        private static void GetOutput(StreamReader reader, string cwd, out string[] sourceFiles, out string[] compiledFiles)
        {
            string filePath;
            var s = new List<string>();
            var c = new List<string>();

            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine();
                System.Diagnostics.Debug.WriteLine(line);

                if (!string.IsNullOrEmpty(line) && line.StartsWith("<<"))
                {
                    filePath = line.Substring(2).Trim();
                    s.Add(Path.IsPathRooted(filePath) ? filePath : Path.Combine(cwd, filePath));
                }

                if (!string.IsNullOrEmpty(line) && line.StartsWith(">>"))
                {
                    filePath = line.Substring(2).Trim();
                    c.Add(Path.IsPathRooted(filePath) ? filePath : Path.Combine(cwd, filePath));
                }
            }

            sourceFiles = s.Distinct().ToArray();
            compiledFiles = c.Distinct().ToArray();
        }

        private static IEnumerable<CompilerError> GetErrors(StreamReader reader)
        {
            if (reader == null) yield break;

            JObject json; string line = null;
            while (!reader.EndOfStream)
            {
                line = reader.ReadLine();
#if DEBUG
                System.Diagnostics.Debug.WriteLine(line);
#endif
                if (string.IsNullOrEmpty(line) || !line.StartsWith("{")) continue;

                json = JObject.Parse(line);
                yield return new CompilerError(
                    json["message"].Value<string>(),
                    (json["file"]?.Value<string>() ?? default),
                    (json["line"]?.Value<int>() ?? default),
                    (json["column"]?.Value<int>() ?? default),
                    ((ErrorSeverity)(json["level"]?.Value<int>() ?? (int)ErrorSeverity.Error)),
                    (json["status"]?.Value<int>() ?? default)
                );
            }
        }
    }
}