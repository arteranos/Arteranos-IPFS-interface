using NUnit.Framework;
using System.Collections;
using UnityEngine.TestTools;

using UnityEngine;
using Newtonsoft.Json.Linq;
using Ipfs.Cryptography.Proto;
using Ipfs.Cryptography;
using System.IO;
using Ipfs.Unity;
using System;

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
            Assert.IsNotNull(ipfs.FileSystemEx);
            Assert.IsNotNull(ipfs.Routing);
            Assert.IsNotNull(ipfs.NameEx);

            yield return null;
        }

        [UnityTest]
        public IEnumerator GetSelfId()
        {
            var ipfs = new IpfsClientEx();
            Assert.IsNotNull(ipfs);

            JToken IdString = null;
            yield return Asyncs.Async2Coroutine(ipfs.Config.GetAsync("Identity.PeerID"), _r => IdString = _r);

            Assert.IsNotNull(IdString);
            Debug.Log(IdString);
        }

        [UnityTest]
        public IEnumerator ReadDaemonPrivateKey()
        {
            var ipfs = new IpfsClientEx();
            Assert.IsNotNull(ipfs);

            PrivateKey pk = IpfsClientEx.ReadDaemonPrivateKey();
            Assert.IsNotNull(pk);

            yield return Asyncs.Async2Coroutine(ipfs.VerifyDaemonAsync(pk));
        }

        [UnityTest]
        public IEnumerator ReadDaemonPrivateKey_Negative()
        {
            var ipfs = new IpfsClientEx();
            Assert.IsNotNull(ipfs);

            KeyPair kp = KeyPair.Generate("ed25519");
            PrivateKey pk = kp;

            Assert.IsNotNull(pk);

            Exception e = null;
            yield return Asyncs.Async2Coroutine(ipfs.VerifyDaemonAsync(pk), _e => e = _e);
            Assert.Throws<InvalidDataException>(() => throw e);
        }
    }
}
