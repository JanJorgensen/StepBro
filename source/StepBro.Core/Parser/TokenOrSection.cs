using Antlr4.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.Core.Parser
{
    internal class TokenOrSection : IToken
    {
        public TokenOrSection(IToken start, IToken end, string text)
        {
            this.Line = start.Line;
            this.Column = start.Column;
            this.StartIndex = start.StartIndex;
            this.StopIndex = end.StopIndex;
            this.TokenSource = start.TokenSource;
        }

        public string Text { get; set; }

        public int Type { get; set; }

        public int Line { get; set; }

        public int Column { get; set; }

        public int Channel { get; set; }

        public int TokenIndex { get; set; }

        public int StartIndex { get; set; }

        public int StopIndex { get; set; }

        public ITokenSource TokenSource { get; set; }

        public ICharStream InputStream { get; set; }
    }
}
