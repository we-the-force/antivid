﻿using System.Collections;
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
    float _progressCostPerTic;
    bool _shouldTic = false;
    bool _vaccineGenerated = false;


    int _currentTic;
    [SerializeField]
    int _ticCutout = 80;

    [SerializeField]
    Text _progressText;
    [SerializeField]
    Image _progressFill;
    [SerializeField]
    Slider _resourceSlider;

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
    }
    private void Start()
    {
        WorldManager.TicDelegate += OnWorldTic;
    }

    public void OnWorldTic()
    {
        if (_shouldTic)
        {
            if (_currentTic <= _ticCutout)
            {
                _currentProgress += _currentProgressPerTic;
                ShowProgressInUI();
                if (_currentProgress >= 100f)
                {
                    _currentProgress = 100;
                    FinishVaccine();
                }
                _currentTic = 0;
            }
        }
    }
    public void ChangeExtraTicPerFrame(float newValue)
    {
        _extraProgressPerTic = _resourceSlider.value * _progressPerSliderLevel;
        _progressCostPerTic = _resourceSlider.value * _costPerSliderLevel;

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

        //TODO: Que el jugador gane o algo
    }
}
