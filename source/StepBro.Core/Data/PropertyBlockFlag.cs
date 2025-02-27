﻿using System.Text;

namespace StepBro.Core.Data
{
    public class PropertyBlockFlag : PropertyBlockEntry
    {
        public PropertyBlockFlag(int line, string name) : base(line, PropertyBlockEntryType.Flag, name)
        {
        }

        protected override uint GetValueHashCode()
        {
            return 8376307;
        }

        public override string ToString()
        {
            return "Flag: " + this.Name;
        }
        public override void GetTestString(StringBuilder text)
        {
            text.Append("Flag: " + this.Name);
        }

        public override PropertyBlockEntry Clone(bool skipUsedOrApproved = false)
        {
            return new PropertyBlockFlag(this.Line, this.Name).CloneBase(this);
        }

        public override SerializablePropertyBlockEntry CloneForSerialization()
        {
            return new SerializablePropertyBlockFlag()
            {
                Name = this.Name,
                Line = this.Line,
                SpecifiedType = this.SpecifiedTypeName
            };
        }
    }
}
