using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestModule
{
    public class TestClassWithOnlyStaticMethods
    {
        public static void MethodThrowingException()
        {
            throw new KeyNotFoundException();
        }
    }
}
