using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class VideoController : MonoBehaviour
{
    public Toggle showHideToggle;
    public Button skipButton;
    public Image fadeImage;
    public VideoPlayer videoPlayer;


    [SerializeField, Range(0.1f, 2f)]
    float transitionDuration;

    private void Awake()
    {
        fadeImage.CrossFadeAlpha(0, transitionDuration * 3f, true);
    }

    private void Start()
    {
        StartCoroutine(PlayVideo());
    }

    public void ShowHideUI()
    {
        skipButton.gameObject.SetActive(showHideToggle.isOn);
    }

    public void SkipIntro()
    {
        StartCoroutine(Skip());
    }

    IEnumerator PlayVideo()
    {
        videoPlayer.Play();
        yield return new WaitForSeconds(0.1f);
        do
        {
            //Debug.Log($"Playing video: {videoPlayer.isPlaying} ({videoPlayer.time}/{videoPlayer.clip.length})");
            yield return null;
        } while (videoPlayer.time < 175f);
        //Debug.Log($"Finished playing video");
        yield return Skip();
    }

    IEnumerator Skip()
    {
        //yield return WaitForSeconds(FadeOut());
        yield return FadeOut();

        ///Este debe ser el primer scene, el segundo debe ser el menu principal

        SceneManager.LoadScene(1);
    }

    IEnumerator FadeOut()
    {
        fadeImage.CrossFadeAlpha(1f, transitionDuration, false);
        float currentTime = 0;
        float currentProgress = 0;
        do
        {
            if (currentProgress < transitionDuration)
            {
                currentProgress = currentTime / transitionDuration;
                videoPlayer.SetDirectAudioVolume(0, Mathf.SmoothStep(videoPlayer.GetDirectAudioVolume(0), 0, currentProgress));
            }
            currentTime += Time.unscaledDeltaTime;
            //Debug.Log($"CurrentVolume: {videoPlayer.GetDirectAudioVolume(0)}");
            yield return null;
        } while (currentTime < transitionDuration + 0.15f);
        //yield return new WaitForSeconds(transitionDuration + 0.15f);

        Debug.Log($"Thingie {fadeImage.color.a}, limit {transitionDuration + 0.15f}");
    }
}