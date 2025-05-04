using System.Collections;
using NUnit.Framework.Constraints;
using RenderHeads.Media.AVProVideo;
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
    [SerializeField] private float skipSeconds = 5;

    public UnityEvent<bool> OnVideoPaused = new UnityEvent<bool>();
    public UnityEvent<double> OnVideoSeek = new UnityEvent<double>();

    private MediaPlayer avVideoPlayer;
    
    void Start()
    {
        ConfigureAVVideoPlayer();
    }

    void ConfigureAVVideoPlayer()
    {
        avVideoPlayer = GetComponent<MediaPlayer>();

        avVideoPlayer.Events.AddListener((MediaPlayer mp, MediaPlayerEvent.EventType eventType, ErrorCode errorCode) =>
        {
            switch (eventType)
            {
                case MediaPlayerEvent.EventType.Started:
                    playButton.gameObject.SetActive(false);
                    pauseButton.gameObject.SetActive(true);
                    break;
            }
        });

        progressSlider.onValueChanged.AddListener((progress) =>
        {
            avVideoPlayer.Control.SeekFast((progress * avVideoPlayer.Info.GetDurationMs()));
            OnVideoSeek?.Invoke(avVideoPlayer.Control.GetCurrentTimeMs());
        });

        pauseButton.onClick.AddListener(() =>
        {
            avVideoPlayer.Control.Pause();
            OnVideoPaused?.Invoke(true);
            playButton.gameObject.SetActive(true);
            pauseButton.gameObject.SetActive(false);
        });

        playButton.onClick.AddListener(() =>
        {
            avVideoPlayer.Control.Play();
            OnVideoPaused?.Invoke(false);
            playButton.gameObject.SetActive(false);
            pauseButton.gameObject.SetActive(true);
        });

        skipForward.onClick.AddListener(() =>
        {
            float seekTime = Mathf.Clamp(avVideoPlayer.Control.GetCurrentTimeMs() + (skipSeconds * 1000), 0f, avVideoPlayer.Info.GetDurationMs());

            Debug.Log($"[Controller] SeekTime: {seekTime}, current time: {avVideoPlayer.Control.GetCurrentTimeMs()}, duration: {avVideoPlayer.Info.GetDurationMs()}");

            avVideoPlayer.Control.SeekFast(seekTime);
            OnVideoSeek?.Invoke(avVideoPlayer.Control.GetCurrentTimeMs());
            Debug.Log($"[Controller] Current time: {avVideoPlayer.Control.GetCurrentTimeMs()}");
        });

        skipBackward.onClick.AddListener(() =>
        {
            float seekTime = Mathf.Clamp(avVideoPlayer.Control.GetCurrentTimeMs() - (skipSeconds * 1000), 0f, avVideoPlayer.Info.GetDurationMs());
            avVideoPlayer.Control.SeekFast(seekTime);
            OnVideoSeek?.Invoke(avVideoPlayer.Control.GetCurrentTimeMs());
            Debug.Log($"[Controller] SeekTime: {seekTime}");
        });

        seekForward.OnHoldEnded.AddListener(() =>
        {
            avVideoPlayer.Control.SetPlaybackRate(1);
            avVideoPlayer.Control.SeekFast(avVideoPlayer.Control.GetCurrentTimeMs());
            OnVideoSeek?.Invoke(avVideoPlayer.Control.GetCurrentTimeMs());
        });

        seekBackward.OnHoldEnded.AddListener(() =>
        {
            avVideoPlayer.Control.SetPlaybackRate(1); 
            avVideoPlayer.Control.SeekFast(avVideoPlayer.Control.GetCurrentTimeMs());
            OnVideoSeek?.Invoke(avVideoPlayer.Control.GetCurrentTimeMs());
        });

        seekForward.OnHoldStarted.AddListener(() =>
        {
            avVideoPlayer.Control.SetPlaybackRate(3);
        });

        seekBackward.OnHoldStarted.AddListener(() =>
        {
            avVideoPlayer.Control.SetPlaybackRate(-3);
        });
    }

    private void Update()
    {
        if (avVideoPlayer.Info.GetDurationMs() > 0)
        {
            float progressValue = (float)(avVideoPlayer.Control.GetCurrentTimeMs() / avVideoPlayer.Info.GetDurationMs());
            progressSlider.SetValueWithoutNotify(progressValue);
        }
    }
}
