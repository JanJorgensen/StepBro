using System;
using System.Collections.Generic;
using System.Text;

namespace StepBro.Core.Data
{
   public class PersistedAsAttribute : Attribute
   {
      private string m_name;
      public PersistedAsAttribute( string name )
      {
         m_name = name;
      }
      public string NameOrValue { get { return m_name; } }
   }
}
