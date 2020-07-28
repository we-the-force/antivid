using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager instance;

    public List<TutorialItem> TutorialItemCollection;

    private void Awake()
    {
        instance = this;
    }

    public void ShowTutorial(int id)
    {
        CanvasControl.instance.ShowTutorial(TutorialItemCollection[id]);
    }
}
