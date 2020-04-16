using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class TransformBuilding
{
    [Tooltip("Wether or not this policy element is enabled")]
    public bool Enabled;
    [Tooltip("The buildings that will be changed")]
    public GlobalObject.NeedScale BuildingToChange;
    [Tooltip("What the buildings will be changed to")]
    public GlobalObject.NeedScale TargetBuilding;
    [Tooltip("The percentage of buildings to be temporarily turned to the target building"), Range(0, 1)]
    public float Percentage;

    public TransformBuilding()
    {
        Enabled = false;
        BuildingToChange = GlobalObject.NeedScale.None;
        TargetBuilding = GlobalObject.NeedScale.None;
        Percentage = 0;
    }

    public override string ToString()
    {
        string aux = "";
        aux += $"Turning {Percentage * 100f}% of {BuildingToChange} to {TargetBuilding}";
        return aux;
    }
}