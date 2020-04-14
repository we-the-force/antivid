using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentIcon : MonoBehaviour
{
    public List<GameObject> IconGameObject = new List<GameObject>();

    private void Start()
    {
        HideAll();
    }

    public void ShowIconFor(GlobalObject.NeedScale _need)
    {
        HideAll();

        switch (_need)
        {
            case GlobalObject.NeedScale.Education:
                IconGameObject[1].SetActive(true);
                break;
            case GlobalObject.NeedScale.Entertainment:
                IconGameObject[2].SetActive(true);
                break;
            case GlobalObject.NeedScale.HealtCare:
                IconGameObject[3].SetActive(true);
                break;
            case GlobalObject.NeedScale.Hunger:
                IconGameObject[0].SetActive(true);
                break;
            case GlobalObject.NeedScale.Sleep:
                IconGameObject[4].SetActive(true);
                break;
            case GlobalObject.NeedScale.Wander:
                IconGameObject[5].SetActive(true);
                break;
            case GlobalObject.NeedScale.Travel:
                IconGameObject[6].SetActive(true);
                break;
        }
    }

    public void HideAll()
    {
        for (int i = 0; i < IconGameObject.Count; i++)
        {
            IconGameObject[i].SetActive(false);
        }
    }

}
