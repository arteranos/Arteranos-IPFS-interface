using System;
using System.Collections;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using Curly;
using System.Runtime.InteropServices;
using Unity.IO.Pipes;

namespace Unity.Net.Http
{
    public class HttpDaemon : MonoBehaviour
    {
        public static HttpDaemon Instance { get; private set; } = null;

        private Curly.Curl _Curl = null;
        private bool SCRAM = false;
        public static void EnsureRunning()
        {
            if(Instance != null) return;

            throw new InvalidOperationException("No HTTP Daemon - Add the HTTP Daemon to the GameObject");
        }

        public void Awake()
        {
            Instance = this;
            _Curl = new();
        }

        public void OnDestroy()
        {
            Instance = null;
            SCRAM = true;

            _Curl.Dispose();
        }

        public class ProgressData
        {
            public Pipe responseInputPipe = null;
            public Easy easy = null;
            public int totalBytes = 0;
            public int responseCode = 0;
            public string contentType = null;
            public bool headerDetected = false;
        }

        public static int HeaderScannerFunc(ProgressData pd, byte[] buffer)
        {
            // Blank line after header...
            if(buffer.Length == 2 && buffer[0] == 0x0d && buffer[1] == 0x0a)
            {
                // ... and something substantial ...
                pd.easy.GetInfo(CURLINFO.RESPONSE_CODE, out int statusCode);

                // Debug.Log($"Header finished {statusCode}");

                // ... then notify the chance for the early exit.
                if (statusCode > 199)
                {
                    pd.responseCode = statusCode;
                    pd.headerDetected = true;
                }
            }

            return buffer.Length;
        }

        public static int DataReadFunc(ProgressData pd, byte[] buffer)
        {
            if (pd.totalBytes == 0)
            {
                pd.easy.GetInfo(CURLINFO.RESPONSE_CODE, out int statusCode);
                pd.easy.GetInfo(CURLINFO.CONTENT_TYPE, out string contentType);

                pd.responseCode = statusCode;
                pd.contentType = contentType;

                //Debug.Log("Starting to receive content");
                //Debug.Log($"HTTP Status code: {statusCode}");
                //Debug.Log($"Content type: {contentType}");
            }

            pd.responseInputPipe.Write(new(buffer));
            pd.totalBytes += buffer.Length;
            return buffer.Length;
        }

        public HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancel = default)
        {
            CancellationTokenSource tokenSource = new();

            byte[] postContent = null;

            if (request.Content != null)
            {
                using MemoryStream ms = new();
                Stream inStream = request.Content.ReadAsStreamAsync().Result;
                inStream.CopyTo(ms);
                postContent = ms.ToArray();
            }

            Pipe responseInputPipe = new();
            ProgressData progressData = new()
            {
                responseInputPipe = responseInputPipe,
            };

            Task<CURLcode> taskResult = Task.Run(() => {
                using Multi multi = new();
                Easy easy = _Curl.GetEasy();

                try
                {
                    using Slist headers = new();

                    progressData.easy = easy;
                    using DataCallbackFunc dataCopier = new(b => DataReadFunc(progressData, b));
                    using DataCallbackFunc headerScanner = new(b => HeaderScannerFunc(progressData, b));

                    easy.SetOpt(CURLoption.URL, request.RequestUri.ToString());
                    easy.SetOpt(CURLoption.CUSTOMREQUEST, request.Method);
                    easy.SetOpt(CURLoption.HEADERFUNCTION, headerScanner.DataHandler);
                    easy.SetOpt(CURLoption.WRITEFUNCTION, dataCopier.DataHandler);
                    easy.SetOpt(CURLoption.USERAGENT, "Curly/0.1");

                    if (postContent != null)
                    {
                        headers.Add($"Content-Type: {request.Content.Headers.ContentType}");
                        easy.SetOpt(CURLoption.HTTPHEADER, headers);
                        easy.SetOpt(CURLoption.POSTFIELDSIZE, postContent.Length);
                        easy.SetOpt(CURLoption.COPYPOSTFIELDS, postContent);
                    }

                    multi.AddHandle(easy);

                    int still_running = 0;
                    while (true)
                    {
                        if (tokenSource.Token.IsCancellationRequested || SCRAM)
                            return CURLcode.ABORTED_BY_CALLBACK;

                        CURLMcode mc = multi.Perform(ref still_running);

                        if (still_running == 0) break;

                        if (mc == CURLMcode.OK) mc = multi.Poll(1000);
                    }
                }
                finally
                {
                    progressData.responseInputPipe.CloseWrite();

                    multi.RemoveHandle(easy);
                    easy.Dispose();
                }

                return CURLcode.OK;
            });

            while(true)
            {
                if (cancel.IsCancellationRequested)
                {
                    tokenSource.Cancel();
                    throw new TaskCanceledException();
                }

                if (progressData.headerDetected && request.CompletionOption == HttpCompletionOption.ResponseHeadersRead) break;

                if (taskResult.IsCompleted) break;

                Thread.Sleep(10);
            }

            if (taskResult.IsFaulted) throw new HttpRequestException(taskResult.Exception.Message);

            // Not completed yet if we stop at the header.
            if (taskResult.IsCompleted)
            {
                CURLcode result = taskResult.Result;
                if (result != CURLcode.OK) throw new HttpRequestException($"Result code: {Curly.Curl.StrError(result)} ({(int)result}).");
            }

            return new HttpResponseMessage()
            {
                RequestMessage = request,
                Content = new StreamContent(responseInputPipe.ReadStream),
                StatusCode = (System.Net.HttpStatusCode)progressData.responseCode
            };
        }

        public Task<HttpResponseMessage> SendAsync(HttpRequestMessage httpRequest, HttpCompletionOption hco, CancellationToken cancel = default)
        {
            httpRequest.CompletionOption = hco;

            return Task.Run(() => {
                return Send(httpRequest, cancel);
            });
        }

    }
}