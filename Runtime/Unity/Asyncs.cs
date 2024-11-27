using System;
using System.Collections;
using System.Runtime.CompilerServices;
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

        public delegate Task TaskProducer();
        public delegate Task<T> TaskProducer<T>();

        [Obsolete("Use Func<Task<T>>")]
        public static IEnumerator Async2Coroutine<T>(Task<T> task, Action<T> callback, Action<Exception> failCallback = null)
        {
            var taskCA = task.ConfigureAwait(false).GetAwaiter();
            yield return new WaitUntil(() => taskCA.IsCompleted);

            if (!CatchFaulted(task, failCallback))
                callback?.Invoke(task.Result);
        }

        public static IEnumerator Async2Coroutine<T>(TaskProducer<T> taskFunc, Action<T> callback, Action<Exception> failCallback = null)
        {
            Task<T> task = Task.Run<T>(async () => 
            {
                return await taskFunc().ConfigureAwait(false);
            });
            var taskCA = task.ConfigureAwait(false).GetAwaiter();
            yield return new WaitUntil(() => taskCA.IsCompleted);

            if (!CatchFaulted(task, failCallback))
                callback?.Invoke(task.Result);
        }

        [Obsolete("Use Func<Task>")]
        public static IEnumerator Async2Coroutine(Task task, Action<Exception> failCallback = null)
        {
            var taskCA = task.ConfigureAwait(false).GetAwaiter();
            yield return new WaitUntil(() => taskCA.IsCompleted);

            CatchFaulted(task, failCallback);
        }
        public static IEnumerator Async2Coroutine(TaskProducer taskFunc, Action<Exception> failCallback = null)
        {
            Task task = Task.Run(async () =>
            {
                await taskFunc().ConfigureAwait(false);
            });
            var taskCA = task.ConfigureAwait(false).GetAwaiter();
            yield return new WaitUntil(() => taskCA.IsCompleted);

            CatchFaulted(task, failCallback);
        }
    }
}
