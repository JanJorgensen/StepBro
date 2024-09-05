using StepBro.Core.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.Core.Data
{
    [Public]
    public class ByteStuffingEncoder
    {
        private byte m_escapeStart = 0x01;
        private List<Tuple<byte, byte>> m_excapeValues = new List<Tuple<byte, byte>>();

        public ByteStuffingEncoder(byte escapeStart)
        {
            m_escapeStart = escapeStart;
        }

        public ByteStuffingEncoder(byte escapeStart, params object[] valueset)
        {
            m_escapeStart = escapeStart;

            if (valueset.Length % 2 != 0) throw new ArgumentException("The values must be is sets of two integer values.");
            foreach (object value in valueset)
            {
                if (value == null)
                {
                    throw new ArgumentException("The values must be is sets of two integer values. A 'null' value was found.");
                }
                else if (value.GetType() != typeof(byte) && value.GetType() != typeof(long))
                {
                    throw new ArgumentException($"The values must be is sets of two integer values. A {value.GetType().Name} was found.");
                }
            }

            for (int i = 0; i < valueset.Length; i += 2)
            {
                this.AddEscapeValue(System.Convert.ToByte(valueset[i]), System.Convert.ToByte(valueset[i + 1]));
            }
        }

        public byte EscapeStart
        {
            get { return m_escapeStart; }
            set { m_escapeStart = value; }
        }

        public void AddEscapeValue(byte value, byte escapeCode)
        {
            m_excapeValues.Add(new Tuple<byte, byte>(value, escapeCode));
        }

        public bool IsEscapedValue(byte value)
        {
            foreach (var escapeSet in m_excapeValues)
            {
                if (escapeSet.Item1 == value) return true;
            }
            return false;
        }

        public byte ToEscapeValue(byte value)
        {
            foreach (var escapeSet in m_excapeValues)
            {
                if (escapeSet.Item1 == value) return escapeSet.Item2;
            }
            throw new ArgumentException($"The specified value ({value.ToString("X2")}) is not set to be escaped.");
        }

        public byte FromEscapeValue(byte value)
        {
            foreach (var escapeSet in m_excapeValues)
            {
                if (escapeSet.Item2 == value) return escapeSet.Item1;
            }
            throw new ArgumentException($"The specified value ({value.ToString("X2")}) is not an escape value.");
        }


        public ByteArray DecodeEscapedData(byte[] data, int index = 0, int length = -1)
        {
            if (length < 0) length = data.Length - index;

            byte[] result = new byte[length];
            var j = 0;

            for (int i = 0; i < length; i++)
            {
                var b = data[i + index];
                if (b == m_escapeStart)
                {
                    b = data[++i + index];
                    result[j++] = this.FromEscapeValue(b);
                }
                else
                {
                    result[j++] = b;
                }
            }

            return new ByteArray(result, 0, j);
        }
        public ByteArray CreateEscapedData(byte[] data, int index = 0, int length = -1)
        {
            if (length < 0) length = data.Length - index;

            byte[] result = new byte[length * 2];   // Allocate double, to be sure to support an all-excaped values block.
            var j = 0;

            for (int i = 0; i < length; i++)
            {
                var b = data[index + i];
                if (this.IsEscapedValue(b))
                {
                    result[j++] = m_escapeStart;
                    result[j++] = this.ToEscapeValue(b);
                }
                else
                {
                    result[j++] = b;
                }
            }

            return new ByteArray(result, 0, j);
        }
    }

    [Public]
    public static class ByteStuffingHelp
    {
        public static ByteArray Encode(this ByteArray input, ByteStuffingEncoder encoder, int index = 0, int length = -1)
        {
            if (length < 0) length = input.Length - index;
            return encoder.CreateEscapedData(input.Data, index + input.Start, length);
        }
        public static ByteArray Decode(this ByteArray input, ByteStuffingEncoder encoder, int index = 0, int length = -1)
        {
            if (length < 0) length = input.Length - index;
            return encoder.DecodeEscapedData(input.Data, index + input.Start, length);
        }
    }
}
