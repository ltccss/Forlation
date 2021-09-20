using UnityEngine.Networking;
using System.Collections;
using UnityEngine;

public class RequestHandler
{
    public string fileName;
    public UnityWebRequest request;
    public int errorCount = 0;
    public bool requestDisposed = false;
    /// <summary>
    /// 是否下载完成，成功的才算完成
    /// </summary>
    public bool isDone = false;

    public void StartRequest()
    {
        if (errorCount == 0)
        {
            _RequestImmediately();
        }
        else
        {
            _RequestDelay(3f);
        }
    }

    private void _RequestDelay(float duration)
    {
        CoroutineUtil.StartCoroutine(_RequestCo(duration));
    }

    private IEnumerator _RequestCo(float duration)
    {
        yield return new WaitForSeconds(duration);
        request.SendWebRequest();
    }

    private void _RequestImmediately()
    {
        if (request != null)
        {
            request.SendWebRequest();
        }
    }
}
