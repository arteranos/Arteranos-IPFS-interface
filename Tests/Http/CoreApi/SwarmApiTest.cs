using NUnit.Framework;
using UnityEngine.TestTools;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Ipfs.Http
{
    [TestFixture]
    public class SwarmApiTest: TestFixture
    {
        [UnityTest]
        public System.Collections.IEnumerator Async_Addresses()
        {
            yield return Unity.Asyncs.Async2Coroutine(Addresses());
        }

        public async Task Addresses()
        {
            var ipfs = TestFixture.Ipfs;
            var swarm = await ipfs.Swarm.AddressesAsync();
            foreach (var peer in swarm)
            {
                Assert.IsNotNull(peer.Id);
                Assert.IsNotNull(peer.Addresses);
                Assert.AreNotEqual(0, peer.Addresses.Count());
            }
        }

        [UnityTest]
        public System.Collections.IEnumerator Async_Peers()
        {
            yield return Unity.Asyncs.Async2Coroutine(Peers());
        }

        public async Task Peers()
        {
            var ipfs = TestFixture.Ipfs;
            var peers = await ipfs.Swarm.PeersAsync();
            Assert.AreNotEqual(0, peers.Count());
            foreach (var peer in peers)
            {
                Assert.IsNotNull(peer.Id);
                Assert.IsNotNull(peer.ConnectedAddress);
            }
        }

        [UnityTest]
        public System.Collections.IEnumerator Async_Peers_Info()
        {
            yield return Unity.Asyncs.Async2Coroutine(Peers_Info());
        }

        public async Task Peers_Info()
        {
            var ipfs = TestFixture.Ipfs;
            var peers = await ipfs.Swarm.PeersAsync();
            await Task.WhenAll(peers
                .Where(p => p.Latency != TimeSpan.Zero)
                .OrderBy(p => p.Latency)
                .Take(1)
                .Select(async p =>
                {
                    var peer = await ipfs.IdAsync(p.Id);
                    Assert.AreNotEqual("", peer.PublicKey);
                }));
        }

        [UnityTest]
        public System.Collections.IEnumerator Async_Connection()
        {
            yield return Unity.Asyncs.Async2Coroutine(Connection());
        }

        public async Task Connection()
        {
            var ipfs = TestFixture.Ipfs;
            var peers = await ipfs.Swarm.PeersAsync();

            // Sometimes we cannot connect to a specific peer.  This
            // tests that a connection can be made to at least one peer.
            foreach (var peer in peers.Take(2))
            {
                try
                {
                    await ipfs.Swarm.ConnectAsync(peer.ConnectedAddress);
                    return;
                }
                catch (Exception)
                {
                    // eat it
                }
            }

            Assert.Fail("Cannot connect to any peer");
        }

        [UnityTest]
        public System.Collections.IEnumerator Async_Filter_Add_Remove()
        {
            yield return Unity.Asyncs.Async2Coroutine(Filter_Add_Remove());
        }

        public async Task Filter_Add_Remove()
        {
            var ipfs = TestFixture.Ipfs;
            var somewhere = new MultiAddress("/ip4/192.127.0.0/ipcidr/16");
            var filter = await ipfs.Swarm.AddAddressFilterAsync(somewhere, true);
            Assert.IsNotNull(filter);
            Assert.AreEqual(somewhere, filter);
            var filters = await ipfs.Swarm.ListAddressFiltersAsync();
            Assert.IsTrue(filters.Any(a => a == somewhere));
            filters = await ipfs.Swarm.ListAddressFiltersAsync(true);
            Assert.IsTrue(filters.Any(a => a == somewhere));

            filter = await ipfs.Swarm.RemoveAddressFilterAsync(somewhere, true);
            Assert.IsNotNull(filter);
            Assert.AreEqual(somewhere, filter);
            filters = await ipfs.Swarm.ListAddressFiltersAsync();
            Assert.IsFalse(filters.Any(a => a == somewhere));
            filters = await ipfs.Swarm.ListAddressFiltersAsync(true);
            Assert.IsFalse(filters.Any(a => a == somewhere));
        }

        [UnityTest]
        public System.Collections.IEnumerator Async_Filter_List()
        {
            yield return Unity.Asyncs.Async2Coroutine(Filter_List());
        }

        public async Task Filter_List()
        {
            var ipfs = TestFixture.Ipfs;
            var filters = await ipfs.Swarm.ListAddressFiltersAsync(persist: false);
            Assert.IsNotNull(filters);
        }

        [UnityTest]
        public System.Collections.IEnumerator Async_Filter_Remove_Unknown()
        {
            yield return Unity.Asyncs.Async2Coroutine(Filter_Remove_Unknown());
        }

        public async Task Filter_Remove_Unknown()
        {
            var ipfs = TestFixture.Ipfs;
            var somewhere = new MultiAddress("/ip4/192.168.0.3/ipcidr/2");

            var filter = await ipfs.Swarm.RemoveAddressFilterAsync(somewhere);
            Assert.IsNull(filter);
        }
    }
}
