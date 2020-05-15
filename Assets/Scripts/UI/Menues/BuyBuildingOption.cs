using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuyBuildingOption : MonoBehaviour
{
    public int myID;
    public Image buildingImage;
    public Image buildingType;
    public Text buildingText;

    public BuyBuildingWindow parentWindow;
       
    public void Click()
    {
        parentWindow.SelectBuilding(myID);
    }

}
