using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PolicyToggle : MonoBehaviour
{
    [SerializeField]
    Policy assignedPolicy;
    public Text policyNameText;
    public Toggle policyToggle;

    private void Start()
    {
        assignedPolicy.Enabled = false;
        policyToggle.isOn = assignedPolicy.Enabled;
        SetPolicyName();
    }
    public void SetAssignedPolicy(Policy pol)
    {
        assignedPolicy = pol;
    }
    public void SetPolicyName()
    {
        policyNameText.text = assignedPolicy.PolicyName;
    }
    public void Toggle()
    {
        PolicyManager.Instance.TogglePolicy(assignedPolicy, policyToggle.isOn);
    }
}
