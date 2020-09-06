using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.Core.Data
{
    public class ByteArray
    {
        public byte[] Data { get; private set; }
        public int Start { get; private set; }
        public int Length { get; private set; }

        public ByteArray(byte[] data, int start = 0, int length = -1)
        {
            this.Data = data;
            this.Start = (start >= 0) ? start : 0;
            this.Length = (length >= 0) ? length : data.Length - this.Start;
        }

        public static implicit operator ByteArray(byte[] data)
        {
            return new ByteArray(data);
        }
    }
}
