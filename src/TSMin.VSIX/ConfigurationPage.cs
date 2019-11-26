using Microsoft.VisualStudio.Shell;
using System.ComponentModel;

namespace Acklann.TSMin
{
    public class ConfigurationPage : DialogPage
    {
        public static bool ShouldCompileOnSave;

        [DisplayName("Compile On Save")]
        [Description("Invoke compiler when a .ts file is saved.")]
        public bool CompileOnSave
        {
            get => ShouldCompileOnSave;
            set => ShouldCompileOnSave = value;
        }
    }
}