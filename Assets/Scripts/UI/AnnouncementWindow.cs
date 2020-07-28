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
    public Text EventEffectDescription;

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
        AudioManager.Instance.Play(AudioManager.Instance.HideWindow);
        //WorldManager.instance.ChangeTimeScale(1);
        WorldManager.instance.Pause(false);
        CanvasControl.instance.ShowHideUI(true);
    }

    public void SpecialEvent(GlobalObject.SpecialEventName EventName)
    {
        string txtExtra = "";

        EventEffectDescription.text = "";

        for (int i=0;i<SpecialEventsCollection.Count;i++)
        {
            if (SpecialEventsCollection[i].specialEventName == EventName)
            {
                if (SpecialEventsCollection[i].done)
                    return;

                switch (EventName)
                {
                    case GlobalObject.SpecialEventName.FirstInfected:
                        AudioManager.Instance.Play(AudioManager.Instance.EventVAlert);
                        AudioManager.Instance.ChangeBGM(AudioManager.Instance.OnYourWayBack);

                        int _ciclos = Mathf.CeilToInt(VaccineManager.Instance.TicsToStart / CurrencyManager.Instance.ticCutout);
                        txtExtra = " " + _ciclos.ToString() + " ciclos";

                        VaccineManager.Instance.ShouldTic = true;
                        break;
                    case GlobalObject.SpecialEventName.FirstStage:
                        AudioManager.Instance.Play(AudioManager.Instance.EventVAlert);
                        break;
                    case GlobalObject.SpecialEventName.SecondStage:
                        //--- Habilita la politica de cuarentena
                        AudioManager.Instance.Play(AudioManager.Instance.EventVAlert);
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
                        AudioManager.Instance.Play(AudioManager.Instance.VaccineFound);
                        //--- Curar a toda la banderola
                        for (int a = 0; a < WorldAgentController.instance.AgentCollection.Count; a++)
                        {
                            WorldAgentController.instance.AgentCollection[a].HandleCellsWithVaccine();
                        }

                        Debug.LogError("VaccineCompleted");
                        break;
                }

                //WorldManager.instance.ChangeTimeScale(0);
                WorldManager.instance.Pause(true);

                SpecialEventsCollection[i].done = true;

                EventDescription.text = SpecialEventsCollection[i].Description + txtExtra;

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
        currentCooldown = 0;

        int rnd = Random.Range(0, EventsCollection.Count);

        if (!EventsCollection[rnd].RandomEventIsActive)
        {
            return;
        }
        //--- a partir de aqui si muestra y aplica la ventana del evento
        //--- Se pone el juego en pausa
        //WorldManager.instance.ChangeTimeScale(0);
        WorldManager.instance.Pause(true);
        
        EventDescription.text = EventsCollection[rnd].Description;
        EventEffectDescription.text = EventsCollection[rnd].EffectDescrption;
        //-- Se aplican los valores relacionados al evento
               
        if (EventsCollection[rnd].Need == GlobalObject.NeedScale.Currency_Not_a_need)
        {
            CurrencyManager.Instance.CurrentCurrency += EventsCollection[rnd].Value;
        }
        else
        {
            for (int i = 0; i < WorldAgentController.instance.AgentCollection.Count; i++)
            {
                AgentController _agent = WorldAgentController.instance.AgentCollection[i];

                if (EventsCollection[rnd].Need == GlobalObject.NeedScale.HealtCare)
                {
                    float _healthValue = _agent.PorcentageContagio * EventsCollection[rnd].Value;
                    WorldAgentController.instance.AgentCollection[i].AddContagion(EventsCollection[rnd].Value, false);
                }
                else
                {
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
        }


        //--- Escondiendo la UI
        CanvasControl.instance.ShowHideUI(false);

        HideAllNewAvailable();

        SpecialEventIcon.SetActive(false);

        gameObject.SetActive(true);
        AudioManager.Instance.Play(AudioManager.Instance.EventNormal);

    }
}

[System.Serializable]
public class SpecialEventObject
{
    public bool RandomEventIsActive = true;

    public GlobalObject.SpecialEventName specialEventName;
    public GlobalObject.NeedScale Need;
    public string Name;
    public string EffectDescrption;
    public string Description;
    public float Value;
    public bool isPercentage;
    public bool done;
    public bool availableOnQuarentine;

    public List<int> NewAvailablePolicieID;
}