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
            float policyCosts = WorldAgentController.instance.TotalPolicyUpkeepCost;
            float agentIncome = WorldAgentController.instance.TotalAgentIncome;
            float vaccineCost = VaccineManager.Instance.ProgressCostPerTic;
            float totalResource = agentIncome + extraIncome - buildingCosts - policyCosts - vaccineCost;

            WorldManager.instance.currentTimeCycle++;

            //--- Contar la poblacion 
            int healtyPop = 0;
            int mildPop = 0;
            int seriousPop = 0;
            int outPop = 0;
            for (int i = 0; i < WorldAgentController.instance.AgentCollection.Count; i++)
            {
                AgentStatus _status = WorldAgentController.instance.AgentCollection[i].myStatus;

                switch (_status)
                {
                    case AgentStatus.Healty:
                    case AgentStatus.Inmune:
                        healtyPop++;
                        break;
                    case AgentStatus.Mild_Case:
                    case AgentStatus.BeingTreated:
                        mildPop++;
                        break;
                    case AgentStatus.Serious_Case:
                        seriousPop++;
                        break;
                    case AgentStatus.Out_of_circulation:
                        outPop++;
                        break;
                }
            }
                       
            for (int i = 0; i < WorldManager.instance.StatisticMinMaxCollection.Count; i++)
            {
                StatisticMinMaxObject stat = WorldManager.instance.StatisticMinMaxCollection[i];

                StatisticObject statObj = new StatisticObject();
                statObj.Entry = stat.Entry;
                statObj.TimeCycle = WorldManager.instance.currentTimeCycle;

                switch (stat.Entry)
                {
                    case GlobalObject.StatisticEntry.AgentIncome:
                        if (agentIncome < stat.minValue)
                            stat.minValue = agentIncome;

                        if (agentIncome > stat.maxValue)
                            stat.maxValue = agentIncome;

                        statObj.Type = 0;
                        statObj.Value = agentIncome;
                        break;

                    case GlobalObject.StatisticEntry.BuildingCosts:
                        if (buildingCosts < stat.minValue)
                            stat.minValue = buildingCosts;

                        if (buildingCosts > stat.maxValue)
                            stat.maxValue = buildingCosts;

                        statObj.Type = 0;
                        statObj.Value = buildingCosts;

                        break;

                    case GlobalObject.StatisticEntry.ExtraIncome:
                        if (extraIncome < stat.minValue)
                            stat.minValue = extraIncome;

                        if (extraIncome > stat.maxValue)
                            stat.maxValue = extraIncome;

                        statObj.Type = 0;
                        statObj.Value = extraIncome;

                        break;

                    case GlobalObject.StatisticEntry.HealtyPop:
                        if (healtyPop < stat.minValue)
                            stat.minValue = healtyPop;

                        if (healtyPop > stat.maxValue)
                            stat.maxValue = healtyPop;
                        statObj.Type = 1;
                        statObj.Value = healtyPop;
                        break;

                    case GlobalObject.StatisticEntry.MildSickness:
                        if (mildPop < stat.minValue)
                            stat.minValue = mildPop;

                        if (mildPop > stat.maxValue)
                            stat.maxValue = mildPop;
                        statObj.Type = 1;
                        statObj.Value = mildPop;
                        break;

                    case GlobalObject.StatisticEntry.OutOfOrder:
                        if (outPop < stat.minValue)
                            stat.minValue = outPop;

                        if (outPop > stat.maxValue)
                            stat.maxValue = outPop;
                        statObj.Type = 1;
                        statObj.Value = outPop;
                        break;

                    case GlobalObject.StatisticEntry.PolicyCosts:
                        if (policyCosts < stat.minValue)
                            stat.minValue = policyCosts;

                        if (policyCosts > stat.maxValue)
                            stat.maxValue = policyCosts;
                        statObj.Type = 0;
                        statObj.Value = policyCosts;
                        break;

                    case GlobalObject.StatisticEntry.SeriousSickness:
                        if (seriousPop < stat.minValue)
                            stat.minValue = seriousPop;

                        if (seriousPop > stat.maxValue)
                            stat.maxValue = seriousPop;
                        statObj.Type = 1;
                        statObj.Value = seriousPop;
                        break;

                    case GlobalObject.StatisticEntry.VaccineCost:
                        if (vaccineCost < stat.minValue)
                            stat.minValue = vaccineCost;

                        if (vaccineCost > stat.maxValue)
                            stat.maxValue = vaccineCost;
                        statObj.Type = 0;
                        statObj.Value = vaccineCost;
                        break;
                }

                WorldManager.instance.StatisticCollection.Add(statObj);
            }
            
            //Debug.Log($"b: {buildingCosts}, p: {policyCosts}, a: {agentIncome} ({agentIncome} + {extraIncome} - {buildingCosts} - {policyCosts} = {totalResource})");

            UpdateIncomeText(buildingCosts, agentIncome, totalResource);
            CurrentCurrency += totalResource;

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

        extraIncome += auxIncome;

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
