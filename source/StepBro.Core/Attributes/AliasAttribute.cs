using System;

namespace StepBro.Core.Data
{
   public class AliasAttribute : Attribute
   {
      private string m_name;
      public AliasAttribute( string name )
      {
         m_name = name;
      }
      public string Name { get { return m_name; } }
   }
}
