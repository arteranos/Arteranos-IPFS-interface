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
            return Task.Run<int>(() =>
            {
                return _pipe.ReadInternal(new Memory<byte>(buffer, offset, count), default);
            }).Result;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            return Task.Run(() =>
            {
                return _pipe.ReadInternal(new Memory<byte>(buffer, offset, count), cancellationToken);
            });
        }

        public override Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
        {
            return base.CopyToAsync(destination, bufferSize, cancellationToken);
        }

        public override void CopyTo(Stream destination, int bufferSize)
        {
            base.CopyTo(destination, bufferSize);
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