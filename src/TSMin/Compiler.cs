using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Acklann.TSMin
{
    public class Compiler
    {
        public static CompilerResult Compile(CompilerOptions options, params string[] sourceFiles)
        {
            string scriptPath = Path.Combine(NodeJS.InstallationDirectory, "compiler.js");
            if (!File.Exists(scriptPath)) throw new FileNotFoundException($"Could not find file at '{scriptPath}'.");

            long start = System.DateTime.Now.Ticks;
            using (Process node = NodeJS.Execute($"/c node \"{scriptPath}\" \"{string.Join(";", sourceFiles)}\" {options.ToArgs()}"))
            {
                return new CompilerResult
                {
                    SourceFiles = sourceFiles,
                    Success = (node.ExitCode == 0),
                    Errors = GetErrors(node.StandardError).ToArray(),
                    GeneratedFiles = GetGeneratedFiles(node.StandardOutput).ToArray(),
                    Elapse = System.TimeSpan.FromTicks(System.DateTime.Now.Ticks - start)
                };
            }
        }

        public static Task<CompilerResult> CompilerAsync(CompilerOptions options, params string[] sourceFiles)
            => Task.Run(() => Compile(options, sourceFiles));

        public static IEnumerable<string> FindFiles(string directoryPath)
        {
            if (!Directory.Exists(directoryPath)) throw new DirectoryNotFoundException($"Could not find directory at '{directoryPath}'.");

            return from x in Directory.EnumerateFiles(directoryPath, "*.ts", SearchOption.AllDirectories)
                   where !x.Contains(@"\node_modules\")
                   select x;
        }

        private static IEnumerable<string> GetGeneratedFiles(StreamReader reader)
        {
            string line = null;
            while (!reader.EndOfStream)
            {
                line = reader.ReadLine();
#if DEBUG
                System.Diagnostics.Debug.WriteLine(line);
#endif
                if (string.IsNullOrEmpty(line) || !line.StartsWith("-> ")) continue;

                yield return line.Substring(3).Trim();
            }
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