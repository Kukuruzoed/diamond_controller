using UnityEngine;
using UnityEngine.Video;
using System.IO;

public class VRVideoPlayer : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public string videoFileName = "example_360.mp4"; // Имя файла в папке VR

    void Start()
    {
        StartVideo(videoFileName);
    }
	
	public void StartVideo(string videoName)
	{
		string path = "/storage/emulated/0/VR/" + videoName;

        if (File.Exists(path))
        {
            videoPlayer.source = VideoSource.Url;
            videoPlayer.url = "file://" + path;
            videoPlayer.Play();
            Debug.Log("Запускаем видео: " + path);
        }
        else
        {
            Debug.LogError("Видео не найдено: " + path);
        }
	}
}