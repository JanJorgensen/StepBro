using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.Core.Parser
{
    internal enum TypeMatch : int
    {
        NoTypeMatch = 0,    // Types are not compatible.
        UnsupportedSource,  // Maybe the input is not a local variable reference when required.

        MatchLowest,        // Lowest possible matching.
        MatchLevel3,        // Matching at three inheritance levels below.
        MatchLevel2,        // Matching at two inheritance levels below.
        MatchLevel1,        // Matching at one inheritance level below.
        ExactMatch          // Best possible match
    }
}
