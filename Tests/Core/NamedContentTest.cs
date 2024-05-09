using NUnit.Framework;
using UnityEngine.TestTools;

namespace Ipfs
{
    [TestFixture]
    public class NamedContentTest
    {
        [Test]
        public void Properties()
        {
            var nc = new NamedContent
            {
                ContentPath = "/ipfs/...",
                NamePath = "/ipns/..."
            };
            Assert.AreEqual("/ipfs/...", nc.ContentPath);
            Assert.AreEqual("/ipns/...", nc.NamePath);
        }
    }
}
