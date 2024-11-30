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
            _Curl.Dispose();
        }

        public class ProgressData
        {
            public Pipe responseInputPipe = null;
            public Easy easy = null;
            public int totalBytes = 0;
            public int responseCode = 0;
            public string contentType = null;
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
                try
                {
                    using Easy easy = _Curl.GetEasy();
                    using Slist headers = new();

                    progressData.easy = easy;
                    DataCallbackFunc dataCopier = new(b => DataReadFunc(progressData, b));

                    easy.SetOpt(CURLoption.URL, request.RequestUri.ToString());
                    easy.SetOpt(CURLoption.CUSTOMREQUEST, request.Method);
                    easy.SetOpt(CURLoption.WRITEFUNCTION, dataCopier.DataHandler);
                    easy.SetOpt(CURLoption.USERAGENT, "Curly/0.1");

                    if (postContent != null)
                    {
                        headers.Add($"Content-Type: {request.Content.Headers.ContentType}");
                        easy.SetOpt(CURLoption.HTTPHEADER, headers);
                        easy.SetOpt(CURLoption.POSTFIELDSIZE, postContent.Length);
                        easy.SetOpt(CURLoption.COPYPOSTFIELDS, postContent);
                    }

                    CURLcode result = easy.Perform();
                    // In case if we don't get any content and we haven't the chance to
                    // glean the response code.
                    if(result == CURLcode.OK)
                        easy.GetInfo(CURLINFO.RESPONSE_CODE, out progressData.responseCode);
                    return result;
                }
                finally
                {
                    progressData.responseInputPipe.CloseWrite();
                }
            });

            while(true)
            {
                if (cancel.IsCancellationRequested) throw new TaskCanceledException();

                if (progressData.responseCode != 0 && request.CompletionOption == HttpCompletionOption.ResponseHeadersRead) break;

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