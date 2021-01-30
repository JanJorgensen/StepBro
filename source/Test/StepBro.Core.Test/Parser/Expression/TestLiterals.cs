using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Text;
using StepBro.Core.Data;
using StepBro.Core.Parser;
using Range = StepBro.Core.Data.Range;

namespace StepBroCoreTest.Parser
{
    [TestClass]
    public class TestLiterals
    {
        [TestMethod]
        public void TestLiteralInteger()
        {
            var result = FileBuilder.ParseLiteral("16");
            Assert.IsTrue(result.IsConstant);
            Assert.IsTrue(result.Value is long);
            Assert.AreEqual(16L, (long)result.Value);

            result = FileBuilder.ParseLiteral("0x44");
            Assert.IsTrue(result.IsConstant);
            Assert.IsTrue(result.Value is long);
            Assert.AreEqual(0x44L, (long)result.Value);

            result = FileBuilder.ParseLiteral("7Bh");
            Assert.IsTrue(result.IsConstant);
            Assert.IsTrue(result.Value is long);
            Assert.AreEqual(0x7BL, (long)result.Value);
        }

        [TestMethod]
        public void TestLiteralIntegerWithPostfix()
        {
            var result = FileBuilder.ParseLiteral("4k");
            Assert.IsTrue(result.IsConstant);
            Assert.IsTrue(result.Value is long);
            Assert.AreEqual(4000L, (long)result.Value);

            result = FileBuilder.ParseLiteral("73M");
            Assert.IsTrue(result.IsConstant);
            Assert.IsTrue(result.Value is long);
            Assert.AreEqual(73000000L, (long)result.Value);

            result = FileBuilder.ParseLiteral("26G");
            Assert.IsTrue(result.IsConstant);
            Assert.IsTrue(result.Value is long);
            Assert.AreEqual(26000000000L, (long)result.Value);

            result = FileBuilder.ParseLiteral("52T");
            Assert.IsTrue(result.IsConstant);
            Assert.IsTrue(result.Value is long);
            Assert.AreEqual(52000000000000L, (long)result.Value);

            result = FileBuilder.ParseLiteral("8P");
            Assert.IsTrue(result.IsConstant);
            Assert.IsTrue(result.Value is long);
            Assert.AreEqual(8000000000000000L, (long)result.Value);

            result = FileBuilder.ParseLiteral("2.6k");
            Assert.IsTrue(result.IsConstant);
            Assert.IsTrue(result.Value is long);
            Assert.AreEqual(2600L, (long)result.Value);

            result = FileBuilder.ParseLiteral("92982.22M");
            Assert.IsTrue(result.IsConstant);
            Assert.IsTrue(result.Value is long);
            Assert.AreEqual(92982220000L, (long)result.Value);

            result = FileBuilder.ParseLiteral("4.321G");
            Assert.IsTrue(result.IsConstant);
            Assert.IsTrue(result.Value is long);
            Assert.AreEqual(4321000000L, (long)result.Value);
        }

        [TestMethod]
        public void TestLiteralBoolean()
        {
            var result = FileBuilder.ParseLiteral("true");
            Assert.IsTrue(result.IsConstant);
            Assert.IsTrue(result.Value is Boolean);
            Assert.AreEqual(true, (bool)result.Value);

            result = FileBuilder.ParseLiteral("false");
            Assert.IsTrue(result.IsConstant);
            Assert.IsTrue(result.Value is Boolean);
            Assert.AreEqual(false, (bool)result.Value);
        }

        [TestMethod]
        public void TestLiteralVerdict()
        {
            var result = FileBuilder.ParseLiteral("pass");
            Assert.IsTrue(result.IsConstant);
            Assert.IsTrue(result.Value is Verdict);
            Assert.AreEqual(Verdict.Pass, (Verdict)result.Value);

            result = FileBuilder.ParseLiteral("inconclusive");
            Assert.IsTrue(result.IsConstant);
            Assert.IsTrue(result.Value is Verdict);
            Assert.AreEqual(Verdict.Inconclusive, (Verdict)result.Value);

            result = FileBuilder.ParseLiteral("fail");
            Assert.IsTrue(result.IsConstant);
            Assert.IsTrue(result.Value is Verdict);
            Assert.AreEqual(Verdict.Fail, (Verdict)result.Value);

            result = FileBuilder.ParseLiteral("error");
            Assert.IsTrue(result.IsConstant);
            Assert.IsTrue(result.Value is Verdict);
            Assert.AreEqual(Verdict.Error, (Verdict)result.Value);

            result = FileBuilder.ParseLiteral("unset");
            Assert.IsTrue(result.IsConstant);
            Assert.IsTrue(result.Value is Verdict);
            Assert.AreEqual(Verdict.Unset, (Verdict)result.Value);
        }

        [TestMethod]
        public void TestLiteralString()
        {
            var result = FileBuilder.ParseLiteral("\"\"");
            Assert.IsTrue(result.IsConstant);
            Assert.IsTrue(result.Value is String);
            Assert.AreEqual("", (String)result.Value);

            result = FileBuilder.ParseLiteral("\"J\"");
            Assert.IsTrue(result.IsConstant);
            Assert.IsTrue(result.Value is String);
            Assert.AreEqual("J", (String)result.Value);

            result = FileBuilder.ParseLiteral("\"Madsen\"");
            Assert.IsTrue(result.IsConstant);
            Assert.IsTrue(result.Value is String);
            Assert.AreEqual("Madsen", (String)result.Value);

            result = FileBuilder.ParseLiteral("\"   \"");
            Assert.IsTrue(result.IsConstant);
            Assert.IsTrue(result.Value is String);
            Assert.AreEqual("   ", (String)result.Value);

            result = FileBuilder.ParseLiteral("\"\\\'\"");
            Assert.IsTrue(result.IsConstant);
            Assert.IsTrue(result.Value is String);
            Assert.AreEqual("\'", (String)result.Value);

            result = FileBuilder.ParseLiteral("\"\\\"\"");
            Assert.IsTrue(result.IsConstant);
            Assert.IsTrue(result.Value is String);
            Assert.AreEqual("\"", (String)result.Value);

            result = FileBuilder.ParseLiteral("\"\\\\\"");
            Assert.IsTrue(result.IsConstant);
            Assert.IsTrue(result.Value is String);
            Assert.AreEqual("\\", (String)result.Value);

            result = FileBuilder.ParseLiteral("\"\\a\"");
            Assert.IsTrue(result.IsConstant);
            Assert.IsTrue(result.Value is String);
            Assert.AreEqual("\a", (String)result.Value);

            result = FileBuilder.ParseLiteral("\"\\b\"");
            Assert.IsTrue(result.IsConstant);
            Assert.IsTrue(result.Value is String);
            Assert.AreEqual("\b", (String)result.Value);

            result = FileBuilder.ParseLiteral("\"\\f\"");
            Assert.IsTrue(result.IsConstant);
            Assert.IsTrue(result.Value is String);
            Assert.AreEqual("\f", (String)result.Value);

            result = FileBuilder.ParseLiteral("\"\\n\"");
            Assert.IsTrue(result.IsConstant);
            Assert.IsTrue(result.Value is String);
            Assert.AreEqual("\n", (String)result.Value);

            result = FileBuilder.ParseLiteral("\"\\r\"");
            Assert.IsTrue(result.IsConstant);
            Assert.IsTrue(result.Value is String);
            Assert.AreEqual("\r", (String)result.Value);

            result = FileBuilder.ParseLiteral("\"\\t\"");
            Assert.IsTrue(result.IsConstant);
            Assert.IsTrue(result.Value is String);
            Assert.AreEqual("\t", (String)result.Value);

            result = FileBuilder.ParseLiteral("\"\\v\"");
            Assert.IsTrue(result.IsConstant);
            Assert.IsTrue(result.Value is String);
            Assert.AreEqual("\v", (String)result.Value);

            result = FileBuilder.ParseLiteral("\"\\u67BE\"");
            Assert.IsTrue(result.IsConstant);
            Assert.IsTrue(result.Value is String);
            Assert.AreEqual("\u67BE", (String)result.Value);

            result = FileBuilder.ParseLiteral("\"\\x5A21\"");
            Assert.IsTrue(result.IsConstant);
            Assert.IsTrue(result.Value is String);
            Assert.AreEqual("\u5A21", (String)result.Value);

            result = FileBuilder.ParseLiteral("\"A23/*x%\\tdk\\r\\ndk\"");
            Assert.IsTrue(result.IsConstant);
            Assert.IsTrue(result.Value is String);
            Assert.AreEqual("A23/*x%\tdk\r\ndk", (String)result.Value);
        }

        [TestMethod]
        public void TestLiteralIdentifier()
        {
            var result = FileBuilder.ParseLiteral("'Hey'");
            Assert.IsTrue(result.IsConstant);
            Assert.IsTrue(result.Value is Identifier);
            Assert.AreEqual(new Identifier("Hey"), (Identifier)result.Value);

            result = FileBuilder.ParseLiteral("'P55'");
            Assert.IsTrue(result.IsConstant);
            Assert.IsTrue(result.Value is Identifier);
            Assert.AreEqual(new Identifier("P55"), (Identifier)result.Value);
        }

        [TestMethod]
        public void TestLiteralVerbatimString()
        {
            var result = FileBuilder.ParseLiteral("@\"\"");
            Assert.IsTrue(result.IsConstant);
            Assert.IsTrue(result.Value is String);
            Assert.AreEqual("", (String)result.Value);

            result = FileBuilder.ParseLiteral("@\"Jan\"");
            Assert.IsTrue(result.IsConstant);
            Assert.IsTrue(result.Value is String);
            Assert.AreEqual("Jan", (String)result.Value);

            result = FileBuilder.ParseLiteral("@\"c:\\win\\sys32\"");
            Assert.IsTrue(result.IsConstant);
            Assert.IsTrue(result.Value is String);
            Assert.AreEqual("c:\\win\\sys32", (String)result.Value);

            result = FileBuilder.ParseLiteral("@\" \\r \\n \\t \\a \\b \\f \"");
            Assert.IsTrue(result.IsConstant);
            Assert.IsTrue(result.Value is String);
            Assert.AreEqual(" \\r \\n \\t \\a \\b \\f ", (String)result.Value);
        }

        [TestMethod]
        public void TestLiteralTimespan()
        {
            var result = FileBuilder.ParseLiteral("10s");
            Assert.IsTrue(result.IsConstant);
            Assert.IsTrue(result.Value is TimeSpan);
            Assert.AreEqual(TimeSpan.FromSeconds(10), (TimeSpan)result.Value);

            result = FileBuilder.ParseLiteral("5.6ms");
            Assert.IsTrue(result.IsConstant);
            Assert.IsTrue(result.Value is TimeSpan);
            Assert.AreEqual(TimeSpan.FromTicks((TimeSpan.TicksPerMillisecond * 56) / 10), (TimeSpan)result.Value);

            result = FileBuilder.ParseLiteral("@0:10");
            Assert.IsTrue(result.IsConstant);
            Assert.IsTrue(result.Value is TimeSpan);
            Assert.AreEqual(TimeSpan.FromTicks(TimeSpan.TicksPerSecond * 10), (TimeSpan)result.Value);

            result = FileBuilder.ParseLiteral("@0:02:20");
            Assert.IsTrue(result.IsConstant);
            Assert.IsTrue(result.Value is TimeSpan);
            Assert.AreEqual(TimeSpan.FromTicks(TimeSpan.TicksPerSecond * (2 * 60 + 20)), (TimeSpan)result.Value);

            result = FileBuilder.ParseLiteral("@0:10.72");
            Assert.IsTrue(result.IsConstant);
            Assert.IsTrue(result.Value is TimeSpan);
            Assert.AreEqual(TimeSpan.FromTicks(TimeSpan.TicksPerMillisecond * 10720), (TimeSpan)result.Value);

            result = FileBuilder.ParseLiteral("@0:03:45.35");
            Assert.IsTrue(result.IsConstant);
            Assert.IsTrue(result.Value is TimeSpan);
            Assert.AreEqual(TimeSpan.FromTicks(TimeSpan.TicksPerMillisecond * (225350)), (TimeSpan)result.Value);
        }

        [TestMethod]
        public void TestLiteralDateTime()
        {
            var result = FileBuilder.ParseLiteral("@2016-11-23");
            Assert.IsTrue(result.IsConstant);
            Assert.IsTrue(result.Value is DateTime);
            Assert.AreEqual(new DateTime(2016, 11, 23), (DateTime)result.Value);

            result = FileBuilder.ParseLiteral("@2002-08-04 16:03:22.934 UTC");
            Assert.IsTrue(result.IsConstant);
            Assert.IsTrue(result.Value is DateTime);
            Assert.AreEqual(new DateTime(2002, 08, 04, 16, 03, 22, 934, DateTimeKind.Utc), (DateTime)result.Value);
        }

        [TestMethod]
        public void TestLiteralRange()
        {
            var result = FileBuilder.ParseLiteral("@[5]");
            Assert.IsTrue(result.IsConstant);
            Assert.IsTrue(result.Value is Range);
            Assert.AreEqual("5", result.Value.ToString());

            result = FileBuilder.ParseLiteral("@[..7]");
            Assert.IsTrue(result.IsConstant);
            Assert.IsTrue(result.Value is Range);
            Assert.AreEqual("..7", result.Value.ToString());

            result = FileBuilder.ParseLiteral("@[-4..]");
            Assert.IsTrue(result.IsConstant);
            Assert.IsTrue(result.Value is Range);
            Assert.AreEqual("-4..", result.Value.ToString());

            result = FileBuilder.ParseLiteral("@[-2..72]");
            Assert.IsTrue(result.IsConstant);
            Assert.IsTrue(result.Value is Range);
            Assert.AreEqual("-2..72", result.Value.ToString());

            result = FileBuilder.ParseLiteral("@[..-31,-15,-2,0,7,92,100..199,300..]");
            Assert.IsTrue(result.IsConstant);
            Assert.IsTrue(result.Value is Range);
            Assert.AreEqual("..-31, -15, -2, 0, 7, 92, 100..199, 300..", result.Value.ToString());
        }

        [TestMethod]
        public void TestLiteralBinaryBlock()
        {
            var result = FileBuilder.ParseLiteral("@00");
            Assert.IsTrue(result.IsConstant);
            Assert.IsTrue(result.Value is byte[]);
            Assert.AreEqual("00", ValueToString(result.Value));

            result = FileBuilder.ParseLiteral("@000000000000000000000000");
            Assert.IsTrue(result.IsConstant);
            Assert.IsTrue(result.Value is byte[]);
            Assert.AreEqual("00 00 00 00 00 00 00 00 00 00 00 00", ValueToString(result.Value));

            result = FileBuilder.ParseLiteral("@ABBA1973");
            Assert.IsTrue(result.IsConstant);
            Assert.IsTrue(result.Value is byte[]);
            Assert.AreEqual("AB BA 19 73", ValueToString(result.Value));

            result = FileBuilder.ParseLiteral("@FFFEFDFCFBFAF9F8F7F6F5F4F3F2F1F0");
            Assert.IsTrue(result.IsConstant);
            Assert.IsTrue(result.Value is byte[]);
            Assert.AreEqual("FF FE FD FC FB FA F9 F8 F7 F6 F5 F4 F3 F2 F1 F0", ValueToString(result.Value));
        }

        [TestMethod, Ignore]
        public void TestContract()
        {
            FileBuilder.Test();
        }

        private static string ValueToString(object value)
        {
            if (value is byte[])
            {
                var array = value as byte[];
                var sb = new StringBuilder(array.Length * 3);
                foreach (var b in array)
                {
                    sb.Append(b.ToString("X2"));
                    sb.Append(" ");
                }
                return sb.ToString().TrimEnd();
            }
            throw new NotImplementedException();
        }
    }
}
