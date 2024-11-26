using NUnit.Framework;
using UnityEngine.TestTools;
using System.Threading.Tasks;

namespace Ipfs.Http
{
    [TestFixture]
    public class BlockRepositoryTest : TestFixture
    {
        [UnityTest]
        public System.Collections.IEnumerator Async_Stats()
        {
            yield return Unity.Asyncs.Async2Coroutine(Stats);
        }

        public async Task Stats()
        {
            var ipfs = TestFixture.Ipfs;
            CoreApi.RepositoryData stats = await ipfs.BlockRepository.StatisticsAsync();
            Assert.IsNotNull(stats);
            UnityEngine.Debug.Log($"Number of objects: {stats.NumObjects}");
            UnityEngine.Debug.Log($"Repo Size: {stats.RepoSize}");
            UnityEngine.Debug.Log($"Max Storage Size: {stats.StorageMax}");
            UnityEngine.Debug.Log($"Path: {stats.RepoPath}");
        }

        [UnityTest]
        public System.Collections.IEnumerator Async_Version()
        {
            yield return Unity.Asyncs.Async2Coroutine(Version);
        }

        public async Task Version()
        {
            var ipfs = TestFixture.Ipfs;
            var version = await ipfs.BlockRepository.VersionAsync();
            Assert.IsFalse(string.IsNullOrWhiteSpace(version));
        }

    }
}
