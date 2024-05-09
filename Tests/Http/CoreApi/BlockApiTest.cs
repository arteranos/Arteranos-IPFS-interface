﻿using Ipfs.Http;
using NUnit.Framework;
using UnityEngine.TestTools;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ipfs.Http
{
    [TestFixture]
    public class BlockApiTest
    {
        private IpfsClient ipfs = TestFixture.Ipfs;
        private string id = "QmPv52ekjS75L4JmHpXVeuJ5uX2ecSfSZo88NSyxwA3rAQ";
        private byte[] blob = Encoding.UTF8.GetBytes("blorb");

        [Test]
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

        [Test]
        public void Put_Bytes_ContentType()
        {
            var cid = ipfs.Block.PutAsync(blob, contentType: "raw").Result;
            Assert.AreEqual("bafkreiaxnnnb7qz2focittuqq3ya25q7rcv3bqynnczfzako47346wosmu", (string)cid);

            var data = ipfs.Block.GetAsync(cid).Result;
            Assert.AreEqual(blob.Length, data.Length);
            CollectionAssert.AreEqual(blob, data);
        }

        [Test]
        public void Put_Bytes_Hash()
        {
            var cid = ipfs.Block.PutAsync(blob, "raw", "sha2-512").Result;
            Assert.AreEqual("bafkrgqelljziv4qfg5mefz36m2y3h6voaralnw6lwb4f53xcnrf4mlsykkn7vt6eno547tw5ygcz62kxrle45wnbmpbofo5tvu57jvuaf7k7e", (string)cid);

            var data = ipfs.Block.GetAsync(cid).Result;
            Assert.AreEqual(blob.Length, data.Length);
            CollectionAssert.AreEqual(blob, data);
        }

        [Test]
        public void Put_Bytes_Pinned()
        {
            var data1 = new byte[] { 23, 24, 127 };
            var cid1 = ipfs.Block.PutAsync(data1, contentType: "raw", pin: true).Result;
            var pins = ipfs.Pin.ListAsync().Result;
            Assert.IsTrue(pins.Any(pin => pin == cid1));

            var data2 = new byte[] { 123, 124, 27 };
            var cid2 = ipfs.Block.PutAsync(data2, contentType: "raw", pin: false).Result;
            pins = ipfs.Pin.ListAsync().Result;
            Assert.IsFalse(pins.Any(pin => pin == cid2));
        }

        [Test]
        public void Put_Stream()
        {
            var cid = ipfs.Block.PutAsync(new MemoryStream(blob)).Result;
            Assert.AreEqual(id, (string)cid);

            var data = ipfs.Block.GetAsync(cid).Result;
            Assert.AreEqual(blob.Length, data.Length);
            CollectionAssert.AreEqual(blob, data);
        }

        [Test]
        public void Put_Stream_ContentType()
        {
            var cid = ipfs.Block.PutAsync(new MemoryStream(blob), contentType: "raw").Result;
            Assert.AreEqual("bafkreiaxnnnb7qz2focittuqq3ya25q7rcv3bqynnczfzako47346wosmu", (string)cid);

            var data = ipfs.Block.GetAsync(cid).Result;
            Assert.AreEqual(blob.Length, data.Length);
            CollectionAssert.AreEqual(blob, data);
        }

        [Test]
        public void Put_Stream_Hash()
        {
            var cid = ipfs.Block.PutAsync(new MemoryStream(blob), "raw", "sha2-512").Result;
            Assert.AreEqual("bafkrgqelljziv4qfg5mefz36m2y3h6voaralnw6lwb4f53xcnrf4mlsykkn7vt6eno547tw5ygcz62kxrle45wnbmpbofo5tvu57jvuaf7k7e", (string)cid);

            var data = ipfs.Block.GetAsync(cid).Result;
            Assert.AreEqual(blob.Length, data.Length);
            CollectionAssert.AreEqual(blob, data);
        }

        [Test]
        public void Put_Stream_Pinned()
        {
            var data1 = new MemoryStream(new byte[] { 23, 24, 127 });
            var cid1 = ipfs.Block.PutAsync(data1, contentType: "raw", pin: true).Result;
            var pins = ipfs.Pin.ListAsync().Result;
            Assert.IsTrue(pins.Any(pin => pin == cid1));

            var data2 = new MemoryStream(new byte[] { 123, 124, 27 });
            var cid2 = ipfs.Block.PutAsync(data2, contentType: "raw", pin: false).Result;
            pins = ipfs.Pin.ListAsync().Result;
            Assert.IsFalse(pins.Any(pin => pin == cid2));
        }

        [Test]
        public void Get()
        {
            var _ = ipfs.Block.PutAsync(blob).Result;
            var block = ipfs.Block.GetAsync(id).Result;
            CollectionAssert.AreEqual(blob, block);

            var blob1 = new byte[blob.Length];
            CollectionAssert.AreEqual(blob, blob1);
        }

        [Test]
        public void Stat()
        {
            var _ = ipfs.Block.PutAsync(blob).Result;
            var info = ipfs.Block.StatAsync(id).Result;
            Assert.AreEqual(id, (string)info.Id);
            Assert.AreEqual(5, info.Size);
        }

        [Test]
        public async Task Remove()
        {
            var _ = ipfs.Block.PutAsync(blob).Result;
            var cid = await ipfs.Block.RemoveAsync(id);
            Assert.AreEqual(id, (string)cid);
        }

        [Test]
        public void Remove_Unknown()
        {
            ExceptionAssert.Throws<Exception>(() => { var _ = ipfs.Block.RemoveAsync("QmPv52ekjS75L4JmHpXVeuJ5uX2ecSfSZo88NSyxwA3rFF").Result; });
        }

        [Test]
        public async Task Remove_Unknown_OK()
        {
            var cid = await ipfs.Block.RemoveAsync("QmPv52ekjS75L4JmHpXVeuJ5uX2ecSfSZo88NSyxwA3rFF", true);
        }

    }
}
