
using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

namespace Ipfs.Http
{
    public static class Utils
    {
        public static readonly string sample_Dir    = "QmS4ustL54uo8FzR9455qaxZwuMiUhyvMcX9Ba8nUH4uVv";
        public static readonly string sample_File   = "QmS4ustL54uo8FzR9455qaxZwuMiUhyvMcX9Ba8nUH4uVv/about";
        public static IEnumerator Async2Coroutine<T>(Task<T> task, Action<T> callback)
        {
            yield return new WaitUntil(() => task.IsCompleted);

            callback?.Invoke(task.Result);
        }
    }
}