using StepBro.Core.Api;
using StepBro.Core.Data;
using StepBro.Core.Tasks;

namespace StepBro.UI.WinForms
{
    internal class ProcedureActivationButtonLogic
    {
        private IProcedureActivationButton m_parentControl;
        private ICoreAccess m_coreAccess;
        private enum Mode { ActivateOnClick = 0, ClickToStop, RunWhilePushed }
        private Mode m_mode = Mode.ActivateOnClick;
        private ProcedureActivationInfo m_startProcedure;
        private ProcedureActivationInfo m_stopProcedure;
        private ProcedureActivationInfo m_enabledCheckProcedure;
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

        public interface IProcedureActivationButton
        {
            void CommandHandler(ButtonCommand command);
            void BeginInvoke(Action action);
        }


        public ProcedureActivationButtonLogic(IProcedureActivationButton parentControl, ICoreAccess coreAccess)
        {
            m_parentControl = parentControl;
            m_coreAccess = coreAccess;
        }

        public bool Setup(PropertyBlockEntry field)
        {
            if (field.BlockEntryType == PropertyBlockEntryType.Value)
            {
                var valueField = field as PropertyBlockValue;
                if (valueField.Name == "Procedure")
                {
                    if (m_startProcedure == null)
                    {
                        m_startProcedure = new ProcedureActivationInfo();
                    }
                    m_startProcedure.Name = valueField.ValueAsString();
                }
                else if (valueField.Name == "Model" || valueField.Name == "Partner")
                {
                    if (m_startProcedure == null)
                    {
                        m_startProcedure = new ProcedureActivationInfo();
                    }
                    m_startProcedure.Partner = valueField.ValueAsString();
                }
                else if (valueField.Name == "Instance" || valueField.Name == "Object")
                {
                    if (m_startProcedure == null)
                    {
                        m_startProcedure = new ProcedureActivationInfo();
                    }
                    m_startProcedure.TargetObject = valueField.ValueAsString();
                }
                else return false;
            }
            else if (field.BlockEntryType == PropertyBlockEntryType.Flag)
            {
                var flagField = field as PropertyBlockFlag;
                if (flagField.Name == "Stoppable")
                {
                    m_mode = Mode.ClickToStop;
                    this.SendCommand(ButtonCommand.ModeCheckOnClick);
                }
                else if (flagField.Name == "StopOnButtonRelease")
                {
                    m_mode = Mode.RunWhilePushed;
                }
                else return false;
            }
            else return false;
            return true;
        }

        private void SendCommand(ButtonCommand command)
        {
            m_parentControl.CommandHandler(command);
        }

        private void StartProcedure()
        {
            if (m_startProcedure != null && !String.IsNullOrEmpty(m_startProcedure.Name))
            {
                if (m_mode == Mode.ActivateOnClick)
                {
                    this.SendCommand(ButtonCommand.Disable);
                }
                else if (m_mode == Mode.ClickToStop)
                {
                    this.SendCommand(ButtonCommand.ShowActive);
                    this.SendCommand(ButtonCommand.ShowStopSymbol);
                }
                m_execution = m_coreAccess.StartExecution(m_startProcedure.Name, m_startProcedure.Partner, m_startProcedure.TargetObject, null);
                m_execution.CurrentStateChanged += Execution_CurrentStateChanged;
                System.Diagnostics.Debug.WriteLine("StartProcedure End");
            }
        }

        private void StopProcedure()
        {
            if (m_execution != null && !m_execution.State.HasEnded())
            {
                m_execution.RequestStopExecution();
                if (m_mode == Mode.ClickToStop)
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
            if (m_mode == Mode.ActivateOnClick)
            {
                this.StartProcedure();
            }
        }

        public void ButtonPushed()
        {
            if (m_mode == Mode.RunWhilePushed)
            {
                this.StartProcedure();
            }
        }

        public void ButtonReleased()
        {
            if (m_mode == Mode.RunWhilePushed)
            {
                this.StopProcedure();
            }
        }

        public void CheckedChanged(bool @checked)
        {
            if (@checked)
            {
                this.StartProcedure();
            }
            else
            {
                this.StopProcedure();
            }
        }
    }
}
