using StepBro.Core.Data;
using StepBro.Core.Logging;
using static StepBro.Core.Data.PropertyBlockDecoder;

namespace StepBro.UI.WinForms
{
    internal static class WinFormsPropertyBlockDecoder
    {
        public class ValueColor<TParent> : Value<TParent> where TParent : class
        {
            private Func<TParent, Color, string> m_setter;

            public ValueColor(string typeOrName, Func<TParent, Color, string> setter = null) : base(typeOrName)
            {
                m_setter = setter;
            }

            protected override void TryCreateOrSet(PropertyBlockEntry entry, TParent parent, List<Tuple<int, string>> errors)
            {
                if (m_setter == null) return;
                var valueEntry = entry as PropertyBlockValue;
                if (valueEntry.IsStringOrIdentifier)
                {
                    var colorName = valueEntry.ValueAsString();
                    Color color = Color.Red;
                    try
                    {
                        color = (Color)(typeof(Color).GetProperty(colorName).GetValue(null));
                    }
                    catch
                    {
                        errors.Add(new Tuple<int, string>(entry.Line, ": No color named '" + colorName + "'."));
                    }

                    try
                    {
                        var error = (m_setter(parent, color));
                        if (error != null)
                        {
                            errors.Add(new Tuple<int, string>(entry.Line, "Value could not be set for \"" + entry.TypeOrName + "\"; " + error));
                        }
                    }
                    catch (Exception ex)
                    {
                        errors.Add(new Tuple<int, string>(entry.Line, "Error setting value for \"" + entry.TypeOrName + "\"; " + ex.Message));
                    }

                }
                else
                {
                    errors.Add(new Tuple<int, string>(entry.Line, "Value for \"" + entry.TypeOrName + "\"should be a string or an ID."));
                }
            }
        }
    }
}
