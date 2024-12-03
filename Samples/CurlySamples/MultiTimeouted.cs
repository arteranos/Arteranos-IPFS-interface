using Debug = UnityEngine.Debug;
using System.Text;
using Curly;
using System.Threading.Tasks;
using System.Threading;

public class MultiTimeouted : SampleBase
{
    private int totalBytes = 0;

    public override void RunSample()
    {
        string postData = "fieldname1=fieldvalue1&fieldname2=fieldvalue2";

        CancellationTokenSource tokenSource = new();

        // Thread 1: Alive until the request is gone through, or canceled
        Task<CURLcode> taskResult = Task.Run(() => 
        {
            using Multi multi = new();
            Easy easy = Curl.GetEasy();

            try
            {
                DataCallbackFunc dataCopier = new(b => DataReadFunc(easy, b));

                easy.SetOpt(CURLoption.URL, "http://httpbin.org/post");
                // easy.SetOpt(CURLoption.URL, "http://127.0.0.1:4999");
                easy.SetOpt(CURLoption.WRITEFUNCTION, dataCopier.DataHandler);

                easy.SetOpt(CURLoption.POSTFIELDSIZE, Encoding.ASCII.GetByteCount(postData));
                easy.SetOpt(CURLoption.COPYPOSTFIELDS, postData);

                multi.AddHandle(easy);

                int still_running = 0;
                while (true)
                {
                    if (tokenSource.Token.IsCancellationRequested) 
                        return CURLcode.ABORTED_BY_CALLBACK;

                    CURLMcode mc = multi.Perform(ref still_running);

                    if (mc != CURLMcode.OK) Debug.LogWarning($"Perform: {mc}");
                    if (still_running == 0) break;

                    if (mc == CURLMcode.OK) mc = multi.Poll(1000);
                    if (mc != CURLMcode.OK) Debug.LogWarning($"Poll: {mc}");
                }

                return CURLcode.OK;
            }
            finally
            {
                multi.RemoveHandle(easy);
                easy.Dispose();
            }
        });

        // Thread 2: Wait 5 s, then try to cancel thread 1.
        Task task2Result = Task.Run(async () =>
        {
            await Task.Delay(5000);
            // Commented out for the benefit of not exposing Multi,
            // but for the expense of letting the Poll() through.
            // multi.Wakeup();
            tokenSource.Cancel();
        });

        CURLcode result = taskResult.Result;

        Debug.Log($"Result code: {Curl.StrError(result)} ({(int)result}).");
        Debug.Log($"Response body length: {totalBytes}");

    }

    public int DataReadFunc(Easy easy, byte[] buffer)
    {
        if (totalBytes == 0)
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