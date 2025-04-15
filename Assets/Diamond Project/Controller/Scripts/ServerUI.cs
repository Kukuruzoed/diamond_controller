using UnityEngine;
using UnityEngine.UI;

public class ServerUI : MonoBehaviour
{
    [SerializeField] private Button playPause;
    [SerializeField] private Button seekForward;
    [SerializeField] private Button seekBackward;
    [SerializeField] private Button skipForward;
    [SerializeField] private Button skipBackward;

    public CommandServer server;

    private void Awake()
    {
        playPause.onClick.AddListener(() => { OnPlayPauseButton();});
    }

    public void OnPlayPauseButton()
    {
        server.SendCommandToAll(Commands.PLAY_PAUSE);
    }
    public void OnPlayVideoByName(string videoName)
    {
        server.SendCommandToAll(Commands.PLAY_VIDEO + videoName);
    }
}
