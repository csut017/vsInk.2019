using System;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;

namespace vsInk
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [Guid(VisualStudioPackage.PackageGuidString)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideToolWindow(typeof(ConfigurationWindow), MultiInstances = false, Transient = true, Style = VsDockStyle.Tabbed, Window = "3ae79031-e1bc-11d0-8f78-00a0c9110057")]
    public sealed class VisualStudioPackage : AsyncPackage, IVsSolutionEvents, IDisposable
    {
        public const string PackageGuidString = "e266fa7c-a429-4d46-8516-5b73e25fd9cd";

        private IVsSolution solution;
        private uint solutionEventsCookie;
        private AnnotationStoreFactory storeFactory;

        public GlobalSettings Settings { get; private set; }

        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            var componentModel = (IComponentModel)GetGlobalService(typeof(SComponentModel));
            this.Settings = componentModel.GetService<GlobalSettings>();
            this.storeFactory = componentModel.GetService<AnnotationStoreFactory>();
            await this.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
            await EnableDisableInkCommand.InitializeAsync(this, this.Settings);
            await ChangeToPenModeCommand.InitializeAsync(this, this.Settings);
            await ChangeToEraseModeCommand.InitializeAsync(this, this.Settings);
            await ShowConfigurationWindowCommand.InitializeAsync(this, this.Settings);

            this.solution = GetGlobalService(typeof(SVsSolution)) as IVsSolution;
            if (this.solution != null)
            {
                this.solution.AdviseSolutionEvents(this, out this.solutionEventsCookie);
            }
        }

        int IVsSolutionEvents.OnAfterCloseSolution(object pUnkReserved)
        {
            this.storeFactory.Clear();
            return VSConstants.S_OK;
        }

        int IVsSolutionEvents.OnAfterLoadProject(IVsHierarchy pStubHierarchy, IVsHierarchy pRealHierarchy)
        {
            return VSConstants.S_OK;
        }

        int IVsSolutionEvents.OnAfterOpenProject(IVsHierarchy pHierarchy, int fAdded)
        {
            return VSConstants.S_OK;
        }

        int IVsSolutionEvents.OnAfterOpenSolution(object pUnkReserved, int fNewSolution)
        {
            return VSConstants.S_OK;
        }

        int IVsSolutionEvents.OnBeforeCloseProject(IVsHierarchy pHierarchy, int fRemoved)
        {
            return VSConstants.S_OK;
        }

        int IVsSolutionEvents.OnBeforeCloseSolution(object pUnkReserved)
        {
            return VSConstants.S_OK;
        }

        int IVsSolutionEvents.OnBeforeUnloadProject(IVsHierarchy pRealHierarchy, IVsHierarchy pStubHierarchy)
        {
            return VSConstants.S_OK;
        }

        int IVsSolutionEvents.OnQueryCloseProject(IVsHierarchy pHierarchy, int fRemoving, ref int pfCancel)
        {
            return VSConstants.S_OK;
        }

        int IVsSolutionEvents.OnQueryCloseSolution(object pUnkReserved, ref int pfCancel)
        {
            return VSConstants.S_OK;
        }

        int IVsSolutionEvents.OnQueryUnloadProject(IVsHierarchy pRealHierarchy, ref int pfCancel)
        {
            return VSConstants.S_OK;
        }

        public void Dispose()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if ((solution != null) && (solutionEventsCookie != 0))
            {
                GC.SuppressFinalize(this);
                this.solution.UnadviseSolutionEvents(solutionEventsCookie);
                solutionEventsCookie = 0;
                solution = null;
            }
        }
    }
}
