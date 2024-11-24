using NUnit.Framework;
using UnityEngine.TestTools;
using System.Linq;
using System.Threading.Tasks;

namespace Ipfs.Http
{
    [TestFixture]
    public class GenericApiTest : TestFixture
    {
        [UnityTest]
        public System.Collections.IEnumerator Async_Local_Node_Info()
        {
            yield return Unity.Asyncs.Async2Coroutine(Local_Node_Info());
        }

        public async Task Local_Node_Info()
        {
            var ipfs = TestFixture.Ipfs;
            var node = await ipfs.IdAsync();
            Assert.IsInstanceOf(typeof(Peer), node);
        }

        // [UnityTest]
        [Ignore("Surely outdated")]
        public System.Collections.IEnumerator Async_Peer_Node_Info()
        {
            yield return Unity.Asyncs.Async2Coroutine(Peer_Node_Info());
        }

        public async Task Peer_Node_Info()
        {
            var ipfs = TestFixture.Ipfs;
            var mars = await ipfs.IdAsync("QmSoLMeWqB7YGVLJN3pNLQpmmEk35v6wYtsMGLzSr5QBU3");
            Assert.IsNotNull(mars);
        }

        [UnityTest]
        public System.Collections.IEnumerator Async_Version_Info()
        {
            yield return Unity.Asyncs.Async2Coroutine(Version_Info());
        }

        public async Task Version_Info()
        {
            var ipfs = TestFixture.Ipfs;
            var versions = await ipfs.VersionAsync();
            Assert.IsNotNull(versions);
            Assert.IsTrue(versions.ContainsKey("Version"));
            Assert.IsTrue(versions.ContainsKey("Repo"));
        }

        [UnityTest]
        public System.Collections.IEnumerator Async_Resolve()
        {
            yield return Unity.Asyncs.Async2Coroutine(Resolve());
        }

        public async Task Resolve()
        {
            var ipfs = TestFixture.Ipfs;
            var path = await ipfs.ResolveAsync("QmYNQJoKGNHTpPxCBPh9KkDpaExgd2duMa3aF6ytMpHdao");
            Assert.AreEqual("/ipfs/QmYNQJoKGNHTpPxCBPh9KkDpaExgd2duMa3aF6ytMpHdao", path);
        }

        [UnityTest]
        public System.Collections.IEnumerator Async_Ping_Peer()
        {
            yield return Unity.Asyncs.Async2Coroutine(Ping_Peer());
        }

        public async Task Ping_Peer()
        {
            var ipfs = TestFixture.Ipfs;
            MultiHash peer = "QmSoLMeWqB7YGVLJN3pNLQpmmEk35v6wYtsMGLzSr5QBU3";
            var actual = await ipfs.Generic.PingAsync(peer, count: 1);
            Assert.IsNotNull(actual);
            Assert.AreNotEqual(0, actual.Count());
        }

        [UnityTest]
        public System.Collections.IEnumerator Async_Ping_Address()
        {
            yield return Unity.Asyncs.Async2Coroutine(Ping_Address());
        }

        public async Task Ping_Address()
        {
            var ipfs = TestFixture.Ipfs;
            MultiAddress addr = "/ip4/104.236.179.241/tcp/4001/ipfs/QmSoLPppuBtQSGwKDZT2M73ULpjvfd3aZ6ha4oFGL1KrGM";
            var actual = await ipfs.Generic.PingAsync(addr, count: 1);
            Assert.IsNotNull(actual);
            Assert.AreNotEqual(0, actual.Count());
        }
    }
}

