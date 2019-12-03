using System.IO;

namespace Acklann.TSBuild
{
    public class CompilerOptions
    {
        public CompilerOptions()
        {
            Minify = GenerateSourceMaps = true;
        }

        public bool Minify { get; set; }
        public bool GenerateSourceMaps { get; set; }

        public string OutputFile { get; set; }
        public string ConfigurationFile { get; set; }

        public string ToArgs()
        {
            string toJs(bool bit) => (bit ? "true" : "false");
            string escape(object obj) => string.Concat('"', obj, '"');

            return string.Concat(
                escape(ConfigurationFile), ' ',
                escape(Path.ChangeExtension(OutputFile, ".js")), ' ',

                toJs(Minify), ' ',
                toJs(GenerateSourceMaps), ' '
                );
        }
    }
}