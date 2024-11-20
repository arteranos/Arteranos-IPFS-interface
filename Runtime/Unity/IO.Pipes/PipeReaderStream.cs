using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Unity.IO.Pipes
{
    public sealed class PipeReaderStream : Stream
    {
        private Pipe _pipe;
        internal PipeReaderStream(Pipe pipe) { _pipe = pipe; }
        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => throw new NotSupportedException();
        public override long Position { 
            get => throw new NotSupportedException(); 
            set => throw new NotSupportedException(); 
        }
        public override void Flush()
        {

        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return _pipe.ReadAsyncInternal(new Memory<byte>(buffer, offset, count), default).Result;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }
    }
}