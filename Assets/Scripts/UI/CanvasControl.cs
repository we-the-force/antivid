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

    public Image happinessImage;

    public List<Sprite> happinessFaces;

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

    public bool WindowOpen { get; set; }

    bool statisticReturnToResultScreen = false;

    private void Awake()
    {
        instance = this;

        WindowOpen = false;
    }

    public void SetHappinessFace(int i)
    {
        happinessImage.sprite = happinessFaces[i];
    }

    private void Start()
    {
        //--- Inicializa el icono de felicidad en el mas alto nivel
        SetHappinessFace(0);

        SetupCanvasSounds();
        audioManager.ChangeBGM(audioManager.DiamondDust);
    }
    public RectTransform panelPopInfo;
    public RectTransform panelCurrencyInfo;
    public RectTransform panelSpeedControl;
    public RectTransform panelCameraControl;
    public RectTransform panelButtons;


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

    public void ShowStatisticWindow(bool gotToResultScreen = false)
    {
        statisticReturnToResultScreen = gotToResultScreen;
        if (!gotToResultScreen)
        {
            PlayShowMenu();
            ShowHideUI(false);
            //WorldManager.instance.ChangeTimeScale(0);
            WorldManager.instance.Pause(true);

            GraphWindowController.InitGraph();
            GraphWindow.SetActive(true);
        }
        else
        {
            PlayShowMenu();
            EndGameWindow.SetActive(false);
            GraphWindowController.InitGraph();
            GraphWindow.SetActive(true);
        }
    }

    public void HideStatisticWindow()
    {
        if (!statisticReturnToResultScreen)
        {
            Debug.Log("Playing HideStatisticsWindow");
            PlayHideMenu();
            GraphWindow.SetActive(false);
            GraphWindowController.DisableWindow();
            ShowHideUI(true);
            //WorldManager.instance.ChangeTimeScale(1);
            WorldManager.instance.Pause(false);
        }
        else
        {
            PlayHideMenu();
            GraphWindow.SetActive(false);
            GraphWindowController.DisableWindow();

            statisticReturnToResultScreen = false;
            EndGameWindow.SetActive(true);
        }
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
        if (!show)
            WindowOpen = true;
        else
            Invoke("ChangeWindowOpenStatus", 0.04f);

        UIElements.SetActive(show);
    }

    private void ChangeWindowOpenStatus()
    {
        Debug.LogError("Entrando a cambair el statos de la ventanta");
        WindowOpen = false;
    }

    public void EndScenario()
    {
        ShowHideUI(false);

        EndGameWindow.SetActive(true);
        EndGameWindow.GetComponent<ResultWindow>().CalcScores();
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

    public void GoToMenu()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
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