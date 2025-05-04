using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using UnityEngine;

public class LocalHttpServer : MonoBehaviour
{
    private HttpListener listener;
    private Thread listenerThread;
    private string rootPath;

    void Start()
    {
        rootPath = Path.Combine(Application.streamingAssetsPath, "html");
        listener = new HttpListener();
        listener.Prefixes.Add("http://localhost:12345/");
        listener.Start();

        listenerThread = new Thread(HandleIncomingRequests);
        listenerThread.Start();
    }

    void OnApplicationQuit()
    {
        listener.Stop();
        listenerThread.Abort();
    }

    private void HandleIncomingRequests()
    {
        while (listener.IsListening)
        {
            try
            {
                var context = listener.GetContext();
                string urlPath = context.Request.Url.AbsolutePath.TrimStart('/');

                if (string.IsNullOrEmpty(urlPath)) urlPath = "index.html";

                string fullPath = Path.Combine(rootPath, urlPath);

                if (File.Exists(fullPath))
                {
                    byte[] buffer = File.ReadAllBytes(fullPath);
                    context.Response.ContentType = GetContentType(fullPath);
                    context.Response.ContentLength64 = buffer.Length;
                    context.Response.OutputStream.Write(buffer, 0, buffer.Length);
                }
                else if (context.Request.Url.AbsolutePath == "/config")
                {
                    string json = "{\"video\": \"example_360.mp4\", \"startTime\": 0, \"mode\": \"360\"}";
                    byte[] buffer = Encoding.UTF8.GetBytes(json);
                    context.Response.ContentType = "application/json";
                    context.Response.ContentLength64 = buffer.Length;
                    context.Response.OutputStream.Write(buffer, 0, buffer.Length);
                }
                else
                {
                    context.Response.StatusCode = 404;
                }

                context.Response.OutputStream.Close();
            }
            catch (Exception e)
            {
                Debug.LogError("[HTTP Server] Error: " + e.Message);
            }
        }
    }

    private string GetContentType(string path)
    {
        if (path.EndsWith(".html")) return "text/html";
        if (path.EndsWith(".js")) return "application/javascript";
        if (path.EndsWith(".css")) return "text/css";
        if (path.EndsWith(".mp4")) return "video/mp4";
        return "application/octet-stream";
    }

    public void OpenUrlInBrowser(string url)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
    try
    {
        using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        using (AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
        using (AndroidJavaObject intent = new AndroidJavaObject("android.content.Intent", "android.intent.action.VIEW"))
        using (AndroidJavaClass uriClass = new AndroidJavaClass("android.net.Uri"))
        {
            AndroidJavaObject uri = uriClass.CallStatic<AndroidJavaObject>("parse", url);
            intent.Call<AndroidJavaObject>("setData", uri);
            currentActivity.Call("startActivity", intent);
        }
    }
    catch (Exception e)
    {
        Debug.LogError("Failed to open browser: " + e);
    }
#else
        Application.OpenURL(url);
#endif
    }
}