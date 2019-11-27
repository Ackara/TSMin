using Microsoft.VisualStudio.Shell;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Acklann.TSBuild
{
    [Guid("d1a0510d-c863-4819-a5c7-ed6fe0f71a4c")]
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