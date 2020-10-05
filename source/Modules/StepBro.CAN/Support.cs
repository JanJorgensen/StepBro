using System;
using System.Text;

namespace StepBro.CAN
{
    public static class CanSupport
    {
        private static bool g_little = true;
        public static bool IsLittleEndian
        {
            get { return g_little; }
            set { g_little = value; }
        }
        public static bool IsBigEndian
        {
            get { return !g_little; }
            set { g_little = !value; }
        }

        public static void SetData(this IMessage message, ulong data, int length)
        {
            if (length < 0 | length > 8) throw new ArgumentOutOfRangeException("length");
            if (g_little)
            {
                var v = data;
                byte[] bytes = new byte[length];
                for (int i = 0; i < length; i++)
                {
                    bytes[i] = (byte)(v & 0xFF);
                    v >>= 8;
                }
                message.Data = bytes;
            }
            else
            {
                var v = data;
                byte[] bytes = new byte[length];
                for (int i = 0; i < length; i++)
                {
                    bytes[i] = (byte)((v >> 56) & 0xFF);
                    v <<= 8;
                }
                message.Data = bytes;
            }
        }

        public static ulong GetDataAsInteger(this IMessage message)
        {
            if (g_little)
            {
                ulong val = 0UL;
                ulong factor = 1;
                foreach (var v in message.Data)
                {
                    val += (ulong)v * factor;
                    factor <<= 8;
                }
                return val;
            }
            else
            {
                ulong val = 0UL;
                ulong factor = 0x0100000000000000;
                foreach (var v in message.Data)
                {
                    val += (ulong)v * factor;
                    factor >>= 8;
                }
                return val;
            }
        }

        public static string GetDataAsString(this IMessage message)
        {

            if (message.Data.Length == 0)
            {
                return "";
            }
            else
            {
                var s = new StringBuilder();
                string HexAlphabet = "0123456789ABCDEF";
                bool first = true;
                foreach (byte B in message.Data)
                {
                    if (first) first = false;
                    else s.Append(" ");
                    s.Append(HexAlphabet[(int)(B >> 4)]);
                    s.Append(HexAlphabet[(int)(B & 0xF)]);
                }
                return s.ToString();
            }
        }
    }
}
