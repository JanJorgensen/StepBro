using StepBro.Core.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.Core.Data
{
    [Public]
    public static class CRC
    {
        public static ushort CalculateCrc16CcittFalse(ByteArray data, int length = -1, ushort initialCrc = 0xFFFF)
        {
            if (length < 0) length = data.Length;
            ushort crc = initialCrc;
            ushort polynomial = 0x1021;

            for (int i = 0; i < length; i++)
            {
                crc ^= (ushort)(data[i] << 8);

                for (int j = 0; j < 8; j++)
                {
                    if ((crc & 0x8000) != 0)
                    {
                        crc = (ushort)((crc << 1) ^ polynomial);
                    }
                    else
                    {
                        crc <<= 1;
                    }
                }
            }

            return crc;
        }

        public static ushort CalculateCrc16CcittFalse(byte[] data, int length, ushort initialCrc = 0xFFFF)
        {
            ushort crc = initialCrc;
            ushort polynomial = 0x1021;

            for (int i = 0; i < length; i++)
            {
                crc ^= (ushort)(data[i] << 8);

                for (int j = 0; j < 8; j++)
                {
                    if ((crc & 0x8000) != 0)
                    {
                        crc = (ushort)((crc << 1) ^ polynomial);
                    }
                    else
                    {
                        crc <<= 1;
                    }
                }
            }

            return crc;
        }

        public static uint CombineHashCodes(uint h1, uint h2)
        {
            return (((h1 << 5) + h1) ^ h2);
        }

        public static uint GetStringHash(string input)
        {
            uint hash = 8376231;
            foreach (char ch in input)
            {
                hash = ((((uint)ch << 5) + (uint)ch) ^ hash);
            }
            return hash;
        }


    }
}
