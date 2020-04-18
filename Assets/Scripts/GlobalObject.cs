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
        /*
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
        */
        CurrentPercentage += TicValue;
        //}
        return (CurrentPercentage - PercentageToCompare);
    }
}