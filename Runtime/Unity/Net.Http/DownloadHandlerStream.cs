using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;


namespace Unity.Net.Http
{
    /// <summary>
    /// Provides the content reader stream, even if the Async Operation is still in progress.
    /// Once the content starts to arrive, the stream is opened.
    /// </summary>
    public class DownloadHandlerStream : DownloadHandlerScript
    {
        public UnityWebRequest Request { get; internal set; } = null;
        public int StatusCode { get; internal set; } = 0;
        public bool HasContent { get; internal set; } = false;

        private IO.Pipes.Pipe _pipe = new();

        public Stream GetStream()
        {
            return _pipe.ReadStream;
        }

        private void SetupPipe()
        {
        }

        #region UWR callbacks

        // !!! MUST NOT BLOCK !!!

        protected override void CompleteContent()
        {
            base.CompleteContent();

            StatusCode = (int)Request.responseCode;

            _pipe.CloseWrite();
        }

        protected override bool ReceiveData(byte[] data, int dataLength)
        {
            bool res = base.ReceiveData(data, dataLength);

            SetupPipe();

            StatusCode = (int)Request.responseCode;

            try
            {
                _pipe.Write(new ReadOnlyMemory<byte>(data, 0, dataLength));
            }
            catch
            {
                // Signal UnityEngine that it will be senseless to receive more data.
                return false;
            }

            HasContent = true;

            return res;
        }

        #endregion
    }
}
