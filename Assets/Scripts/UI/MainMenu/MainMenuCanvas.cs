using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuCanvas : MonoBehaviour
{
    public GameObject RankingObject;
    public List<GameObject> RankingList;

    public GameObject IconHospital;
    public GameObject IconEntertainment;
    public GameObject IconFood;
    public GameObject IconEducation;
    public Text txtHospital;
    public Text txtEntertainment;
    public Text txtFood;
    public Text txtEducation;

    public Material GrayScaleMaterial;

    public Text txtVersion;

    public GameObject ScenarioButtonPrefab;

    public List<ScenarioInfo> ScenarioCollection;

    public Transform ScenarioAnchor;

    public GameObject InfoWindow;

    public Text txtTotalPopulation;
    public Text txtCapitalInicial;
    public Text txtPuntuacion;
    public Text txtTitle;
    public Image infoProfileImage;

    public int SelectedID;

    public GameObject initButton;
    public GameObject optionsButton;

    public GameObject SeleccionadorDeEscenarios;

    public AudioClip menuClick;
    public AudioClip menuBack;
    public AudioClip menuOpen;
    public AudioClip menuClose;

    private void Start()
    {
        txtVersion.text = "ver. " + Application.version.ToString();

        currentLevel = 0;
        StartCoroutine(SetupScenarioButtons());
    }

    IEnumerator SetupScenarioButtons()
    {
        yield return new WaitForFixedUpdate();

        while(true)
        {
            if (SaveManager.Instance.DataLoaded)
            {
                break;
            }
        }

        bool _isSoundMuted = SaveManager.Instance.IsSoundMuted();
        SetMuteValue(_isSoundMuted);

        for (int i = 0; i < ScenarioCollection.Count; i++)
        {
            GameObject obj = Instantiate(ScenarioButtonPrefab, ScenarioAnchor);

            ScenarioSelector selector = obj.GetComponent<ScenarioSelector>();
            //selector.scenarioName.text = ScenarioCollection[i].ScenarioName;
            selector.picture.sprite = ScenarioCollection[i].ScenarioImg;

            RunScore _runScore = SaveManager.Instance.GetBestRun(ScenarioCollection[i].ScenarioSceneName);
            ScenarioCollection[i].MaxScore = _runScore.score;
            ScenarioCollection[i].MaxRanking = _runScore.rank;

            //selector.ID = i;

            //selector.myCanvas = this;

            //Button objButt = obj.transform.GetChild(0).Find("Button").GetComponent<Button>();
            //objButt.onClick.AddListener(delegate { AudioManager.Instance.Play(menuClick); });
            //obj.transform.GetChild(i).GetChild(0).Find("Button").GetComponent<Button>().onClick.AddListener(delegate { AudioManager.Instance.Play(menuOpen); });

            RectTransform rct = obj.GetComponent<RectTransform>();
            rct.anchoredPosition = new Vector2(rct.sizeDelta.x * i, 0);
        }

        ShowInfo(currentLevel);
    }

    private void SetIcon(GameObject obj, Text txt, int qty)
    {
        //obj.SetActive(false);
        for (int i = 0; i < obj.transform.childCount; i++)
        {
            Image img = obj.transform.GetChild(i).GetComponent<Image>();
            if (img != null)
            {
                img.material = GrayScaleMaterial;
            }
        }

        txt.text = qty.ToString();
        if (qty > 0)
        {
            for (int i = 0; i < obj.transform.childCount; i++)
            {
                Image img = obj.transform.GetChild(i).GetComponent<Image>();
                if (img != null)
                {
                    img.material = null;
                }
            }
            //obj.SetActive(true);
        }
    }

    public void ShowInfo(int _id)
    {
        SelectedID = _id;

        txtTitle.text = ScenarioCollection[_id].ScenarioName;

        txtTotalPopulation.text = ScenarioCollection[_id].TotalPopulation.ToString();
        txtCapitalInicial.text = ScenarioCollection[_id].StartingCapital.ToString();
        txtPuntuacion.text = Mathf.FloorToInt(ScenarioCollection[_id].MaxScore).ToString();

        bool _showRankingObj = true;
        for (int i = 0; i < RankingList.Count; i++)
        {
            RankingList[i].SetActive(false);
        }

        switch (ScenarioCollection[_id].MaxRanking)
        {
            case "SS":
                RankingList[0].SetActive(true);
                break;
            case "S":
                RankingList[1].SetActive(true);
                break;
            case "A":
                RankingList[2].SetActive(true);
                break;
            case "B":
                RankingList[3].SetActive(true);
                break;
            case "C":
                RankingList[4].SetActive(true);
                break;
            case "D":
                RankingList[5].SetActive(true);
                break;
            case "E":
                RankingList[6].SetActive(true);
                break;
            case "F":
                RankingList[7].SetActive(true);
                break;
            default:
                _showRankingObj = false;
                break;
        }

        RankingObject.SetActive(_showRankingObj);

        //infoProfileImage.sprite = ScenarioCollection[_id].ScenarioImg;

        SetIcon(IconHospital, txtHospital, ScenarioCollection[_id].StartingHospitals);
        SetIcon(IconEntertainment, txtEntertainment, ScenarioCollection[_id].StartingEntertainmentBuildings);
        SetIcon(IconFood, txtFood, ScenarioCollection[_id].StartingFoodBuildings);
        SetIcon(IconEducation, txtEducation, ScenarioCollection[_id].StartingEducationBuildings);

        InfoWindow.SetActive(true);
    }

    public void CloseInfoWindow()
    {
        InfoWindow.SetActive(false);
    }

    public void InitScenario()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(ScenarioCollection[SelectedID].ScenarioSceneName, UnityEngine.SceneManagement.LoadSceneMode.Single);
    }

    public GameObject TitleObject;

    public void InitButtonPressed()
    {
        initButton.SetActive(false);
        optionsButton.SetActive(false);
        TitleObject.SetActive(false);

        SeleccionadorDeEscenarios.SetActive(true);
    }

    public float maxLeft = -1590f;
    public float step = 265f;
    public float timeToMove = 1f;

    private int _heading;

    public bool moving = false;

    private int currentLevel;

    public void MovePanel(int heading)
    {
        if (moving)
            return;

        if (heading == 1 && currentLevel == 0)
            return;

        if (heading == -1 && currentLevel == ScenarioCollection.Count - 1)
            return;
               
        InfoWindow.SetActive(false);

        _heading = heading;

        currentLevel += (_heading * -1);
        moving = true;
        StartCoroutine("doMove");
    }

    IEnumerator doMove()
    {
        RectTransform rect = ScenarioAnchor.GetComponent<RectTransform>();
        Vector2 scenarioAnchorPos = rect.anchoredPosition;

        Vector2 scenarioNextAnchorPos = scenarioAnchorPos;
        Vector2 scenarioEndAnchorPos = scenarioAnchorPos;

        /*
        if ((_heading == -1 && scenarioAnchorPos.x <= maxLeft) || (_heading == 1 && scenarioAnchorPos.x >= 0))
        {
            moving = false;
            yield break;
        }
        */

        float movePortion = step;
        /*if (_heading == -1)
        {
            if ((scenarioAnchorPos.x - movePortion) < maxLeft)
            {
                movePortion = Mathf.Abs(scenarioAnchorPos.x - maxLeft);
            }
        }
        if (_heading == 1)
        {
            if ((scenarioAnchorPos.x + movePortion) > 0)
            {
                movePortion = Mathf.Abs(scenarioAnchorPos.x);
            }
        }*/

        scenarioEndAnchorPos.x += movePortion * _heading;

        float seconds = 0;

        while (true)
        {
            if (seconds >= timeToMove)
            {
                rect.anchoredPosition = scenarioEndAnchorPos;
                moving = false;

                ShowInfo(currentLevel);

                InfoWindow.SetActive(true);
                break;
            }

            yield return new WaitForFixedUpdate();

            scenarioNextAnchorPos.x += (movePortion * Time.fixedDeltaTime) * _heading;
            rect.anchoredPosition = scenarioNextAnchorPos;
            seconds += Time.fixedDeltaTime;
           // Debug.LogError("SECONDS  : " + seconds);

        }
    }

    public Button btnMute;
    public List<Sprite> VolumeControlSprite;
    /*public void MuteSound()
    {
        AudioManager.Instance.MuteBGMSource();
        if (AudioManager.Instance.IsMute)
        {
            btnMute.image.sprite = VolumeControlSprite[0];
        }
        else
        {
            btnMute.image.sprite = VolumeControlSprite[1];
        }
    }*/


    public void SetMuteValue(bool muteValue)
    {
        AudioManager.Instance.SetMuteFromSave(muteValue);
        MuteSound(true);
    }

    public void MuteSound(bool fromSave = false)
    {
        if (!fromSave)
            AudioManager.Instance.MuteBGMSource();

        if (AudioManager.Instance.IsMute)
        {
            btnMute.image.sprite = VolumeControlSprite[0];
        }
        else
        {
            btnMute.image.sprite = VolumeControlSprite[1];
        }

        if (!fromSave)
        {
            SaveManager.Instance.gPref.SoundIsMuted = AudioManager.Instance.IsMute;
            SaveManager.Instance.SaveGamePreferenceData();
        }
    }

}
