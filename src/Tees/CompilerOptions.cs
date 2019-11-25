namespace Acklann.Tees
{
    public class CompilerOptions
    {
        public CompilerOptions()
        {
            Suffix = string.Empty;
            Minify = GenerateSourceMaps = true;
        }

        public bool Minify { get; set; }
        public string Suffix { get; set; }
        public string OutputDirectory { get; set; }

        public string SourceMapDirectory { get; set; }
        public bool GenerateSourceMaps { get; set; }

        public string ToArgs()
        {
            string toJs(bool bit) => (bit ? "true" : "false");
            string escape(object obj) => string.Concat('"', obj, '"');

            return string.Concat(
                escape(OutputDirectory), " ",
                escape(SourceMapDirectory), " ",

                escape(Suffix), " ",
                toJs(Minify), " ",

                toJs(GenerateSourceMaps), " "
                );
        }
    }
}