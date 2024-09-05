using StepBro.Core.Api;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.Core.Data
{
    [Public]
    public class ByteArray : IEnumerable
    {
        private byte[] m_data;

        public byte[] Data { get { return m_data; } }
        public int Start { get; private set; }
        public int Length { get; private set; }

        public ByteArray(byte[] data, int start = -1, int length = -1)
        {
            m_data = data;
            this.Start = (start >= 0) ? start : 0;
            this.Length = (length >= 0) ? length : data.Length - this.Start;
        }

        public void ResizeAllocation(int size)
        {
            if (size > this.Data.Length)
            {
                Array.Resize<byte>(ref m_data, size);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            int end = this.Length - this.Start;
            for (int i = this.Start; i < end; i++)
            {
                yield return m_data[i];
            }
        }

        public byte this[int index] { get { return m_data[index + this.Start]; } }

        public override bool Equals(object obj)
        {
            if (obj is ByteArray byteArray)
            {
                if (byteArray.Length != this.Length) return false;
                for (int i = 0; i < this.Length; i++)
                {
                    if (this[i] != byteArray[i]) return false;
                }
                return true;
            }
            return base.Equals(obj);
        }

        public static implicit operator ByteArray(byte[] data)
        {
            return new ByteArray(data);
        }
    }

    [Public]
    public static class ByteArrayUtils
    {
        public static string ToHexString(this ByteArray data)
        {
            var sb = new StringBuilder();
            foreach (byte b in data)
            {
                sb.Append(b.ToString("X2"));
            }
            return sb.ToString();
        }

        public static string ToHexString(this ByteArray data, int index = 0, int length = -1)
        {
            if (length < 0) length = data.Length - index;
            return data.Data.ToHexString(data.Start + index, length);
        }

        public static string ToHexString(this byte[] data, int index = 0, int length = -1)
        {
            if (data == null) throw new ArgumentNullException("data");

            if (length < 0) length = data.Length - index;
            var sb = new StringBuilder();
            for (int i = index; i < (index + length); i++)
            {
                sb.Append(data[i].ToString("X2"));
            }
            return sb.ToString();
        }

        public static string ToHexString(this ByteArray data, string separator, int index = 0, int length = -1)
        {
            if (length < 0) length = data.Length - index;
            return data.Data.ToHexString(separator, data.Start + index, length);
        }

        public static string ToHexString(this byte[] data, string separator = " ", int index = 0, int length = -1)
        {
            if (data == null) throw new ArgumentNullException("data");

            if (length < 0) length = data.Length - index;
            var sb = new StringBuilder();
            bool isFirst = true;
            for (int i = index; i < (index + length); i++)
            {
                if (isFirst)
                {
                    isFirst = false;
                }
                else
                {
                    sb.Append(separator);
                }
                sb.Append(data[i].ToString("X2"));
            }
            return sb.ToString();
        }

        public static ByteArray FromHexStringToByteArray(this string text)
        {
            var data = new List<byte>();
            if (!String.IsNullOrEmpty(text))
            {
                if (text.Contains(' '))
                {
                    var parts = text.Split(' ');
                    foreach (var part in parts)
                    {
                        if (Byte.TryParse(part, System.Globalization.NumberStyles.HexNumber, null, out var value))
                        {
                            data.Add(value);
                        }
                        else
                        {
                            throw new ArgumentException("Failure in hex data string.");
                        }
                    }
                }
                else if (text.Length % 2 == 0)
                {
                    for (int i = 0; i < text.Length / 2; i += 2)
                    {
                        if (Byte.TryParse(text.Substring(i, 2), System.Globalization.NumberStyles.HexNumber, null, out var value))
                        {
                            data.Add(value);
                        }
                        else
                        {
                            throw new ArgumentException("Failure in hex data string.");
                        }
                    }
                }
                else
                {
                    throw new ArgumentException("Failure in hex data string.");
                }
            }
            return new ByteArray(data.ToArray());
        }

        public static string ToLatin1(this byte[] bytes)
        {
            return System.Text.Encoding.Latin1.GetString(bytes);
        }

        public static string ToLatin1(this byte[] bytes, int index, int count)
        {
            return System.Text.Encoding.Latin1.GetString(bytes, index, count);
        }

        public static string ToLatin1(this ByteArray bytes)
        {
            return System.Text.Encoding.Latin1.GetString(bytes.Data, bytes.Start, bytes.Length);
        }

        public static string ToLatin1(this ByteArray bytes, int index, int count)
        {
            return System.Text.Encoding.Latin1.GetString(bytes.Data, bytes.Start + index, count);
        }

        public static ByteArray FromLatin1(this string text)
        {
            return new ByteArray(System.Text.Encoding.Latin1.GetBytes(text));
        }

        public static ByteArray FromLatin1(this string text, int index, int count)
        {
            return new ByteArray(System.Text.Encoding.Latin1.GetBytes(text, index, count));
        }
    }
}
