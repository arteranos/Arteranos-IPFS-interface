using System;
using System.Collections.Generic;
using System.IO;

namespace CurlThinExtractor
{
    internal class Program
    {
        static int Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Need target directory name");
                return 100;
            }

            string targetDir = args[0];
            string arch = args[1];



            List<string> files = new List<string>();
            files.Add("libcurl.dll");

            if(arch == "x86")
            {
                files.Add("libssl-1_1.dll");
                files.Add("libcrypto-1_1.dll");
            }
            else if(arch == "x64")
            {
                files.Add("libssl-1_1-x64.dll");
                files.Add("libcrypto-1_1-x64.dll");
            }
            else
            {
                Console.WriteLine("Unsupported architecture");
                return 101;
            }

            CurlThin.Native.CurlResources.Init();

            try
            {
                Directory.CreateDirectory(targetDir);
            }
            catch { }

            foreach (string file in files)
            {
                try
                {
                    File.Delete($"{targetDir}\\{file}");
                }
                catch { }
            }

            foreach (string file in files)
            {
                File.Copy(file, $"{targetDir}\\{file}");
                Console.WriteLine($"{file} copied");
            }

            Console.WriteLine($"Extractor completed");
            return 0;
        }
    }
}
