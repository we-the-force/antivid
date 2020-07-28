using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialWindow : MonoBehaviour
{
    public Image supportImage1;
    public GameObject go1;

    public Image supportImage2;
    public GameObject go2;

    public Image supportImage3;
    public GameObject go3;


    public Text TutorialText;

    public void ShowWindow(TutorialItem item)
    {
        go1.SetActive(false);
        go2.SetActive(false);
        go3.SetActive(false);

        if (item.image1 != null)
        {
            supportImage1.sprite = item.image1;
            go1.SetActive(true);
        }
        if (item.image2 != null)
        {
            supportImage2.sprite = item.image2;
            go2.SetActive(true);
        }
        if (item.image3 != null)
        {
            supportImage3.sprite = item.image3;
            go3.SetActive(true);
        }

        TutorialText.text = item.Descripcion;
        gameObject.SetActive(true);
    }

    public void CloseWindow()
    {
        WorldManager.instance.Pause(false);
        CanvasControl.instance.ShowHideUI(true);
        gameObject.SetActive(false);
    }

}
