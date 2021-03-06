﻿using System.Text;

namespace StepBro.Core.Data
{
    public class PropertyBlockFlag : PropertyBlockEntry
    {
        public PropertyBlockFlag(int line, string name) : base(line, PropertyBlockEntryType.Flag, name)
        {
        }

        public override string ToString()
        {
            return "Flag: " + this.Name;
        }
        public override void GetTestString(StringBuilder text)
        {
            text.Append("Flag: " + this.Name);
        }
    }
}
