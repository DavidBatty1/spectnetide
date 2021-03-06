﻿using Spect.Net.VsPackage.Vsx;
using Spect.Net.Wpf.Mvvm;

namespace Spect.Net.VsPackage.ToolWindows.StackTool
{
    /// <summary>
    /// Interaction logic for StackToolWindowControl.xaml
    /// </summary>
    public partial class StackToolWindowControl : ISupportsMvvm<StackToolWindowViewModel>
    {
        /// <summary>
        /// Gets the view model instance
        /// </summary>
        public StackToolWindowViewModel Vm { get; private set; }

        /// <summary>
        /// Sets the view model instance
        /// </summary>
        /// <param name="vm">View model instance to set</param>
        void ISupportsMvvm<StackToolWindowViewModel>.SetVm(StackToolWindowViewModel vm)
        {
            DataContext = Vm = vm;
        }

        /// <summary>
        /// Initializes the view
        /// </summary>
        public StackToolWindowControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Override this message to respond to vm state changes events
        /// </summary>
        protected override void OnVmStateChanged(VmState oldState, VmState newState)
        {
            if (newState == VmState.Paused)
            {
                Vm.Refresh();
            }
        }

        /// <summary>
        /// Override this message to respond to screen refresh events
        /// </summary>
        protected override void OnVmScreenRefreshed()
        {
            if (Vm.ScreenRefreshCount % 10 == 0)
            {
                Vm.Refresh();
            }
        }
    }
}
