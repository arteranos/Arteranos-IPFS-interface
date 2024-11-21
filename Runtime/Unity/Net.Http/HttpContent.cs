using System;
using System.Collections;
using System.IO;
using System.Net.Http.Headers;
using System.Text;

using System.Threading.Tasks;

using UnityEngine.Networking;

namespace Unity.Net.Http
{
    public class HttpContent : IDisposable
    {
        protected System.Net.Http.HttpContent _inner;

        public static implicit operator HttpContent(System.Net.Http.HttpContent content) { return new() { _inner = content }; }
        public static implicit operator System.Net.Http.HttpContent(HttpContent content) { return content._inner; }

        public HttpContentHeaders Headers => _inner.Headers;
        public Task<byte[]> ReadAsByteArrayAsync() => _inner.ReadAsByteArrayAsync();
        public Task<string> ReadAsStringAsync() => _inner.ReadAsStringAsync();
        public Task<Stream> ReadAsStreamAsync() => _inner.ReadAsStreamAsync();
        public Task CopyToAsync(Stream stream) => _inner.CopyToAsync(stream);


        private bool _disposed = false;
        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            _inner?.Dispose();
        }
    }

    public class ByteArrayContent : System.Net.Http.ByteArrayContent
    {
        public ByteArrayContent(byte[] bytes) : base(bytes) { }
    }

    public class StreamContent : System.Net.Http.StreamContent
    {
        public StreamContent(Stream stream) : base(stream) { }
    }

    public class StringContent : System.Net.Http.StringContent
    {
        public StringContent(string str) : base(str) { }
        public StringContent(string str, Encoding encoding) : base(str, encoding) { }
    }

    public class MultipartFormDataContent : HttpContent, IEnumerable
    {
        public string Boundary { get; private set; } = null;

        public MultipartFormDataContent() 
        {
            Init(Guid.NewGuid().ToString());
        }

        public MultipartFormDataContent(string boundary)
        {
            Init(boundary);
        }

        private void Init(string boundary)
        {
            _inner = new System.Net.Http.MultipartFormDataContent(boundary);
            Boundary = boundary;
        }

        public void Add(HttpContent content, string name)
            => (_inner as System.Net.Http.MultipartFormDataContent).Add(content, name);

        public void Add(HttpContent content, string name, string fileName)
            => (_inner as System.Net.Http.MultipartFormDataContent).Add(content, name, fileName);

        public IEnumerator GetEnumerator()
        {
            return (_inner as System.Net.Http.MultipartFormDataContent).GetEnumerator();
        }
    }

    public enum HttpCompletionOption
    {
        ResponseContentRead = 0,
        ResponseHeadersRead
    }

    public static class HttpMethod
    {
        public const string Post = UnityWebRequest.kHttpVerbPOST;
        public const string Put = UnityWebRequest.kHttpVerbPUT;
        public const string Delete = UnityWebRequest.kHttpVerbDELETE;
        public const string Get = UnityWebRequest.kHttpVerbGET;
        public const string Head = UnityWebRequest.kHttpVerbHEAD;
    }
}