using NUnit.Framework;
using UnityEngine.TestTools;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Ipfs.Http
{
// DHT API obsoleted and removed to be replaced with the Routing API.
#if REMOVED
    [TestFixture]
    public class DhtApiTest
    {
        private const string helloWorldID = "QmT78zSuBmuS4z925WZfrqQ1qHaJ56DQaTfyMUF7F8ff5o";

        [UnityTest]
        public System.Collections.IEnumerator Async_FindPeer()
        {
            yield return Unity.Asyncs.Async2Coroutine(FindPeer());
        }

        public async Task FindPeer()
        {
            var ipfs = TestFixture.Ipfs;
            var cts = new CancellationTokenSource(TimeSpan.FromMinutes(2));
            var mars = await ipfs.Dht.FindPeerAsync("QmSoLMeWqB7YGVLJN3pNLQpmmEk35v6wYtsMGLzSr5QBU3", cts.Token);
            Assert.IsNotNull(mars);
        }

        [UnityTest]
        public System.Collections.IEnumerator Async_FindProviders()
        {
            yield return Unity.Asyncs.Async2Coroutine(FindProviders());
        }

        public async Task FindProviders()
        {
            var ipfs = TestFixture.Ipfs;
            var cts = new CancellationTokenSource(TimeSpan.FromMinutes(2));
            var providers = await ipfs.Dht.FindProvidersAsync(helloWorldID, 1, cancel: cts.Token);
            Assert.AreNotEqual(0, providers.Count());
        }

    }
#endif
}
