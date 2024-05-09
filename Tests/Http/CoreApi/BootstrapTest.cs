using NUnit.Framework;
using UnityEngine.TestTools;
using System.Linq;
using System.Threading.Tasks;

namespace Ipfs.Http
{
    [TestFixture]
    public class BootstapApiTest
    {
        IpfsClient ipfs = TestFixture.Ipfs;
        MultiAddress somewhere = "/ip4/127.0.0.1/tcp/4009/ipfs/QmPv52ekjS75L4JmHpXVeuJ5uX2ecSfSZo88NSyxwA3rAQ";

        [UnityTest]
        public System.Collections.IEnumerator Async_Add_Remove()
        {
            yield return Unity.Asyncs.Async2Coroutine(Add_Remove());
        }

        public async Task Add_Remove()
        {
            var addr = await ipfs.Bootstrap.AddAsync(somewhere);
            Assert.IsNotNull(addr);
            Assert.AreEqual(somewhere, addr);
            var addrs = await ipfs.Bootstrap.ListAsync();
            Assert.IsTrue(addrs.Any(a => a == somewhere));

            addr = await ipfs.Bootstrap.RemoveAsync(somewhere);
            Assert.IsNotNull(addr);
            Assert.AreEqual(somewhere, addr);
            addrs = await ipfs.Bootstrap.ListAsync();
            Assert.IsFalse(addrs.Any(a => a == somewhere));
        }

        [UnityTest]
        public System.Collections.IEnumerator Async_List()
        {
            yield return Unity.Asyncs.Async2Coroutine(List());
        }

        public async Task List()
        {
            var addrs = await ipfs.Bootstrap.ListAsync();
            Assert.IsNotNull(addrs);
            Assert.AreNotEqual(0, addrs.Count());
        }

        [UnityTest]
        public System.Collections.IEnumerator Async_Remove_All()
        {
            yield return Unity.Asyncs.Async2Coroutine(Remove_All());
        }

        public async Task Remove_All()
        {
            var original = await ipfs.Bootstrap.ListAsync();
            await ipfs.Bootstrap.RemoveAllAsync();
            var addrs = await ipfs.Bootstrap.ListAsync();
            Assert.AreEqual(0, addrs.Count());
            foreach (var addr in original)
            {
                await ipfs.Bootstrap.AddAsync(addr);
            }
        }

        [UnityTest]
        public System.Collections.IEnumerator Async_Add_Defaults()
        {
            yield return Unity.Asyncs.Async2Coroutine(Add_Defaults());
        }

        public async Task Add_Defaults()
        {
            var original = await ipfs.Bootstrap.ListAsync();
            await ipfs.Bootstrap.RemoveAllAsync();
            try
            {
                await ipfs.Bootstrap.AddDefaultsAsync();
                var addrs = await ipfs.Bootstrap.ListAsync();
                Assert.AreNotEqual(0, addrs.Count());
            }
            finally
            {
                await ipfs.Bootstrap.RemoveAllAsync();
                foreach (var addr in original)
                {
                    await ipfs.Bootstrap.AddAsync(addr);
                }
            }
        }
    }
}
