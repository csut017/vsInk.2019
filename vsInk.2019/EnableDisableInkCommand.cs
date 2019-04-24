using System;
using System.ComponentModel.Design;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;

namespace vsInk
{
    internal sealed class EnableDisableInkCommand
    {
        public const int CommandId = 0x3001;
        public static readonly Guid CommandSet = new Guid("D05DFCEA-D613-4AFC-A9D0-B53E1D836DC3");
        private readonly AsyncPackage package;
        private readonly MenuCommand command;
        private readonly GlobalSettings settings;

        private EnableDisableInkCommand(AsyncPackage package, OleMenuCommandService commandService, GlobalSettings settings)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));
            this.settings = settings;
            this.settings.SettingsChanged += this.UpdateState;

            var menuCommandID = new CommandID(CommandSet, CommandId);
            this.command = new MenuCommand(this.Execute, menuCommandID);
            commandService.AddCommand(this.command);
        }

        private void UpdateState(object sender, EventArgs e)
        {
            this.command.Checked = this.settings.IsEnabled;
        }

        public static EnableDisableInkCommand Instance
        {
            get;
            private set;
        }

        private Microsoft.VisualStudio.Shell.IAsyncServiceProvider ServiceProvider
        {
            get
            {
                return this.package;
            }
        }

        public static async Task InitializeAsync(AsyncPackage package, GlobalSettings settings)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);
            OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new EnableDisableInkCommand(package, commandService, settings);
        }

        private void Execute(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            this.settings.IsEnabled = !this.settings.IsEnabled;
            this.settings.NotifySettingsChanged();
        }
    }
}
