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

    private void Start()
    {
        for (int i = 0; i < ScenarioCollection.Count; i++)
        {
            GameObject obj = Instantiate(ScenarioButtonPrefab, ScenarioAnchor);

            ScenarioSelector selector = obj.GetComponent<ScenarioSelector>();
            selector.scenarioName.text = ScenarioCollection[i].ScenarioName;
            selector.picture.sprite = ScenarioCollection[i].ScenarioImg;
            selector.ID = i;

            selector.myCanvas = this;


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


}
