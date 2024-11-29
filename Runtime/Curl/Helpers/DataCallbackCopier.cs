using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Curly
{
    public class DataCallbackCopier : IDisposable
    {
        public DataCallbackCopier(Stream stream = null)
        {
            Stream = stream ?? new MemoryStream();
            DataHandler = DataImporter(Stream);
        }

        private static Easy.DataHandler DataImporter(Stream Stream)
        {
            return (data, size, nmemb, userdata) =>
            {
                int length = (int)size * (int)nmemb;
                byte[] buffer = new byte[length];
                Marshal.Copy(data, buffer, 0, length);
                Stream.Write(buffer, 0, length);
                return (UIntPtr)length;
            };
        }

        public Stream Stream { get; }
        public Easy.DataHandler DataHandler { get; }

        public void Dispose()
        {
            Stream?.Dispose();
        }

        public void Reset()
        {
            Stream.Position = 0;
            Stream.SetLength(0);
        }

        public string ReadAsString()
        {
            Stream.Seek(0, SeekOrigin.Begin);
            StreamReader reader = new(Stream);
            return reader.ReadToEnd();
        }
    }

    public class DataCallbackFunc : IDisposable
    {
        public delegate int DataFunc(byte[] buffer);
        public DataCallbackFunc(DataFunc func)
        {
            this.func = func;
            DataHandler = DataImporter();
        }

        private Easy.DataHandler DataImporter()
        {
            return (data, size, nmemb, userdata) =>
            {
                int length = (int)size * (int)nmemb;
                byte[] buffer = new byte[length];
                Marshal.Copy(data, buffer, 0, length);
                return (UIntPtr)func?.Invoke(buffer);
            };
        }

        private DataFunc func = null;

        public Easy.DataHandler DataHandler { get; }

        public void Dispose()
        {
            func = null;
        }
    }

}