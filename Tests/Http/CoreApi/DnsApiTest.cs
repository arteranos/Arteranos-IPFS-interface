using NUnit.Framework;
using UnityEngine.TestTools;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Ipfs.Http
{
    public class DnsApiTest
    {
        // [Test]
        [Ignore("Nonexistent Kubo API endpoint: dns")]
        public void Api_Exists()
        {
            IpfsClient ipfs = TestFixture.Ipfs;
            Assert.IsNotNull(ipfs.Dns);
        }

        // [UnityTest]
        [Ignore("Nonexistent Kubo API endpoint: dns")]
        public System.Collections.IEnumerator Async_Resolve()
        {
            yield return Unity.Asyncs.Async2Coroutine(Resolve());
        }

        public async Task Resolve()
        {
            IpfsClient ipfs = TestFixture.Ipfs;
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            var path = await ipfs.Dns.ResolveAsync("ipfs.io", recursive: true, cancel: cts.Token);
            StringAssert.StartsWith(path, "/ipfs/");
        }
    }
}
