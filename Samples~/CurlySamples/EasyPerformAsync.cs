using Debug = UnityEngine.Debug;
using System.Text;
using Curly;
using System.Threading.Tasks;

public class EasyPerformAsync : SampleBase
{
    private int totalBytes = 0;

    public override void RunSample()
    {
        string postData = "fieldname1=fieldvalue1&fieldname2=fieldvalue2";

        Task<CURLcode> taskResult = Task.Run(() => {
            using Easy easy = Curl.GetEasy();

            DataCallbackFunc dataCopier = new(b => DataReadFunc(easy, b));

            easy.SetOpt(CURLoption.URL, "http://httpbin.org/post");
            easy.SetOpt(CURLoption.WRITEFUNCTION, dataCopier.DataHandler);

            easy.SetOpt(CURLoption.POSTFIELDSIZE, Encoding.ASCII.GetByteCount(postData));
            easy.SetOpt(CURLoption.COPYPOSTFIELDS, postData);

            return easy.Perform(); 
        });

        CURLcode result = taskResult.Result;

        Debug.Log($"Result code: {Curl.StrError(result)} ({(int)result}).");
        Debug.Log($"Response body length: {totalBytes}");

    }

    public int DataReadFunc(Easy easy, byte[] buffer)
    {
        if(totalBytes == 0)
        {
            easy.GetInfo(CURLINFO.RESPONSE_CODE, out int statusCode);
            easy.GetInfo(CURLINFO.CONTENT_TYPE, out string contentType);

            Debug.Log("Starting to receive content");
            Debug.Log($"HTTP Status code: {statusCode}");
            Debug.Log($"Content type: {contentType}");
        }

        totalBytes += buffer.Length;
        return buffer.Length;
    }
}