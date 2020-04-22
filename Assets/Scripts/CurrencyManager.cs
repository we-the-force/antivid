using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CurrencyManager : MonoBehaviour
{
    static CurrencyManager _instance = null;

    [SerializeField]
    float _currentCurrency;
    [SerializeField]
    float initialCurrency;
    [SerializeField]
    float extraIncome;

    [SerializeField]
    Text currentCurrencyText;
    [SerializeField]
    Image cycleProgressImage;
    [SerializeField]
    float currentProgress = 0;

    [SerializeField]
    Text currentUpkeep;
    [SerializeField]
    Text currentIncome;
    [SerializeField]
    Text currentExtraIncome;
    [SerializeField]
    Text totalResourceGain;

    [SerializeField]
    int ticCutout = 30;
    [SerializeField]
    int currentTic = 0;

    public float CurrentCurrency
    {
        get { return _currentCurrency; }
        set { _currentCurrency = value; UpdateCurrencyText(); /*Debug.Log($"Thing changed by {value}");*/ }
    }
    public static CurrencyManager Instance
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
    }
    void Start()
    {
        WorldManager.TicDelegate += OnWorldTic;
        CurrentCurrency = initialCurrency;
        currentTic = 0;

        float buildingCosts = WorldAgentController.instance.TotalBuildingUpkeepCost;
        float agentIncome = WorldAgentController.instance.TotalAgentIncome;
        float totalResource = agentIncome + extraIncome - buildingCosts;

        UpdateIncomeText(buildingCosts, agentIncome, totalResource);
    }

    public void OnWorldTic()
    {
        currentTic++;
        if (currentTic > ticCutout)
        {
            currentTic = 0;
            float buildingCosts = WorldAgentController.instance.TotalBuildingUpkeepCost;
            float agentIncome = WorldAgentController.instance.TotalAgentIncome;
            float totalResource = agentIncome + extraIncome - buildingCosts;

            UpdateIncomeText(buildingCosts, agentIncome, totalResource);

            CurrentCurrency += totalResource;

            //CurrentCurrency += extraIncome;

            //CurrentCurrency -= buildingCosts;
            //CurrentCurrency += agentIncome;
        }
        currentProgress = 1f / ticCutout * currentTic;
        UpdateCycleImage(currentProgress);
    }
    public bool HasEnoughCurrency(float toCompare)
    {
        return CurrentCurrency >= toCompare;
    }
    void UpdateCurrencyText()
    {
        int _currencyToShow = Mathf.FloorToInt(_currentCurrency);

        currentCurrencyText.text = $"${_currencyToShow}";
    }
    void UpdateCycleImage(float progress)
    {
        cycleProgressImage.fillAmount = progress;
    }
    void UpdateIncomeText(float buildingCost, float agentIncome, float totalResource)
    {
        currentUpkeep.text = $"${buildingCost}";
        currentIncome.text = $"${agentIncome}";
        currentExtraIncome.text = $"+ ${extraIncome}";

        totalResourceGain.text = $"{totalResource}";
        totalResourceGain.color = totalResource > 0 ? Color.green : totalResource < 0 ? Color.red : Color.gray;
    }
}
