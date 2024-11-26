using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace CurlThin.SafeHandles
{
    public sealed class SafeLPStr : IDisposable
    {
        private List<byte> lpstrBuffer = null;
        private GCHandle handle;

        public SafeLPStr(string str) 
        { 
            lpstrBuffer = Encoding.UTF8.GetBytes(str).ToList();
            lpstrBuffer.Add(0x00);

            handle = GCHandle.Alloc(lpstrBuffer.ToArray(), GCHandleType.Pinned);
        }

        ~SafeLPStr()
        {
            Dispose(false);
        }

        public static implicit operator IntPtr(SafeLPStr h) => h.handle.AddrOfPinnedObject();

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            Dispose(true);
        }

        public void Dispose(bool disposing)
        {
            lock (this)
            {
                // if (disposing) cleanup managed resources
                // cleanup unmanaged resources
                handle.Free();
            }
        }
    }
}