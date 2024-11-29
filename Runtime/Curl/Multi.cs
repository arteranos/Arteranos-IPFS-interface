using System;
using System.Runtime.InteropServices;
// using Debug = UnityEngine.Debug;

namespace Curly
{
    public class Multi : ManagedHandle<IntPtr>
    {
        public delegate int TimerCallback(IntPtr multiHandle, int timeoutMs, IntPtr userp);
        public delegate int SocketCallback(IntPtr easy, IntPtr s, CURLpoll what, IntPtr userp, IntPtr socketp);

        private const string LIBCURL = "libcurl";

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct CURLMsg
        {
            public CURLMSG msg; /* what this message means */
            public IntPtr easy_handle; /* the handle it concerns */
            public CURLMsgData data;

            [StructLayout(LayoutKind.Explicit)]
            public struct CURLMsgData
            {
                [FieldOffset(0)] public IntPtr whatever; /* (void*) message-specific data */
                [FieldOffset(0)] public CURLcode result; /* return code for transfer */
            }
        }

        internal static class Native
        {
            [DllImport(LIBCURL, EntryPoint = "curl_multi_init")]
            public static extern IntPtr Init();

            [DllImport(LIBCURL, EntryPoint = "curl_multi_cleanup")]
            public static extern CURLMcode Cleanup(IntPtr multiHandle);

            [DllImport(LIBCURL, EntryPoint = "curl_multi_add_handle")]
            public static extern CURLMcode AddHandle(IntPtr multiHandle, IntPtr easyHandle);

            [DllImport(LIBCURL, EntryPoint = "curl_multi_remove_handle")]
            public static extern CURLMcode RemoveHandle(IntPtr multiHandle, IntPtr easyHandle);

            [DllImport(LIBCURL, EntryPoint = "curl_multi_setopt")]
            public static extern CURLMcode SetOpt(IntPtr multiHandle, CURLMoption option, int value);

            [DllImport(LIBCURL, EntryPoint = "curl_multi_setopt")]
            public static extern CURLMcode SetOpt(IntPtr multiHandle, CURLMoption option, TimerCallback value);

            [DllImport(LIBCURL, EntryPoint = "curl_multi_setopt")]
            public static extern CURLMcode SetOpt(IntPtr multiHandle, CURLMoption option,
                SocketCallback value);

            [DllImport(LIBCURL, EntryPoint = "curl_multi_info_read")]
            public static extern IntPtr InfoRead(IntPtr multiHandle, out int msgsInQueue);

            [DllImport(LIBCURL, EntryPoint = "curl_multi_socket_action")]
            public static extern CURLMcode SocketAction(IntPtr multiHandle, SafeSocketHandle sockfd,
                CURLcselect evBitmask,
                out int runningHandles);

        }
        public Multi() : base(
            () =>
            {
                IntPtr res = Native.Init();
                // Debug.Log($"Acquired multi handle {res:X}");
                return res;
            },
            r =>
            {
                Native.Cleanup(r);
                // Debug.Log($"Released multi handle {r:X}");
            })
        { }

        public CURLMcode AddHandle(Easy easy) => Native.AddHandle(this, easy);
        public CURLMcode RemoveHandle(Easy easy) => Native.RemoveHandle(this, easy);
        public CURLMcode SetOpt(CURLMoption option, int value) => Native.SetOpt(this, option, value);
        public CURLMcode SetOpt(CURLMoption option, TimerCallback value) => Native.SetOpt(this, option, value);
        public CURLMcode SetOpt(CURLMoption option, SocketCallback value) => Native.SetOpt(this, option, value);
        public IntPtr InfoRead(out int msgsInQueue) => Native.InfoRead(this, out msgsInQueue);
        public CURLMcode SocketAction(SafeSocketHandle sockfd, CURLcselect evBitmask, out int runningHandles) => Native.SocketAction(this, sockfd, evBitmask, out runningHandles);
    }
}