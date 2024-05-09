using NUnit.Framework;
using UnityEngine.TestTools;
using System.Threading.Tasks;

namespace Ipfs.Http
{
    [TestFixture]
    public class StatsApiTest
    {

        [Test]
        public async Task SmokeTest()
        {
            var ipfs = TestFixture.Ipfs;
            var bandwidth = await ipfs.Stats.BandwidthAsync();
            var bitswap = await ipfs.Stats.BitswapAsync();
            var repository = await ipfs.Stats.RepositoryAsync();
        }
    }
}
