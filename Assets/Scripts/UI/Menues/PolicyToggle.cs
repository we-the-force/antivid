using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PolicyToggle : MonoBehaviour
{
    [SerializeField]
    Policy _assignedPolicy;
    public Text policyNameText;
    public Toggle policyToggle;

    public Text txtInitialCost;
    public Text txtUpkeepCost;

    public Policy AssignedPolicy
    {
        get { return _assignedPolicy; }
    }

    private void Start()
    {
        _assignedPolicy.Enabled = false;
        policyToggle.isOn = _assignedPolicy.Enabled;
        SetPolicyName();
    }
    public void SetAssignedPolicy(Policy pol)
    {
        _assignedPolicy = pol;
    }
    public void SetPolicyName()
    {
        policyNameText.text = _assignedPolicy.PolicyName;

        txtInitialCost.text = _assignedPolicy.InitialCost.ToString();
        txtUpkeepCost.text = _assignedPolicy.UpkeepCost.ToString();
    }
    public void Toggle()
    {
        PolicyManager.Instance.TogglePolicy(_assignedPolicy, policyToggle.isOn);
    }
}
