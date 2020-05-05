using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    static AudioManager _instance;

    public AudioClip bgmClip;
    [SerializeField]
    bool startBGMOnAwake;

    AudioSource BGMSource;
    List<AudioSource> audioSourceList = new List<AudioSource>();
    [SerializeField]
    List<bool> audioSourceIsPlaying = new List<bool>();

    public static AudioManager Instance
    {
        get { return _instance; }
    }

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
        }
        _instance = this;

        BGMSource = transform.GetChild(0).GetComponent<AudioSource>();
        for (int i = 1; i < transform.childCount; i++)
        {
            audioSourceList.Add(transform.GetChild(i).GetComponent<AudioSource>());
            audioSourceIsPlaying.Add(audioSourceList[i - 1].isPlaying);
        }
        BGMSource.clip = bgmClip;
        if (startBGMOnAwake)
        {
            BGMSource.Play();
        }
    }

    private void Update()
    {
        for (int i = 0; i < audioSourceList.Count; i++)
        {
            audioSourceIsPlaying[i] = audioSourceList[i].isPlaying;
        }
    }

    public void Play(AudioClip auClip)
    {
        if (HasAvailableAudioSource())
        {
            StartCoroutine(PlayClip(GetAvailableAudioSource(), auClip));
        }
    }

    IEnumerator PlayClip(AudioSource audSource, AudioClip auClip)
    {
        audSource.clip = auClip;
        audSource.Play();
        yield return null;
        do
        {
            //Debug.Log($"isPlaying {audSource.isPlaying}, ({audSource.time}, {audSource.clip.length})");
            yield return null;
        } while (audSource.isPlaying);

        audSource.clip = null;
    }

    AudioSource GetAvailableAudioSource()
    {
        return audioSourceList.Find(x => !x.isPlaying);
    }
    bool HasAvailableAudioSource()
    {
        foreach (AudioSource auso in audioSourceList)
        {
            if (!auso.isPlaying)
            {
                return true;
            }
        }
        return false;
    }
}
