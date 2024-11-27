using Ipfs.CoreApi;
using NUnit.Framework;
using UnityEngine.TestTools;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ipfs.Http
{
    [TestFixture]
    public class FileSystemApiTest : TestFixture
    {
        [UnityTest]
        public System.Collections.IEnumerator Async_AddText()
        {
            yield return Unity.Asyncs.Async2Coroutine(AddText());
        }

        public async Task AddText()
        {
            var ipfs = TestFixture.Ipfs;
            var result = await ipfs.FileSystem.AddTextAsync("hello world");
            Assert.AreEqual("Qmf412jQZiuVUtdgnB36FXFX7xg5V6KEbSJ4dpQuhkLyfD", (string)result.Id);
        }

        [UnityTest]
        public System.Collections.IEnumerator Async_ReadText()
        {
            yield return Unity.Asyncs.Async2Coroutine(ReadText());
        }

        public async Task ReadText()
        {
            var ipfs = TestFixture.Ipfs;
            var node = await ipfs.FileSystem.AddTextAsync("hello world");
            var text = await ipfs.FileSystem.ReadAllTextAsync(node.Id);
            Assert.AreEqual("hello world", text);
        }

        [UnityTest]
        public System.Collections.IEnumerator Async_AddFile()
        {
            yield return Unity.Asyncs.Async2Coroutine(AddFile());
        }

        public async Task AddFile()
        {
            var path = Path.GetTempFileName();
            File.WriteAllText(path, "hello world");
            try
            {
                var ipfs = TestFixture.Ipfs;
                var result = await ipfs.FileSystem.AddFileAsync(path);
                Assert.AreEqual("Qmf412jQZiuVUtdgnB36FXFX7xg5V6KEbSJ4dpQuhkLyfD", (string)result.Id);
                Assert.AreEqual(0, result.Links.Count());
            }
            finally
            {
                File.Delete(path);
            }
        }

        [UnityTest]
        public System.Collections.IEnumerator Async_AddFile_Large()
        {
            yield return Unity.Asyncs.Async2Coroutine(AddFile_Large());
        }

        public async Task AddFile_Large()
        {
            var path = "Packages/com.willneedit.ipfs.iface/Tests/halloween_pumpkin.glb";

            try
            {
                var ipfs = TestFixture.Ipfs;
                var result = await ipfs.FileSystem.AddFileAsync(path);
                Assert.AreEqual("QmZLCE2E2c5WmG2bkeUPEypcqcp8xUpxSMzK5mgjUKSg4p", (string)result.Id);
                Assert.AreEqual(0, result.Links.Count());
            }
            finally
            {
                // Nothing to clean up
            }
        }

        [UnityTest]
        public System.Collections.IEnumerator Async_Read_With_Offset()
        {
            yield return Unity.Asyncs.Async2Coroutine(Read_With_Offset());
        }

        public async Task Read_With_Offset()
        {
            var ipfs = TestFixture.Ipfs;
            var indata = new MemoryStream(new byte[] { 10, 20, 30 });
            var node = await ipfs.FileSystem.AddAsync(indata);
            using (var outdata = ipfs.FileSystem.ReadFileAsync(node.Id, offset: 1).Result)
            {
                Assert.AreEqual(20, outdata.ReadByte());
                Assert.AreEqual(30, outdata.ReadByte());
                Assert.AreEqual(-1, outdata.ReadByte());
            }
        }

        [UnityTest]
        public System.Collections.IEnumerator Async_Read_With_Offset_Length_1()
        {
            yield return Unity.Asyncs.Async2Coroutine(Read_With_Offset_Length_1());
        }

        public async Task Read_With_Offset_Length_1()
        {
            var ipfs = TestFixture.Ipfs;
            var indata = new MemoryStream(new byte[] { 10, 20, 30 });
            var node = await ipfs.FileSystem.AddAsync(indata);
            using (var outdata = ipfs.FileSystem.ReadFileAsync(node.Id, offset: 1, count: 1).Result)
            {
                Assert.AreEqual(20, outdata.ReadByte());
                Assert.AreEqual(-1, outdata.ReadByte());
            }
        }

        [UnityTest]
        public System.Collections.IEnumerator Async_Read_With_Offset_Length_2()
        {
            yield return Unity.Asyncs.Async2Coroutine(Read_With_Offset_Length_2());
        }

        public async Task Read_With_Offset_Length_2()
        {
            var ipfs = TestFixture.Ipfs;
            var indata = new MemoryStream(new byte[] { 10, 20, 30 });
            var node = await ipfs.FileSystem.AddAsync(indata);
            using (var outdata = ipfs.FileSystem.ReadFileAsync(node.Id, offset: 1, count: 2).Result)
            {
                Assert.AreEqual(20, outdata.ReadByte());
                Assert.AreEqual(30, outdata.ReadByte());
                Assert.AreEqual(-1, outdata.ReadByte());
            }
        }

        [UnityTest]
        public System.Collections.IEnumerator Async_Add_NoPin()
        {
            yield return Unity.Asyncs.Async2Coroutine(Add_NoPin());
        }

        public async Task Add_NoPin()
        {
            var ipfs = TestFixture.Ipfs;
            var data = new MemoryStream(new byte[] { 11, 22, 33 });
            var options = new AddFileOptions { Pin = false };
            var node = await ipfs.FileSystem.AddAsync(data, "", options);
            var pins = await ipfs.Pin.ListAsync();
            Assert.IsFalse(pins.Any(pin => pin == node.Id));
        }

        [UnityTest]
        public System.Collections.IEnumerator Async_Add_Wrap()
        {
            yield return Unity.Asyncs.Async2Coroutine(Add_Wrap());
        }

        public async Task Add_Wrap()
        {
            var path = "hello.txt";
            File.WriteAllText(path, "hello world");
            try
            {
                var ipfs = TestFixture.Ipfs;
                var options = new AddFileOptions
                {
                    Wrap = true
                };
                var node = await ipfs.FileSystem.AddFileAsync(path, options);
                Assert.AreEqual("QmNxvA5bwvPGgMXbmtyhxA1cKFdvQXnsGnZLCGor3AzYxJ", (string)node.Id);
                Assert.AreEqual(true, node.IsDirectory);
                Assert.IsNotNull(node.Links);
                Assert.AreEqual(1, node.Links.Count());
                Assert.AreEqual("hello.txt", node.Links.First().Name);
                Assert.AreEqual("Qmf412jQZiuVUtdgnB36FXFX7xg5V6KEbSJ4dpQuhkLyfD", (string)node.Links.First().Id);
            }
            finally
            {
                File.Delete(path);
            }
        }

        [UnityTest]
        public System.Collections.IEnumerator Async_Add_SizeChunking()
        {
            yield return Unity.Asyncs.Async2Coroutine(Add_SizeChunking());
        }

        public async Task Add_SizeChunking()
        {
            var ipfs = TestFixture.Ipfs;
            var options = new AddFileOptions
            {
                ChunkSize = 3
            };
            options.Pin = true;
            var node = await ipfs.FileSystem.AddTextAsync("hello world", options);
            Assert.AreEqual("QmVVZXWrYzATQdsKWM4knbuH5dgHFmrRqW3nJfDgdWrBjn", (string)node.Id);
            Assert.AreEqual(false, node.IsDirectory);

            var links = (await ipfs.Object.LinksAsync(node.Id)).ToArray();
            Assert.AreEqual(4, links.Length);
            Assert.AreEqual("QmevnC4UDUWzJYAQtUSQw4ekUdqDqwcKothjcobE7byeb6", (string)links[0].Id);
            Assert.AreEqual("QmTdBogNFkzUTSnEBQkWzJfQoiWbckLrTFVDHFRKFf6dcN", (string)links[1].Id);
            Assert.AreEqual("QmPdmF1n4di6UwsLgW96qtTXUsPkCLN4LycjEUdH9977d6", (string)links[2].Id);
            Assert.AreEqual("QmXh5UucsqF8XXM8UYQK9fHXsthSEfi78kewr8ttpPaLRE", (string)links[3].Id);

            var text = await ipfs.FileSystem.ReadAllTextAsync(node.Id);
            Assert.AreEqual("hello world", text);
        }

        [UnityTest]
        public System.Collections.IEnumerator Async_Add_Raw()
        {
            yield return Unity.Asyncs.Async2Coroutine(Add_Raw());
        }

        public async Task Add_Raw()
        {
            var ipfs = TestFixture.Ipfs;
            var options = new AddFileOptions
            {
                RawLeaves = true
            };
            var node = await ipfs.FileSystem.AddTextAsync("hello world", options);
            Assert.AreEqual("bafkreifzjut3te2nhyekklss27nh3k72ysco7y32koao5eei66wof36n5e", (string)node.Id);
            Assert.AreEqual(11, node.Size);

            var text = await ipfs.FileSystem.ReadAllTextAsync(node.Id);
            Assert.AreEqual("hello world", text);
        }

        [UnityTest]
        public System.Collections.IEnumerator Async_Add_RawAndChunked()
        {
            yield return Unity.Asyncs.Async2Coroutine(Add_RawAndChunked());
        }

        public async Task Add_RawAndChunked()
        {
            var ipfs = TestFixture.Ipfs;
            var options = new AddFileOptions
            {
                RawLeaves = true,
                ChunkSize = 3
            };
            var node = await ipfs.FileSystem.AddTextAsync("hello world", options);
            Assert.AreEqual("QmUuooB6zEhMmMaBvMhsMaUzar5gs5KwtVSFqG4C1Qhyhs", (string)node.Id);
            Assert.AreEqual(false, node.IsDirectory);

            var links = (await ipfs.Object.LinksAsync(node.Id)).ToArray();
            Assert.AreEqual(4, links.Length);
            Assert.AreEqual("bafkreigwvapses57f56cfow5xvoua4yowigpwcz5otqqzk3bpcbbjswowe", (string)links[0].Id);
            Assert.AreEqual("bafkreiew3cvfrp2ijn4qokcp5fqtoknnmr6azhzxovn6b3ruguhoubkm54", (string)links[1].Id);
            Assert.AreEqual("bafkreibsybcn72tquh2l5zpim2bba4d2kfwcbpzuspdyv2breaq5efo7tq", (string)links[2].Id);
            Assert.AreEqual("bafkreihfuch72plvbhdg46lef3n5zwhnrcjgtjywjryyv7ffieyedccchu", (string)links[3].Id);

            var text = await ipfs.FileSystem.ReadAllTextAsync(node.Id);
            Assert.AreEqual("hello world", text);
        }

        [UnityTest]
        public System.Collections.IEnumerator Async_AddDirectory()
        {
            yield return Unity.Asyncs.Async2Coroutine(AddDirectory());
        }

        public async Task AddDirectory()
        {
            var ipfs = TestFixture.Ipfs;
            var temp = MakeTemp();
            try
            {
                var dir = await ipfs.FileSystem.AddDirectoryAsync(temp, false);
                Assert.IsTrue(dir.IsDirectory);

                var files = dir.Links.ToArray();
                Assert.AreEqual(2, files.Length);
                Assert.AreEqual("alpha.txt", files[0].Name);
                Assert.AreEqual("beta.txt", files[1].Name);

                byte[] file0DAG = await ipfs.Block.GetAsync(files[0].Id);
                Assert.AreEqual(file0DAG.LongLength, files[0].Size);

                byte[] file1DAG = await ipfs.Block.GetAsync(files[1].Id);
                Assert.AreEqual(file1DAG.LongLength, files[1].Size);

                Assert.AreEqual("alpha", ipfs.FileSystem.ReadAllTextAsync(files[0].Id).Result);
                Assert.AreEqual("beta", ipfs.FileSystem.ReadAllTextAsync(files[1].Id).Result);

                Assert.AreEqual("alpha", ipfs.FileSystem.ReadAllTextAsync(dir.Id + "/alpha.txt").Result);
                Assert.AreEqual("beta", ipfs.FileSystem.ReadAllTextAsync(dir.Id + "/beta.txt").Result);

                byte[] rawDAG = await ipfs.Block.GetAsync(dir.Id);
                long cumulativeSize = files[0].Size + files[1].Size + rawDAG.LongLength;
                Assert.IsNotNull(rawDAG);
                Assert.AreEqual(cumulativeSize, dir.Size);

                var recoveredDir = await ipfs.FileSystem.ListAsync(dir.Id);
                var recoveredFiles = recoveredDir.Links.ToArray();
                Assert.AreEqual(2, recoveredFiles.Length);
                Assert.AreEqual("alpha.txt", recoveredFiles[0].Name);
                Assert.AreEqual("beta.txt", recoveredFiles[1].Name);

                // kubo doesn't yield the cumulative size, only in the submit.
            }
            finally
            {
                DeleteTemp(temp);
            }
        }

        [UnityTest]
        public System.Collections.IEnumerator Async_AddDirectoryRecursive()
        {
            yield return Unity.Asyncs.Async2Coroutine(AddDirectoryRecursive());
        }

        public async Task AddDirectoryRecursive()
        {
            var ipfs = TestFixture.Ipfs;
            var temp = MakeTemp();
            try
            {
                var dir = await ipfs.FileSystem.AddDirectoryAsync(temp, true);
                Assert.IsTrue(dir.IsDirectory);

                var files = dir.Links.ToArray();
                Assert.AreEqual(3, files.Length);
                Assert.AreEqual("alpha.txt", files[0].Name);
                Assert.AreEqual("beta.txt", files[1].Name);
                Assert.AreEqual("x", files[2].Name);
                Assert.AreNotEqual(0, files[0].Size);
                Assert.AreNotEqual(0, files[1].Size);

                // Sure it cannot work. We have to use ListAsync() to retrieve the directory
                // structure, not by using a half-finished FSN.
#if false
                var xfiles = new FileSystemNode
                {
                    Id = files[2].Id,
                }.Links.ToArray();
                Assert.AreEqual(2, xfiles.Length);
                Assert.AreEqual("x.txt", xfiles[0].Name);
                Assert.AreEqual("y", xfiles[1].Name);

                var yfiles = new FileSystemNode
                {
                    Id = xfiles[1].Id,
                }.Links.ToArray();
                Assert.AreEqual(1, yfiles.Length);
                Assert.AreEqual("y.txt", yfiles[0].Name);

                var y = new FileSystemNode
                {
                    Id = yfiles[0].Id,
                };
#endif
                Assert.AreEqual("y", ipfs.FileSystem.ReadAllTextAsync(dir.Id + "/x/y/y.txt").Result);
            }
            finally
            {
                DeleteTemp(temp);
            }
        }

        [UnityTest]
        public System.Collections.IEnumerator Async_GetTar_EmptyDirectory()
        {
            yield return Unity.Asyncs.Async2Coroutine(GetTar_EmptyDirectory());
        }

        public async Task GetTar_EmptyDirectory()
        {
            var ipfs = TestFixture.Ipfs;
            var temp = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(temp);
            try
            {
                var dir = await ipfs.FileSystem.AddDirectoryAsync(temp, true);
                var dirid = dir.Id.Encode();

                using (var tar = await ipfs.FileSystem.GetAsync(dir.Id))
                {
                    var buffer = new byte[3 * 512];
                    var offset = 0;
                    while (offset < buffer.Length)
                    {
                        var n = await tar.ReadAsync(buffer, offset, buffer.Length - offset);
                        Assert.IsTrue(n > 0);
                        offset += n;
                    }
                    Assert.AreEqual(-1, tar.ReadByte());
                }
            }
            finally
            {
                DeleteTemp(temp);
            }
        }


        [UnityTest]
        public System.Collections.IEnumerator Async_AddFile_WithProgress()
        {
            yield return Unity.Asyncs.Async2Coroutine(AddFile_WithProgress());
        }

        public async Task AddFile_WithProgress()
        {
            var path = Path.GetTempFileName();
            File.WriteAllText(path, "hello world");
            try
            {
                var ipfs = TestFixture.Ipfs;
                var bytesTransferred = 0UL;
                var options = new AddFileOptions
                {
                    Progress = new Progress<TransferProgress>(t =>
                    {
                        bytesTransferred += t.Bytes;
                    })
                };
                var result = await ipfs.FileSystem.AddFileAsync(path, options);
                Assert.AreEqual("Qmf412jQZiuVUtdgnB36FXFX7xg5V6KEbSJ4dpQuhkLyfD", (string)result.Id);

                // Progress reports get posted on another synchronisation context.
                var stop = DateTime.Now.AddSeconds(3);
                while (DateTime.Now < stop)
                {
                    if (bytesTransferred == 11UL)
                        break;
                    await Task.Delay(10);
                }
                Assert.AreEqual(11UL, bytesTransferred);
            }
            finally
            {
                File.Delete(path);
            }
        }

        void DeleteTemp(string temp)
        {
            while (true)
            {
                try
                {
                    Directory.Delete(temp, true);
                    break;
                }
                catch (Exception)
                {
                    Thread.Sleep(1);
                    continue;  // most likely anti-virus is reading a file
                }
            }
        }

        string MakeTemp()
        {
            var temp = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            var x = Path.Combine(temp, "x");
            var xy = Path.Combine(x, "y");
            Directory.CreateDirectory(temp);
            Directory.CreateDirectory(x);
            Directory.CreateDirectory(xy);

            File.WriteAllText(Path.Combine(temp, "alpha.txt"), "alpha");
            File.WriteAllText(Path.Combine(temp, "beta.txt"), "beta");
            File.WriteAllText(Path.Combine(x, "x.txt"), "x");
            File.WriteAllText(Path.Combine(xy, "y.txt"), "y");
            return temp;
        }
    }
}
