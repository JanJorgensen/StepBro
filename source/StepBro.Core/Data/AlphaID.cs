using System.Collections.Generic;
using System.Text;

namespace StepBro.Core.Data
{
    public sealed class AlphaID
    {
        private static readonly string[] IDStrings =
        {
            "BA","DA","FA","GA","HA","JA","KA","LA","MA","NA","PA","RA","SA","TA","VA",
            "BE","DE","FE","GE","HE","JE","KE","LE","ME","NE","PE","RE","SE","TE","VE",
            "BI","DI","FI","GI","HI","JI","KI","LI","MI","NI","PI","RI","SI","TI","VI",
            "BO","DO","FO","GO","HO","JO","KO","LO","MO","NO","PO","RO","SO","TO","VO",
            "BU","DU","FU","GU","HU","JU","KU","LU","MU","NU","PU","RU","SU","TU","VU"
        };

        static private readonly int[] IDWeight;

        static AlphaID()
        {
            List<int> weights = new List<int>();
            int w = IDStrings.Length;
            weights.Add(0);
            for (int i = 1; i < 10; i++)
            {
                weights.Add(w);
                w *= IDStrings.Length;
            }
            IDWeight = weights.ToArray();
        }

        public static string Create(int value, int width)
        {
            StringBuilder s = new StringBuilder(2 * width);

            int rest = value;
            for (int i = width - 1; i >= 0; i--)
            {
                int w = IDWeight[i];
                if (w == 0) s.Append(IDStrings[rest]);
                else
                {
                    int v = rest / w;
                    rest = rest - w * v;
                    s.Append(IDStrings[v]);
                }
            }
            return s.ToString();
        }
    }
}
