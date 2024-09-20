using StepBro.Core.Api;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.Core.Data
{
    [Public]
    public class BinaryEncoder
    {
        BinaryEncoding m_encoding;
        byte[] m_data;
        int m_writeIndex = 0;

        public BinaryEncoder(BinaryEncoding encoding)
        {
            m_encoding = encoding;
            m_data = new byte[256];
        }

        private int SizeLeft { get { return m_data.Length - m_writeIndex; } }

        public int WriteIndex { get { return m_writeIndex; } }

        public ByteArray GetResult(int start = 0, int length = -1)
        {
            if (length < 0) length = m_writeIndex - start;
            return new ByteArray(m_data, start, length);
        }

        #region Basic Functionality

        public bool Skip(int size)
        {
            if (this.SizeLeft < size || size < 0)
            {
                return false;
            }
            else
            {
                m_writeIndex += size;
                return true;
            }
        }

        public void WriteByteDirectly(byte value)
        {
            m_data[m_writeIndex++] = value;
        }

        /// <summary>
        /// Reserves/allocates the specified space to be available from the current writing index.
        /// </summary>
        /// <param name="size">Size of data to be free for comming write operations.</param>
        public void ReserveSpace(int size)
        {
            if ((m_data.Length - m_writeIndex) < size)
            {
                int requiredSize = Math.Max(m_data.Length * 2, m_writeIndex + size);
                Array.Resize<byte>(ref m_data, requiredSize);
            }
        }

        #endregion

        #region Integer Functions

        public void WriteUInt32(UInt32 value)
        {
            this.ReserveSpace(4);
            m_encoding.WriteUInt32(m_data, m_writeIndex, value);
            m_writeIndex += 4;
        }

        public void WriteInt32(Int32 value)
        {
            this.ReserveSpace(4);
            m_encoding.WriteInt32(m_data, m_writeIndex, value);
            m_writeIndex += 4;
        }

        public void WriteUInt16(UInt16 value)
        {
            this.ReserveSpace(2);
            m_encoding.WriteUInt16(m_data, m_writeIndex, value);
            m_writeIndex += 2;
        }

        public void WriteInt16(Int16 value)
        {
            this.ReserveSpace(2);
            m_encoding.WriteInt16(m_data, m_writeIndex, value);
            m_writeIndex += 2;
        }

        public void WriteUInt8(byte value)
        {
            this.WriteByte(value);
        }

        public void WriteByte(byte value)
        {
            this.ReserveSpace(1);
            m_data[m_writeIndex++] = value;
        }

        public void WriteInt8(SByte value)
        {
            this.ReserveSpace(1);
            m_data[m_writeIndex++] = (byte)value;
        }


        #endregion

        public void WriteBlock(byte[] source, int sourceOffset, int size)
        {
            this.ReserveSpace(size);
            Array.Copy(source, sourceOffset, m_data, m_writeIndex, size);
            m_writeIndex += size;
        }
        public void WriteBlock(ByteArray data)
        {
            this.ReserveSpace(data.Length);
            Array.Copy(data.Data, data.Start, m_data, m_writeIndex, data.Length);
            m_writeIndex += data.Length;
        }
    }
}
