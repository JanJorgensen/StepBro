using System;
using System.Reflection;
using System.Linq;

namespace StepBro.Core.Data
{
   /// <summary>
   /// Used to mark a member or class as taking the place of an old member/class.
   /// </summary>
   [AttributeUsage( AttributeTargets.All, AllowMultiple = true, Inherited = true )]
   public class ReplacesAttribute : Attribute
   {
      private string m_oldName;

      public string OldName
      {
         get
         {
            return m_oldName;
         }
      }

      /// <summary>
      /// Mark target as replacing an older target.
      /// </summary>
      /// <param name="_oldName">Name of older target.</param>
      public ReplacesAttribute( string _oldName )
      {
         m_oldName = _oldName;
      }

      public static string[] List( ReplacesAttribute[] attributes )
      {
         string[] ret = new string[attributes.Length];
         for ( int i = 0; i < attributes.Length; i++ )
         {
            ret[i] = attributes[i].OldName;
         }
         return ret;
      }

      public static string[] List( MemberInfo _memberInfo )
      {
         ReplacesAttribute[] attributes = (ReplacesAttribute[]) Attribute.GetCustomAttributes( _memberInfo, typeof( ReplacesAttribute ), true );
         return List( attributes );
      }

      public static string[] List( Type _type )
      {
         ReplacesAttribute[] attributes = (ReplacesAttribute[]) _type.GetCustomAttributes( typeof( ReplacesAttribute ), true );
         return List( attributes );
      }
   }
}
