using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Unity.IO.Pipes
{
    public sealed class PipeWriterStream : Stream
    {
        private Pipe _pipe;
        internal PipeWriterStream(Pipe pipe) { _pipe = pipe; }

        public override bool CanRead => false;

        public override bool CanSeek => false;

        public override bool CanWrite => true;

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
            throw new NotSupportedException();
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
            _pipe.Write(new ReadOnlyMemory<byte>(buffer, offset, count));
        }
    }
}