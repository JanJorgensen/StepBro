using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using StepBro.Core.Data;

namespace StepBroCoreTest
{
    [TestClass]
    public class TestTimeUtils
    {
        [TestMethod]
        public void TestTimeSpanParsingSunshine()
        {
            Assert.AreEqual(TimeSpan.Zero, TimeUtils.ParseTimeSpan("0s"));
            Assert.AreEqual(TimeSpan.FromTicks(TimeSpan.TicksPerSecond * 5), TimeUtils.ParseTimeSpan("5s"));
            Assert.AreEqual(TimeSpan.FromTicks(TimeSpan.TicksPerSecond * 72), TimeUtils.ParseTimeSpan("72s"));
            Assert.AreEqual(TimeSpan.FromTicks(TimeSpan.TicksPerSecond * 736), TimeUtils.ParseTimeSpan("736s"));
            Assert.AreEqual(TimeSpan.FromTicks(TimeSpan.TicksPerSecond * 2412), TimeUtils.ParseTimeSpan("2412s"));
            Assert.AreEqual(TimeSpan.FromTicks(TimeSpan.TicksPerSecond * 12378), TimeUtils.ParseTimeSpan("12378s"));
            Assert.AreEqual(TimeSpan.FromTicks(TimeSpan.TicksPerSecond * 102030), TimeUtils.ParseTimeSpan("102030s"));
            Assert.AreEqual(TimeSpan.FromTicks(TimeSpan.TicksPerSecond * 7654323), TimeUtils.ParseTimeSpan("7654323s"));
            Assert.AreEqual(TimeSpan.FromTicks(TimeSpan.TicksPerSecond * 12453678), TimeUtils.ParseTimeSpan("12453678s"));

            Assert.AreEqual(TimeSpan.Zero, TimeUtils.ParseTimeSpan("0ms"));
            Assert.AreEqual(TimeSpan.FromTicks(TimeSpan.TicksPerMillisecond * 4), TimeUtils.ParseTimeSpan("4ms"));
            Assert.AreEqual(TimeSpan.FromTicks(TimeSpan.TicksPerMillisecond * 93), TimeUtils.ParseTimeSpan("93ms"));
            Assert.AreEqual(TimeSpan.FromTicks(TimeSpan.TicksPerMillisecond * 761), TimeUtils.ParseTimeSpan("761ms"));
            Assert.AreEqual(TimeSpan.FromTicks(TimeSpan.TicksPerMillisecond * 4371), TimeUtils.ParseTimeSpan("4371ms"));
            Assert.AreEqual(TimeSpan.FromTicks(TimeSpan.TicksPerMillisecond * 76392), TimeUtils.ParseTimeSpan("76392ms"));
            Assert.AreEqual(TimeSpan.FromTicks(TimeSpan.TicksPerMillisecond * 321654), TimeUtils.ParseTimeSpan("321654ms"));
            Assert.AreEqual(TimeSpan.FromTicks(TimeSpan.TicksPerMillisecond * 5283736), TimeUtils.ParseTimeSpan("5283736ms"));
            Assert.AreEqual(TimeSpan.FromTicks(TimeSpan.TicksPerMillisecond * 88273554), TimeUtils.ParseTimeSpan("88273554ms"));

            Assert.AreEqual(TimeSpan.Zero, TimeUtils.ParseTimeSpan("0.0s"));
            Assert.AreEqual(TimeSpan.FromTicks(7 * TimeSpan.TicksPerSecond / 10), TimeUtils.ParseTimeSpan("0.7s"));
            Assert.AreEqual(TimeSpan.FromTicks(3 * TimeSpan.TicksPerSecond / 100), TimeUtils.ParseTimeSpan("0.03s"));
            Assert.AreEqual(TimeSpan.FromTicks(4 * TimeSpan.TicksPerSecond / 1000), TimeUtils.ParseTimeSpan("0.004s"));
            Assert.AreEqual(TimeSpan.FromTicks(6 * TimeSpan.TicksPerSecond / 10000), TimeUtils.ParseTimeSpan("0.0006s"));
            Assert.AreEqual(TimeSpan.FromTicks(2 * TimeSpan.TicksPerSecond / 100000), TimeUtils.ParseTimeSpan("0.00002s"));
            Assert.AreEqual(TimeSpan.FromTicks(3 * TimeSpan.TicksPerSecond / 1000000), TimeUtils.ParseTimeSpan("0.000003s"));
            Assert.AreEqual(TimeSpan.FromTicks(93 * TimeSpan.TicksPerSecond / 10), TimeUtils.ParseTimeSpan("9.3s"));
            Assert.AreEqual(TimeSpan.FromTicks(723 * TimeSpan.TicksPerSecond / 10), TimeUtils.ParseTimeSpan("72.3s"));
            Assert.AreEqual(TimeSpan.FromTicks(1234 * TimeSpan.TicksPerSecond / 10), TimeUtils.ParseTimeSpan("123.4s"));
            Assert.AreEqual(TimeSpan.FromTicks(87261 * TimeSpan.TicksPerSecond / 10), TimeUtils.ParseTimeSpan("8726.1s"));
            Assert.AreEqual(TimeSpan.FromTicks(662598 * TimeSpan.TicksPerSecond / 10), TimeUtils.ParseTimeSpan("66259.8s"));
            Assert.AreEqual(TimeSpan.FromTicks(1239755 * TimeSpan.TicksPerSecond / 10), TimeUtils.ParseTimeSpan("123975.5s"));
            Assert.AreEqual(TimeSpan.FromTicks(6372 * TimeSpan.TicksPerSecond / 1000), TimeUtils.ParseTimeSpan("6.372s"));

            Assert.AreEqual(TimeSpan.Zero, TimeUtils.ParseTimeSpan("0.0ms"));
            Assert.AreEqual(TimeSpan.FromTicks(3 * TimeSpan.TicksPerMillisecond / 10), TimeUtils.ParseTimeSpan("0.3ms"));
            Assert.AreEqual(TimeSpan.FromTicks(4 * TimeSpan.TicksPerMillisecond / 100), TimeUtils.ParseTimeSpan("0.04ms"));
            Assert.AreEqual(TimeSpan.FromTicks(7 * TimeSpan.TicksPerMillisecond / 1000), TimeUtils.ParseTimeSpan("0.007ms"));
            Assert.AreEqual(TimeSpan.FromTicks(2 * TimeSpan.TicksPerMillisecond / 10000), TimeUtils.ParseTimeSpan("0.0002ms"));
            Assert.AreEqual(TimeSpan.FromTicks(15 * TimeSpan.TicksPerMillisecond / 10), TimeUtils.ParseTimeSpan("1.5ms"));
            Assert.AreEqual(TimeSpan.FromTicks(312 * TimeSpan.TicksPerMillisecond / 10), TimeUtils.ParseTimeSpan("31.2ms"));
            Assert.AreEqual(TimeSpan.FromTicks(7216 * TimeSpan.TicksPerMillisecond / 10), TimeUtils.ParseTimeSpan("721.6ms"));
            Assert.AreEqual(TimeSpan.FromTicks(98219 * TimeSpan.TicksPerMillisecond / 10), TimeUtils.ParseTimeSpan("9821.9ms"));
            Assert.AreEqual(TimeSpan.FromTicks(901613 * TimeSpan.TicksPerMillisecond / 10), TimeUtils.ParseTimeSpan("90161.3ms"));
            Assert.AreEqual(TimeSpan.FromTicks(2146734 * TimeSpan.TicksPerMillisecond / 10), TimeUtils.ParseTimeSpan("214673.4ms"));
            Assert.AreEqual(TimeSpan.FromTicks(98372 * TimeSpan.TicksPerMillisecond / 1000), TimeUtils.ParseTimeSpan("98.372ms"));
        }

        [TestMethod]
        public void TestDateTimeParsingSunshine()
        {
            DateTime t = TimeUtils.ParseDateTime("@2016-11-23", 1);
            Assert.AreEqual(new DateTime(2016, 11, 23), t);
            Assert.AreEqual(DateTimeKind.Unspecified, t.Kind);

            t = TimeUtils.ParseDateTime("@1999-01-01", 1);
            Assert.AreEqual(new DateTime(1999, 01, 01), t);
            Assert.AreEqual(DateTimeKind.Unspecified, t.Kind);

            t = TimeUtils.ParseDateTime("@2005-12-29 08:02", 1);
            Assert.AreEqual(new DateTime(2005, 12, 29, 08, 02, 00), t);
            Assert.AreEqual(DateTimeKind.Unspecified, t.Kind);

            t = TimeUtils.ParseDateTime("@2002-08-28 23:30:26", 1);
            Assert.AreEqual(new DateTime(2002, 08, 28, 23, 30, 26), t);
            Assert.AreEqual(DateTimeKind.Unspecified, t.Kind);

            t = TimeUtils.ParseDateTime("@1999-01-01 15:23:07.428", 1);
            Assert.AreEqual(new DateTime(1999, 01, 01, 15, 23, 07, 428), t);
            Assert.AreEqual(DateTimeKind.Unspecified, t.Kind);

            t = TimeUtils.ParseDateTime("@2005-12-29 08:02 UTC", 1);
            Assert.AreEqual(new DateTime(2005, 12, 29, 08, 02, 00), t);
            Assert.AreEqual(DateTimeKind.Utc, t.Kind);

            t = TimeUtils.ParseDateTime("@2002-08-28 23:30:26 UTC", 1);
            Assert.AreEqual(new DateTime(2002, 08, 28, 23, 30, 26), t);
            Assert.AreEqual(DateTimeKind.Utc, t.Kind);

            t = TimeUtils.ParseDateTime("@1999-01-01 15:23:07.428 UTC", 1);
            Assert.AreEqual(new DateTime(1999, 01, 01, 15, 23, 07, 428), t);
            Assert.AreEqual(DateTimeKind.Utc, t.Kind);

            t = TimeUtils.ParseDateTime("@2005-12-29 08:02 Local", 1);
            Assert.AreEqual(new DateTime(2005, 12, 29, 08, 02, 00), t);
            Assert.AreEqual(DateTimeKind.Local, t.Kind);

            t = TimeUtils.ParseDateTime("@2002-08-28 23:30:26 Local", 1);
            Assert.AreEqual(new DateTime(2002, 08, 28, 23, 30, 26), t);
            Assert.AreEqual(DateTimeKind.Local, t.Kind);

            t = TimeUtils.ParseDateTime("@1999-01-01 15:23:07.428 Local", 1);
            Assert.AreEqual(new DateTime(1999, 01, 01, 15, 23, 07, 428), t);
            Assert.AreEqual(DateTimeKind.Local, t.Kind);
        }
    }
}