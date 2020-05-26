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

    public Material GrayScaleMaterial;

    public bool isBuyable;

    public BuyBuildingWindow parentWindow;
       
    public void Click()
    {
        if (!isBuyable)
        {
            return;
        }
        parentWindow.SelectBuilding(myID);
    }

    public void SetGrayScale()
    {
        if (isBuyable)
            return;

        gameObject.GetComponent<Image>().material = GrayScaleMaterial;
        buildingImage.material = GrayScaleMaterial;
        buildingType.material = GrayScaleMaterial;
    }

}
