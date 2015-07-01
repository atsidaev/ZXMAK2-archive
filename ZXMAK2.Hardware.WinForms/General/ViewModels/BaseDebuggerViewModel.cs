using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using ZXMAK2.Engine.Interfaces;
using ZXMAK2.Mvvm;

namespace ZXMAK2.Hardware.WinForms.General
{
    public abstract class BaseDebuggerViewModel : BaseViewModel
    {
        private readonly ISynchronizeInvoke _synchronizeInvoke;
        
        
        protected BaseDebuggerViewModel(IDebuggable target, ISynchronizeInvoke synchronizeInvoke)
        {
            Target = target;
            _synchronizeInvoke = synchronizeInvoke;
        }

        public void Attach()
        {
            Target.UpdateState += Target_OnUpdateState;
            OnTargetStateChanged();
        }

        public void Detach()
        {
            Target.UpdateState -= Target_OnUpdateState;
        }


        #region Properties

        private bool _isRunning;
        
        public bool IsRunning
        {
            get { return _isRunning; }
            private set { PropertyChangeVal("IsRunning", ref _isRunning, value); }
        }

        #endregion Properties


        #region Private

        protected IDebuggable Target { get; private set; }

        protected void Invoke(Action action)
        {
            if (_synchronizeInvoke.InvokeRequired)
            {
                _synchronizeInvoke.Invoke(action, null);
            }
            else
            {
                action();
            }
        }

        private void Target_OnUpdateState(object sender, EventArgs e)
        {
            Invoke(OnTargetStateChanged);
        }

        protected virtual void OnTargetStateChanged()
        {
            IsRunning = Target == null || Target.IsRunning;
        }
        
        #endregion Private
    }
}
