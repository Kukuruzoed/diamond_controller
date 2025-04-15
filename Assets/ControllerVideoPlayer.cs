using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class ControllerVideoPlayer : MonoBehaviour
{
    [SerializeField] private Slider progressSlider;
    [SerializeField] private Button skipForward;
    [SerializeField] private Button skipBackward;

    private VideoPlayer player;
    void Start()
    {
        player = GetComponent<VideoPlayer>();
        progressSlider.onValueChanged.AddListener((progress) =>
        {
            player.time = progress * player.length;
        });

        skipForward.onClick.AddListener(() =>
        {
            if(player.time < player.length - 10)
            {
                player.time += 10;
                player.Play();
            }
        });

        skipBackward.onClick.AddListener(() =>
        {
            if(player.time > 10)
            {
                player.time -= 10;
                player.Play();
            }
        });
    }

    private void Update()
    {
        progressSlider.SetValueWithoutNotify((float)(player.time / player.length));
    }

}
