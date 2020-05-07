using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PolicyManager : MonoBehaviour
{
    static PolicyManager _instance = null;

    [SerializeField]
    List<Policy> _policies = new List<Policy>();

    [SerializeField]
    GameObject viewportContent;
    [SerializeField]
    GameObject baseToggle;

    [SerializeField]
    bool _isOnQarantine;
    public static PolicyManager Instance
    {
        get { return _instance; }
    }
    public bool IsOnQuarantine
    {
        get { return _isOnQarantine; }
    }
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        _instance = this;

        DontDestroyOnLoad(gameObject);

        object[] objects = Resources.LoadAll("Policies", typeof(Policy));
        foreach (object ob in objects)
        {
            _policies.Add(ObjectToPolicy(ob));
        }
        DisableAllPolicies();
        CreateToggles();
    }
    void DisableAllPolicies()
    {
        foreach (Policy pol in _policies)
        {
            pol.Enabled = false;
        }
    }
    public void CreateToggles()
    {
        DestroyUnavailablePolicyToggles();
        foreach (Policy pol in _policies)
        {
            if (pol.Available && !ViewportContainsPolicy(pol))
            {
                GameObject go = Instantiate(baseToggle as GameObject);
                go.SetActive(true);
                PolicyToggle pt = go.GetComponent<PolicyToggle>();
                pt.SetAssignedPolicy(pol);
                go.transform.SetParent(viewportContent.transform);
                go.transform.localScale = Vector3.one;
                go.transform.localRotation = Quaternion.Euler(0, 0, 0);
                go.transform.localPosition = Vector3.zero;
            }
        }
    }
    void DestroyUnavailablePolicyToggles()
    {
        for (int i = 0; i < viewportContent.transform.childCount; i++)
        {
            if (!viewportContent.transform.GetChild(i).GetComponent<PolicyToggle>().AssignedPolicy.Available)
            {
                Destroy(viewportContent.transform.GetChild(i).gameObject);
            }
            //Destroy(viewportContent.transform.GetChild(i).gameObject);
        }
    }
    bool ViewportContainsPolicy(Policy toCheck)
    {
        for (int i = 0; i < viewportContent.transform.childCount; i++)
        {
            if (viewportContent.transform.GetChild(i).GetComponent<PolicyToggle>().AssignedPolicy == toCheck)
            {
                Debug.Log($"Policy was contained {toCheck.ToString()}");
                return true;
            }
        }
        Debug.Log($"Policy wasn't contained {toCheck.ToString()}");
        return false;
    }
    List<Policy> GetEnabledPolicies()
    {
        return _policies.FindAll(x => x.Enabled);
    }

    Policy ObjectToPolicy(object toTransform)
    {
        Policy aux = (Policy)toTransform;
        Debug.LogWarning(aux.ToString());
        return aux;
    }

    public void TogglePolicy(Policy policy, bool enabled)
    {
        if (enabled)
        {
            CurrencyManager.Instance.CurrentCurrency -= policy.InitialCost;
        }

        policy.Enabled = enabled;
        SendPoliciesToWorldManager();
        Debug.LogWarning(PoliciesToString());
        _isOnQarantine = IsInQuarantine();
        Debug.Log($"Changed policy {policy.PolicyName} to {enabled}");
        if (enabled)
        {
            AudioManager.Instance.Play(AudioManager.Instance.ChkBoxClick);
        }
        else
        {
            AudioManager.Instance.Play(AudioManager.Instance.MenuBack);
        }
        //Policy aux = _policies.Find(x => x.PolicyName == policyName);
        //if (aux != null)
        //{
        //    aux.Enabled = !aux.Enabled;
        //    SendPoliciesToWorldManager();
        //    Debug.LogWarning(PoliciesToString());
        //}
    }
    public void SendPoliciesToWorldManager()
    {
        WorldAgentController.instance.ReceivePolicies(GetEnabledPolicies());
    }
    bool IsInQuarantine()
    {
        foreach (Policy pol in GetEnabledPolicies())
        {
            if (pol.IsQuarantine)
            {
                return true;
            }
        }
        return false;
    }
    public string PoliciesToString()
    {
        string aux = "";
        aux += "Policies\r\n\r\n";
        foreach (Policy pol in _policies)
        {
            aux += $"○{pol.ToString()}\r\n";
        }
        return aux;
    }

    public void EnablePolicy(int policyID)
    {
        for (int i = 0; i < _policies.Count; i++)
        {
            if (_policies[i].PolicyID == policyID)
            {
                _policies[i].Available = true;
                break;
            }
        }

    }
}