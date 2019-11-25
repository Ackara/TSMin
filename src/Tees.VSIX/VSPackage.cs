using System;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.Win32;
using Task = System.Threading.Tasks.Task;

namespace Acklann.Tees
{
    [Guid(Symbol.Package.GuidString)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "pkgdef, VS and vsixmanifest are valid VS terms")]
    public sealed class VSPackage : AsyncPackage
    {
        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            await this.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

            vs = (EnvDTE.DTE)await GetServiceAsync(typeof(EnvDTE.DTE));

            var commandService = (await GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService);
            commandService.AddCommand(new OleMenuCommand(OnGotoConfigurationPageCommandInvoked, new CommandID(Symbol.CmdSet.Guid, Symbol.CmdSet.GotoConfigurationPageCommandId)));
            commandService.AddCommand(new OleMenuCommand(OnConfigureCompileOnBuildCommandInvoked, new CommandID(Symbol.CmdSet.Guid, Symbol.CmdSet.ConfigureCompileOnBuildCommandId)));
        }

        private void OnConfigureCompileOnBuildCommandInvoked(object sender, EventArgs e)
        {
            
        }

        private void OnGotoConfigurationPageCommandInvoked(object sender, EventArgs e)
        {

        }

        #region Backing Members
        private EnvDTE.DTE vs;
        #endregion
    }
}
