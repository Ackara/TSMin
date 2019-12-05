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
            ConfigurationFileName = Configuration.CompilerOptions.DEFAULT_FILE_NAME;
            ShouldCompileOnSave = ShouldGenerateSourceMaps = ShouldMinify = ShouldShowCompilerErrors = true;
        }

        public const string Category = "General";
        public static string ConfigurationFileName;
        public static bool ShouldCompileOnSave, ShouldGenerateSourceMaps, ShouldMinify, ShouldShowCompilerErrors;

        [Category(Category)]
        [DisplayName("Compile On Save")]
        [Description("When enabled the saved Typescript file will be compiled immediately.")]
        public bool CompileOnSave
        {
            get => ShouldCompileOnSave;
            set => ShouldCompileOnSave = value;
        }

        [Category(Category)]
        [DisplayName("Generate Source Maps")]
        [Description("When enabled a .map files will be generated for each compiled Typescript file.")]
        public bool GenerateSourceMaps
        {
            get => ShouldGenerateSourceMaps;
            set { ShouldGenerateSourceMaps = value; }
        }

        [Category(Category)]
        [DisplayName("Minify Javascript Files")]
        [Description("When enabled the compiled Typescript files will be minified before they are saved to disk.")]
        public bool Minfiy
        {
            get => ShouldMinify;
            set { ShouldMinify = value; }
        }

        [Category(Category)]
        [DisplayName("Show Compilations Errors")]
        [Description("Show compiler errors for Typescript files.")]
        public bool DisplayErrors
        {
            get => ShouldShowCompilerErrors;
            set { ShouldShowCompilerErrors = value; }
        }

        [Category(Category)]
        [DisplayName("Default Configuration File Name")]
        public string DefaultName
        {
            get => ConfigurationFileName;
            set { ConfigurationFileName = value; }
        }
    }
}