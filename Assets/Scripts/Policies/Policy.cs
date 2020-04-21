using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

public enum QuarantineType { Light, Medium, Severe };

[Serializable]
[CreateAssetMenu(fileName = "New Policy", menuName = "Policy")]
public class Policy : ScriptableObject
{
    public string PolicyName;
    public string PolicyDescription;
    public bool Enabled = false;
    public bool IsQuarantine = false;
    public float UpkeepCost;

    public TransformBuilding TransformBuildingSection;
    public LimitBuilding LimitBuildingSection;

    public BuildingEffectivity BuildingEffectivitySection;
    public DistributionEffectivity DistributionEffectivitySection;

    public override string ToString()
    {
        string aux = "";
        aux += $"'{PolicyName}'        ({(Enabled ? "Enabled" : "Disabled")})\r\n\r\n{PolicyDescription.Replace(@"\r\n","\r\n")}";
        aux += "\r\n\r\n---Sections---\r\n";
        if (IsQuarantine)
        {
            aux += $"••Quarantine••\r\n";
        }
        if (TransformBuildingSection.Enabled)
        {
            aux += $"•Transform Building\r\n    {TransformBuildingSection.ToString()}\r\n";
        }
        if (LimitBuildingSection.Enabled)
        {
            aux += $"•Limit Building    {LimitBuildingSection.ToString()}\r\n";
        }
        if (BuildingEffectivitySection.Enabled)
        {
            aux += $"•Building Effectivity\r\n{BuildingEffectivitySection.ToString()}\r\n";
        }
        if (BuildingEffectivitySection.Enabled)
        {
            aux += $"•Distribution Effectivity\r\n{DistributionEffectivitySection.ToString()}\r\n";
        }
        return aux;
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(Policy), true)]
[CanEditMultipleObjects]
public class PolicyEditor : Editor
{
    private Policy policy { get { return (target as Policy); } }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        //if (GUILayout.Button("Coso"))
        //{
        //    string aux = policy.PolicyDescription.Replace(@"\r\n", "\r\n");
        //    Debug.Log(aux);
        //}
    }
}
#endif
