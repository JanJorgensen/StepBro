using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StepBroCoreTest.Data;

namespace StepBroCoreTest.Parser
{
    public static class ExtenderClass
    {
        public static TSource MySecondOrDefault<TSource>(this IEnumerable<TSource> source)
        {
            int i = 0;
            foreach (var v in source)
            {
                if (i++ == 1) return v;
            }
            return default(TSource);
        }

        public static long MyExtMethodListLong(this List<long> source)
        {
            if (source != null && source.Count >= 1)
                return source[0] + 27;
            return -1L;
        }

        public static long MyExtMethodIListLong(this IList<long> source)
        {
            if (source != null && source.Count >= 1)
                return source[0] + 29;
            return -1L;
        }

        public static long MyExtMethodIEnumLong(this IEnumerable<long> source)
        {
            foreach (var v in source)
            {
                return v + 14;
            }
            return -1L;
        }

        public static long MyExtMethodArrayLong(this long[] source)
        {
            if (source != null && source.Length >= 1)
                return source[0] + 32;
            else return -1L;
        }

        public static string GetMyTypeName<T>(this T obj)
        {
            if (obj != null) return obj.GetType().Name;
            else return typeof(T).Name;
        }

        public static long Plus10(this DummyClass obj)
        {
            return obj.PropInt + 10L;
        }
        public static long Plus10Plus(this DummyClass obj, long a)
        {
            return obj.PropInt + 10L + a;
        }
    }
}
