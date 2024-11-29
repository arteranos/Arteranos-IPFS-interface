using System;

namespace Curly
{
    public class ManagedHandle<T> : IDisposable
    {
        public delegate T Creator();
        public delegate void Destructor(T resource);

        public static implicit operator T(ManagedHandle<T> handle) => handle.resource;

        private Destructor destructor = null;

        private T resource = default;

        public ManagedHandle(Creator creator, Destructor destructor)
        {
            this.destructor = destructor;
            resource = creator();
        }

        ~ManagedHandle()
        {
            Dispose(false);
        }

        public void Attach(T newResource, bool detachOld = true)
        {
            if (detachOld) destructor?.Invoke(resource);
            resource = newResource;
        }

        public T Detach()
        {
            destructor = null;

            return resource;
        }

        // ---------------------------------------------------------------
        public void Dispose()
        {
            GC.SuppressFinalize(this);
            Dispose(true);
        }

        private void Dispose(bool _ /* disposing */)
        {
            lock (this)
            {
                // if (disposing) /* cleanup managed resources */
                // cleanup unmanaged resources
                destructor?.Invoke(resource);
                destructor = null;
            }
        }
    }
}
