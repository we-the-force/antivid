using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScenarioSelector : MonoBehaviour
{
    public MainMenuCanvas myCanvas { get; set; }
    public Text scenarioName;
    public int ID;

    public Image picture;

    public void SelectThis()
    {
        myCanvas.ShowInfo(ID);
    }
}
