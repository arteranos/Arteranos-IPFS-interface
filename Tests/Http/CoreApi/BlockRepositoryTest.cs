using NUnit.Framework;
using UnityEngine.TestTools;
using System.Threading.Tasks;

namespace Ipfs.Http
{
    [TestFixture]
    public class BlockRepositoryTest
    {
        [UnityTest]
        public System.Collections.IEnumerator Async_Stats()
        {
            yield return Unity.Asyncs.Async2Coroutine(Stats());
        }

        public async Task Stats()
        {
            var ipfs = TestFixture.Ipfs;
            var stats = await ipfs.BlockRepository.StatisticsAsync();
            Assert.IsNotNull(stats);
        }

        [UnityTest]
        public System.Collections.IEnumerator Async_Version()
        {
            yield return Unity.Asyncs.Async2Coroutine(Version());
        }

        public async Task Version()
        {
            var ipfs = TestFixture.Ipfs;
            var version = await ipfs.BlockRepository.VersionAsync();
            Assert.IsFalse(string.IsNullOrWhiteSpace(version));
        }

    }
}
