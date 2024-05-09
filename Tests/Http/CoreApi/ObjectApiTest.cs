using NUnit.Framework;
using UnityEngine.TestTools;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ipfs.Http
{
    [TestFixture]
    public class ObjectApiTest
    {
        private IpfsClient ipfs = TestFixture.Ipfs;

        [UnityTest]
        public System.Collections.IEnumerator Async_New_Template_Null()
        {
            yield return Unity.Asyncs.Async2Coroutine(New_Template_Null());
        }

        public async Task New_Template_Null()
        {
            var node = await ipfs.Object.NewAsync();
            Assert.AreEqual("QmdfTbBqBPQ7VNxZEYEj14VmRuZBkqFbiwReogJgS1zR1n", (string)node.Id);
        }

        [UnityTest]
        public System.Collections.IEnumerator Async_New_Template_UnixfsDir()
        {
            yield return Unity.Asyncs.Async2Coroutine(New_Template_UnixfsDir());
        }

        public async Task New_Template_UnixfsDir()
        {
            var node = await ipfs.Object.NewAsync("unixfs-dir");
            Assert.AreEqual("QmUNLLsPACCz1vLxQVkXqqLX5R1X345qqfHbsf67hvA3Nn", (string)node.Id);

            node = await ipfs.Object.NewDirectoryAsync();
            Assert.AreEqual("QmUNLLsPACCz1vLxQVkXqqLX5R1X345qqfHbsf67hvA3Nn", (string)node.Id);

        }

        [UnityTest]
        public System.Collections.IEnumerator Async_Put_Get_Dag()
        {
            yield return Unity.Asyncs.Async2Coroutine(Put_Get_Dag());
        }

        public async Task Put_Get_Dag()
        {
            var adata = Encoding.UTF8.GetBytes("alpha");
            var bdata = Encoding.UTF8.GetBytes("beta");
            var alpha = new DagNode(adata);
            var beta = new DagNode(bdata, new[] { alpha.ToLink() });
            var x = await ipfs.Object.PutAsync(beta);
            var node = await ipfs.Object.GetAsync(x.Id);
            CollectionAssert.AreEqual(beta.DataBytes, node.DataBytes);
            Assert.AreEqual(beta.Links.Count(), node.Links.Count());
            Assert.AreEqual(beta.Links.First().Id, node.Links.First().Id);
            Assert.AreEqual(beta.Links.First().Name, node.Links.First().Name);
            Assert.AreEqual(beta.Links.First().Size, node.Links.First().Size);
        }

        [UnityTest]
        public System.Collections.IEnumerator Async_Put_Get_Data()
        {
            yield return Unity.Asyncs.Async2Coroutine(Put_Get_Data());
        }

        public async Task Put_Get_Data()
        {
            var adata = Encoding.UTF8.GetBytes("alpha");
            var bdata = Encoding.UTF8.GetBytes("beta");
            var alpha = new DagNode(adata);
            var beta = await ipfs.Object.PutAsync(bdata, new[] { alpha.ToLink() });
            var node = await ipfs.Object.GetAsync(beta.Id);
            CollectionAssert.AreEqual(beta.DataBytes, node.DataBytes);
            Assert.AreEqual(beta.Links.Count(), node.Links.Count());
            Assert.AreEqual(beta.Links.First().Id, node.Links.First().Id);
            Assert.AreEqual(beta.Links.First().Name, node.Links.First().Name);
            Assert.AreEqual(beta.Links.First().Size, node.Links.First().Size);
        }

        [UnityTest]
        public System.Collections.IEnumerator Async_Data()
        {
            yield return Unity.Asyncs.Async2Coroutine(Data());
        }

        public async Task Data()
        {
            var adata = Encoding.UTF8.GetBytes("alpha");
            var node = await ipfs.Object.PutAsync(adata);
            using (var stream = await ipfs.Object.DataAsync(node.Id))
            {
                var bdata = new byte[adata.Length];
                stream.Read(bdata, 0, bdata.Length);
                CollectionAssert.AreEqual(adata, bdata);
            }
        }

        [UnityTest]
        public System.Collections.IEnumerator Async_Links()
        {
            yield return Unity.Asyncs.Async2Coroutine(Links());
        }

        public async Task Links()
        {
            var adata = Encoding.UTF8.GetBytes("alpha");
            var bdata = Encoding.UTF8.GetBytes("beta");
            var alpha = new DagNode(adata);
            var beta = await ipfs.Object.PutAsync(bdata, new[] { alpha.ToLink() });
            var links = await ipfs.Object.LinksAsync(beta.Id);
            Assert.AreEqual(beta.Links.Count(), links.Count());
            Assert.AreEqual(beta.Links.First().Id, links.First().Id);
            Assert.AreEqual(beta.Links.First().Name, links.First().Name);
            Assert.AreEqual(beta.Links.First().Size, links.First().Size);
        }

        [UnityTest]
        public System.Collections.IEnumerator Async_Stat()
        {
            yield return Unity.Asyncs.Async2Coroutine(Stat());
        }

        public async Task Stat()
        {
            var data1 = Encoding.UTF8.GetBytes("Some data 1");
            var data2 = Encoding.UTF8.GetBytes("Some data 2");
            var node2 = new DagNode(data2);
            var node1 = await ipfs.Object.PutAsync(data1,
                new[] { node2.ToLink("some-link") });
            var info = await ipfs.Object.StatAsync(node1.Id);
            Assert.AreEqual(1, info.LinkCount);
            Assert.AreEqual(64, info.BlockSize);
            Assert.AreEqual(53, info.LinkSize);
            Assert.AreEqual(11, info.DataSize);
            Assert.AreEqual(77, info.CumulativeSize);
        }

        [UnityTest]
        public System.Collections.IEnumerator Async_Get_Nonexistent()
        {
            yield return Unity.Asyncs.Async2Coroutine(Get_Nonexistent());
        }

        public async Task Get_Nonexistent()
        {
            var data = Encoding.UTF8.GetBytes("Some data for net-ipfs-http-client-test that cannot be found");
            var node = new DagNode(data);
            var id = node.Id;
            var cs = new CancellationTokenSource(500);
            try
            {
                var _ = await ipfs.Object.GetAsync(id, cs.Token);
                Assert.Fail("Did not throw TaskCanceledException");
            }
            catch (TaskCanceledException)
            {
                return;
            }
        }

    }
}
