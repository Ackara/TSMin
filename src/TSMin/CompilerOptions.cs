namespace Acklann.TSMin
{
    public class CompilerOptions
    {
        public CompilerOptions()
        {
            Minify = GenerateSourceMaps = true;
        }

        public bool Minify { get; set; }
        public string OutputFile { get; set; }
        public string OutputDirectory { get; set; }

        public string SourceMapDirectory { get; set; }
        public bool GenerateSourceMaps { get; set; }

        public string ToArgs()
        {
            string toJs(bool bit) => (bit ? "true" : "false");
            string escape(object obj) => string.Concat('"', obj, '"');

            return string.Concat(
                toJs(Minify), " ",
                escape(OutputFile), " ",

                escape(SourceMapDirectory), " ",
                toJs(GenerateSourceMaps)
                );
        }
    }
}