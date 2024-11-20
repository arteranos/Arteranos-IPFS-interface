using System;
using System.Collections;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Unity.Net.Http
{
    public class HttpDaemon : MonoBehaviour
    {
        internal class WebRequest
        {
            public HttpRequestMessage request = null;
            public volatile bool done = false;
            public volatile DownloadHandlerStream downloadHandler = null;
            public volatile Exception exception = null;
        }

        public static HttpDaemon Instance { get; private set; } = null;

        public static void EnsureRunning()
        {
            if(Instance != null) return;

            throw new InvalidOperationException("No HTTP Daemon - Add the HTTP Daemon to the GameObject");
        }

        private void Awake()
        {
            Instance = this;
        }

        private ConcurrentQueue<WebRequest> WebRequestQueue = new();

        private void Update()
        {
            if (WebRequestQueue.TryDequeue(out WebRequest request))
                StartCoroutine(ProcessWebRequest(request));
        }

        private static IEnumerator ProcessWebRequest(WebRequest incoming)
        {
            incoming.downloadHandler = new DownloadHandlerStream();
            HttpRequestMessage httpRequest = incoming.request;

            UploadHandler uploadHandler = null;
            

            HttpContent sendContent = httpRequest.Content;
            if (sendContent != null)
            {
                Task<byte[]> taskContentBlob = sendContent.ReadAsByteArrayAsync();

                yield return new WaitUntil(() => taskContentBlob.IsCompleted);

                uploadHandler = new UploadHandlerRaw(taskContentBlob.Result);
                uploadHandler.contentType = sendContent.Headers.ContentType.MediaType;
            }

            using UnityWebRequest uwr = new(
                httpRequest.RequestUri, 
                httpRequest.Method, 
                incoming.downloadHandler, 
                uploadHandler);
            incoming.downloadHandler.Request = uwr;

            UnityWebRequestAsyncOperation uwrAo = uwr.SendWebRequest();

            // Allow the worker task to break out as soon as the content starts to arrive
            if (httpRequest.CompletionOption == HttpCompletionOption.ResponseHeadersRead)
                incoming.done = true;

            yield return uwrAo;

            incoming.downloadHandler.StatusCode = (int)uwr.responseCode;

            incoming.exception = uwr.result switch
            {
                UnityWebRequest.Result.InProgress => new InvalidOperationException(), // Should never happen, after the 'yield return'
                UnityWebRequest.Result.Success => null,
                UnityWebRequest.Result.ConnectionError => new IOException(),
                UnityWebRequest.Result.ProtocolError => new HttpRequestException(uwr.error),
                UnityWebRequest.Result.DataProcessingError => new HttpRequestException(uwr.error),
                _ => new NotSupportedException(),
            };

            // All done.
            incoming.done = true;
        }

        public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage httpRequest, HttpCompletionOption hco, CancellationToken cancel = default)
        {
            httpRequest.CompletionOption = hco;
            WebRequest request = new() { request = httpRequest };

            WebRequestQueue.Enqueue(request);

            while (true)
            {
                if (cancel.IsCancellationRequested) throw new TaskCanceledException();
                if (request.exception != null) throw request.exception;

                // Only if the worker says we're good to go and there is a status code
                // has arrived
                if (request.done && request.downloadHandler?.StatusCode != 0) break;

                await Task.Yield();
            }

            HttpContent responseContent = new StreamContent(request.downloadHandler.GetStream());

            HttpResponseMessage response = new()
            {
                RequestMessage = httpRequest,
                Content = responseContent,
                StatusCode = (System.Net.HttpStatusCode)request.downloadHandler.StatusCode
            };

            return response;
        }

    }
}