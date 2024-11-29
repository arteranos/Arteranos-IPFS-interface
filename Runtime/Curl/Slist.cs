using System;
using System.Collections;
using System.Runtime.InteropServices;
// using Debug = UnityEngine.Debug;

namespace Curly
{
    public class Slist : ManagedHandle<IntPtr>, IEnumerable
    {
        private const string LIBCURL = "libcurl";
        internal static class Native
        {
            [DllImport(LIBCURL, EntryPoint = "curl_slist_append", CharSet = CharSet.Ansi)]
            public static extern IntPtr Append(IntPtr slist, [MarshalAs(UnmanagedType.LPStr)] string data);

            [DllImport(LIBCURL, EntryPoint = "curl_slist_free_all")]
            public static extern void FreeAll(IntPtr pList);
        }

        public Slist() : base(
            () => IntPtr.Zero,
            r =>
            {
                if (r != IntPtr.Zero) Native.FreeAll(r);
            })
        { }

        public void Add(string data) => Attach(Native.Append(this, data), false);

        public IEnumerator GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}