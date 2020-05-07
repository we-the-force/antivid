using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class CanvasControl : MonoBehaviour
{
    public AudioManager audioManager;

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

    private void Start()
    {
        SetupCanvasSounds();
        audioManager.ChangeBGM(audioManager.DiamondDust);
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
        //PlayShowMenu();
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
        PlayShowMenu();
        ShowHideUI(false);
        //WorldManager.instance.ChangeTimeScale(0);
        WorldManager.instance.Pause(true);

        GraphWindowController.InitGraph();
        GraphWindow.SetActive(true);
    }

    public void HideStatisticWindow()
    {
        Debug.Log("Playing HideStatisticsWindow");
        PlayHideMenu();
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
        PlayShowMenu();
        //WorldManager.instance.ChangeTimeScale(0);
        WorldManager.instance.Pause(true);

        PolicyManager.Instance.CreateToggles();

        ShowHideUI(false);
        PolicyWindow.SetActive(true);
    }

    public void HidePolicyWindow()
    {
        PlayHideMenu();
        //WorldManager.instance.ChangeTimeScale(1);
        WorldManager.instance.Pause(false);

        PolicyWindow.SetActive(false);
        ShowPolicyIcon();
        ShowHideUI(true);

    }

    public void ShowVaccineWindow()
    {
        PlayShowMenu();
        //WorldManager.instance.ChangeTimeScale(0);
        WorldManager.instance.Pause(true);

        ShowHideUI(false);
        VaccineWindow.SetActive(true);
    }
    public void HideVaccineWindow()
    {
        PlayHideMenu();
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

    public TutorialWindow myTutorialWindow;

    public void ShowTutorial(TutorialItem item)
    {
        if (myTutorialWindow == null)
            return;

        WorldManager.instance.Pause(true);
        ShowHideUI(false);


        //--- mostrar mi ventana de tutorial
        myTutorialWindow.ShowWindow(item);
    }

    public void SetupCanvasSounds()
    {
        SetupSpeedControls();
        SetupCameraControls();
        SetupBuyBuildingControls();
        SetupStatsControls();
    }
    void SetupSpeedControls()
    {
        Transform speed = UIElements.transform.Find("SpeedModifier");
        for (int i = 0; i < speed.childCount; i++)
        {
            Button asd = speed.GetChild(i).GetComponent<Button>();
            asd.onClick.AddListener(() => audioManager.Play(audioManager.MenuClick));
        }
    }
    void SetupCameraControls()
    {
        Transform cameraControls = UIElements.transform.Find("TouchControls");

        cameraControls.GetChild(0).GetComponent<Button>().onClick.AddListener(() => audioManager.Play(audioManager.MenuBack));
        cameraControls.GetChild(1).GetComponent<Button>().onClick.AddListener(() => audioManager.Play(audioManager.ChkBoxClick));
        //cameraControls.GetChild(1).GetComponent<Button>().onClick.AddListener(() => audioManager.ChangeBGM(audioManager.OnYourWayBack));
        cameraControls.GetChild(2).GetComponent<Button>().onClick.AddListener(() => audioManager.Play(audioManager.MenuClick));
    }
    void SetupBuyBuildingControls()
    {
        Transform bbPanel = transform.Find("BuyBuildingWindow").Find("Panel");
        Transform bbScrView = transform.Find("BuyBuildingWindow").Find("Scroll View").Find("Viewport").Find("Content");

        //bbPanel.Find("btnBuy").GetComponent<Button>().onClick.AddListener(() => audioManager.Play(audioManager.MenuClick));
        //bbPanel.Find("btnBuy").GetComponent<Button>().onClick.AddListener(() => audioManager.Play(audioManager.BuildStart));
        bbPanel.Find("btnClose").GetComponent<Button>().onClick.AddListener(() => audioManager.Play(audioManager.MenuBack));

        for (int i = 0; i < bbScrView.childCount; i++)
        {
            bbScrView.GetChild(i).GetComponent<Button>().onClick.AddListener(() => audioManager.Play(audioManager.MenuClick));
        }
    }
    void SetupStatsControls()
    {
        Transform stats = transform.Find("StatisticWindow");
        Transform economyToggles = stats.Find("PanelLegendEconomy");
        Transform populationToggles = stats.Find("PanelLegendPopulation");

        //stats.Find("EconomyPanelButton").GetComponent<Button>().onClick.AddListener(() => audioManager.Play(audioManager.OpenWindow));
        //stats.Find("PopulationPanelButton").GetComponent<Button>().onClick.AddListener(() => audioManager.Play(audioManager.OpenWindow));

        for (int i = 0; i < economyToggles.childCount; i++)
        {
            //economyToggles.GetChild(i).GetComponent<Button>().onClick.AddListener(() => audioManager.Play(audioManager.ChkBoxClick));
        }
        for (int i = 0; i < populationToggles.childCount; i++)
        {
            //populationToggles.GetChild(i).GetComponent<Button>().onClick.AddListener(() => audioManager.Play(audioManager.ChkBoxClick));
        }
    }

    void PlayMenuClick()
    {
        audioManager.Play(audioManager.MenuClick);
    }
    void PlayMenuBack()
    {
        audioManager.Play(audioManager.MenuBack);
    }
    void PlayShowMenu()
    {
        audioManager.Play(audioManager.ShowWindow);
    }
    void PlayHideMenu()
    {
        audioManager.Play(audioManager.HideWindow);
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(CanvasControl), true)]
public class CanvasControlEditor : Editor
{
    private CanvasControl control { get { return (target as CanvasControl); } }
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Setup Sounds"))
        {
            control.SetupCanvasSounds();
        }
    }
}
#endif