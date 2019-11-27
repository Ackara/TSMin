using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.ComponentModel.Design;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Threading;
using Task = System.Threading.Tasks.Task;

namespace Acklann.TSMin
{
    [Guid(Symbol.Package.GuidString)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [ProvideOptionPage(typeof(ConfigurationPage), Symbol.Name, "General", 0, 0, true)]
    [ProvideAutoLoad(VSConstants.UICONTEXT.SolutionExistsAndFullyLoaded_string, PackageAutoLoadFlags.BackgroundLoad)]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "pkgdef, VS and vsixmanifest are valid VS terms")]
    public sealed class VSPackage : AsyncPackage
    {
        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            NodeJS.Install((msg, _, __) => { System.Diagnostics.Debug.WriteLine(msg); });
            await this.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

            vs = (EnvDTE.DTE)await GetServiceAsync(typeof(EnvDTE.DTE));

            var commandService = (await GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService);
            commandService.AddCommand(new OleMenuCommand(OnGotoConfigurationPageCommandInvoked, new CommandID(Symbol.CmdSet.Guid, Symbol.CmdSet.GotoConfigurationPageCommandId)));
            commandService.AddCommand(new OleMenuCommand(OnConfigureCompileOnBuildCommandInvoked, new CommandID(Symbol.CmdSet.Guid, Symbol.CmdSet.ConfigureCompileOnBuildCommandId)));

            var outputWindow = (IVsOutputWindow)await GetServiceAsync(typeof(SVsOutputWindow));
            if (outputWindow != null)
            {
                var guid = new Guid("3b0eb69d-dbb6-495b-9e72-f8967f02bd23");
                outputWindow.CreatePane(ref guid, Symbol.Name, 1, 1);
                outputWindow.GetPane(ref guid, out IVsOutputWindowPane pane);

                _watcher = new TypescriptWatcher(this, pane);
            }
        }

        private void OnConfigureCompileOnBuildCommandInvoked(object sender, EventArgs e)
        {
        }

        private void OnGotoConfigurationPageCommandInvoked(object sender, EventArgs e)
        {
        }

        #region Backing Members

        private EnvDTE.DTE vs;
        private TypescriptWatcher _watcher;

        #endregion Backing Members
    }
}