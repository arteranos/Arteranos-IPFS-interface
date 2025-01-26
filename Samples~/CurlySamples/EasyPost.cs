using Debug = UnityEngine.Debug;
using System.Text;
using System.IO;
using Curly;

public class EasyPost : SampleBase
{
    public override void RunSample()
    {
        using Easy easy = Curl.GetEasy();

        using MemoryStream stream = new();
        using DataCallbackCopier dataCopier = new(stream);
        easy.SetOpt(CURLoption.WRITEFUNCTION, dataCopier.DataHandler);

        string postData = "fieldname1=fieldvalue1&fieldname2=fieldvalue2";

        easy.SetOpt(CURLoption.URL, "http://httpbin.org/post");

        // This one has to be called before setting COPYPOSTFIELDS.
        easy.SetOpt(CURLoption.POSTFIELDSIZE, Encoding.ASCII.GetByteCount(postData));
        easy.SetOpt(CURLoption.COPYPOSTFIELDS, postData);

        CURLcode result = easy.Perform();

        Debug.Log($"Result code: {Curl.StrError(result)} ({(int)result}).");
        Debug.Log("Response body:");

        Debug.Log(Encoding.UTF8.GetString(stream.ToArray()));

    }
}