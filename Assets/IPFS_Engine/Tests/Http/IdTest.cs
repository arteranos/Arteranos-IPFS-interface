using NUnit.Framework;
using System.Collections;
using UnityEngine.TestTools;

using UnityEngine;
using Newtonsoft.Json.Linq;
using Ipfs.Cryptography.Proto;
using Ipfs.Cryptography;
using System.IO;

namespace Ipfs.Http
{
    [TestFixture]
    public class IdTest
    {
        [UnityTest]
        public IEnumerator IpfsPresent()
        {
            var ipfs = new IpfsClientEx();
            Assert.IsNotNull(ipfs);

            CoreApi.BitswapData data = null;
            yield return Utils.Async2Coroutine(ipfs.Stats.BitswapAsync(), _data => data = _data);

            Assert.IsNotNull(data.Peers);

            //foreach(MultiHash peer in data.Peers)
            //    Debug.Log(peer);
        }

        [UnityTest]
        public IEnumerator GetSelfId()
        {
            var ipfs = new IpfsClientEx();
            Assert.IsNotNull(ipfs);

            JToken IdString = null;
            yield return Utils.Async2Coroutine(ipfs.Config.GetAsync("Identity.PeerID"), _r => IdString = _r);

            Assert.IsNotNull(IdString);
            Debug.Log(IdString);
        }

        [UnityTest]
        public IEnumerator ReadDaemonPrivateKey()
        {
            var ipfs = new IpfsClientEx();
            Assert.IsNotNull(ipfs);

            PrivateKey pk = ipfs.ReadDaemonPrivateKey();
            Assert.IsNotNull(pk);

            bool success = false;
            yield return Utils.Async2Coroutine(ipfs.VerifyDaemonAsync(pk), _r => success = _r);
            Assert.True(success);
        }

        [UnityTest]
        public IEnumerator ReadDaemonPrivateKey_Negative()
        {
            var ipfs = new IpfsClientEx();
            Assert.IsNotNull(ipfs);

            KeyPair kp = KeyPair.Generate("ed25519");
            PrivateKey pk = kp;

            Assert.IsNotNull(pk);

            bool success = false;
            yield return Utils.Async2Coroutine(ipfs.VerifyDaemonAsync(pk), _r => success = _r);
            Assert.False(success);
        }

    }
}
