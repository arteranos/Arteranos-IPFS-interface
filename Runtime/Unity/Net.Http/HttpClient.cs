using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Unity.Net.Http
{
    public class HttpClient
    {
        public TimeSpan Timeout { get; set; } = System.Threading.Timeout.InfiniteTimeSpan;

        public Dictionary<string, string> DefaultRequestHeaders = new();

        public HttpClient(HttpMessageHandler _ /* messageHandler */)
        { }

        public Task<HttpResponseMessage> GetAsync(Uri requestUri, HttpCompletionOption hco, CancellationToken cancel = default)
        {
            HttpRequestMessage request = new(HttpMethod.Get, requestUri, null);
            return SendAsync(request, hco, cancel);
        }

        public Task<HttpResponseMessage> PostAsync(Uri requestUri, HttpContent content, CancellationToken cancel = default)
        {
            HttpRequestMessage request = new(HttpMethod.Post, requestUri, content);
            return SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancel);
        }

        public Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, HttpCompletionOption hco, CancellationToken cancel = default)
        {
            HttpDaemon.EnsureRunning();

            return HttpDaemon.Instance.SendAsync(request, hco, cancel);
        }
    }
}