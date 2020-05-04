using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public class CanvasControl : MonoBehaviour
{
    public Text txtHappiness;

    public static CanvasControl instance;

    public BuyBuildingWindow BuildWindow;

    public GameObject UIElements;

    public GameObject PolicyWindow;

    public GameObject VaccineWindow;
    public GameObject VaccineIcon;

    public GameObject EndGameWindow;

    public List<BuildingCanvasEntry> BuyableBuildingsCollection;

    public AnnouncementWindow _announcementWindow;

    public GameObject GraphWindow;
    public GraphController GraphWindowController;

    public Text txtTotPop;
    public Text txtSick;
    public Text txtDead;
    public Text txtInmune;

    public List<Sprite> PolicyIconSprites;
    public List<Image> PolicyIconCollection;

    public List<GameObject> speedControlSelectedImage;


    private void Awake()
    {
        instance = this;
    }

    public RectTransform panelPopInfo;
    public RectTransform panelCurrencyInfo;
    public RectTransform panelSpeedControl;
    public RectTransform panelCameraControl;
    public RectTransform panelButtons;

    [SerializeField, Range(0.01f, 5f)]
    float incapWeight = 1;
    [SerializeField, Range(0.005f, 0.5f)]
    float moneyWeight = 1;

    public void RearangeElements(float _aspect)
    {
        if (_aspect < 2f)
        {
            panelPopInfo.anchoredPosition = new Vector2(23, -22);
            panelCurrencyInfo.anchoredPosition = new Vector2(-27, -24);
            panelSpeedControl.anchoredPosition = new Vector2(136, 47);
            //panelCameraControl.anchoredPosition = new Vector2(0, 0);
            panelButtons.anchoredPosition = new Vector2(-60, 30);
        }
    }


    public void SpeedActiveButton(int idx)
    {
        for (int i = 0; i < speedControlSelectedImage.Count; i++)
        {
            if (idx == i)
                speedControlSelectedImage[i].SetActive(true);
            else
                speedControlSelectedImage[i].SetActive(false);
        }
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
        if (activePolicyIdx.Count == 0)
        {
            return;
        }

        for (int i = 0; i < activePolicyIdx.Count; i++)
        {
            PolicyIconCollection[i].sprite = PolicyIconSprites[activePolicyIdx[i]];
            PolicyIconCollection[i].gameObject.SetActive(true);
        }
    }

    public void ShowStatisticWindow()
    {
        ShowHideUI(false);
        //WorldManager.instance.ChangeTimeScale(0);
        WorldManager.instance.Pause(true);

        GraphWindowController.InitGraph();
        GraphWindow.SetActive(true);
    }

    public void HideStatisticWindow()
    {
        GraphWindow.SetActive(false);
        GraphWindowController.DisableWindow();
        ShowHideUI(true);
        //WorldManager.instance.ChangeTimeScale(1);
        WorldManager.instance.Pause(false);
    }

    public void ShowBuildWindow(BuyablePlot _buyablePlot)
    {
        ShowHideUI(false);
        BuildWindow.ShowWindow(_buyablePlot);
    }

    public void ShowPolicyWindow()
    {
        //WorldManager.instance.ChangeTimeScale(0);
        WorldManager.instance.Pause(true);

        PolicyManager.Instance.CreateToggles();

        ShowHideUI(false);
        PolicyWindow.SetActive(true);
    }

    public void HidePolicyWindow()
    {
        //WorldManager.instance.ChangeTimeScale(1);
        WorldManager.instance.Pause(false);

        PolicyWindow.SetActive(false);
        ShowPolicyIcon();
        ShowHideUI(true);

    }

    public void ShowVaccineWindow()
    {
        //WorldManager.instance.ChangeTimeScale(0);
        WorldManager.instance.Pause(true);

        ShowHideUI(false);
        VaccineWindow.SetActive(true);
    }
    public void HideVaccineWindow()
    {
        //WorldManager.instance.ChangeTimeScale(1);
        WorldManager.instance.Pause(false);

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

        float normalPoints = CalcPoints();
        float percentPoints = CalcScorePercentage();
        string rank = GetRank(percentPoints);

        WriteTempData(rank);
        EndGameWindow.SetActive(true);
    }

    float CalcPoints()
    {
        List<AgentController> auxAgentList = WorldAgentController.instance.AgentCollection;
        int totalPop = auxAgentList.Count;
        int incap = auxAgentList.FindAll(x => x.myStatus == GlobalObject.AgentStatus.Out_of_circulation).Count;

        int validPop = totalPop - incap;

        float positivePoints = (validPop * 100) + (CurrencyManager.Instance.CurrentCurrency * 50);
        float negativePoints = (incap * 150);

        float totalPoints = (positivePoints - negativePoints);

        return totalPoints;
    }
    float CalcScorePercentage()
    {
        List<AgentController> auxAgentList = WorldAgentController.instance.AgentCollection;
        int totalPop = auxAgentList.Count;
        int incap = auxAgentList.FindAll(x => x.myStatus == GlobalObject.AgentStatus.Out_of_circulation).Count;

        float weightPerDude = 100f / totalPop;
        float negativeScore = incap * weightPerDude * incapWeight;
        float moneyScore = CurrencyManager.Instance.CurrentCurrency * moneyWeight;

        float totalScore = 100f + moneyScore - negativeScore;

        return totalScore;
    }
    string GetRank(float percentScore)
    {
        //50  60  70  80  90  95  100 >100
        //F,  E,  D,  C,  B,  A,  S,  SS,
        //F     [  0,  50)
        //E     [ 50,  60)
        //D     [ 60,  70)
        //C     [ 70,  80)
        //B     [ 80,  90)
        //A     [ 90,  98)
        //S     [ 98, 102.5)
        //SS    100+

        if (percentScore <= 50f)
        {
            return "F";
        }
        else if (percentScore < 60f)
        {
            return "E";
        }
        else if (percentScore < 70f)
        {
            return "D";
        }
        else if (percentScore < 80f)
        {
            return "C";
        }
        else if (percentScore < 90f)
        {
            return "B";
        }
        else if (percentScore < 98f)
        {
            return "A";
        }
        else if (percentScore < 102.5f)
        {
            return "S";
        }
        else
        {
            return "SS";
        }
    }
    public void WriteTempData(string rank)
    {
        string id = SceneManager.GetActiveScene().name;
        ScenarioSaveData auxData = new ScenarioSaveData() { scenarioID = id, grades = new List<string> { rank } };
        Debug.LogError($"Writing to tempData\r\n{auxData.ToString()}");

        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(Application.persistentDataPath + "/Data/Temp/result.dat");
        Debug.Log($"File created at {Application.persistentDataPath}/Data/Temp/Data.dat");
        bf.Serialize(file, auxData);
        file.Close();
    }
}
