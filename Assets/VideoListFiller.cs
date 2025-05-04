using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine;
using TMPro;
using UnityEngine.Android;
using UnityEngine.UI;

public class VideoListFiller : MonoBehaviour
{
    [SerializeField] GameObject videoItemPrefab;
    [SerializeField] GameObject videoNotFoundText;
    [SerializeField] Transform scrollContent;

    private void Awake()
    {
        if (!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageRead))
        {
            Permission.RequestUserPermission(Permission.ExternalStorageRead);
        }
        PopulateVideoList();
    }
    void PopulateVideoList()
    {
        Debug.Log("Start populating");
        string rootPath = GetExternalStoragePath(); // точка входа для обхода всей файловой системы на Android
        List<string> foundFiles = new List<string>();

        try
        {
            var stack = new Stack<string>();
            stack.Push(rootPath);

            while (stack.Count > 0)
            {
                string currentDir = stack.Pop();

                Debug.Log(Directory.GetDirectories(currentDir));
                try
                {
                    foreach (var dir in Directory.GetDirectories(currentDir))
                    {
                        Debug.Log(dir);
                        if (Path.GetFileName(dir).Equals("VR", StringComparison.OrdinalIgnoreCase))
                        {
                            string[] videoFiles = Directory.GetFiles(dir);
                            foreach (string file in videoFiles)
                            {
                                if (file.EndsWith(".mp4") || file.EndsWith(".mov") || file.EndsWith(".avi"))
                                {
                                    foundFiles.Add(file);
                                }
                            }
                            FillList(foundFiles);
                            return;
                        }
                        stack.Push(dir);
                    }
                }
                catch { }
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning("[Server] Error while searching for video files: " + e.Message);
        }

        
    }
    void FillList(List<string> foundFiles)
    {
        Debug.Log(foundFiles.Count);
        if (foundFiles.Count == 0)
        {
            return;
        }

        videoNotFoundText.SetActive(false);
        int fileIndex = 0;
        foreach (string file in foundFiles)
        {
            GameObject item = Instantiate(videoItemPrefab, scrollContent);
            Vector2 pos = item.GetComponent<RectTransform>().anchoredPosition;
            pos.y = -120 * fileIndex - 70;
            item.GetComponent<RectTransform>().anchoredPosition = pos;
            item.SetActive(true);
            TextMeshProUGUI label = item.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null)
            {
                label.text = Path.GetFileName(file);
            }
            item.GetComponent<Button>().onClick.AddListener(() => FindFirstObjectByType<ServerUI>().StartVideo(file));
            fileIndex++;
        }

        Vector2 size = scrollContent.GetComponent<RectTransform>().sizeDelta;
        size.y = foundFiles.Count * 120; // например, хотим высоту 300
        scrollContent.GetComponent<RectTransform>().sizeDelta = size;
    }

    string GetExternalStoragePath()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
    using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
    using (var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
    using (var env = new AndroidJavaClass("android.os.Environment"))
    {
        AndroidJavaObject file = env.CallStatic<AndroidJavaObject>("getExternalStorageDirectory");
        return file.Call<string>("getAbsolutePath");
    }
#else
        return Application.persistentDataPath;
#endif
    }
}
