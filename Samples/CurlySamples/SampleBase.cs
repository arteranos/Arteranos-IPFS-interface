using UnityEngine;
using System.Threading.Tasks;
using Curly;

public abstract class SampleBase : MonoBehaviour
{
    protected Curl Curl = null;

    public void Start()
    {
        Curl = new();
        Task.Run(() => { RunSample(); });
    }

    public void OnDestroy()
    {
        Curl?.Dispose();
    }

    public abstract void RunSample();
}