using System;

namespace StepBro.Core.Data
{
   /// <summary>
   /// Mark a previously existing member as obsolete, this can be
   /// used, for example, to mark a removed enum field as obsolete but
   /// when a string value with enum values in it is parsed - any
   /// instances of the obsolete field should just be ignored.
   /// 
   /// This is obviously only usable for [Flags] enums, since missing
   /// flags in can be ignored, but if the enum value is only one value
   /// it can not be ignored.
   /// 
   /// <see cref="EnumUtils<T>.Parse"/>
   /// </summary>
   [AttributeUsage( AttributeTargets.All, AllowMultiple = true, Inherited = true )]
   public class ObsoleteIgnoredAttribute : Attribute
   {
      protected string m_obsoleteName;

      public string ObsoleteName
      {
         get
         {
            return m_obsoleteName;
         }
      }

      public ObsoleteIgnoredAttribute( string _obsoleteName )
      {
         m_obsoleteName = _obsoleteName;
      }
   }
}
