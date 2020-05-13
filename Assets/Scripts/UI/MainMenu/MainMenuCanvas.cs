using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuCanvas : MonoBehaviour
{
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
        StartCoroutine(SetupScenarioButtons());
    }

    IEnumerator SetupScenarioButtons()
    {
        yield return null;

        for (int i = 0; i < ScenarioCollection.Count; i++)
        {
            GameObject obj = Instantiate(ScenarioButtonPrefab, ScenarioAnchor);

            ScenarioSelector selector = obj.GetComponent<ScenarioSelector>();
            selector.scenarioName.text = ScenarioCollection[i].ScenarioName;
            selector.picture.sprite = ScenarioCollection[i].ScenarioImg;
            selector.ID = i;

            selector.myCanvas = this;

            Button objButt = obj.transform.GetChild(0).Find("Button").GetComponent<Button>();
            objButt.onClick.AddListener(delegate { AudioManager.Instance.Play(menuClick); });
            //obj.transform.GetChild(i).GetChild(0).Find("Button").GetComponent<Button>().onClick.AddListener(delegate { AudioManager.Instance.Play(menuOpen); });

            RectTransform rct = obj.GetComponent<RectTransform>();
            rct.anchoredPosition = new Vector2(35 + (rct.sizeDelta.x * i) + (60 * i), 0);
        }
    }

    public void ShowInfo(int _id)
    {
        SelectedID = _id;

        txtTitle.text = ScenarioCollection[_id].ScenarioName;

        txtTotalPopulation.text = ScenarioCollection[_id].TotalPopulation.ToString();
        txtCapitalInicial.text = ScenarioCollection[_id].StartingCapital.ToString();
        txtPuntuacion.text = ScenarioCollection[_id].MaxScore.ToString();

        infoProfileImage.sprite = ScenarioCollection[_id].ScenarioImg;

        InfoWindow.SetActive(true);
    }

    public void CloseInfoWindow()
    {
        InfoWindow.SetActive(false);
    }

    public void InitScenario()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(ScenarioCollection[SelectedID].ScenarioSceneName);
    }


    public void InitButtonPressed()
    {
        initButton.SetActive(false);
        optionsButton.SetActive(false);

        SeleccionadorDeEscenarios.SetActive(true);
    }

    public float maxLeft = -1590f;
    public float step = 265f;
    public float timeToMove = 1f;

    private int _heading;

    public bool moving = false;
    public void MovePanel(int heading)
    {
        if (moving)
            return;

        _heading = heading;
        moving = true;
        StartCoroutine("doMove");
    }

    IEnumerator doMove()
    {
        RectTransform rect = ScenarioAnchor.GetComponent<RectTransform>();
        Vector2 scenarioAnchorPos = rect.anchoredPosition;

        Vector2 scenarioNextAnchorPos = scenarioAnchorPos;
        Vector2 scenarioEndAnchorPos = scenarioAnchorPos;

        if ((_heading == -1 && scenarioAnchorPos.x <= maxLeft) || (_heading == 1 && scenarioAnchorPos.x >= 0))
        {
            moving = false;
            yield break;
        }

        float movePortion = step * 4f;
        if (_heading == -1)
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
        }

        scenarioEndAnchorPos.x += movePortion * _heading;

        float seconds = 0;

        while (true)
        {
            if (seconds >= timeToMove)
            {
                rect.anchoredPosition = scenarioEndAnchorPos;
                moving = false;
                break;
            }

            yield return new WaitForFixedUpdate();

            scenarioNextAnchorPos.x += (movePortion * Time.fixedDeltaTime) * _heading;
            rect.anchoredPosition = scenarioNextAnchorPos;
            seconds += Time.fixedDeltaTime;
            Debug.LogError("SECONDS  : " + seconds);

        }
    }

}
