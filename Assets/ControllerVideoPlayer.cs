using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.Video;

public class ControllerVideoPlayer : MonoBehaviour
{
    [SerializeField] private Slider progressSlider;
    [SerializeField] private HoldingButton seekForward;
    [SerializeField] private HoldingButton seekBackward;
    [SerializeField] private Button skipForward;
    [SerializeField] private Button skipBackward;
    [SerializeField] private Button playButton;
    [SerializeField] private Button pauseButton;

    public UnityEvent<bool> OnVideoPaused = new UnityEvent<bool>();
    public UnityEvent<double> OnVideoSeek = new UnityEvent<double>();

    private VideoPlayer player;
    void Start()
    {
        player = GetComponent<VideoPlayer>();
        progressSlider.onValueChanged.AddListener((progress) =>
        {
            player.time = progress * player.length;
        });

        pauseButton.onClick.AddListener(() =>
        {
            player.Pause();
            OnVideoPaused?.Invoke(true);
            playButton.gameObject.SetActive(true);
            pauseButton.gameObject.SetActive(false);
        });

        playButton.onClick.AddListener(() =>
        {
            player.Play(); 
            OnVideoPaused?.Invoke(false);
            playButton.gameObject.SetActive(false);
            pauseButton.gameObject.SetActive(true);
        });

        skipForward.onClick.AddListener(() =>
        {
            if(player.time < player.length - 10)
            {
                player.time += 10;
                player.Play();
                OnVideoSeek?.Invoke(player.time);
            }
        });

        skipBackward.onClick.AddListener(() =>
        {
            if(player.time > 10)
            {
                player.time -= 10;
                player.Play();
                OnVideoSeek?.Invoke(player.time);
            }
        });

        StartCoroutine(SeekCoroutine());
    }

    private void Update()
    {
        progressSlider.SetValueWithoutNotify((float)(player.time / player.length));
    }

    private IEnumerator SeekCoroutine()
    {
        while (true)
        {
            if (seekForward.isDown)
            {
                player.time += 50 * Time.deltaTime;
                yield return new WaitForEndOfFrame();
                OnVideoSeek?.Invoke(player.time);
                player.Play();
                Debug.Log($"{player.time}, {Time.deltaTime}");
            }

            if (seekBackward.isDown)
            {
                player.time -= 50 * Time.deltaTime;
                yield return new WaitForEndOfFrame();
                OnVideoSeek?.Invoke(player.time);
                player.Play();
                Debug.Log($"{player.time}, {Time.deltaTime}");
            }

            yield return new WaitForEndOfFrame();
        }
    }

}
