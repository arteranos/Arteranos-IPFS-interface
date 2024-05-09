using NUnit.Framework;
using UnityEngine.TestTools;
using System.Linq;
using System.Threading.Tasks;

namespace Ipfs.Http
{
    [TestFixture]
    public class PinApiTest
    {
        [Test]
        public void List()
        {
            var ipfs = TestFixture.Ipfs;
            var pins = ipfs.Pin.ListAsync().Result;
            Assert.IsNotNull(pins);
            Assert.IsTrue(pins.Count() > 0);
        }

        [UnityTest]
        public System.Collections.IEnumerator Async_Add_Remove()
        {
            yield return Unity.Asyncs.Async2Coroutine(Add_Remove());
        }

        public async Task Add_Remove()
        {
            var ipfs = TestFixture.Ipfs;
            var result = await ipfs.FileSystem.AddTextAsync("I am pinned");
            var id = result.Id;

            var pins = await ipfs.Pin.AddAsync(id);
            Assert.IsTrue(pins.Any(pin => pin == id));
            var all = await ipfs.Pin.ListAsync();
            Assert.IsTrue(all.Any(pin => pin == id));

            pins = await ipfs.Pin.RemoveAsync(id);
            Assert.IsTrue(pins.Any(pin => pin == id));
            all = await ipfs.Pin.ListAsync();
            Assert.IsFalse(all.Any(pin => pin == id));
        }

    }
}
