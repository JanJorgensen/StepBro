using StepBro.Core.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.Embedded
{
    public static class Utils
    {
        public static ByteArray GetDataFromHexDumpLine(this string line, bool skipAddressHeader = true)
        {
            var lineData = line;
            if (skipAddressHeader && line.Contains(':'))    // Address header ends with ':'.
            {
                lineData = line.Substring(line.IndexOf(":") + 1);
            }
            return lineData.Trim().FromHexStringToByteArray();
        }

        public static ByteArray GetDataFromHexDumpLines(this List<string> lines, bool skipAddressHeader = true)
        {
            ByteArray data = null;
            var linesData = new List<string>();
            foreach (var line in lines)
            {
                if (!String.IsNullOrEmpty(line))
                {
                    var lineData = line.GetDataFromHexDumpLine(skipAddressHeader);
                    if (data != null)
                    {
                        data.Append(lineData);
                    }
                    else
                    {
                        data = lineData;
                    }
                }
            }
            return data;
        }
    }
}
