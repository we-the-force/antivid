using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum DistributionParameter { None, Food, Education, Entertainment }
[Serializable]
public class DistributionParameterModification
{
    [Tooltip("Parameter to change")]
    public DistributionParameter Parameter;
    [Tooltip("Percentage to change it by")]
    public int Units;
    public DistributionParameterModification()
    {
        Parameter = DistributionParameter.None;
        Units = 0;
    }
    public DistributionParameterModification(DistributionParameter distPara, int units)
    {
        Parameter = distPara;
        Units = units;
    }

    public void ResetUnits()
    {
        Units = 0;
    }

    public override string ToString()
    {
        string aux = "";
        aux += $"Need: {Parameter} by {Units} per tic";
        return aux;
    }
}
