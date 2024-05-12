using Ipfs.Http;
using NUnit.Framework;
using UnityEngine.TestTools;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ipfs.Http
{
// REMOVED - Block API is meant to be for fiddling with the repository.
// kubo can hang with pins after removing blocks.
#if EXPERIMENTAL
    [TestFixture]
    public class BlockApiTest
    {
        private IpfsClient ipfs = TestFixture.Ipfs;
        private string id = "QmPv52ekjS75L4JmHpXVeuJ5uX2ecSfSZo88NSyxwA3rAQ";
        private byte[] blob = Encoding.UTF8.GetBytes("blorb");

        [UnityTest]
        public System.Collections.IEnumerator Async_Put_Bytes()
        {
            yield return Unity.Asyncs.Async2Coroutine(Put_Bytes());
        }

        public async Task Put_Bytes()
        {
            var cid = await ipfs.Block.PutAsync(blob);
            Assert.AreEqual(id, (string)cid);

            var data = await ipfs.Block.GetAsync(cid);
            Assert.AreEqual(blob.Length, data.Length);

            var stream = await ipfs.FileSystem.ReadFileAsync(cid);
            using var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream);
            var bytes = memoryStream.ToArray();

            CollectionAssert.AreEqual(blob, bytes);
        }

        [UnityTest]
        public System.Collections.IEnumerator Async_Put_Bytes_ContentType()
        {
            yield return Unity.Asyncs.Async2Coroutine(Put_Bytes_ContentType());
        }

        public async Task Put_Bytes_ContentType()
        {
            var cid = await ipfs.Block.PutAsync(blob, contentType: "raw");
            Assert.AreEqual("bafkreiaxnnnb7qz2focittuqq3ya25q7rcv3bqynnczfzako47346wosmu", (string)cid);

            var data = await ipfs.Block.GetAsync(cid);
            Assert.AreEqual(blob.Length, data.Length);
            CollectionAssert.AreEqual(blob, data);
        }

        [UnityTest]
        public System.Collections.IEnumerator Async_Put_Bytes_Hash()
        {
            yield return Unity.Asyncs.Async2Coroutine(Put_Bytes_Hash());
        }

        public async Task Put_Bytes_Hash()
        {
            var cid = await ipfs.Block.PutAsync(blob, "raw", "sha2-512");
            Assert.AreEqual("bafkrgqelljziv4qfg5mefz36m2y3h6voaralnw6lwb4f53xcnrf4mlsykkn7vt6eno547tw5ygcz62kxrle45wnbmpbofo5tvu57jvuaf7k7e", (string)cid);

            var data = await ipfs.Block.GetAsync(cid);
            Assert.AreEqual(blob.Length, data.Length);
            CollectionAssert.AreEqual(blob, data);
        }

        [UnityTest]
        public System.Collections.IEnumerator Async_Put_Bytes_Pinned()
        {
            yield return Unity.Asyncs.Async2Coroutine(Put_Bytes_Pinned());
        }

        public async Task Put_Bytes_Pinned()
        {
            var data1 = new byte[] { 23, 24, 127 };
            var cid1 = await ipfs.Block.PutAsync(data1, contentType: "raw", pin: true);
            var pins = await ipfs.Pin.ListAsync();
            Assert.IsTrue(pins.Any(pin => pin == cid1));

            var data2 = new byte[] { 123, 124, 27 };
            var cid2 = await ipfs.Block.PutAsync(data2, contentType: "raw", pin: false);
            pins = await ipfs.Pin.ListAsync();
            Assert.IsFalse(pins.Any(pin => pin == cid2));
        }

        [UnityTest]
        public System.Collections.IEnumerator Async_Put_Stream()
        {
            yield return Unity.Asyncs.Async2Coroutine(Put_Stream());
        }

        public async Task Put_Stream()
        {
            var cid = await ipfs.Block.PutAsync(new MemoryStream(blob));
            Assert.AreEqual(id, (string)cid);

            var data = await ipfs.Block.GetAsync(cid);
            Assert.AreEqual(blob.Length, data.Length);
            CollectionAssert.AreEqual(blob, data);
        }

        [UnityTest]
        public System.Collections.IEnumerator Async_Put_Stream_ContentType()
        {
            yield return Unity.Asyncs.Async2Coroutine(Put_Stream_ContentType());
        }

        public async Task Put_Stream_ContentType()
        {
            var cid = await ipfs.Block.PutAsync(new MemoryStream(blob), contentType: "raw");
            Assert.AreEqual("bafkreiaxnnnb7qz2focittuqq3ya25q7rcv3bqynnczfzako47346wosmu", (string)cid);

            var data = await ipfs.Block.GetAsync(cid);
            Assert.AreEqual(blob.Length, data.Length);
            CollectionAssert.AreEqual(blob, data);
        }

        [UnityTest]
        public System.Collections.IEnumerator Async_Put_Stream_Hash()
        {
            yield return Unity.Asyncs.Async2Coroutine(Put_Stream_Hash());
        }

        public async Task Put_Stream_Hash()
        {
            var cid = await ipfs.Block.PutAsync(new MemoryStream(blob), "raw", "sha2-512");
            Assert.AreEqual("bafkrgqelljziv4qfg5mefz36m2y3h6voaralnw6lwb4f53xcnrf4mlsykkn7vt6eno547tw5ygcz62kxrle45wnbmpbofo5tvu57jvuaf7k7e", (string)cid);

            var data = await ipfs.Block.GetAsync(cid);
            Assert.AreEqual(blob.Length, data.Length);
            CollectionAssert.AreEqual(blob, data);
        }

        [UnityTest]
        public System.Collections.IEnumerator Async_Put_Stream_Pinned()
        {
            yield return Unity.Asyncs.Async2Coroutine(Put_Stream_Pinned());
        }

        public async Task Put_Stream_Pinned()
        {
            var data1 = new MemoryStream(new byte[] { 23, 24, 127 });
            var cid1 = await ipfs.Block.PutAsync(data1, contentType: "raw", pin: true);
            var pins = await ipfs.Pin.ListAsync();
            Assert.IsTrue(pins.Any(pin => pin == cid1));

            var data2 = new MemoryStream(new byte[] { 123, 124, 27 });
            var cid2 = await ipfs.Block.PutAsync(data2, contentType: "raw", pin: false);
            pins = await ipfs.Pin.ListAsync();
            Assert.IsFalse(pins.Any(pin => pin == cid2));
        }

        [UnityTest]
        public System.Collections.IEnumerator Async_Get()
        {
            yield return Unity.Asyncs.Async2Coroutine(Get());
        }

        public async Task Get()
        {
            var _ = await ipfs.Block.PutAsync(blob);
            var block = await ipfs.Block.GetAsync(id);
            CollectionAssert.AreEqual(blob, block);

            var blob1 = new byte[blob.Length];
            CollectionAssert.AreEqual(blob, blob1);
        }

        [UnityTest]
        public System.Collections.IEnumerator Async_Stat()
        {
            yield return Unity.Asyncs.Async2Coroutine(Stat());
        }

        public async Task Stat()
        {
            var _ = await ipfs.Block.PutAsync(blob);
            var info = await ipfs.Block.StatAsync(id);
            Assert.AreEqual(id, (string)info.Id);
            Assert.AreEqual(5, info.Size);
        }

        [UnityTest]
        public System.Collections.IEnumerator Async_Remove()
        {
            yield return Unity.Asyncs.Async2Coroutine(Remove());
        }

        public async Task Remove()
        {
            var _ = await ipfs.Block.PutAsync(blob);
            var cid = await ipfs.Block.RemoveAsync(id);
            Assert.AreEqual(id, (string)cid);
        }

        [Test]
        public void Remove_Unknown()
        {
            ExceptionAssert.Throws<Exception>(() => { var _ = ipfs.Block.RemoveAsync("QmPv52ekjS75L4JmHpXVeuJ5uX2ecSfSZo88NSyxwA3rFF"); });
        }

        [UnityTest]
        public System.Collections.IEnumerator Async_Remove_Unknown_OK()
        {
            yield return Unity.Asyncs.Async2Coroutine(Remove_Unknown_OK());
        }

        public async Task Remove_Unknown_OK()
        {
            var cid = await ipfs.Block.RemoveAsync("QmPv52ekjS75L4JmHpXVeuJ5uX2ecSfSZo88NSyxwA3rFF", true);
        }

    }

#endif
}
