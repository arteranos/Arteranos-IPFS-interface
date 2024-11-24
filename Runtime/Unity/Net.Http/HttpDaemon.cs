using System;
using System.Collections;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using System.Net;
using System.Text;
using Debug = UnityEngine.Debug;

using Unity.IO.Pipes;

// using SeasideResearch.LibCurlNet;
using Unity.Libcurl;
using System.Runtime.InteropServices;

namespace Unity.Net.Http
{
    public class HttpDaemon : UnityEngine.MonoBehaviour
    {
        internal enum ProcessStage
        {
            _invalid = 0,
            Started,
            StatusCodeReceived,
            HeaderReceived,
            ContentReceived
        }

        internal class WebRequest
        {
            public HttpRequestMessage request = null;
            public HttpContent responseContent = null;
            public HttpStatusCode statusCode = HttpStatusCode.InternalServerError;
            public ProcessStage stage = ProcessStage._invalid;
            public CURLcode performResult = (CURLcode)(-1);
            public Pipe contentDownloadPipe = null;
            public Stream contentUploadStream = null;
        }

        public static HttpDaemon Instance { get; private set; } = null;

        private Unity.Libcurl.Curl _curl;
        public static void EnsureRunning()
        {
            if(Instance != null) return;

            throw new InvalidOperationException("No HTTP Daemon - Add the HTTP Daemon to the GameObject");
        }

        private void Awake()
        {
            Instance = this;
            _curl = new();
        }

        private void OnDestroy()
        {
            _curl = null;
        }

        public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage httpRequest, HttpCompletionOption hco, CancellationToken cancel = default)
        {
            httpRequest.CompletionOption = hco;
            WebRequest request = new() { request = httpRequest };

            request.contentDownloadPipe = new();
            request.responseContent = new StreamContent(request.contentDownloadPipe.ReadStream);

            request.contentUploadStream = httpRequest.Content?.ReadAsStreamAsync().Result;

            Task<CURLcode> t = PerformAsync(request);

            while(true)
            {
                // Content completion or faulted
                if (t.IsCompleted) break;

                // Header completion, detaching worker
                if (request.stage == ProcessStage.HeaderReceived && 
                    httpRequest.CompletionOption == HttpCompletionOption.ResponseHeadersRead) break;

                // Task canceled
                if (cancel.IsCancellationRequested) throw new TaskCanceledException();

                await Task.Yield();
            }

            // Definitely faulted, propagate exception
            if (t.IsFaulted)
            {
                Debug.LogException(t.Exception);
                throw t.Exception;
            }

            // We got what we wanted.
            return new()
            {
                Content = request.responseContent,
                RequestMessage = httpRequest,
                StatusCode = request.statusCode,
            };
        }

        private Task<CURLcode> PerformAsync(WebRequest request)
        {
            return Task.Run(() => {
                Easy easy = _curl.CreateEasy();
                GCHandle requestHandle = GCHandle.Alloc(request);
                IntPtr requestPtr = (IntPtr)requestHandle;

                GCHandle gch = (GCHandle)requestPtr;

                // Slist headers = null;

                easy.SetOpt(CURLoption.CURLOPT_URL, request.request.RequestUri.ToString());
                easy.SetOpt(CURLoption.CURLOPT_CUSTOMREQUEST, request.request.Method);
                if (request.contentUploadStream != null)
                {
                    using MemoryStream ms = new();
                    request.contentUploadStream.CopyTo(ms);

                    HttpContent sendContent = request.request.Content;
                    string ctstring = null;
                    if (sendContent is MultipartFormDataContent mp)
                        ctstring = $"Content-Type: {sendContent.Headers.ContentType.MediaType}; boundary=\"{mp.Boundary}\"";
                    else
                        ctstring = $"Content-Type: {sendContent.Headers.ContentType.MediaType}";

                    byte[] bytes = ms.ToArray();
                    easy.SetOpt(CURLoption.CURLOPT_POSTFIELDS, bytes);
                    easy.SetOpt(CURLoption.CURLOPT_POSTFIELDSIZE, bytes.Length);

                    // headers = new();
                    // headers.Append(ctstring);
                }

                easy.SetOpt(CURLoption.CURLOPT_WRITEDATA, requestPtr);
                easy.SetOpt(CURLoption.CURLOPT_WRITEFUNCTION, GotContentData);
                easy.SetOpt(CURLoption.CURLOPT_HEADERDATA, requestPtr);
                easy.SetOpt(CURLoption.CURLOPT_HEADERFUNCTION, GotHeaderData);

                // easy.SetOpt(CURLoption.CURLOPT_USERAGENT, "Mozilla 4.0 (compatible; MSIE 6.0; Win32");

                // if(headers != null) easy.SetOpt(CURLoption.CURLOPT_HTTPHEADER, headers);
                request.performResult = easy.Perform();
                //if(headers != null)
                //{
                //    headers?.FreeAll();
                //}

                request.contentDownloadPipe.CloseWrite();

                requestHandle.Free();

                switch (request.performResult)
                {
                    case CURLcode.CURLE_OK:
                        break;
                    case CURLcode.CURLE_ABORTED_BY_CALLBACK:
                        throw new TaskCanceledException();
                    default:
                        throw new HttpRequestException($"Curl error {request.performResult}");
                }

                request.stage = ProcessStage.ContentReceived;

                return request.performResult;
            });
        }

        private static int GotHeaderData(byte[] buffer, int size, int nmemb, IntPtr extra)
        {
            GCHandle gch = (GCHandle)extra;
            WebRequest request = (WebRequest)gch.Target;
            if (request == null)
            {
                Debug.LogWarning("Missing Webrequest");
                return 0;
            }

            Debug.Log(Encoding.UTF8.GetString(buffer));
            string[] msg = Encoding.UTF8.GetString(buffer).Split(' ');
            if (request.stage != ProcessStage.StatusCodeReceived)
            {
                if (msg[0].StartsWith("HTTP")) 
                    request.statusCode = (HttpStatusCode) int.Parse(msg[1]);

                request.stage = ProcessStage.StatusCodeReceived;
                return buffer.Length; // Not _really_ a header.
            }

            if (msg.Length > 1)
            {
                //string headerKey = msg[0].Trim()[0..^1];
                //string headerValue = string.Join(' ', msg[1..]);

                //Debug.Log($"Header: key={headerKey}, value={headerValue}");
                // TODO response Header collection
            }
            if (msg.Length == 1 && (msg[0] == "\r\n" || msg[0] == "\n")) 
            {
                //Debug.Log("Reader received, expecting content");
                request.stage = ProcessStage.HeaderReceived;
            }
            //else if (msg.Length == 1)
            //    Debug.Log($"Unknown header line: '{msg[0]}' ({msg[0].Length} code points)");

            return buffer.Length; // keep going
        }

        private static int GotContentData(byte[] buffer, int size, int nmemb, IntPtr extra)
        {
            GCHandle gch = (GCHandle) extra;
            WebRequest request = (WebRequest) gch.Target;
            if(request == null)
            {
                Debug.LogWarning("Missing Webrequest");
                return 0;
            }
            // Ensure we keep it on track, even if the header seemed to be borked.
            request.stage = ProcessStage.HeaderReceived;
            
            request.contentDownloadPipe.Write(new ReadOnlyMemory<byte>(buffer));
            Debug.Log($"Received {buffer.Length} bytes of content");

            return buffer.Length; // keep going
        }
    }
}