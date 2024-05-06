using NUnit.Framework;
using System.Collections;
using UnityEngine.TestTools;

using UnityEngine;
using Newtonsoft.Json.Linq;

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
    }
}
