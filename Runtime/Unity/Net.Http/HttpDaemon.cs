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

using System.Runtime.InteropServices;

using CurlThin;
using CurlThin.Enums;
using CurlThin.SafeHandles;
using CurlThin.Helpers;

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
            public CURLcode performResult;
            public Pipe contentDownloadPipe = null;
            public Stream contentUploadStream = null;
        }

        public static HttpDaemon Instance { get; private set; } = null;

        private static CURLcode _curlStatus;
        public static void EnsureRunning()
        {
            if(Instance != null) return;

            throw new InvalidOperationException("No HTTP Daemon - Add the HTTP Daemon to the GameObject");
        }

        private void Awake()
        {
            Instance = this;

            _curlStatus = CurlNative.Init();
        }

        private void OnDestroy()
        {
            if(_curlStatus == CURLcode.OK) CurlNative.Cleanup();
        }

        public Task<HttpResponseMessage> SendAsync(HttpRequestMessage httpRequest, HttpCompletionOption hco, CancellationToken cancel = default)
        {
            return Task.Run(() =>
            {
                return Send(httpRequest, hco, cancel);
            });
        }

        public HttpResponseMessage Send(HttpRequestMessage httpRequest, HttpCompletionOption hco, CancellationToken cancel = default)
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

                Thread.Yield();
            }

            // Definitely faulted, propagate exception
            if (t.IsFaulted)
            {
                // Debug.LogException(t.Exception);
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
            return Task.Run(() =>
            {
                return Perform(request);
            });
        }

        private static CURLcode Perform(WebRequest request)
        {
            //unsafe
            {
                SafeEasyHandle easy = CurlNative.Easy.Init();
                SafeLPStr url = new(request.request.RequestUri.ToString());
                SafeLPStr method = new(request.request.Method);
                SafeLPStr agent = new("CurlThinner");
                GCHandle requestHandle = GCHandle.Alloc(request);
                IntPtr requestPtr = (IntPtr)requestHandle;

                GCHandle gch = (GCHandle)requestPtr;

                // Slist headers = null;


                SafeSlistHandle headers = null;
                GCHandle pinnedContentBlob = default;
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
                    pinnedContentBlob = GCHandle.Alloc(bytes, GCHandleType.Pinned);
                    headers = CurlNative.Slist.Append(SafeSlistHandle.Null, ctstring);

                    CurlNative.Easy.SetOpt(easy, CURLoption.POSTFIELDS, pinnedContentBlob.AddrOfPinnedObject());
                    CurlNative.Easy.SetOpt(easy, CURLoption.POSTFIELDSIZE, bytes.Length);
                    CurlNative.Easy.SetOpt(easy, CURLoption.HTTPHEADER, headers.DangerousGetHandle());

                    // headers = new();
                    // headers.Append(ctstring);
                }

                var datacopier = new DataCallbackCopier();
                CurlNative.Easy.SetOpt(easy, CURLoption.WRITEDATA, requestPtr);
                CurlNative.Easy.SetOpt(easy, CURLoption.WRITEFUNCTION, WrapWriteHandler(GotContentData));
                CurlNative.Easy.SetOpt(easy, CURLoption.HEADERDATA, requestPtr);
                CurlNative.Easy.SetOpt(easy, CURLoption.HEADERFUNCTION, WrapWriteHandler(GotHeaderData));

                CurlNative.Easy.SetOpt(easy, CURLoption.URL, url);
                CurlNative.Easy.SetOpt(easy, CURLoption.CUSTOMREQUEST, method);
                CurlNative.Easy.SetOpt(easy, CURLoption.USERAGENT, agent);


                // if(headers != null) CurlNative.Easy.SetOpt(easy, CURLoption.HTTPHEADER, headers);
                request.performResult = CurlNative.Easy.Perform(easy);
                if (request.contentUploadStream != null)
                {
                    pinnedContentBlob.Free();
                    CurlNative.Slist.FreeAll(headers);
                }

                //if(headers != null)
                //{
                //    headers?.FreeAll();
                //}

                request.contentDownloadPipe.CloseWrite();
                requestHandle.Free();

                switch (request.performResult)
                {
                    case CURLcode.OK:
                        break;
                    case CURLcode.ABORTED_BY_CALLBACK:
                        throw new TaskCanceledException();
                    default:
                        throw new HttpRequestException($"Curl error {request.performResult}");
                }

                request.stage = ProcessStage.ContentReceived;

            }
            return request.performResult;
        }

        private static CurlNative.Easy.DataHandler WrapWriteHandler(Func<byte[], IntPtr, int> func)
        {
            return (data, size, nmemb, userdata) =>
            {
                var length = (int)size * (int)nmemb;
                var buffer = new byte[length];
                Marshal.Copy(data, buffer, 0, length);
                return (UIntPtr) func(buffer, userdata);
            };
        }

        private static int GotHeaderData(byte[] buffer, IntPtr extra)
        {
            GCHandle gch = (GCHandle)extra;
            WebRequest request = (WebRequest)gch.Target;
            if (request == null)
            {
                Debug.LogWarning("Missing Webrequest");
                return 0;
            }

            // Debug.Log(Encoding.UTF8.GetString(buffer));
            string[] msg = Encoding.UTF8.GetString(buffer).Split(' ');
            if (request.stage != ProcessStage.StatusCodeReceived)
            {
                if (msg[0].StartsWith("HTTP"))
                {
                    int v = int.Parse(msg[1]);

                    // Ignore 100 - Continue
                    if(v > 199)
                    {
                        request.statusCode = (HttpStatusCode)v;
                        request.stage = ProcessStage.StatusCodeReceived;
                    }
                }

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

                if(request.stage == ProcessStage.StatusCodeReceived) 
                    request.stage = ProcessStage.HeaderReceived;
            }
            //else if (msg.Length == 1)
            //    Debug.Log($"Unknown header line: '{msg[0]}' ({msg[0].Length} code points)");

            return buffer.Length; // keep going
        }

        private static int GotContentData(byte[] buffer, IntPtr extra)
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
            // Debug.Log($"Received {buffer.Length} bytes of content");

            return buffer.Length; // keep going
        }
    }
}