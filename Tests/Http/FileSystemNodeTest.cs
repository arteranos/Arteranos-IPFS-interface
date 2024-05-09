using NUnit.Framework;
using UnityEngine.TestTools;
using Newtonsoft.Json;
using System.Linq;
using System.Threading.Tasks;

namespace Ipfs.Http
{
    [TestFixture]
    public class FileSystemNodeTest
    {
        [UnityTest]
        public System.Collections.IEnumerator Async_Serialization()
        {
            yield return Unity.Asyncs.Async2Coroutine(Serialization());
        }

        public async Task Serialization()
        {
            var ipfs = TestFixture.Ipfs;
            var a = await ipfs.FileSystem.AddTextAsync("hello world");
            Assert.AreEqual("Qmf412jQZiuVUtdgnB36FXFX7xg5V6KEbSJ4dpQuhkLyfD", (string)a.Id);

            var b = await ipfs.FileSystem.ListAsync(a.Id);
            var json = JsonConvert.SerializeObject(b);
            var c = JsonConvert.DeserializeObject<FileSystemNode>(json);
            Assert.AreEqual(b.Id, c.Id);
            Assert.AreEqual(b.IsDirectory, c.IsDirectory);
            Assert.AreEqual(b.Size, c.Size);
            CollectionAssert.AreEqual(b.Links.ToArray(), c.Links.ToArray());
        }
    }
}
