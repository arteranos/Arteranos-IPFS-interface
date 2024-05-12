using NUnit.Framework;
using UnityEngine.TestTools;
using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;

namespace Ipfs.Http
{
    [TestFixture]
    public class ConfigApiTest
    {
        private const string apiAddress = "/ip4/127.0.0.1/tcp/";
        private const string gatewayAddress = "/ip4/127.0.0.1/tcp/";

        [Test]
        public void Get_Entire_Config()
        {
            IpfsClient ipfs = TestFixture.Ipfs;
            var config = ipfs.Config.GetAsync().Result;
            StringAssert.StartsWith(apiAddress, config["Addresses"]["API"].Value<string>());
        }

        [Test]
        public void Get_Scalar_Key_Value()
        {
            IpfsClient ipfs = TestFixture.Ipfs;
            var api = ipfs.Config.GetAsync("Addresses.API").Result;
            StringAssert.StartsWith(apiAddress, api.Value<string>());
        }

        [Test]
        public void Get_Object_Key_Value()
        {
            IpfsClient ipfs = TestFixture.Ipfs;
            var addresses = ipfs.Config.GetAsync("Addresses").Result;
            StringAssert.StartsWith(apiAddress, addresses["API"].Value<string>());
            StringAssert.StartsWith(gatewayAddress, addresses["Gateway"].Value<string>());
        }

        [Test]
        public void Keys_are_Case_Sensitive()
        {
            IpfsClient ipfs = TestFixture.Ipfs;
            var api = ipfs.Config.GetAsync("Addresses.API").Result;
            StringAssert.StartsWith(apiAddress, api.Value<string>());

            ExceptionAssert.Throws<Exception>(() => { var x = ipfs.Config.GetAsync("Addresses.api").Result; });
        }

        [Test]
        public void Set_String_Value()
        {
            const string key = "foo";
            const string value = "foobar";
            IpfsClient ipfs = TestFixture.Ipfs;
            ipfs.Config.SetAsync(key, value).Wait();
            Assert.AreEqual(value, ((string) ipfs.Config.GetAsync(key).Result));
        }

        [Test]
        public void Set_JSON_Value()
        {
            const string key = "API.HTTPHeaders.Access-Control-Allow-Origin";
            JToken value = JToken.Parse("['http://example.io']");
            IpfsClient ipfs = TestFixture.Ipfs;
            ipfs.Config.SetAsync(key, value).Wait();
            Assert.AreEqual("http://example.io", ((string) ipfs.Config.GetAsync(key).Result[0]));
        }

        [UnityTest]
        public System.Collections.IEnumerator Async_Replace_Entire_Config()
        {
            yield return Unity.Asyncs.Async2Coroutine(Replace_Entire_Config());
        }

        public async Task Replace_Entire_Config()
        {
            IpfsClient ipfs = TestFixture.Ipfs;
            var original = await ipfs.Config.GetAsync();
            try
            {
                var a = JObject.Parse("{ \"foo-x-bar\": 1 }");
                await ipfs.Config.ReplaceAsync(a);
            }
            finally
            {
                await ipfs.Config.ReplaceAsync(original);
            }
        }

    }
}
