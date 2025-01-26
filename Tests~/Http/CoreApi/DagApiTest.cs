using NUnit.Framework;
using UnityEngine.TestTools;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Linq;
using System.Collections.Generic;
using Ipfs.CoreApi;
using System.IO;
using System.Text;
using System.Threading;

namespace Ipfs.Http
{
    [TestFixture]
    public class DagApiTest : TestFixture
    {
        class Name
        {
            public string First { get; set; }

            public string Last { get; set; }
        }

// DANGEROUS - Pinned Dag Node with dangling (recursive) links causes pin operations to hang.
#if false
        [UnityTest]
        public System.Collections.IEnumerator Async_TwoLinkData()
        {
            yield return Unity.Asyncs.Async2Coroutine(TwoLinkData());
        }

        public async Task TwoLinkData()
        {
            var ipfs = TestFixture.Ipfs;
            var expected = JObject.Parse(@"{""Data"":{""/"":{""bytes"":""c29tZSBkYXRh""}},""Links"":[{""Hash"":{""/"":""QmXg9Pp2ytZ14xgmQjYEiHjVjMFXzCVVEcRTWJBmLgR39U""},""Name"":""some link"",""Tsize"":100000000},{""Hash"":{""/"":""QmXg9Pp2ytZ14xgmQjYEiHjVjMFXzCVVEcRTWJBmLgR39V""},""Name"":""some other link"",""Tsize"":8}]}");
            var expectedId = "bafyreia4kjmr364wv7snvuffjjfx6e3ssyhcaxcv3mmewrm6lkg426ycpu";
            var link1expectedId = "QmXg9Pp2ytZ14xgmQjYEiHjVjMFXzCVVEcRTWJBmLgR39U";
            var id = await ipfs.Dag.PutAsync(expected);
            Assert.IsNotNull(id);
            Assert.AreEqual(expectedId, (string)id);

            var actual = await ipfs.Dag.GetAsync(id);
            Assert.IsNotNull(actual);
            UnityEngine.Debug.Log(actual);

            var link1 = await ipfs.Dag.GetAsync(expectedId + "/Links");
            Assert.AreEqual(link1expectedId, ((string) link1[0]["Hash"]["/"]));

        }
#endif

        [UnityTest]
        public System.Collections.IEnumerator Async_KnownGood()
        {
            yield return Unity.Asyncs.Async2Coroutine(KnownGood);
        }

        public async Task KnownGood()
        {
            var ipfs = TestFixture.Ipfs;
            var knownGoodId = "QmS4ustL54uo8FzR9455qaxZwuMiUhyvMcX9Ba8nUH4uVv";

            var actual = await ipfs.Dag.GetAsync(knownGoodId);
            Assert.IsNotNull(actual);
            UnityEngine.Debug.Log(actual.ToString());

            var link1 = await ipfs.Dag.GetAsync(knownGoodId + "/about");
            UnityEngine.Debug.Log(link1);
        }



        [UnityTest]
        public System.Collections.IEnumerator Async_PutAndGet_JSON()
        {
            yield return Unity.Asyncs.Async2Coroutine(PutAndGet_JSON);
        }

        public async Task PutAndGet_JSON()
        {
            var ipfs = TestFixture.Ipfs;
            var expected = new JObject();
            expected["a"] = "alpha";
            var expectedId = "bafyreigdhej736dobd6z3jt2vxsxvbwrwgyts7e7wms6yrr46rp72uh5bu";
            var id = await ipfs.Dag.PutAsync(expected);
            Assert.IsNotNull(id);
            Assert.AreEqual(expectedId, (string)id);

            var actual = await ipfs.Dag.GetAsync(id);
            Assert.IsNotNull(actual);
            Assert.AreEqual(expected["a"], actual["a"]);

            var value = (string)await ipfs.Dag.GetAsync(expectedId + "/a");
            Assert.AreEqual((string) expected["a"], value);
        }

        [UnityTest]
        public System.Collections.IEnumerator Async_CreateDir()
        {
            yield return Unity.Asyncs.Async2Coroutine(CreateDir);
        }

        public async Task CreateDir()
        {
            Task<IFileSystemNode> AddNamedTextAsync(string text, string name = "", AddFileOptions options = null, CancellationToken cancel = default)
            {
                var ipfs = TestFixture.Ipfs;
                return ipfs.FileSystem.AddAsync(new MemoryStream(Encoding.UTF8.GetBytes(text), false), name, options, cancel);
            }

            void AddFileLink(List<JToken> linkList, IFileSystemNode node)
            {
                JToken newLink = JToken.Parse(@"{ ""Hash"": { ""/"": """" }, ""Name"": """", ""Tsize"": 0 }");
                newLink["Hash"]["/"] = node.Id.ToString();
                newLink["Name"] = node.Name;
                newLink["Tsize"] = node.Size;

                linkList.Add(newLink);
            }

            var ipfs = TestFixture.Ipfs;
            IFileSystemNode file1 = await AddNamedTextAsync("hello world", "hello.txt");
            IFileSystemNode file2 = await AddNamedTextAsync("goodbye world", "goodbye.txt");

            JToken emptyDirNode = JToken.Parse(@"{ ""Data"": { ""/"": { ""bytes"": ""CAE"" } }, ""Links"": [] }");

            JToken dir = emptyDirNode.DeepClone();

            List<JToken> links = new();
            AddFileLink(links, file1);
            AddFileLink(links, file2);
            dir["Links"] = JToken.FromObject(links);

            var id = await ipfs.Dag.PutAsync(dir, "dag-pb");
            Assert.IsNotNull(id);

            UnityEngine.Debug.Log(id);

            var text = await ipfs.FileSystem.ReadAllTextAsync(id + "/hello.txt");
            Assert.AreEqual("hello world", text);

            text = await ipfs.FileSystem.ReadAllTextAsync(id + "/goodbye.txt");
            Assert.AreEqual("goodbye world", text);
        }

        [UnityTest]
        public System.Collections.IEnumerator Async_PutAndGet_POCO()
        {
            yield return Unity.Asyncs.Async2Coroutine(PutAndGet_POCO);
        }

        public async Task PutAndGet_POCO()
        {
            var ipfs = TestFixture.Ipfs;
            var expected = new Name { First = "John", Last = "Smith" };
            var id = await ipfs.Dag.PutAsync(expected);
            Assert.IsNotNull(id);

            var actual = await ipfs.Dag.GetAsync<Name>(id);
            Assert.IsNotNull(actual);
            Assert.AreEqual(expected.First, actual.First);
            Assert.AreEqual(expected.Last, actual.Last);

            var value = (string)await ipfs.Dag.GetAsync(id.Encode() + "/Last");
            Assert.AreEqual(expected.Last, value);
        }
    }
}

