using NUnit.Framework;
using System;

namespace Ipfs.Unity
{
    [TestFixture]
    public class TimeSpanTest
    {
        [Test]
        public void ToShortString() 
        {
            Assert.AreEqual("1s", new TimeSpan(0, 0, 0, 1).ToShortString());
            Assert.AreEqual("1m", new TimeSpan(0, 0, 1, 0).ToShortString());
            Assert.AreEqual("1h", new TimeSpan(0, 1, 0, 0).ToShortString());
            Assert.AreEqual("1d", new TimeSpan(1, 0, 0, 0).ToShortString());

            Assert.AreEqual("2s", new TimeSpan(0, 0, 0, 2).ToShortString());
            Assert.AreEqual("3m", new TimeSpan(0, 0, 3, 0).ToShortString());
            Assert.AreEqual("4h", new TimeSpan(0, 4, 0, 0).ToShortString());
            Assert.AreEqual("5d", new TimeSpan(5, 0, 0, 0).ToShortString());

            Assert.AreEqual("3m1s", new TimeSpan(0, 0, 3, 1).ToShortString());
            Assert.AreEqual("4h2s", new TimeSpan(0, 4, 0, 2).ToShortString());
            Assert.AreEqual("5d23h59m59s", new TimeSpan(5, 23, 59, 59).ToShortString());

            Assert.AreEqual("-1h1s", new TimeSpan(-1, 0, -1).ToShortString());
        }
    }
}
