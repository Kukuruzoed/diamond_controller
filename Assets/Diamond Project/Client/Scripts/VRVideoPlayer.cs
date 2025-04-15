using UnityEngine;
using UnityEngine.Video;
using System.IO;
using UnityEngine.Android;
using UnityEngine.InputSystem;

public class VRVideoPlayer : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public string videoFileName = "example_360.mp4"; // Имя файла в папке VR
    public InputAction startVideo;


    private void Awake()
    {
        AutoScanner autoScanner = FindFirstObjectByType<AutoScanner>();
        autoScanner.OnCommandReceived.AddListener((command) => {
            switch (command)
            {
                case Commands.PLAY_PAUSE:
                    PlayPause();
                    break;
            }
        });
    }
    void Start()
    {
        startVideo.Enable();
        startVideo.performed += StartVideo_performed;
        if (!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageRead))
        {
            Permission.RequestUserPermission(Permission.ExternalStorageRead);
        }
    }

    private void StartVideo_performed(InputAction.CallbackContext obj)
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
            Debug.Log("launching video: " + path);
        }
        else
        {
            Debug.LogError("video not found: " + path);
        }
	}

    public void PlayPause()
    {
        if (!videoPlayer.isPaused)
        {
            videoPlayer.Pause();
        }
        else
        {
            videoPlayer.Play();
        }
    }
}