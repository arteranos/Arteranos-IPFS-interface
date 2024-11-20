using System;

namespace Unity.Net.Http
{
    public class HttpRequestException : Exception
    {
        public HttpRequestException() { }
        public HttpRequestException(string message) : base(message) { }
    }
}
