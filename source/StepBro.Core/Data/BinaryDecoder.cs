using StepBro.Core.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.Core.Data
{
    [Public]
    public class BinaryDecoder
    {
        BinaryEncoding m_encoding;
        ByteArray m_data;
        int m_readIndex = 0;

        public BinaryDecoder(BinaryEncoding encoding, ByteArray source)
        {
            m_encoding = encoding;
            m_data = source;
        }

        public int SizeLeft { get { return m_data.Length - m_readIndex; } }

        public int ReadIndex { get { return m_readIndex; } }

        #region Basic Functionality

        public bool Skip(int size)
        {
            if (this.SizeLeft < size || size < 0)
            {
                return false;
            }
            else
            {
                m_readIndex += size;
                return true;
            }
        }

        public byte ReadByteDirectly()
        {
            return m_data.Data[m_data.Start + m_readIndex++];
        }

        #endregion

        #region Integer Functions

        public UInt32 ReadUInt32()
        {
            if (this.SizeLeft < 4)
            {
                throw new InsufficientMemoryException();
            }
            UInt32 value = m_encoding.ReadUInt32(m_data.Data, m_data.Start + m_readIndex);
            m_readIndex += 4;
            return value;
        }
        public Int32 ReadInt32()
        {
            return (Int32)this.ReadUInt32();
        }
        public UInt16 ReadUInt16()
        {
            if (this.SizeLeft < 2)
            {
                throw new InsufficientMemoryException();
            }
            UInt16 value = m_encoding.ReadUInt16(m_data.Data, m_data.Start + m_readIndex);
            m_readIndex += 2;
            return value;
        }
        public Int16 ReadInt16()
        {
            return (Int16)this.ReadUInt16();
        }
        public byte ReadByte()
        {
            if (this.SizeLeft < 1)
            {
                throw new InsufficientMemoryException();
            }
            return this.ReadByteDirectly();
        }
        public SByte ReadInt8()
        {
            return (SByte)this.ReadByte();
        }

        #endregion

        public ByteArray GetBlock(int size)
        {
            if (size > this.SizeLeft)
            {
                throw new InsufficientMemoryException();
            }
            var start = m_data.Start + m_readIndex;
            m_readIndex += size;
            return new ByteArray(m_data.Data, start, size);
        }
    }
}
