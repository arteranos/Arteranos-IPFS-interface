using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

using Unity.Net.Http;
using System.Net.Http.Headers;
using System.Text;

public class UWRExample : MonoBehaviour
{
#if false
    internal class WebRequest
    {
        public HttpRequestMessage request;
        public volatile DownloadHandlerStream downloadHandler;
        public volatile Exception exception;
    }


    internal ConcurrentQueue<WebRequest> WebRequestQueue = new();

    // Start is called before the first frame update
    private void Start()
    {
        HttpClient client = new(null);

        var content = new MultipartFormDataContent();
        byte[] payload = Encoding.UTF8.GetBytes("abcdef äöüßÄÖÜ€ ghijkl");
        var streamContent = new ByteArrayContent(payload);

        streamContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

        content.Add(streamContent, "file", name);

        string contentBlob = content.ReadAsStringAsync().Result;

        Debug.Log($"Content:\n{contentBlob}");

        for (int i = 0; i < 1; ++i)
        {
            Debug.Log($"Round {i+1}:");

            Task.Run(async () =>
            {
                try
                {
                    //HttpRequestMessage httpRequest = new(HttpMethod.Post,
                    //    new Uri(
                    //    "http://127.0.0.1:5002/api/v0/cat?arg=" +
                    //    "QmZTyaVhJmrRfupD5peRG9AKCFF3z9DkuzCWxPhkHWmoDd" +
                    //    "&progress=true"),
                    //    null);

                    HttpRequestMessage httpRequest = new(HttpMethod.Post,
                        new Uri("http://127.0.0.1:5002/api/v0/config/show"));

                    HttpResponseMessage response = await client.SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);

                    Debug.Log($"Completed PostAsync, status={response?.StatusCode}");

                    if((response?.StatusCode ?? 0) == System.Net.HttpStatusCode.OK)
                    {
                        string content = response.Content.ReadAsStringAsync().Result;

                        Debug.Log($"Result:\n{content}");
                    }
                }
                catch ( Exception e )
                {
                    Debug.LogException( e );
                }
            }).ConfigureAwait(false);
        }

        Debug.Log("Completed.");
    }
#endif

    private void Start()
    {


    }
}
