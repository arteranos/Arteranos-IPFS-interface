using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

using System.Net.Http.Headers;
using System.Text;
using System.Runtime.InteropServices;

using Unity.Net.Http;

public class UWRExample : MonoBehaviour
{
    public void Start()
    {
        HttpClient client = new(null);

        Task.Run(async () =>
        {
#if false
            HttpRequestMessage httpRequest = new(HttpMethod.Post,
                new Uri(
                "http://127.0.0.1:5002/api/v0/cat?arg=" +
                "QmZTyaVhJmrRfupD5peRG9AKCFF3z9DkuzCWxPhkHWmoDd" +
                "&progress=true"),
                null);
#elif false
            HttpRequestMessage httpRequest = new(HttpMethod.Post, 
                new Uri(
                "http://127.0.0.1:5002/api/v0/config/show"),
                null);
#elif true
            HttpRequestMessage httpRequest = new(HttpMethod.Post,
                new Uri(
                "http://127.0.0.1:5001/api/v0/config/show"),
                null);
#endif

            try
            {
                HttpResponseMessage response = await client.SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);

                Debug.Log($"Completed PostAsync, status={response?.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    byte[] content = response.Content.ReadAsByteArrayAsync().Result;

                    Debug.Log($"Received {content.Length} bytes of content");
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        });

        Debug.Log("Completed.");
    }
}
