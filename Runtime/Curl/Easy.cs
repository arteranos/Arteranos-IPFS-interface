using System;
using System.Runtime.InteropServices;
// using Debug = UnityEngine.Debug;

namespace Curly
{
    public class Easy : ManagedHandle<IntPtr>
    {
        public delegate UIntPtr DataHandler(IntPtr data, UIntPtr size, UIntPtr nmemb, IntPtr userdata);

        private const string LIBCURL = "libcurl";
        internal static class Native
        {
            [DllImport(LIBCURL, EntryPoint = "curl_easy_init")]
            public static extern IntPtr Init();

            [DllImport(LIBCURL, EntryPoint = "curl_easy_cleanup")]
            public static extern void Cleanup(IntPtr handle);

            [DllImport(LIBCURL, EntryPoint = "curl_easy_perform")]
            public static extern CURLcode Perform(IntPtr handle);

            [DllImport(LIBCURL, EntryPoint = "curl_easy_reset")]
            public static extern void Reset(IntPtr handle);

            [DllImport(LIBCURL, EntryPoint = "curl_easy_setopt")]
            public static extern CURLcode SetOpt(IntPtr handle, CURLoption option, int value);

            [DllImport(LIBCURL, EntryPoint = "curl_easy_setopt")]
            public static extern CURLcode SetOpt(IntPtr handle, CURLoption option, IntPtr value);

            [DllImport(LIBCURL, EntryPoint = "curl_easy_setopt", CharSet = CharSet.Ansi)]
            public static extern CURLcode SetOpt(IntPtr handle, CURLoption option, [MarshalAs(UnmanagedType.LPStr)] string value);

            [DllImport(LIBCURL, EntryPoint = "curl_easy_setopt")]
            public static extern CURLcode SetOpt(IntPtr handle, CURLoption option, DataHandler value);

            [DllImport(LIBCURL, EntryPoint = "curl_easy_setopt")]
            public static extern CURLcode SetOpt(IntPtr handle, CURLoption option, byte[] value);

            [DllImport(LIBCURL, EntryPoint = "curl_easy_getinfo")]
            public static extern CURLcode GetInfo(IntPtr handle, CURLINFO option, out int value);

            [DllImport(LIBCURL, EntryPoint = "curl_easy_getinfo")]
            public static extern CURLcode GetInfo(IntPtr handle, CURLINFO option, out IntPtr value);

            [DllImport(LIBCURL, EntryPoint = "curl_easy_getinfo")]
            public static extern CURLcode GetInfo(IntPtr handle, CURLINFO option, out double value);

            [DllImport(LIBCURL, EntryPoint = "curl_easy_getinfo", CharSet = CharSet.Ansi)]
            public static extern CURLcode GetInfo(IntPtr handle, CURLINFO option, IntPtr value);
        }
        internal Easy() : base(
            () =>
            {
                IntPtr res = Native.Init();
                // Debug.Log($"Acquired easy handle {res:X}");
                return res;
            },
            r =>
            {
                Native.Cleanup(r);
                // Debug.Log($"Released easy handle {r:X}");
            })
        { }

        public CURLcode Perform() => Native.Perform(this);
        public void Reset() => Native.Reset(this);
        public CURLcode SetOpt(CURLoption option, int value) => Native.SetOpt(this, option, value);
        public CURLcode SetOpt(CURLoption option, IntPtr value) => Native.SetOpt(this, option, value);
        public CURLcode SetOpt(CURLoption option, DataHandler value) => Native.SetOpt(this, option, value);
        public CURLcode SetOpt(CURLoption option, string value) => Native.SetOpt(this, option, value);
        public CURLcode SetOpt(CURLoption option, byte[] value) => Native.SetOpt(this, option, value);
        public CURLcode GetInfo(CURLINFO option, out int value) => Native.GetInfo(this, option, out value);
        public CURLcode GetInfo(CURLINFO option, out IntPtr value) => Native.GetInfo(this, option, out value);
        public CURLcode GetInfo(CURLINFO option, out double value) => Native.GetInfo(this, option, out value);
        public CURLcode GetInfo(CURLINFO option, out string value)
        {
            CURLcode result = Native.GetInfo(this, option, out IntPtr strPtr);
            value = (result == CURLcode.OK)
                ? Marshal.PtrToStringAnsi(strPtr)
                : null;
            return result;
        }
    }
}