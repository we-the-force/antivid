using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class LimitBuilding 
{
    [Tooltip("Wether or not this policy element is enabled")]
    public bool Enabled;
    [Tooltip("The buildings that will be changed")]
    public GlobalObject.NeedScale BuildingToChange;
    [Tooltip("The percentage to lower the limit of the building by"), Range(0, 2)]
    public float Percentage;

    public LimitBuilding()
    {
        Enabled = false;
        BuildingToChange = GlobalObject.NeedScale.None;
        Percentage = 0; 
    }
    public override string ToString()
    {
        string aux = "";
        aux += $"Modifying {BuildingToChange}'s limit by {Percentage * 100}%";
        return aux;
    }
}
