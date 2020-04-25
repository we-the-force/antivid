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

    public GameObject VaccineWindow;
    public GameObject VaccineIcon;

    public GameObject EndGameWindow;

    public List<BuildingCanvasEntry> BuyableBuildingsCollection;

    public AnnouncementWindow _announcementWindow;

    public Text txtTotPop;
    public Text txtSick;
    public Text txtDead;
    public Text txtInmune;

    public List<Sprite> PolicyIconSprites;
    public List<Image> PolicyIconCollection;



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

    public void ShowPolicyIcon()
    {
        for (int i = 0; i < PolicyIconCollection.Count; i++)
        {
            PolicyIconCollection[i].gameObject.SetActive(false);
        }

        List<int> activePolicyIdx = new List<int>();
        activePolicyIdx = WorldAgentController.instance.GetPolicyIdx();
        if(activePolicyIdx.Count == 0)
        {
            return;
        }

        for (int i = 0; i < activePolicyIdx.Count; i++)
        {
            PolicyIconCollection[i].sprite = PolicyIconSprites[activePolicyIdx[i]];
            PolicyIconCollection[i].gameObject.SetActive(true);
        }
    }

    public void ShowBuildWindow(BuyablePlot _buyablePlot)
    {
        ShowHideUI(false);
        BuildWindow.ShowWindow(_buyablePlot);
    }

    public void ShowPolicyWindow()
    {
        WorldManager.instance.ChangeTimeScale(0);

        PolicyManager.Instance.CreateToggles();

        ShowHideUI(false);
        PolicyWindow.SetActive(true);
    }

    public void HidePolicyWindow()
    {
        WorldManager.instance.ChangeTimeScale(1);

        ShowPolicyIcon();
        ShowHideUI(true);
        PolicyWindow.SetActive(false);

    }

    public void ShowVaccineWindow()
    {
        WorldManager.instance.ChangeTimeScale(0);

        ShowHideUI(false);
        VaccineWindow.SetActive(true);
    }
    public void HideVaccineWindow()
    {
        WorldManager.instance.ChangeTimeScale(1);

        ShowHideUI(true);
        VaccineWindow.SetActive(false);
    }
    public void ShowVaccineIcon(bool show)
    {
        VaccineIcon.SetActive(show);
    }

    public void ShowHideUI(bool show)
    {
        UIElements.SetActive(show);
    }

    public void EndScenario()
    {
        ShowHideUI(false);
        EndGameWindow.SetActive(true);
    }
}
