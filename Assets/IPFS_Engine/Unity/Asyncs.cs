using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

namespace Ipfs.Unity
{
    /// <summary>
    /// Helpers to interface Coroutines with async routines, together with completion
    /// and exception callbacks, like JavaScript's Promise/then/catch scheme.
    /// </summary>
    public static class Asyncs
    {
        private static bool CatchFaulted(Task t, Action<Exception> failCallback)
        {
            Exception e = null;
            if (t.IsCanceled)
                e = new TaskCanceledException();
            else if (t.IsFaulted)
                e = t.Exception.InnerException; // Look for the cause in the task.

            if (e == null) return false;

            if (failCallback == null)
                throw e;

            failCallback.Invoke(e);
            return true;
        }

        public static IEnumerator Async2Coroutine<T>(Task<T> task, Action<T> callback, Action<Exception> failCallback = null)
        {
            yield return new WaitUntil(() => task.IsCompleted);

            if (!CatchFaulted(task, failCallback))
                callback?.Invoke(task.Result);
        }

        public static IEnumerator Async2Coroutine(Task task, Action<Exception> failCallback = null)
        {
            yield return new WaitUntil(() => task.IsCompleted);

            CatchFaulted(task, failCallback);
        }

    }
}
