using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VaccineManager : MonoBehaviour
{
    static VaccineManager _instance = null;

    [SerializeField]
    float _progressPerSliderLevel;
    [SerializeField]
    float _costPerSliderLevel;

    [SerializeField]
    float _currentProgress;
    [SerializeField]
    float _baseProgressPerTic;
    [SerializeField]
    float _currentProgressPerTic;
    float _extraProgressPerTic;
    [SerializeField]
    float _progressCostPerTic;
    bool _shouldTic = false;
    bool _vaccineGenerated = false;


    int _currentTic;
    [SerializeField]
    int _ticCutout = 80;

    int _ticsToStart = 0;
    public int TicsToStart = 160;

    [SerializeField]
    Text _progressText;
    [SerializeField]
    Image _progressFill;
    [SerializeField]
    Slider _resourceSlider;

    bool vaccineStarted;
    bool vaccine5Percent;
    bool vaccine20Percent;
    bool vaccine40Percent;
    bool vaccine50Percent;
    bool vaccineCompleted;

    public float CurrentProgress
    {
        get { return _currentProgress; }
    }
    public float BaseProgressPerTic
    {
        get { return _baseProgressPerTic; }
    }
    public float CurrentProgressPerTic
    {
        get { return _currentProgressPerTic; }
    }
    public float ExtraProgressPerTic
    {
        get { return _extraProgressPerTic; }
    }
    public float ProgressCostPerTic
    {
        get { return _progressCostPerTic; }
    }
    public bool ShouldTic
    {
        get { return _shouldTic; }
        set { _shouldTic = value; }
    }
    public bool IsVaccineGenerated
    {
        get { return _vaccineGenerated; }
    }

    public static VaccineManager Instance
    {
        get { return _instance; }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);

        _currentProgress = 0;
        _shouldTic = false;
        _vaccineGenerated = false;
        _currentProgressPerTic = _baseProgressPerTic + _extraProgressPerTic;
        _currentTic = 0;

        vaccineStarted = false;
        vaccine5Percent = false;
        vaccine20Percent = false;
        vaccine40Percent = false;
        vaccine50Percent = false;
        vaccineCompleted = false;
    }
    private void Start()
    {
        WorldManager.TicDelegate += OnWorldTic;
    }

    public void OnWorldTic()
    {
        if (_shouldTic)
        {
            if (_ticsToStart < TicsToStart)
            {
                _ticsToStart++;
            }
            else
            {
                if (!vaccineStarted)
                {
                    vaccineStarted = true;
                    CanvasControl.instance.ShowVaccineIcon(true);
                    CanvasControl.instance._announcementWindow.SpecialEvent(GlobalObject.SpecialEventName.StartVaccineStudy);

                    CurrencyManager.Instance.nextTutorial = 3;
                    CurrencyManager.Instance.playTutorial = true;
                }
                if (_currentTic <= _ticCutout)
                {
                    _currentProgress += _currentProgressPerTic;
                    ShowProgressInUI();

                    if (_currentProgress >= 5f && !vaccine5Percent)
                    {
                        vaccine5Percent = true;
                        CanvasControl.instance._announcementWindow.SpecialEvent(GlobalObject.SpecialEventName.FirstStage);
                    }
                    if (_currentProgress >= 20f && !vaccine20Percent)
                    {
                        vaccine20Percent = true;
                        CanvasControl.instance._announcementWindow.SpecialEvent(GlobalObject.SpecialEventName.SecondStage);

                        CurrencyManager.Instance.nextTutorial = 2;
                        CurrencyManager.Instance.playTutorial = true;
                    }
                    if (_currentProgress >= 40f && !vaccine40Percent)
                    {
                        vaccine40Percent = true;
                        CanvasControl.instance._announcementWindow.SpecialEvent(GlobalObject.SpecialEventName.Vaccine40percent);
                    }
                    if (_currentProgress >= 50f && !vaccine50Percent)
                    {
                        vaccine50Percent = true;
                        CanvasControl.instance._announcementWindow.SpecialEvent(GlobalObject.SpecialEventName.Vaccine50percent);
                    }
                    if (_currentProgress >= 100f && !vaccineCompleted)
                    {
                        vaccineCompleted = true;
                        _currentProgress = 100;
                        Debug.LogError(">>> La vacuna se ha completado, deberia mandar el evento especial");
                        FinishVaccine();
                    }
                    _currentTic = 0;
                }
            }
        }
    }

    public float vaccineCost()
    {
        float _cost = 0;

        _cost = (_resourceSlider.value * _costPerSliderLevel) * WorldAgentController.instance.AgentCollection.Count;

        _cost = Mathf.Round(_cost * 100f) / 100f;

        return _cost;
    }

    public void ChangeExtraTicPerFrame()
    {
        _extraProgressPerTic = _resourceSlider.value * _progressPerSliderLevel;

        _progressCostPerTic = vaccineCost(); // _resourceSlider.value * _costPerSliderLevel;

        CanvasControl.instance.OnVaccineCostChange();

        _currentProgressPerTic = _baseProgressPerTic + _extraProgressPerTic;
    }

    void ShowProgressInUI()
    {
        _progressText.text = _currentProgress.ToString("F2");
        _progressFill.fillAmount = _currentProgress / 100f;
    }

    public bool IsVaccineDone()
    {
        if (_currentProgress >= 100f)
        {
            return true;
        }
        return false;
    }

    void FinishVaccine()
    {
        _vaccineGenerated = true;
        _shouldTic = false;
        CanvasControl.instance._announcementWindow.SpecialEvent(GlobalObject.SpecialEventName.VaccineCompleted);
    }
}
