using System;
using System.ComponentModel.Design;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;

namespace vsInk
{
    internal sealed class ChangeToEraseModeCommand
    {
        public const int CommandId = 0x3004;
        public static readonly Guid CommandSet = new Guid("D05DFCEA-D613-4AFC-A9D0-B53E1D836DC3");
        private readonly MenuCommand command;
        private readonly GlobalSettings settings;

        private ChangeToEraseModeCommand(OleMenuCommandService commandService, GlobalSettings settings)
        {
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));
            this.settings = settings;
            this.settings.SettingsChanged += this.UpdateState;

            var menuCommandID = new CommandID(CommandSet, CommandId);
            this.command = new MenuCommand(this.Execute, menuCommandID);
            commandService.AddCommand(this.command);
        }

        private void UpdateState(object sender, EventArgs e)
        {
            this.command.Enabled = this.settings.IsEnabled;
            this.command.Checked = this.settings.CurrentMode == GlobalSettings.Mode.Erase;
        }

        public static ChangeToEraseModeCommand Instance
        {
            get;
            private set;
        }

        public static async Task InitializeAsync(AsyncPackage package, GlobalSettings settings)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);
            OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new ChangeToEraseModeCommand(commandService, settings);
        }

        private void Execute(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            this.settings.CurrentMode = GlobalSettings.Mode.Erase;
            this.settings.NotifySettingsChanged();
        }
    }
}
