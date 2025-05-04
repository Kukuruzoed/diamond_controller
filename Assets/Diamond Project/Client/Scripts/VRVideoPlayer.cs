using UnityEngine;
using UnityEngine.Video;
using System.IO;
using UnityEngine.Android;
using UnityEngine.InputSystem;
using System.Collections;
using RenderHeads.Media.AVProVideo;
using UnityEngine.Events;
using TMPro;

public class VRVideoPlayer : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI infoText;
    [SerializeField] private GameObject infoPanel;
    [SerializeField] private MediaPlayer avMediaPlayer;
    [SerializeField] private string videoFileName = "example_360.mp4";
    [SerializeField] private InputAction startVideo;
    [SerializeField] private Mesh mesh360;
    [SerializeField] private Mesh mesh180;
    [SerializeField] private MeshFilter meshFilter;

    private void Awake()
    {

        IPScanner autoScanner = FindFirstObjectByType<IPScanner>();
        autoScanner.OnCommandReceived.AddListener((command) => {
            if (command.Contains(Commands.PLAY_VIDEO))
            {
                StartVideo(command.Replace(Commands.PLAY_VIDEO, ""));
                infoPanel.SetActive(false);
            }
            else if (command.Contains(Commands.PLAY))
            {
                avMediaPlayer.Play();
                infoPanel.SetActive(false);
            }
            else if (command.Contains(Commands.PAUSE))
            {
                avMediaPlayer.Pause();
            }
            else if(command.Contains(Commands.SEEK))
            {
                float time = float.Parse(command.Replace(Commands.SEEK, "").Replace(",", "."));
                Debug.Log($"Time: {time}");
                PlayFromTime(time);
            }
            else if(command.Contains(Commands.SET_FORMAT))
            {
                Set360Format(command.Replace(Commands.SET_FORMAT, "").Equals("1"));
            }
        });
    }

    private void Set360Format(bool is360)
    {
        meshFilter.mesh = is360 ? mesh360 : mesh180;
    }

    public void PlayFromTime(float millisSeconds)
    {
        if (avMediaPlayer.Control != null && avMediaPlayer.Info != null)
        {
            float seekTime = Mathf.Clamp(millisSeconds, 0f, avMediaPlayer.Info.GetDurationMs());
            avMediaPlayer.Control.SeekFast(seekTime);
            Debug.Log($"SeekTime: {seekTime}");
        }
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
#if UNITY_ANDROID && !UNITY_EDITOR
		string path = $"/storage/emulated/0/VR/{videoName}";
#else
        string path = $"{Application.persistentDataPath}/VR/{videoName}";
#endif
        if (File.Exists(path))
        {
            Debug.Log("launching video: " + path);
            avMediaPlayer.m_VideoPath = path;
            avMediaPlayer.m_VideoLocation = MediaPlayer.FileLocation.AbsolutePathOrURL;
            avMediaPlayer.OpenVideoFromFile(MediaPlayer.FileLocation.AbsolutePathOrURL, path, autoPlay: true);
        }
        else
        {
            infoPanel.SetActive(true);
            infoText.text = $"Не найдено видео {videoName}";
            Debug.LogError("video not found: " + path);
        }
	}
}