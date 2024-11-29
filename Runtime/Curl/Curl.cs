using System;
using System.Runtime.InteropServices;
// using Debug = UnityEngine.Debug;

namespace Curly
{
    public class Curl : IDisposable
    {
        private readonly CURLcode result = CURLcode.FAILED_INIT;

        internal static class Native
        {
            private const string LIBCURL = "libcurl";

            [DllImport(LIBCURL, EntryPoint = "curl_global_init")]
            public static extern CURLcode Init(CURLglobal flags = CURLglobal.DEFAULT);

            [DllImport(LIBCURL, EntryPoint = "curl_global_cleanup")]
            public static extern void Cleanup();

            [DllImport(LIBCURL, EntryPoint = "curl_easy_strerror", CharSet = CharSet.Ansi)]
            public static extern IntPtr StrError(CURLcode errornum);
        }

        public Curl()
        {
            result = Native.Init();
            // Debug.Log($"Acquired Curl, result={result}");
        }

        ~Curl()
        {
            Dispose(false);
        }

        public Easy GetEasy() => new();

        public static string StrError(CURLcode errno)
        {
            // [return: MarshalAs(UnmanagedType.LPStr)] didn't work.
            IntPtr ptr = Native.StrError(errno);
            return Marshal.PtrToStringAnsi(ptr);
        }

        // ---------------------------------------------------------------
        public void Dispose()
        {
            GC.SuppressFinalize(this);
            Dispose(true);
        }

        private void Dispose(bool _ /* disposing */)
        {
            lock (this)
            {
                // if (disposing) /* cleanup managed resources */
                // cleanup unmanaged resources
                if (result == CURLcode.OK) Native.Cleanup();
                // Debug.Log("Released curl");
            }
        }
    }
}