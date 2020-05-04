using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalObject
{
    [System.Serializable]
    public enum AgentStatus
    {
        Healty,
        Mild_Case,
        Serious_Case,
        Out_of_circulation,
        Inmune,
        BeingTreated
    }

    [System.Serializable]
    public enum NeedScale
    {
        Hunger,
        Sleep,
        Entertainment,
        HealtCare,
        Education,
        Wander, //--- Esta necesidad activa una accion idle del agente que lo lleva a deambular por las calles nomas asi, selecciona un tile de camino al azar y se mueve para alla
        None,
        Travel
    }

    [System.Serializable]
    public enum AgentPerk
    {
        None = 0,
        Athletic,
        Workaholic,
        Introvert,
        Extrovert,
        LeasureTraveler,
        Executive,
        Gamer,
        Random
    }

    [System.Serializable]
    public enum SpecialEventName
    {
        FirstInfected,
        StartVaccineStudy,
        FirstStage,
        SecondStage,
        Vaccine40percent,
        Vaccine50percent,
        VaccineCompleted
    }

    [System.Serializable]
    public enum StatisticEntry
    {
        HealtyPop,
        MildSickness,
        SeriousSickness,
        OutOfOrder,
        AgentIncome,
        ExtraIncome,
        BuildingCosts,
        PolicyCosts,
        VaccineCost
    }
}

[System.Serializable]
public class StatisticObject
{
    public int Type;
    public int TimeCycle;
    public float Value;
    public GlobalObject.StatisticEntry Entry;
}

[System.Serializable]
public class StatisticMinMaxObject
{
    public GlobalObject.StatisticEntry Entry;
    public float minValue;
    public float maxValue;
}

[System.Serializable]
public class BuildingCanvasEntry
{
    public GameObject objPrefab;
    public string Name;
    public string Description;
    public float Cost;
    public float TimeToBuild;
    public Sprite ButtonImage;
    public Sprite BigImage;
}

[System.Serializable]
public class AnimationForPerk
{
    public GlobalObject.AgentPerk Perk;
    public GameObject AnimationPrefab;
}

[System.Serializable]
public class PathCost
{
    public int TileID;
    public int Cost;
}

[System.Serializable]
public class PerkPercentages
{
    public GlobalObject.AgentPerk Perk;
    public List<NeedPercentage> NeedPercentageCollection;
    public float resourceProduction;
    public bool WillAttendHospitalOnMildCase;
    public bool WillRandomlyAttendHospital;
    public float PercentageForBackPack;

    public List<NeedPercentage> CloneNeedPercentage()
    {
        List<NeedPercentage> auxList = new List<NeedPercentage>();
        foreach (NeedPercentage np in NeedPercentageCollection)
        {
            auxList.Add(np.Clone());
        }
        return auxList;
    }
}

[System.Serializable]
public class PerkQuantityForScenario
{
    public GlobalObject.AgentPerk Perk;
    public float Percentage;
    public int Qty { get; set; }
    public int CreatedQty { get; set; }
}

[System.Serializable]
public class WarehouseItemObject
{
    public GlobalObject.NeedScale Need;
    public float CurrentQty;
    public float BaseMaxQty;
    public float CurrentMaxQty;
}


[System.Serializable]
public class ScenarioInfo
{
    public string ScenarioName;
    public string ScenarioDescription;
    public string ScenarioSceneName;
    public int TotalPopulation;
    public int StartingHospitals;
    public int StartingFoodBuildings;
    public int StartingEntertainmentBuildings;
    public int StartingEducationBuildings;
    public float StartingCapital;
    public float MaxScore;
}


[System.Serializable]
public class NeedPercentage
{
    public GlobalObject.NeedScale Need;
    public float PercentageToCompare = 100;

    public float CurrentPercentage/* { get; set; }*/;

    // public float PercentageToAddPerTic;

    public float TicsToCoverNeed = 10;
    public float TicValue = 1;

    private float _quarter = 0;

    public bool MaxPercentage()
    {
        if (CurrentPercentage > PercentageToCompare)
        {
            _quarter += 0.25f;

            if (_quarter >= 1.0f)
            {
                CurrentPercentage++;
                _quarter = 0;
            }
            return true;
        }
        else
        {
            CurrentPercentage += TicValue;
            return false;
        }
    }

    public float PercentageDifference()
    {
        
        if (CurrentPercentage > PercentageToCompare)
        {
            _quarter += 0.25f;

            if (_quarter >= 1.0f)
            {
                CurrentPercentage += TicValue;
                _quarter = 0;
            }
        }
        else
        {

        CurrentPercentage += TicValue;
        }
        return (CurrentPercentage - PercentageToCompare);
    }

    public NeedPercentage Clone()
    {
        NeedPercentage auxNP = new NeedPercentage()
        {
            Need = this.Need,
            PercentageToCompare = this.PercentageToCompare,
            CurrentPercentage = this.CurrentPercentage,
            TicsToCoverNeed = this.TicsToCoverNeed,
            TicValue = this.TicValue,
            _quarter = this._quarter
        };
        return auxNP;
    }
}