using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Unity.IO.Pipes
{
    internal class PipeChunk
    {
        public ReadOnlyMemory<byte> data;
        public int consumed;
    }

    public class Pipe : IDisposable
    {
        public Stream ReadStream => new PipeReaderStream(this);
        public Stream writeStream => new PipeWriterStream(this);

        private ConcurrentQueue<PipeChunk> _chunks = new();
        private volatile bool _writeEOF = false;
        private volatile bool _readEOF = false;

        private Guid _guid = Guid.NewGuid();

        private int tasknum = 0;
        internal int ReadInternal(Memory<byte> buffer, CancellationToken cancel)
        {
            ThrowIfDisposed();

            if (!Thread.CurrentThread.IsBackground)
                Debug.LogWarning("In main thread!");

            int current = tasknum++;

            // Debug.Log($"Pipe {_guid.ToString()} read {current}, wants {buffer.Length}");
            PipeChunk chunk = null;
            while(!_readEOF)
            {
                if (_chunks.TryPeek(out chunk)) break;
                if(cancel.IsCancellationRequested) throw new TaskCanceledException();
                Thread.Sleep(1);                
            }

            if (chunk == null)
            {
                // Debug.Log("... hit EOF");
                _readEOF = true;
                return 0;
            }

            int chunkLength = chunk.data.Length - chunk.consumed;

            // Never empty, because otherwise it has to be already thrown out.
            Debug.Assert(chunkLength > 0);

            int receivingLength = Math.Min(chunkLength, buffer.Length);

            ReadOnlyMemory<byte> slice = chunk.data.Slice(chunk.consumed, receivingLength);
            slice.CopyTo(buffer);

            chunk.consumed += receivingLength;

            if (chunk.consumed >= chunk.data.Length) _chunks.TryDequeue(out _);

            // Debug.Log($"... got {receivingLength} bytes");
            return receivingLength;
        }

        public void Write(ReadOnlyMemory<byte> buffer)
        {
            ThrowIfDisposed();

            if (_writeEOF)
                throw new IOException("Write past close");

            // No sense to queue empty chunks.
            if(buffer.Length == 0) return;

            _chunks.Enqueue(new() { data = buffer, consumed = 0 });
            // Debug.Log($"Pipe {_guid.ToString()} Enqueued {buffer.Length} bytes");
            return;
        }

        public void CloseWrite()
        {
            ThrowIfDisposed();

            if (!_writeEOF)
                _chunks.Enqueue(null);
            _writeEOF = true;

            // Debug.Log($"Pipe {_guid.ToString()} Enqueued EOF");
        }

        // ---------------------------------------------------------------

        private bool _disposed = false;


        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            _readEOF = true;
            if (_chunks.Count > 0)
                Debug.Log($"Pipe {_guid.ToString()} Discarding data in pipe, {_chunks.Count} chunks");
        }

        private void ThrowIfDisposed()
        {
            if (_disposed) throw new ObjectDisposedException(GetType().FullName);
        }
    }
}
