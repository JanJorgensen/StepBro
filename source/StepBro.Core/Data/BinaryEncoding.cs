using StepBro.Core.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace StepBro.Core.Data
{
    [Public]
    public class BinaryEncoding
    {
        public enum Endianness
        {
            LittleEndian = 0,
            BigEndian = 1
        }

        private Endianness m_endianness;
        private System.Text.Encoding m_textEncoding;

        public BinaryEncoding(Endianness endianness, System.Text.Encoding textEncoding = null)
        {
            m_endianness = endianness;
            if (textEncoding == null)
            {
                textEncoding = System.Text.Encoding.Latin1;
            }
        }

        public BinaryEncoding.Endianness DataEndianess { get { return m_endianness; } }
        public System.Text.Encoding TextEncoding { get { return m_textEncoding; } }

        #region Read 

        public UInt16 ReadUInt16(byte[] data, int index)
        {
            UInt16 value = 0;
            if (m_endianness == Endianness.LittleEndian)
            {
                value = (UInt16)data[index];
                value += (UInt16)(data[index + 1] << 8);
            }
            else
            {
                value = (UInt16)(data[index] << 8);
                value += (UInt16)(data[index + 1]);
            }
            return value;
        }
        public Int16 ReadInt16(byte[] data, int index)
        {
            return (Int16)ReadUInt16(data, index);
        }
        public UInt32 ReadUInt32(byte[] data, int index)
        {
            UInt32 value = 0;
            if (m_endianness == Endianness.LittleEndian)
            {
                value = (UInt32)data[index];
                value += (UInt32)(data[index + 1] << 8);
                value += (UInt32)(data[index + 2] << 16);
                value += (UInt32)(data[index + 3] << 24);
            }
            else
            {
                value = (UInt32)(data[index] << 24);
                value += (UInt32)(data[index + 1] << 16);
                value += (UInt32)(data[index + 2] << 8);
                value += (UInt32)data[index + 3];
            }
            return value;
        }
        public Int32 ReadInt32(byte[] data, int index)
        {
            return (Int32)ReadUInt32(data, index);
        }

        #endregion


        #region Write

        public void WriteUInt16(byte[] data, int index, UInt16 value)
        {
            if (m_endianness == Endianness.LittleEndian)
            {
                data[index] = (byte)(value & 0xFF);
                data[index + 1] = (byte)((value >> 8) & 0xFF);
            }
            else
            {
                data[index] = (byte)((value >> 8) & 0xFF);
                data[index + 1] = (byte)(value & 0xFF);
            }
        }

        public void WriteInt16(byte[] data, int index, Int16 value)
        {
            WriteUInt16(data, index, (UInt16)value);
        }

        public void WriteUInt32(byte[] data, int index, UInt32 value)
        {
            if (m_endianness == Endianness.LittleEndian)
            {
                data[index] = (byte)(value & 0xFF);
                data[index + 1] = (byte)((value >> 8) & 0xFF);
                data[index + 2] = (byte)((value >> 16) & 0xFF);
                data[index + 3] = (byte)((value >> 24) & 0xFF);
            }
            else
            {
                data[index] = (byte)((value >> 24) & 0xFF);
                data[index + 1] = (byte)((value >> 16) & 0xFF);
                data[index + 2] = (byte)((value >> 8) & 0xFF);
                data[index + 3] = (byte)(value & 0xFF);
            }
        }

        public void WriteInt32(byte[] data, int index, Int32 value)
        {
            WriteUInt32(data, index, (UInt32)value);
        }

        #endregion
    }
}
