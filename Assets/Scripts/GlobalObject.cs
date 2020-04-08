﻿using System.Collections;
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
        Inmune
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
        None
    }

    [System.Serializable]
    public enum AgentPerk
    {
        None = 0,
        Athletic,
        Workaholic,
        Introvert,
        Extrovert
    }
}

[System.Serializable]
public class NeedPercentage
{
    public GlobalObject.NeedScale Need;
    public int PercentageToCompare;
    public int CurrentPercentage;
    public int TicValue;

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
}