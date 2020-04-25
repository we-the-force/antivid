using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AnnouncementWindow : MonoBehaviour
{
    public List<NewAvailable> NewAvailableCollection;

    public List<SpecialEventObject> EventsCollection;

    public List<SpecialEventObject> SpecialEventsCollection;

    public Text EventDescription;

    public GameObject SpecialEventIcon;

    public float randomEventChance = 0.1f;
    private float currentRandomEventChance = 0.1f;
    
    public int CyclesCooldown = 5;
    private int currentCooldown = 0;


    public void HideAllNewAvailableDescriptions()
    {
        for (int i = 0; i < NewAvailableCollection.Count; i++)
        {
            NewAvailableCollection[i].HideDescription();
        }
    }

    public void HideAllNewAvailable()
    {
        for (int i = 0; i < NewAvailableCollection.Count; i++)
        {
            NewAvailableCollection[i].HideDescription();
            NewAvailableCollection[i].gameObject.SetActive(false);
        }
    }

    public void CloseWindow()
    {
        gameObject.SetActive(false);
        WorldManager.instance.ChangeTimeScale(1);
        CanvasControl.instance.ShowHideUI(true);
    }

    public void SpecialEvent(GlobalObject.SpecialEventName EventName)
    {
        for(int i=0;i<SpecialEventsCollection.Count;i++)
        {
            if (SpecialEventsCollection[i].specialEventName == EventName)
            {
                if (SpecialEventsCollection[i].done)
                    return;

                switch (EventName)
                {
                    case GlobalObject.SpecialEventName.FirstInfected:

                        CanvasControl.instance.ShowVaccineIcon(true);
                        VaccineManager.Instance.ShouldTic = true;
                        break;
                    case GlobalObject.SpecialEventName.FirstStage:
                        break;
                    case GlobalObject.SpecialEventName.SecondStage:
                        //--- Habilita la politica de cuarentena
                        PolicyManager.Instance.EnablePolicy(0);
                        break;
                    case GlobalObject.SpecialEventName.StartVaccineStudy:
                        break;
                    case GlobalObject.SpecialEventName.Vaccine40percent:
                        PolicyManager.Instance.EnablePolicy(1);
                        Debug.LogError("Vaccine40Percent");
                        break;
                    case GlobalObject.SpecialEventName.Vaccine50percent:
                        Debug.LogError("Vaccine50Percent");
                        break;
                    case GlobalObject.SpecialEventName.VaccineCompleted:
                        //--- Curar a toda la banderola
                        for (int a = 0; a < WorldAgentController.instance.AgentCollection.Count; a++)
                        {
                            WorldAgentController.instance.AgentCollection[a].HandleCellsWithVaccine();
                        }

                        Debug.LogError("VaccineCompleted");
                        break;
                }

                WorldManager.instance.ChangeTimeScale(0);

                SpecialEventsCollection[i].done = true;

                EventDescription.text = SpecialEventsCollection[i].Description;

                CanvasControl.instance.ShowHideUI(false);

                HideAllNewAvailable();

                SpecialEventIcon.SetActive(true);

                gameObject.SetActive(true);

                break;
            }

        }
    }


    public void RandomEvent()
    {
        if (currentCooldown < CyclesCooldown)
        {
            //--- hara cooldown antes de calcular si hay un evento random
            currentCooldown++;
            return;
        }

        float frnd = Random.Range(0.0f, 1.0f);
        if (frnd > currentRandomEventChance)
        {
            currentRandomEventChance += 0.05f;
            return; // No se cumplio el porcentaje para mostrar evento random
        }

        currentRandomEventChance = randomEventChance;

        //--- a partir de aqui si muestra y aplica la ventana del evento
        //--- Se pone el juego en pausa
        WorldManager.instance.ChangeTimeScale(0);

        currentCooldown = 0;

        int rnd = Random.Range(0, EventsCollection.Count);

        EventDescription.text = EventsCollection[rnd].Description;

        //-- Se aplican los valores relacionados al evento
        for (int i = 0; i < WorldAgentController.instance.AgentCollection.Count; i++)
        {
            if (EventsCollection[rnd].Need == GlobalObject.NeedScale.HealtCare)
            {
                WorldAgentController.instance.AgentCollection[i].AddContagion(EventsCollection[rnd].Value, false);
            }
            else
            {
                AgentController _agent = WorldAgentController.instance.AgentCollection[i];
                for (int j = 0; j < _agent.myNeedList.Count; j++)
                {
                    if (_agent.myNeedList[j].Need == EventsCollection[rnd].Need)
                    {
                        _agent.myNeedList[j].CurrentPercentage -= EventsCollection[rnd].Value;
                        if (_agent.myNeedList[j].CurrentPercentage < 0)
                            _agent.myNeedList[j].CurrentPercentage = 0;
                        break;
                    }
                }
            }
        }

        //--- Escondiendo la UI
        CanvasControl.instance.ShowHideUI(false);

        HideAllNewAvailable();

        SpecialEventIcon.SetActive(false);

        gameObject.SetActive(true);
    }
}

[System.Serializable]
public class SpecialEventObject
{
    public GlobalObject.SpecialEventName specialEventName;
    public GlobalObject.NeedScale Need;
    public string Name;
    public string Description;
    public float Value;
    public bool done;
    public bool availableOnQuarentine;

    public List<int> NewAvailablePolicieID;
}