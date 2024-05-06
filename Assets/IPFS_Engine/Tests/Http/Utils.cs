
using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

namespace Ipfs.Http
{
    public static class Utils
    {
        public static readonly string sample_Dir        = "QmS4ustL54uo8FzR9455qaxZwuMiUhyvMcX9Ba8nUH4uVv";
        public static readonly string sample_File       = "QmS4ustL54uo8FzR9455qaxZwuMiUhyvMcX9Ba8nUH4uVv/about";

        // Second node 'next to' the tested application - configure with 'ipfs --repo-dir=/path/to/IPFS init' and run
        public static readonly string sample_LANPeer    = "12D3KooWN861g7WNzNJSUMGN4H3fUvB82djer2cEKPp8hri8x1uK";

        public static IEnumerator Async2Coroutine<T>(Task<T> task, Action<T> callback)
        {
            yield return new WaitUntil(() => task.IsCompleted);

            callback?.Invoke(task.Result);
        }

        public static IEnumerator Async2Coroutine(Task task)
        {
            yield return new WaitUntil(() => task.IsCompleted);
        }
    }
}