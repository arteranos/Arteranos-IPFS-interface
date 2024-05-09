using NUnit.Framework;
using UnityEngine.TestTools;
using System.Net.Http;
using System.Threading;

namespace Ipfs.Http
{
    /// <summary>
    ///This is a test class for IpfsClientTest and is intended
    ///to contain all IpfsClientTest Unit Tests
    ///</summary>
    [TestFixture]
    public partial class IpfsClientTest
    {
        /// <summary>
        ///   A test for IpfsClient Constructor
        ///</summary>
        [Test]
        public void Can_Create()
        {
            IpfsClient target = TestFixture.Ipfs;
            Assert.IsNotNull(target);
        }

        [Test]
        public void Do_Command_Throws_Exception_On_Invalid_Command()
        {
            IpfsClient target = TestFixture.Ipfs;
            object unknown;
            ExceptionAssert.Throws<HttpRequestException>(() => unknown = target.DoCommandAsync("foobar", default(CancellationToken)).Result);
        }

        [Test]
        public void Do_Command_Throws_Exception_On_Missing_Argument()
        {
            IpfsClient target = TestFixture.Ipfs;
            object unknown;
            ExceptionAssert.Throws<HttpRequestException>(() => unknown = target.DoCommandAsync("key/gen", default(CancellationToken)).Result);
        }
    }
}
