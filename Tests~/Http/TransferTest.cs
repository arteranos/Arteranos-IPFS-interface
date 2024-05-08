using NUnit.Framework;
using System.Collections;
using UnityEngine.TestTools;

using UnityEngine;
using Newtonsoft.Json.Linq;
using System.IO;
using Ipfs.Unity;

namespace Ipfs.Http
{
    [TestFixture]
    public class TransferTest
    {
        [UnityTest]
        public IEnumerator DownloadTarFile()
        {
            var ipfs = new IpfsClient();
            Assert.IsNotNull(ipfs);

            Stream s = null;
            yield return Asyncs.Async2Coroutine(ipfs.FileSystem.GetAsync(Utils.sample_Dir), _s => s = _s);
            Assert.IsNotNull(s);
            byte[] buffer = new byte[1024];
            int n = 0;
            int total = 0;
            do
            {
                n = s.Read(buffer, 0, buffer.Length);
                total += n;
            } while (n > 0);

            Assert.AreNotEqual(0, total);
            Debug.Log($"Total bytes: {total}");
        }

        [UnityTest]
        public IEnumerator DownloadPlainFile()
        {
            var ipfs = new IpfsClient();
            Assert.IsNotNull(ipfs);

            Stream s = null;
            yield return Asyncs.Async2Coroutine(ipfs.FileSystem.ReadFileAsync(Utils.sample_File), _s => s = _s);
            Assert.IsNotNull(s);
            StreamReader sr = new(s);

            string contents = sr.ReadToEnd();
            StringAssert.Contains("IPFS -- Inter-Planetary File system", contents);
            Debug.Log(contents);
        }
    }
}