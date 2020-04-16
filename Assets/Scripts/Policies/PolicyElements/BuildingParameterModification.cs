using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using NeedScale = GlobalObject.NeedScale;

[Serializable]
public class BuildingParameterModification
{
    public enum BuildingParameter { None, Effectivity, OperationCosts }
    [Tooltip("Building type to affect")]
    public NeedScale BuildingType;
    [Tooltip("Parameter to change")]
    public BuildingParameter ParameterToChange;
    [Tooltip("Percentage to change it by"), Range(0, 4)]
    public float Percentage = 1;

    public BuildingParameterModification()
    {
        BuildingType = NeedScale.None;
        ParameterToChange = BuildingParameter.None;
    }

    public override string ToString()
    {
        string aux = "";
        aux += $"{BuildingType}: {ParameterToChange} => {Percentage * 100}%";
        return aux;
    }
}
