using Debug = UnityEngine.Debug;
using System.Text;
using System.IO;
using Curly;

public class EasyHttpHeaders : SampleBase
{
    public override void RunSample()
    {
        using Easy easy = Curl.GetEasy();

        using MemoryStream stream = new();
        using DataCallbackCopier dataCopier = new(stream);

        easy.SetOpt(CURLoption.URL, "http://httpbin.org/headers");
        easy.SetOpt(CURLoption.WRITEFUNCTION, dataCopier.DataHandler);

        using Slist headers = new()
        {
            // Initialize HTTP header list with first value.
            "X-Foo: Bar",
            // Add one more value to existing HTTP header list.
            "X-Qwerty: Asdfgh"
        };

        // Configure libcurl easy handle to send HTTP headers we configured.
        easy.SetOpt(CURLoption.HTTPHEADER, headers);

        CURLcode result = easy.Perform();

        Debug.Log($"Result code: {Curl.StrError(result)} ({(int)result}).");
        Debug.Log("Response body:");

        Debug.Log(Encoding.UTF8.GetString(stream.ToArray()));

    }
}