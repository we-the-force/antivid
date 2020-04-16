using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingRandomizer : MonoBehaviour
{
    public int SelectedTemplate;

    public Transform DecorationAnchor;
    public List<BuildingTemplate> TemplateCollection;


    public void InitBuilding(int _template = -1)
    {
        for (int i = 0; i < DecorationAnchor.childCount; i++)
        {
            DecorationAnchor.GetChild(i).gameObject.SetActive(false);
        }

        SelectedTemplate = _template;
        if (_template == -1)
        {
            //--- Randomizar y asignar el template al edificio
            int rnd = Random.Range(0, TemplateCollection.Count);
            SelectedTemplate = rnd;
        }

        ShowDecorations();
    }

    private void ShowDecorations()
    {
        for (int i = 0; i < TemplateCollection[SelectedTemplate].gameObjects.Count; i++)
        {
            TemplateCollection[SelectedTemplate].gameObjects[i].SetActive(true);
        }
    }
}

[System.Serializable]
public class BuildingTemplate
{
    public List<GameObject> gameObjects;
}