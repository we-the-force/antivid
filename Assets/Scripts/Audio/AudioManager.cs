using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AudioManager : MonoBehaviour
{
    static AudioManager _instance;

    public UnityAction PlayClick;

    public AudioClip bgmClip;
    [SerializeField]
    bool startBGMOnAwake;

    AudioSource BGMSource;
    List<AudioSource> audioSourceList = new List<AudioSource>();

    [SerializeField]
    GameObject audioSourceObject;

    [SerializeField]
    int audioSourceCount;
    [SerializeField]
    List<bool> audioSourceIsPlaying = new List<bool>();

    [SerializeField, Range(0f, 2f)]
    float transitionDuration = 0.5f;
    [SerializeField]
    bool isTransitioningBGM = false;

    /// <summary>
    /// Lista con los sonidos
    ///  0: Menu Click
    ///  1: Menu Click2
    ///  2: Show Window
    ///  3: Hide window
    ///  4: Evento Quincena
    ///  5: Evento Normal
    ///  6: Construir edificio
    ///  7: Edificio Completado
    ///  8: Evento Alerta Virus
    ///  9: Open Window
    /// 10: CHK BOX Click
    /// 11: Game Over
    /// 12: Vaccine Found
    /// 13: Random Enfermos
    /// </summary>
    [SerializeField]
    List<AudioClip> sfxList = new List<AudioClip>();
    /// <summary>
    /// 0: Main Menu
    /// 1: Plateau
    /// 2: Diamond Dust
    /// 3: AlpenRose
    /// 4: OnYourWayBack
    /// </summary>
    [SerializeField]
    List<AudioClip> bgmList = new List<AudioClip>();

    public AudioClip MenuClick
    {
        get { return sfxList[0]; }
    }
    public AudioClip MenuBack
    {
        get { return sfxList[1]; }
    }
    public AudioClip ShowWindow
    {
        get { return sfxList[2]; }
    }
    public AudioClip HideWindow
    {
        get { return sfxList[3]; }
    }
    public AudioClip EventQuincena
    {
        get { return sfxList[4]; }
    }
    public AudioClip EventNormal
    {
        get { return sfxList[5]; }
    }
    public AudioClip BuildStart
    {
        get { return sfxList[6]; }
    }
    public AudioClip BuildFinish
    {
        get { return sfxList[7]; }
    }
    public AudioClip EventVirusAlert
    {
        get { return sfxList[8]; }
    }
    public AudioClip OpenWindow
    {
        get { return sfxList[9]; }
    }
    public AudioClip ChkBoxClick
    {
        get { return sfxList[10]; }
    }
    public AudioClip GameOver
    {
        get { return sfxList[11]; }
    }
    public AudioClip VaccineFound
    {
        get { return sfxList[12]; }
    }
    public AudioClip RandomSick
    {
        get { return sfxList[13]; }
    }

    public AudioClip MainMenu
    {
        get { return bgmList[0]; }
    }

    public AudioClip Plateau
    {
        get { return bgmList[1]; }
    }
    public AudioClip DiamondDust
    {
        get { return bgmList[2]; }
    }
    public AudioClip AlpenRose
    {
        get { return bgmList[3]; }
    }
    public AudioClip OnYourWayBack
    {
        get { return bgmList[4]; }
    }

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
        //DontDestroyOnLoad(gameObject);

        AddAudioSources();

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

        //PlayClick += Play(MenuClick);
    }

    void AddAudioSources()
    {
        for (int i = 0; i < audioSourceCount; i++)
        {
            GameObject obj = GameObject.Instantiate(audioSourceObject, transform);
            obj.name = $"AudioSource{i + 1}";
        }
    }

    private void Update()
    {
        for (int i = 0; i < audioSourceList.Count; i++)
        {
            audioSourceIsPlaying[i] = audioSourceList[i].isPlaying;
        }
    }

    public void StopBGM()
    {
        BGMSource.Stop();
    }
    public void PlayBGM()
    {
        BGMSource.Play();
    }
    public void ChangeBGM(AudioClip newBGM, float durationOverride = -1)
    {
        if (!isTransitioningBGM)
        {
            Debug.Log($"Changing BGM from {BGMSource.clip.name} to {newBGM.name}");
            StartCoroutine(SwitchBGM(newBGM, durationOverride != -1 ? durationOverride : transitionDuration));
        }
    }

    IEnumerator SwitchBGM(AudioClip newBGM, float duration)
    {
        isTransitioningBGM = true;
        yield return fadeSource(BGMSource, BGMSource.volume, 0, duration);
        StopBGM();
        BGMSource.clip = newBGM;
        PlayBGM();
        yield return fadeSource(BGMSource, BGMSource.volume, 1, duration);
        isTransitioningBGM = false;
    }

    IEnumerator fadeSource(AudioSource toFade, float initialValue, float targetValue, float duration)
    {
        float currentTime = 0;
        float currentProgress = 0;
        do
        {
            if (duration != 0)
            {
                currentProgress = currentTime / duration;
            }
            else
            {
                currentProgress = 1;
            }

            toFade.volume = Mathf.SmoothStep(initialValue, targetValue, currentProgress);

            currentTime += Time.unscaledDeltaTime;
            yield return null;
        } while (currentTime < duration + 0.1f);
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
