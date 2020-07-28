using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

[Serializable]
public class BuildingEffectivity
{
    [Tooltip("Wether or not this policy element is enabled")]
    public bool Enabled;
    [Tooltip("The parameters this policy will change")]
    public List<BuildingParameterModification> ParameterMods;

    public BuildingEffectivity()
    {
        Enabled = false;
        ParameterMods = new List<BuildingParameterModification>();
    }
    public override string ToString()
    {
        string aux = "";
        List<BuildingParameterModification> auxList = ParameterMods.OrderBy(x => x.BuildingType).ToList();
        foreach (BuildingParameterModification bpm in auxList)
        {
            aux += $"►{bpm.ToString()}\r\n";
        }
        aux += "\r\n";
        return aux;
    }
}
