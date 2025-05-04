using System.IO;
using RenderHeads.Media.AVProVideo;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class ServerUI : MonoBehaviour
{
    [SerializeField] private ControllerVideoPlayer videoPlayerController;
    [SerializeField] private CommandServer server;
    [SerializeField] private GameObject videoPlayerUI;
    [SerializeField] private GameObject videosListUI;
    [SerializeField] private TextMeshProUGUI clientsCountText;
    [SerializeField] private TextMeshProUGUI versionText;
    [SerializeField] private MediaPlayer avMediaPlayer;
    [SerializeField] private Button mode360;
    [SerializeField] private Button mode180;

    private void Awake()
    {
        versionText.text = $"Версия {Application.version}";

        videoPlayerController.OnVideoPaused.AddListener((isPaused) => 
        {
            server.SendCommandToAll(isPaused ? Commands.PAUSE : Commands.PLAY);
        });

        videoPlayerController.OnVideoSeek.AddListener((time) =>
        {
            server.SendCommandToAll($"{Commands.SEEK}{time}");
        });

        server.OnConnectedClientCountChanged.AddListener((clientsCount) => 
        {
            clientsCountText.text = $"Клиентов подключено - {clientsCount}";
            FindFirstObjectByType<MammothProtect>(FindObjectsInactive.Include).gameObject.SetActive(true);
        });

        mode360.onClick.AddListener(() =>
        {
            server.SendCommandToAll($"{Commands.SET_FORMAT}1");
            mode360.gameObject.SetActive(false);
            mode180.gameObject.SetActive(true);
        });

        mode180.onClick.AddListener(() =>
        {
            server.SendCommandToAll($"{Commands.SET_FORMAT}0");
            mode180.gameObject.SetActive(false);
            mode360.gameObject.SetActive(true);
        });
    }

    public void StartVideo(string fullName)
    {
        videoPlayerUI.gameObject.SetActive(true);
        Vector2 videosListOffsetMax = videosListUI.GetComponent<RectTransform>().offsetMax;
        videosListOffsetMax.y = -620;
        videosListUI.GetComponent<RectTransform>().offsetMax = videosListOffsetMax;
        server.SendCommandToAll($"{Commands.PLAY_VIDEO}{Path.GetFileName(fullName)}");

        avMediaPlayer.m_VideoPath = fullName;
        avMediaPlayer.m_VideoLocation = MediaPlayer.FileLocation.AbsolutePathOrURL;
        avMediaPlayer.OpenVideoFromFile(MediaPlayer.FileLocation.AbsolutePathOrURL, fullName, autoPlay: true);
    }
}
