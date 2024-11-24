using System;

namespace Unity.Libcurl
{
    public class Curl : IDisposable
    {
        public Curl() 
        { 
            CURLcode result = External.curl_global_init((int)CURLinitFlag.CURL_GLOBAL_ALL);
            if (result != CURLcode.CURLE_OK)
                throw new Exception("libcurl failed to initialize");
        }

        ~Curl() 
        {
            Dispose(false);
        }

        public Easy CreateEasy() => new();
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
                External.curl_global_cleanup();
            }
        }
    }
}