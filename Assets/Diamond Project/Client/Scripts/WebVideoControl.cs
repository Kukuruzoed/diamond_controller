using UnityEngine;

public class WebVideoControl : MonoBehaviour
{
    WebViewObject webView;

    void Start()
    {
        webView = (new GameObject("WebViewObject")).AddComponent<WebViewObject>();
        webView.Init();
        webView.SetMargins(0, 0, 0, 0);

#if UNITY_ANDROID && !UNITY_EDITOR
        string url = "file:///android_asset/html/index.html";
#else
        //string url = System.IO.Path.Combine(Application.streamingAssetsPath, "html/index.html");
        string url = System.IO.Path.Combine(Application.streamingAssetsPath, "https://www.google.com");
#endif

        webView.LoadURL(url);
        webView.SetVisibility(true);

    }

    public void Play() => webView.EvaluateJS("Unity.call('play')");
    public void Pause() => webView.EvaluateJS("Unity.call('pause')");
    public void Seek(float seconds) => webView.EvaluateJS($"Unity.call('seek:{seconds}')");
    public void SwitchTo180() => webView.EvaluateJS("Unity.call('mode:180')");
    public void SwitchTo360() => webView.EvaluateJS("Unity.call('mode:360')");
    public void LoadVideo(string path) => webView.EvaluateJS($"Unity.call('src:{path}')");
}

