using Debug = UnityEngine.Debug;
using System.Text;
using System.IO;
using Curly;

public class EasyGet : SampleBase
{
    public override void RunSample()
    {
        using Easy easy = Curl.GetEasy();

        using MemoryStream stream = new();
        DataCallbackCopier dataCopier = new(stream);

        //easy.SetOpt(CURLoption.URL, "http://127.0.0.1:5003");
        easy.SetOpt(CURLoption.URL, "http://httpbin.org/ip");
        easy.SetOpt(CURLoption.WRITEFUNCTION, dataCopier.DataHandler);

        var result = easy.Perform();

        Debug.Log($"Result code: {Curl.StrError(result)} ({(int) result}).");
        Debug.Log("Response body:");

        Debug.Log(Encoding.UTF8.GetString(stream.ToArray()));

    }
}