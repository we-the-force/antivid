using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CanvasControl : MonoBehaviour
{
    public static CanvasControl instance;

    public BuyBuildingWindow BuildWindow;

    public GameObject UIElements;

    public GameObject PolicyWindow;

    public Text txtTotPop;
    public Text txtSick;
    public Text txtDead;
    public Text txtInmune;

    public GameObject policyQuarentine;
    public GameObject policyFood;
    public GameObject policyMoreHospitals;
    public GameObject policyHospitalsEfficient;

    private void Awake()
    {
        instance = this;
    }

    public void Statistic(List<AgentController> fromPopulation)
    {
        int totSick = 0;
        int totDead = 0;
        int totInmune = 0;

        for (int i = 0; i < fromPopulation.Count; i++)
        {
            switch (fromPopulation[i].myStatus)
            {
                case GlobalObject.AgentStatus.Healty:
                    break;
                case GlobalObject.AgentStatus.Inmune:
                    totInmune++;
                    break;
                case GlobalObject.AgentStatus.Mild_Case:
                case GlobalObject.AgentStatus.BeingTreated:
                case GlobalObject.AgentStatus.Serious_Case:
                    totSick++;
                    break;
                case GlobalObject.AgentStatus.Out_of_circulation:
                    totDead++;
                    break;
            }
        }

        txtTotPop.text = fromPopulation.Count.ToString();
        txtDead.text = totDead.ToString();
        txtSick.text = totSick.ToString();
        txtInmune.text = totInmune.ToString();
    }

    public void ShowBuildWindow(BuyablePlot _buyablePlot)
    {
        ShowHideUI(false);
        BuildWindow.ShowWindow(_buyablePlot);
    }

    public void ShowPolicyWindow()
    {
        WorldManager.instance.ChangeTimeScale(0);  

        ShowHideUI(false);
        PolicyWindow.SetActive(true);
    }

    public void HidePolicyWindow()
    {
        WorldManager.instance.ChangeTimeScale(1);

        ShowHideUI(true);
        PolicyWindow.SetActive(false);
    }

    public void ShowHideUI(bool show)
    {
        UIElements.SetActive(show);
    }

    public void ShowPolicySymbol(bool Quarantine, bool MoreFood, bool MoreHospitals, bool HospitalEfficient)
    {
        policyQuarentine.SetActive(Quarantine);
        policyFood.SetActive(MoreFood);
        policyMoreHospitals.SetActive(MoreHospitals);
        policyHospitalsEfficient.SetActive(HospitalEfficient);
    }

}
