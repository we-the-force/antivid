using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CanvasControl : MonoBehaviour
{
    public static CanvasControl instance;

    public BuyBuildingWindow BuildWindow;

    public GameObject EconomicPanel;
    public GameObject ControlsPanel;
    public GameObject InfoPanel;
    public GameObject PolicyPanel;

    public Text txtTotPop;
    public Text txtSick;
    public Text txtDead;
    public Text txtInmune;

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
        EconomicPanel.SetActive(false);
        ControlsPanel.SetActive(false);
        InfoPanel.SetActive(false);
        PolicyPanel.SetActive(false);

        BuildWindow.ShowWindow(_buyablePlot);
    }

}
