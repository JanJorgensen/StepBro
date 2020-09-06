using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace StepBro.Core.Data
{
   [AttributeUsage( AttributeTargets.Property | AttributeTargets.Field )]
   public class PersistedAttribute : Attribute
   {
      private string m_Name;
      private bool m_persisted;
      private string m_dynamicPersistedProperty = null;

      public PersistedAttribute( bool persisted, string name )
      {
         m_persisted = persisted;
         m_Name = name;
      }

      public PersistedAttribute()
         : this( true, null )
      {
      }

      public PersistedAttribute( string property )
         : this( true, null )
      {
         m_dynamicPersistedProperty = property;
      }

      public PersistedAttribute( bool persisted )
         : this( persisted, null )
      {
      }

      public bool Persisted { get { return m_persisted; } }
      public string Name { get { return m_Name; } }

      public string GetPersistedName( PropertyInfo p )
      {
         if ( String.IsNullOrEmpty( m_Name ) )
         {
            return p.Name;
         }
         else
         {
            return m_Name;
         }
      }

      public bool IsPropertyPersisted( object target, PropertyInfo property, bool inherit )
      {
         if ( String.IsNullOrEmpty( m_dynamicPersistedProperty ) )
         {
            return m_persisted;
         }
         else
         {
            object answer = target.GetType().InvokeMember(
               m_dynamicPersistedProperty,
               BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public,
               null,
               target,
               new object[ 0 ] );
            if ( answer is bool )
            {
               return (bool)answer;
            }
            else
            {
               throw new ExceptionPersistedAttributePropertyError(
                  string.Format( "The property \"{0}\" in the class \"{1}\" refers to a \"DynamicPersisted-property\" by the name \"{2}\" which is NOT a boolean property as it must be.",
                  property.Name, target.GetType().FullName, m_dynamicPersistedProperty ) );
            }
         }
      }
   }

   public class ExceptionPersistedAttributePropertyError : System.Exception
   {
      public ExceptionPersistedAttributePropertyError( string message ) : base( message ) { }
   }
}
