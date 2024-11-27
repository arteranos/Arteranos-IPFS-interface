using NUnit.Framework;
using System.Collections;
using UnityEngine.TestTools;

using UnityEngine;
using System.Collections.Generic;
using System.Linq;

using Debug = UnityEngine.Debug;
using System.Net;

using Ipfs.Unity;
using System.Net.Sockets;

namespace Ipfs.Http
{
    [TestFixture]
    public class DiscoveryTest
    {
        [UnityTest]
        public IEnumerator FindPeer()
        {
            var ipfs = new IpfsClient();
            Assert.IsNotNull(ipfs);

            Peer peer = null;
            yield return Asyncs.Async2Coroutine(() => ipfs.IdAsync(), _peer => peer = _peer);
            Assert.IsNotNull(peer);

            foreach(MultiAddress multiAddress in peer.Addresses)
                Debug.Log(multiAddress);
        }

        [UnityTest]
        public IEnumerator FindProvider()
        {
            void FoundPeer(Peer peer)
            {
                Debug.Log($"Found peer: {peer}");
            }

            var ipfs = new IpfsClientEx();
            Assert.IsNotNull(ipfs);

            // yield vs. async clash!
            List<Peer> peers = null;
            yield return Asyncs.Async2Coroutine(() => ipfs.Routing.FindProvidersAsync(Utils.sample_Dir, providerFound: FoundPeer), _peers => peers = _peers.ToList());
            Assert.IsNotNull(peers);
            Assert.IsTrue(peers.Count() > 0);
        }

        [UnityTest]
        // [Ignore("Peer may be likely nonexistent or rekeyed")]
        public IEnumerator FindPeerAddresses()
        {
            var ipfs = new IpfsClientEx();
            Assert.IsNotNull(ipfs);

            IEnumerable<(IPAddress, ProtocolType, int)> ipAddresses = null;
            yield return Asyncs.Async2Coroutine(() => ipfs.Routing.FindPeerAddressesAsync(Utils.sample_LANPeer), addrs => ipAddresses = addrs);
            Assert.IsNotNull(ipAddresses);

            Assert.IsTrue(ipAddresses.Any());
            foreach (var addr in ipAddresses)
                Debug.Log(addr.Item1);

        }
    }
}