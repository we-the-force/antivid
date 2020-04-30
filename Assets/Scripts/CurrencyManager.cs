using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NeedScale = GlobalObject.NeedScale;
using AgentStatus = GlobalObject.AgentStatus;

public class CurrencyManager : MonoBehaviour
{
    static CurrencyManager _instance = null;

    [SerializeField]
    float _currentCurrency;
    [SerializeField]
    float initialCurrency;
    float extraIncome;

    [SerializeField]
    float sleepUseCost;
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

    private float GetMinMaxValue(List<float> valueList, out float MaxValue)
    {
        float minValue = 0;
        MaxValue = 0;

        for (int i = 0; i < valueList.Count; i++)
        {
            if (valueList[i] < minValue)
                minValue = valueList[i];

            if (valueList[i] > MaxValue)
                MaxValue = valueList[i];
        }
        return minValue;
    }


    public int FinalCycleCounter { get; set; }
    public void OnWorldTic()
    {
        currentTic++;

        //--- Aqui se marca el ciclo de cada quincena del juego
        //--- Tambien aqui se actualizan los datos estadisticos
        if (currentTic > ticCutout)
        {
            currentTic = 0;
            float buildingCosts = WorldAgentController.instance.TotalBuildingUpkeepCost;

            buildingCosts += costToCure;

            float policyCosts = WorldAgentController.instance.TotalPolicyUpkeepCost;
            float agentIncome = WorldAgentController.instance.TotalAgentIncome;
            float vaccineCost = VaccineManager.Instance.ProgressCostPerTic;
            float totalResource = agentIncome + extraIncome - buildingCosts - policyCosts - vaccineCost;

            WorldManager.instance.currentTimeCycle++;

            UpdateIncomeText(buildingCosts, agentIncome, totalResource);
            CurrentCurrency += totalResource;

            //--- Contar la poblacion 
            int healtyPop = 0;
            int mildPop = 0;
            int seriousPop = 0;
            int outPop = 0;

            float happiness = 0;
            float totPop = 0;
            for (int i = 0; i < WorldAgentController.instance.AgentCollection.Count; i++)
            {
                AgentStatus _status = WorldAgentController.instance.AgentCollection[i].myStatus;

                if (_status != AgentStatus.Out_of_circulation)
                {
                    happiness += WorldAgentController.instance.AgentCollection[i].HappinessCoeficient;
                    totPop++;
                }               

                switch (_status)
                {
                    case AgentStatus.Healty:
                    case AgentStatus.Inmune:
                        healtyPop++;
                        break;
                    case AgentStatus.Mild_Case:
                        mildPop++;
                        break;
                    case AgentStatus.BeingTreated:
                    case AgentStatus.Serious_Case:
                        seriousPop++;
                        break;
                    case AgentStatus.Out_of_circulation:
                        outPop++;
                        break;
                }
            }

            happiness = happiness / totPop;
            CanvasControl.instance.txtHappiness.text = happiness.ToString();

            GraphController statWindow = CanvasControl.instance.GraphWindowController;

            statWindow.HealtyPopList.Add(healtyPop);
            statWindow.MildSicknessList.Add(mildPop);
            statWindow.SeriousSicknessList.Add(seriousPop);
            statWindow.OutofOrderList.Add(outPop);

            List<float> economyList = new List<float>();

            statWindow.AgentIncomeList.Add(Mathf.RoundToInt(agentIncome));
            economyList.Add(agentIncome);

            statWindow.ExtraIncomeList.Add(Mathf.RoundToInt(extraIncome));
            economyList.Add(extraIncome);

            statWindow.BuildingCost.Add(Mathf.RoundToInt(buildingCosts));
            economyList.Add(buildingCosts);

            statWindow.VaccioneCosts.Add(Mathf.RoundToInt(vaccineCost));
            economyList.Add(vaccineCost);

            statWindow.PolicyCosts.Add(Mathf.RoundToInt(policyCosts));
            economyList.Add(policyCosts);

            statWindow.TotalIncomeList.Add(Mathf.RoundToInt(CurrentCurrency));
            economyList.Add(CurrentCurrency);

            float minValue = 0;
            float maxValue = 0;
            minValue = GetMinMaxValue(economyList, out maxValue);

            if (minValue < statWindow.MinEconomy)
                statWindow.MinEconomy = minValue;
            if (maxValue > statWindow.MaxEconomy)
                statWindow.MaxEconomy = maxValue;  
            //Debug.Log($"b: {buildingCosts}, p: {policyCosts}, a: {agentIncome} ({agentIncome} + {extraIncome} - {buildingCosts} - {policyCosts} = {totalResource})");

            extraIncome = 0;

            if (VaccineManager.Instance.IsVaccineGenerated)
            {
                //--- La vacuna fue generada, esta contando los ultimos ciclos
                FinalCycleCounter++;
                if (FinalCycleCounter == 2)
                {
                    //--- END THIS SCENARIO
                    WorldManager.instance.ChangeTimeScale(0);
                    CanvasControl.instance.EndScenario();
                    return;
                }
            }
            else
            {
                //--- Determina si va a haber un evento Random
                CanvasControl.instance._announcementWindow.RandomEvent();
            }
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

    private float costToCure = 0;
    public void UseBuilding(NeedScale type, AgentStatus status = AgentStatus.Healty)
    {
        //float auxIncome = 0;
        switch (type)
        {
            case NeedScale.Sleep:
                extraIncome += sleepUseCost;
                break;
            case NeedScale.Hunger:
                extraIncome += foodUseCost;
                break;
            case NeedScale.Entertainment:
                extraIncome += entertainmentUseCost;
                break;
            case NeedScale.Education:
                extraIncome += educationUseCost;
                break;
            case NeedScale.HealtCare:
                if (status == AgentStatus.Mild_Case)
                {
                    costToCure += healthcareUseCost * 2;
                }
                else if (status == AgentStatus.Serious_Case)
                {
                    costToCure += healthcareUseCost * 4;
                }
                else
                {
                    costToCure += healthcareUseCost;
                }
                break;
            case NeedScale.Travel:
                extraIncome += travelUseCost;
                break;
        }
        //Debug.Log($"Currency Changed by {auxIncome} ({CurrentCurrency} => {(CurrentCurrency - auxIncome)})");

        //extraIncome += auxIncome;

        //CurrentCurrency += auxIncome;
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
