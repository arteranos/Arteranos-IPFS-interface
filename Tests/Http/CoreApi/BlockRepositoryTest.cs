using NUnit.Framework;
using UnityEngine.TestTools;
using System.Threading.Tasks;

namespace Ipfs.Http
{
    [TestFixture]
    public class BlockRepositoryTest
    {
        [Test]
        public async Task Stats()
        {
            var ipfs = TestFixture.Ipfs;
            var stats = await ipfs.BlockRepository.StatisticsAsync();
            Assert.IsNotNull(stats);
        }

        [Test]
        public async Task Version()
        {
            var ipfs = TestFixture.Ipfs;
            var version = await ipfs.BlockRepository.VersionAsync();
            Assert.IsFalse(string.IsNullOrWhiteSpace(version));
        }

    }
}
