using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.CAN
{
    public class CANChannel : ICANChannel
    {
        private ICANChannel m_driverChannel = null;
        ICANChannel DriverChannel { get { return m_driverChannel; } }

        #region ICANChannel Implementation

        public ICANAdapter Adapter
        {
            get
            {
                return m_driverChannel.Adapter;
            }
        }

        public string ErrorStatus
        {
            get
            {
                return m_driverChannel.ErrorStatus;
            }
        }

        public bool IsOpen
        {
            get
            {
                return m_driverChannel.IsOpen;
            }
        }

        public string LastOperationStatus
        {
            get
            {
                return m_driverChannel.LastOperationStatus;
            }
        }

        public bool Close()
        {
            return m_driverChannel.Close();
        }

        public ICANMessage CreateMessage(uint id, byte[] data)
        {
            return m_driverChannel.CreateMessage(id, data);
        }

        public ICANMessage CreateMessage(CANMessageType type, uint id, byte[] data)
        {
            return m_driverChannel.CreateMessage(type, id, data);
        }

        public void Flush()
        {
            m_driverChannel.Flush();
        }

        public ICANMessage GetReceived()
        {
            return m_driverChannel.GetReceived();
        }

        public bool Open()
        {
            return m_driverChannel.Open();
        }

        public void ResetErrors()
        {
            m_driverChannel.ResetErrors();
        }

        public bool Send(ICANMessage message)
        {
            return m_driverChannel.Send(message);
        }

        public ICANMessage Send(uint id, byte[] data)
        {
            return m_driverChannel.Send(id, data);
        }

        public ICANMessage Send(CANMessageType type, uint id, byte[] data)
        {
            return m_driverChannel.Send(type, id, data);
        }

        public void Setup(CANBaudrate baudrate, CANChannelMode mode, TimeSpan transmitTimeout)
        {
            m_driverChannel.Setup(baudrate, mode, transmitTimeout);
        }

        #endregion
    }
}
