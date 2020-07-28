using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class NewAvailable : MonoBehaviour
{
    public Button actionButton;
    public GameObject Panel;
    public Text txtDescripcion;

    private AnnouncementWindow myWindow;

    private void Start()
    {
        myWindow = transform.parent.GetComponent<AnnouncementWindow>();
    }

    public void HideDescription()
    {
        Panel.SetActive(false);
    }

    public void ShowHideDescription()
    {
        if (Panel.activeSelf)
        {
            Panel.SetActive(false);
            return;
        }

        myWindow.HideAllNewAvailableDescriptions();
        Panel.SetActive(true);
    }

}
