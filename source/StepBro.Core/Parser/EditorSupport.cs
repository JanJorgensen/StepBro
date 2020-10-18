using System;

namespace StepBro.Core.Parser
{
    public static class EditorSupport
    {
        public static int CalculateMatchingWeight(string typed, string checkedName, int additionalWeight = 0)
        {
            int weight = 0;
            int matchIndex = checkedName.IndexOf(typed, StringComparison.InvariantCultureIgnoreCase);

            if (matchIndex >= 0)
            {
                weight += 10000 * typed.Length;
                weight -= (matchIndex * 100);       // Not as big a match if the match comes later in the name.

                for (int i = 0; i < typed.Length; i++)
                {
                    if (typed[i] != checkedName[i + matchIndex]) weight -= 5;
                }
            }
            return weight;
        }
    }
}
