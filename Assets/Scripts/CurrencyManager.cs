using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NeedScale = GlobalObject.NeedScale;
using AgentStatus = GlobalObject.AgentStatus;

public class CurrencyManager : MonoBehaviour
{
    static CurrencyManager _instance = null;

    float _currentCurrency;
    [SerializeField]
    float initialCurrency;
    float extraIncome;

    [SerializeField]
    float foodUseCost;
    [SerializeField]
    float entertainmentUseCost;
    [SerializeField]
    float educationUseCost;
    [SerializeField]
    float healthcareUseCost;
    [SerializeField]
    float travelUseCost;

    [SerializeField]
    Text currentCurrencyText;
    [SerializeField]
    Image cycleProgressImage;
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
            float policyCosts = WorldAgentController.instance.TotalPolicyUpkeepCost;
            float agentIncome = WorldAgentController.instance.TotalAgentIncome;
            float vaccineCost = VaccineManager.Instance.ProgressCostPerTic;
            float totalResource = agentIncome + extraIncome - buildingCosts - policyCosts - vaccineCost;

            //Debug.Log($"b: {buildingCosts}, p: {policyCosts}, a: {agentIncome} ({agentIncome} + {extraIncome} - {buildingCosts} - {policyCosts} = {totalResource})");

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
    public void UseBuilding(NeedScale type, AgentStatus status = AgentStatus.Healty)
    {
        float auxIncome = 0;
        switch (type)
        {
            case NeedScale.Hunger:
                auxIncome = foodUseCost;
                break;
            case NeedScale.Entertainment:
                auxIncome = entertainmentUseCost;
                break;
            case NeedScale.Education:
                auxIncome = educationUseCost;
                break;
            case NeedScale.HealtCare:
                if (status == AgentStatus.Mild_Case)
                {
                    auxIncome = healthcareUseCost * 2;
                }
                else if (status == AgentStatus.Serious_Case)
                {
                    auxIncome = healthcareUseCost * 4;
                }
                else
                {
                    auxIncome = healthcareUseCost;
                }
                break;
            case NeedScale.Travel:
                auxIncome = travelUseCost;
                break;
        }
        //Debug.Log($"Currency Changed by {auxIncome} ({CurrentCurrency} => {(CurrentCurrency - auxIncome)})");
        CurrentCurrency += auxIncome;
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
