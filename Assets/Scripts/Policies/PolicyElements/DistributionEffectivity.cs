using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

[Serializable]
public class DistributionEffectivity
{
    [Tooltip("Wether or not this policy element is enabled")]
    public bool Enabled;
    [Tooltip("The parameters this policy will change")]
    public List<DistributionParameterModification> ParameterMods;

    public DistributionEffectivity()
    {
        Enabled = false;
        ParameterMods = new List<DistributionParameterModification>();
    }

    public void Reset()
    {
        ParameterMods.Clear();
    }

    public override string ToString()
    {
        string aux = "";
        foreach (DistributionParameterModification dpm in ParameterMods)
        {
            aux += $"►{dpm.ToString()}\r\n";
        }
        return aux;
    }
}