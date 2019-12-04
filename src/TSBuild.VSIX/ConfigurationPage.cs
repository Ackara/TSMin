using Microsoft.VisualStudio.Shell;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Acklann.TSBuild
{
    [Guid("d1a0510d-c863-4819-a5c7-ed6fe0f71a4c")]
    public class ConfigurationPage : DialogPage
    {
        public ConfigurationPage()
        {
            ShouldCompileOnSave = ShouldGenerateSourceMaps = ShouldMinify = true;
            ConfigurationFileName = Configuration.CompilerOptions.DEFAULT_FILE_NAME;
        }

        public static string ConfigurationFileName = null;
        public static bool ShouldCompileOnSave, ShouldGenerateSourceMaps, ShouldMinify;

        [DisplayName("Compile On Save")]
        [Description("When enabled the saved .ts file will be compiled immediately.")]
        public bool CompileOnSave
        {
            get => ShouldCompileOnSave;
            set => ShouldCompileOnSave = value;
        }

        [DisplayName("Generate Source Maps")]
        [Description("When enabled a .map files will be generated for each compiled .ts file.")]
        public bool GenerateSourceMaps
        {
            get => ShouldGenerateSourceMaps;
            set { ShouldGenerateSourceMaps = value; }
        }

        [DisplayName("Minify Javascript Files")]
        [Description("When enabled the compiled .ts files will be minified before they are saved to disk.")]
        public bool Minfiy
        {
            get => ShouldMinify;
            set { ShouldMinify = value; }
        }

        [DisplayName("Default Configuration File Name")]
        public string DefaultName
        {
            get => ConfigurationFileName;
            set { ConfigurationFileName = value; }
        }
    }
}