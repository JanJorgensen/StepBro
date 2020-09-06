using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.Core.Data
{
    public static class TreeDataElementExtensions
    {
        public static bool HasElement(this ITreeDataElement element, string subElement, StringComparison comparisonType = StringComparison.InvariantCulture)
        {
            if (element.ElementCount > 0)
            {
                return element.SubElements.FirstOrDefault(e => String.Equals(e.Name, subElement, comparisonType)) != null;
            }
            return false;
        }

        public static ITreeDataElement GetElement(this ITreeDataElement element, string subElement, StringComparison comparisonType = StringComparison.InvariantCulture)
        {
            if (element.ElementCount > 0)
            {
                return element.SubElements.FirstOrDefault(e => String.Equals(e.Name, subElement, comparisonType));
            }
            throw new Exception("Element not found.");
        }

        public static IEnumerable<ITreeDataElement> LisSBPecificElements(this ITreeDataElement element, params string[] names)
        {
            foreach (var sub in element.SubElements)
            {
                if (names.Contains(sub.Name))
                {
                    yield return sub;
                }
            }
        }

        public static bool TryGetValue(this ITreeDataElement element, string subElement, ref string value)
        {
            if (element.ElementCount > 0)
            {
                var sub = element.SubElements.FirstOrDefault(e => String.Equals(e.Name, subElement, StringComparison.InvariantCulture));
                if (sub != null && sub.HasValue)
                {
                    value = sub.Value;
                    return true;
                }
            }
            return false;
        }

        public static string GetValue(this ITreeDataElement element, string subElement)
        {
            if (element.ElementCount > 0)
            {
                var sub = element.SubElements.FirstOrDefault(e => String.Equals(e.Name, subElement, StringComparison.InvariantCulture));
                if (sub != null)
                {
                    if (sub.HasValue)
                    {
                        return sub.Value;
                    }
                    else
                    {
                        throw new Exception("No sub-elements named '" + element + "' found.");
                    }
                }
                else
                {
                    throw new Exception("No sub-elements found.");
                }
            }
            throw new Exception("No sub-elements found.");
        }

        public static string GetValue(this ITreeDataElement element, string subElement, int index)
        {
            if (element.ElementCount > 0)
            {
                var sub = element.SubElements.Where(e => String.Equals(e.Name, subElement, StringComparison.InvariantCulture)).ElementAtOrDefault(index);
                if (sub != null)
                {
                    if (sub.HasValue)
                    {
                        return sub.Value;
                    }
                    else
                    {
                        throw new Exception("No sub-elements named '" + element + "' found.");
                    }
                }
                else
                {
                    throw new Exception("No sub-elements found.");
                }
            }
            throw new Exception("No sub-elements found.");
        }

        public static bool TryGetValue(this ITreeDataElement element, string subElement, ref bool value)
        {
            if (element.ElementCount > 0)
            {
                var sub = element.SubElements.FirstOrDefault(e => String.Equals(e.Name, subElement, StringComparison.InvariantCulture));
                if (sub != null && sub.HasValue)
                {
                    string s = sub.Value;
                    if (!TypeUtils.TryParse(s, out value))
                    {
                        throw new Exception("Error parsing element \"" + subElement + "\" at " + sub.DataOrigin + " as a boolean value. Value: " + s);
                    }
                    return true;
                }
            }
            return false;
        }

        public static bool TryGetValue(this ITreeDataElement element, string subElement, ref int value)
        {
            if (element.ElementCount > 0)
            {
                var sub = element.SubElements.FirstOrDefault(e => String.Equals(e.Name, subElement, StringComparison.InvariantCulture));
                if (sub != null && sub.HasValue)
                {
                    string s = sub.Value;
                    if (!Int32.TryParse(s, out value))
                    {
                        throw new Exception("Error parsing element \"" + subElement + "\" at " + sub.DataOrigin + " as an integer value. Value: " + s);
                    }
                    return true;
                }
            }
            return false;
        }

        public static bool TryGetValue<TEnum>(this ITreeDataElement element, string subElement, ref TEnum value) where TEnum : struct
        {
            if (element.ElementCount > 0)
            {
                var sub = element.SubElements.FirstOrDefault(e => String.Equals(e.Name, subElement, StringComparison.InvariantCulture));
                if (sub != null && sub.HasValue)
                {
                    return GetEnumValue<TEnum>(sub, ref value);
                }
            }
            return false;
        }

        public static bool TryGetValue<TEnum>(this ITreeDataElement element, string subElement, int index, ref TEnum value) where TEnum : struct
        {
            if (element.ElementCount > 0)
            {
                var sub = element.SubElements.Where(e => String.Equals(e.Name, subElement, StringComparison.InvariantCulture)).ElementAtOrDefault(1);
                if (sub != null && sub.HasValue)
                {
                    return GetEnumValue<TEnum>(sub, ref value);
                }
            }
            return false;
        }

        public static bool TryGetLastValue<TEnum>(this ITreeDataElement element, string subElement, ref TEnum value) where TEnum : struct
        {
            if (element.ElementCount > 0)
            {
                var sub = element.SubElements.LastOrDefault(e => String.Equals(e.Name, subElement, StringComparison.InvariantCulture));
                if (sub != null && sub.HasValue)
                {
                    return GetEnumValue<TEnum>(sub, ref value);
                }
            }
            return false;
        }

        private static bool GetEnumValue<TEnum>(ITreeDataElement element, ref TEnum value) where TEnum : struct
        {
            string name = element.Value;
            try
            {
                value = (TEnum)Enum.Parse(typeof(TEnum), element.Value);
                return true;
            }
            catch (ArgumentNullException)
            {
                if (!typeof(TEnum).IsEnum)
                {
                    throw new Exception("Error parsing element \"" + element + "\" at " + element.DataOrigin + ", The " + typeof(TEnum).Name + " type is not an enum type.");
                }
                else if (name.Trim() == "")
                {
                    throw new Exception("Error parsing element \"" + element.Name + "\" at " + element.DataOrigin + ". Element has no value.");
                }
                else
                {
                    throw new Exception("Error parsing element \"" + element.Name + "\" at " + element.DataOrigin + ". Unexpected enum value: " + name);
                }
            }
            catch (ArgumentException)
            {
                throw new Exception("Error parsing element \"" + element.Name + "\" at " + element.DataOrigin + ". Unexpected enum value: " + name);
            }
        }


        public static int Level(this ITreeDataElement element)
        {
            if (element.Parent != null) return element.Parent.Level() + 1;
            else return 0;
        }

        public static void Dump(this ITreeDataElement element, System.IO.TextWriter writer)
        {
            string indent = String.Concat(Enumerable.Repeat("|   ", element.Level()));
            if (element.ElementCount == 0)
            {
                if (element.HasValue)
                {
                    writer.WriteLine(indent + element.Name + " = " + element.Value);
                }
                else
                {
                    writer.WriteLine(indent + element.Name + " (empty)");
                }
            }
            else
            {
                if (element.HasValue)
                {
                    writer.WriteLine(indent + element.Name + " = " + element.Value);
                }
                else
                {
                    writer.WriteLine(indent + element.Name + "  (" + element.ElementCount.ToString() + " sub entries)");
                }
                foreach (var sub in element.SubElements)
                {
                    sub.Dump(writer);
                }
            }
        }


        public static IEnumerable<ITreeDataElement> GetFlatListOfSubElements(this ITreeDataElement element, Predicate<ITreeDataElement> predicate = null)
        {
            if (element.ElementCount > 0)
            {
                foreach (var sub in element.SubElements)
                {
                    if (predicate == null || predicate(sub))
                    {
                        yield return sub;
                        if (sub.ElementCount > 0)
                        {
                            foreach (var subsub in sub.GetFlatListOfSubElements(predicate)) { yield return subsub; }    // Note: Not sure how effective this method is (many levels of foreach).
                        }
                    }
                }
            }
        }

        //public static IEnumerable<ITreeDataElement> GetFlatListOfSubElements(this ITreeDataElement element, Predicate<ITreeDataElement> predicate = null)
        //{
        //    ITreeDataElement current = element.SubElements.FirstOrDefault();
        //    while (current != null)
        //    {
        //        bool passThroughFilter = predicate == null || predicate(current);
        //        if (passThroughFilter) yield return current;

        //        ITreeDataElement next = (passThroughFilter && current.ElementCount > 0) ? current.SubElements.FirstOrDefault() : null;
        //        if (next != null)
        //        {
        //            current = next;
        //        }
        //        else
        //        {
        //            while (true)
        //            {
        //                next = current.GetNextSibling();
        //                if (next != null)
        //                {
        //                    current = next;
        //                    break;
        //                }
        //                else
        //                {
        //                    current = current.Parent;
        //                    if (current == null || Object.ReferenceEquals(current, element))
        //                    {
        //                        current = null;
        //                    }
        //                }
        //            }
        //        }
        //    }
        //}

        public static ITreeDataElement GetNextSibling(this ITreeDataElement element)
        {
            if (element.Parent == null) return null;
            bool thisFound = false;
            foreach (var sibling in element.Parent.SubElements)
            {
                if (thisFound) return sibling;
                if (Object.ReferenceEquals(sibling, element)) thisFound = true;
            }
            return null;
        }
    }
}
