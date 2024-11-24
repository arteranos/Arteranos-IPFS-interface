using System;

using System.Runtime.InteropServices;

namespace Unity.Libcurl
{
    public delegate int Writer(byte[] buf, int sz, int nmemb, IntPtr parm);
    public delegate byte[] Reader(int sz, int nmemb, IntPtr parm);

    public class Easy : IDisposable
    {
        private IntPtr easy_handle = IntPtr.Zero;

        internal Easy()
        {
            easy_handle = External.curl_easy_init();
            External.curl_easy_setopt(easy_handle, CURLoption.CURLOPT_NOPROGRESS, 0);
        }

        ~Easy()
        {
            Dispose(false);
        }

        public CURLcode SetOpt(CURLoption opt, IntPtr parm) => External.curl_easy_setopt(easy_handle, opt, parm);
        public CURLcode SetOpt(CURLoption opt, string parm) => External.curl_easy_setopt(easy_handle, opt, parm);
        public CURLcode SetOpt(CURLoption opt, int parm) => External.curl_easy_setopt(easy_handle, opt, parm);
        public CURLcode SetOpt(CURLoption opt, byte[] parm) => External.curl_easy_setopt(easy_handle, opt, parm);
        public CURLcode SetOpt(CURLoption opt, Writer parm) => External.curl_easy_setopt(easy_handle, opt, WrapWriteDelegate(parm));
        public CURLcode SetOpt(CURLoption opt, Reader parm) => External.curl_easy_setopt(easy_handle, opt, WrapReadDelegate(parm));

        public CURLcode Perform() => External.curl_easy_perform(easy_handle);

        private static External.CURL_WRITE_DELEGATE WrapWriteDelegate(Writer fn)
        {
            return (bufPtr, sz, nmemb, parm) =>
            {
                // UnityEngine.Debug.Log($"=> WriteDelegate ptr={bufPtr}, sz={sz}, nmemb={nmemb}, parm={parm}");
                int bytes = sz * nmemb;
                byte[] buffer = new byte[bytes];

                Marshal.Copy(bufPtr, buffer, 0, bytes);
                int res = fn?.Invoke(buffer, sz, nmemb, parm) ?? 0;
                // UnityEngine.Debug.Log($"<= WriteDelegate res={res}");
                return res;
            };
        }

        private static External.CURL_READ_DELEGATE WrapReadDelegate(Reader fn)
        {
            return (bufPtr, sz, nmemb, parm) =>
            {
                int bytes = sz * nmemb;
                byte[] buffer = fn?.Invoke(sz, nmemb, parm) ?? new byte[0];

                int nRead = Math.Min(buffer.Length, bytes);
                if(nRead > 0) Marshal.Copy(buffer, 0, bufPtr, nRead);

                return nRead;
            };
        }

        // ---------------------------------------------------------------
        public void Dispose()
        {
            GC.SuppressFinalize(this);
            Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            lock (this)
            {
                // if (disposing) cleanup managed resources
                // cleanup unmanaged resources
                if (easy_handle != IntPtr.Zero)
                {
                    External.curl_easy_cleanup(easy_handle);
                    easy_handle = IntPtr.Zero;
                }
            }
        }
    }
}