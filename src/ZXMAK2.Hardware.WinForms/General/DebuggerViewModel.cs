using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using ZXMAK2.Engine.Interfaces;
using ZXMAK2.Mvvm;

namespace ZXMAK2.Hardware.WinForms.General
{
    public class DebuggerViewModel : BaseViewModel
    {
        private readonly IDebuggable _target;
        private readonly ISynchronizeInvoke _synchronizeInvoke;

        public DebuggerViewModel(IDebuggable target, ISynchronizeInvoke synchronizeInvoke)
        {
            _target = target;
            _synchronizeInvoke = synchronizeInvoke;
            CommandClose = new CommandDelegate(
                CommandClose_OnExecute,
                CommandClose_OnCanExecute,
                "Close");
            CommandContinue = new CommandDelegate(
                CommandContinue_OnExecute, 
                CommandContinue_OnCanExecute, 
                "Continue");
            CommandBreak = new CommandDelegate(
                CommandBreak_OnExecute,
                CommandBreak_OnCanExecute,
                "Break");
            CommandStepInto = new CommandDelegate(
                CommandStepInto_OnExecute,
                CommandStepInto_OnCanExecute,
                "Step Into");
            CommandStepOver = new CommandDelegate(
                CommandStepOver_OnExecute,
                CommandStepOver_OnCanExecute,
                "Step Over");
            CommandStepOut = new CommandDelegate(
                () => { },
                () => false,
                "Step Out");
        }

        public event EventHandler CloseRequest;

        public void Attach()
        {
            _target.UpdateState += Target_OnUpdateState;
        }

        public void Detach()
        {
            _target.UpdateState -= Target_OnUpdateState;
        }

        public void Close()
        {
            var handler = CloseRequest;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }


        #region Properties

        public ICommand CommandClose { get; private set; }
        public ICommand CommandContinue { get; private set; }
        public ICommand CommandBreak { get; private set; }
        public ICommand CommandStepInto { get; private set; }
        public ICommand CommandStepOver { get; private set; }
        public ICommand CommandStepOut { get; private set; }

        public bool IsRunning
        {
            get { return _target == null || _target.IsRunning; }
        }

        #endregion Properties


        #region Commands

        private bool CommandClose_OnCanExecute()
        {
            return true;
        }

        private void CommandClose_OnExecute()
        {
            if (!CommandClose_OnCanExecute())
            {
                return;
            }
            Close();
        }

        private bool CommandContinue_OnCanExecute()
        {
            return _target != null && !IsRunning;
        }

        private void CommandContinue_OnExecute()
        {
            if (!CommandContinue_OnCanExecute())
            {
                return;
            }
            _target.DoRun();
        }

        private bool CommandBreak_OnCanExecute()
        {
            return _target != null && IsRunning;
        }

        private void CommandBreak_OnExecute()
        {
            if (!CommandBreak_OnCanExecute())
            {
                return;
            }
            _target.DoStop();
        }

        private bool CommandStepInto_OnCanExecute()
        {
            return _target != null && !IsRunning;
        }

        private void CommandStepInto_OnExecute()
        {
            if (!CommandStepInto_OnCanExecute())
            {
                return;
            }
            _target.DoStepInto();
        }

        private bool CommandStepOver_OnCanExecute()
        {
            return _target != null && !IsRunning;
        }

        private void CommandStepOver_OnExecute()
        {
            if (!CommandStepOver_OnCanExecute())
            {
                return;
            }
            _target.DoStepOver();
        }

        #endregion Commands


        #region Private

        private void Target_OnUpdateState(object sender, EventArgs e)
        {
            if (_synchronizeInvoke.InvokeRequired)
            {
                _synchronizeInvoke.Invoke(new Action(Target_OnUpdateStateSynchronized), null);
            }
            else
            {
                Target_OnUpdateStateSynchronized();
            }
        }

        private void Target_OnUpdateStateSynchronized()
        {
            OnPropertyChanged("IsRunning");
            CommandClose.Update();
            CommandContinue.Update();
            CommandBreak.Update();
            CommandStepInto.Update();
            CommandStepOver.Update();
            CommandStepOut.Update();
        }

        #endregion Private
    }
}
