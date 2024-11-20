using System;
using System.Net;

namespace Unity.Net.Http
{
    public class HttpResponseMessage : IDisposable
    {
        public HttpContent Content { get; internal set; }

        public HttpRequestMessage RequestMessage { get; internal set; }

        public HttpStatusCode StatusCode { get; internal set; }

        public bool IsSuccessStatusCode => ((int)StatusCode >= 200) && ((int)StatusCode <= 299);


        private bool _disposed = false;
        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            Content?.Dispose();
        }
    }

    public class HttpRequestMessage
    {
        public Uri RequestUri { get; private set; }
        public string Method { get; private set; }
        public HttpContent Content { get; private set; }
        public HttpCompletionOption CompletionOption { get; internal set; }

        public HttpRequestMessage(string method, Uri uri, HttpContent content = null, HttpCompletionOption hco = default)
        {
            Method = method;
            RequestUri = uri;
            Content = content;
            CompletionOption = hco;
        }
    }
}