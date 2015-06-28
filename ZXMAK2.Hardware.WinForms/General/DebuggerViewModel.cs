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
                "Step Into");
            CommandStepOut = new CommandDelegate(
                () => { },
                () => false,
                "Step Out");
        }

        public ICommand CommandContinue;
        public ICommand CommandBreak;
        public ICommand CommandStepInto;
        public ICommand CommandStepOver;
        public ICommand CommandStepOut;


        public void Attach()
        {
            _target.UpdateState += Target_OnUpdateState;
        }

        public void Detach()
        {
            _target.UpdateState -= Target_OnUpdateState;
        }


        #region Commands

        private bool CommandContinue_OnCanExecute()
        {
            return _target != null && !_target.IsRunning;
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
            return _target != null && _target.IsRunning;
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
            return _target != null && !_target.IsRunning;
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
            return _target != null && !_target.IsRunning;
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
            CommandContinue.Update();
            CommandBreak.Update();
            CommandStepInto.Update();
            CommandStepOver.Update();
            CommandStepOut.Update();
        }
    }
}
