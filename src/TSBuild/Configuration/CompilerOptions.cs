namespace Acklann.TSBuild.Configuration
{
    public readonly struct CompilerOptions
    {
        public CompilerOptions(string configurationFile, bool? minify = default, bool? generateSourceMaps = default)
        {
            Minify = minify;
            ConfigurationFile = configurationFile;
            GenerateSourceMaps = generateSourceMaps;
        }

        public const string DEFAULT_FILE_NAME = "transpiler.json";

        public bool? Minify { get; }

        public bool? GenerateSourceMaps { get; }

        public string ConfigurationFile { get; }

        public string ToArgs()
        {
            string escape(object obj) => string.Concat('"', obj, '"');
            string toJs(bool? boolean) { if (boolean.HasValue) return (boolean.Value ? "true" : "false"); else return "\"null\""; }

            return string.Concat(
                escape(ConfigurationFile), ' ',
                toJs(Minify), ' ',
                toJs(GenerateSourceMaps)
                );
        }
    }
}