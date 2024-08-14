using StepBro.Core.Api;
using StepBro.Core.Data;
using StepBro.Core.Tasks;
using StepBro.ToolBarCreator;

namespace StepBro.UI.WinForms
{
    internal class ButtonLogic
    {
        private IButton m_parentControl;
        private ICoreAccess m_coreAccess;
        private enum Mode
        {
            NoAction = 0,
            CheckOnClick,
            FileElementActivateOnClick, 
            ProcedureClickToStop, 
            ProcedureRunWhilePushed, 
            ObjectCommandOnClick
        }
        private Mode m_mode = Mode.NoAction;
        private ActivationInfo m_startInfo;
        //private ProcedureActivationInfo m_stopProcedure;
        //private ProcedureActivationInfo m_enabledCheckProcedure;
        private IExecutionAccess m_execution = null;

        public enum ButtonCommand
        {
            ModeCheckOnClick,   // Set the button to be a "toggle button", like the CheckOnClick property does on a ToolStripMenuItem.
            Enable,
            Disable,
            SetChecked,
            SetUnchecked,
            ShowActive,
            ShowNormal,
            ShowAwaitingExecutionEnd,
            ShowPlaySymbol,
            ShowStopSymbol,
            ShowWaitSymbol
        }

        public interface IButton : IToolBarElement
        {
            void CommandHandler(ButtonCommand command);
            void BeginInvoke(Action action);
            bool Checked { get; set; }
        }

        public void UpdateMode()
        {
            if (m_mode == Mode.NoAction)
            {
                if (m_startInfo.IsFileElementUsed)
                {
                    m_mode = Mode.FileElementActivateOnClick;
                }
                else if (m_startInfo.IsObjectCommandUsed)
                {
                    m_mode = Mode.ObjectCommandOnClick;
                }
            }
        }

        public void SetStoppable()
        {
            m_mode = Mode.ProcedureClickToStop;
            this.SendCommand(ButtonCommand.ModeCheckOnClick);
        }
        public void SetStopOnButtonRelease()
        {
            m_mode = Mode.ProcedureRunWhilePushed;
        }

        public void SetCheckOnClick()
        {
            if (m_mode == Mode.NoAction)
            {
                m_mode = Mode.CheckOnClick;
                this.SendCommand(ButtonCommand.ModeCheckOnClick);
            }
        }

        public bool HasStartAction { get { return m_startInfo != null; } }
        public ActivationInfo StartAction
        {
            get
            {
                if (m_startInfo == null)
                {
                    m_startInfo = new ActivationInfo();
                }
                return m_startInfo;
            }
        }

        public ButtonLogic(IButton parentControl, ICoreAccess coreAccess)
        {
            m_parentControl = parentControl;
            m_coreAccess = coreAccess;
        }

        //public bool Setup(PropertyBlockEntry field)
        //{
        //    if (field.BlockEntryType == PropertyBlockEntryType.Value)
        //    {
        //        var valueField = field as PropertyBlockValue;
        //        if (valueField.Name == "Procedure")
        //        {
        //            if (m_startInfo == null)
        //            {
        //                m_startInfo = new ActivationInfo();
        //            }
        //            m_startInfo.Name = valueField.ValueAsString();
        //        }
        //        else if (valueField.Name == "Model" || valueField.Name == "Partner")
        //        {
        //            if (m_startInfo == null)
        //            {
        //                m_startInfo = new ActivationInfo();
        //            }
        //            m_startInfo.Partner = valueField.ValueAsString();
        //        }
        //        else if (valueField.Name == "Instance" || valueField.Name == "Object")
        //        {
        //            if (m_startInfo == null)
        //            {
        //                m_startInfo = new ActivationInfo();
        //            }
        //            m_startInfo.TargetObject = valueField.ValueAsString();
        //        }
        //        else return false;
        //    }
        //    else if (field.BlockEntryType == PropertyBlockEntryType.Flag)
        //    {
        //        var flagField = field as PropertyBlockFlag;
        //        if (flagField.Name == "Stoppable")
        //        {
        //            m_mode = Mode.ClickToStop;
        //            this.SendCommand(ButtonCommand.ModeCheckOnClick);
        //        }
        //        else if (flagField.Name == "StopOnButtonRelease")
        //        {
        //            m_mode = Mode.RunWhilePushed;
        //        }
        //        else return false;
        //    }
        //    else return false;
        //    return true;
        //}

        private void SendCommand(ButtonCommand command)
        {
            m_parentControl.CommandHandler(command);
        }

        private void DoStartProcedure()
        {
            if (m_startInfo != null && !String.IsNullOrEmpty(m_startInfo.FileElementName))
            {
                if (m_mode == Mode.FileElementActivateOnClick)
                {
                    this.SendCommand(ButtonCommand.Disable);
                }
                else if (m_mode == Mode.ProcedureClickToStop)
                {
                    this.SendCommand(ButtonCommand.ShowActive);
                    this.SendCommand(ButtonCommand.ShowStopSymbol);
                }

                string element = m_startInfo.FileElementName;
                if (element == null) { element = m_parentControl.ParentElement.TryGetChildProperty("Element") as string; }
                string partner = m_startInfo.Partner;
                if (partner == null) { partner = m_parentControl.ParentElement.TryGetChildProperty("Partner") as string; }
                string instance = m_startInfo.TargetObject;
                if (instance == null) { instance = m_parentControl.ParentElement.TryGetChildProperty("Instance") as string; }

                List<object> arguments = m_startInfo.Arguments;

                m_execution = m_coreAccess.StartExecution(element, partner, instance, (arguments != null) ? arguments.ToArray() : null);
                if (m_execution != null)
                {
                    m_execution.CurrentStateChanged += Execution_CurrentStateChanged;
                }
                else
                {
                    this.SendCommand(ButtonCommand.Enable);
                    this.SendCommand(ButtonCommand.ShowNormal);
                }
                System.Diagnostics.Debug.WriteLine("StartProcedure End");
            }
        }

        private void DoStopProcedure()
        {
            if (m_execution != null && !m_execution.State.HasEnded())
            {
                m_execution.RequestStopExecution();
                if (m_mode == Mode.ProcedureClickToStop)
                {
                    this.SendCommand(ButtonCommand.Disable);
                    this.SendCommand(ButtonCommand.ShowAwaitingExecutionEnd);
                }
            }
        }

        private void Execution_CurrentStateChanged(object sender, EventArgs e)
        {
            m_parentControl.BeginInvoke(this.HandleExecutionStateChange);
        }

        private void HandleExecutionStateChange()
        {
            if (m_execution != null)
            {
                System.Diagnostics.Debug.WriteLine("Execution_CurrentStateChanged; in GUI");
                if (m_execution.State.HasEnded())
                {
                    m_execution.CurrentStateChanged -= Execution_CurrentStateChanged;
                    m_execution = null;
                    this.SendCommand(ButtonCommand.Enable);
                    this.SendCommand(ButtonCommand.ShowNormal);
                }
            }
        }

        public void ButtonClicked()
        {
            if (m_mode == Mode.FileElementActivateOnClick)
            {
                this.DoStartProcedure();
            }
            else if (m_mode == Mode.ObjectCommandOnClick)
            {
                string instance = m_startInfo.TargetObject;
                if (instance == null) { instance = m_parentControl.ParentElement.TryGetChildProperty("Instance") as string; }
                m_coreAccess.ExecuteObjectCommand(instance, m_startInfo.ObjectCommand);
            }
            else if (m_mode == Mode.CheckOnClick)
            {
                if (m_parentControl.Checked)
                {
                    this.SendCommand(ButtonCommand.SetChecked);
                }
                else
                {
                    this.SendCommand(ButtonCommand.SetUnchecked);
                }
            }
        }

        public void ButtonPushed()
        {
            if (m_mode == Mode.ProcedureRunWhilePushed)
            {
                this.DoStartProcedure();
            }
        }

        public void ButtonReleased()
        {
            if (m_mode == Mode.ProcedureRunWhilePushed)
            {
                this.DoStopProcedure();
            }
        }

        public void CheckedChanged(bool @checked)
        {
            if (@checked)
            {
                this.DoStartProcedure();
            }
            else
            {
                this.DoStopProcedure();
            }
        }
    }
}
