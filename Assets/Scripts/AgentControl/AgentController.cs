﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentController : MonoBehaviour
{
    public float Speed;

    public int AgentID;

    public GameObject AnimationObject;

    public GlobalObject.AgentStatus myStatus;

    public List<NeedPercentage> myNeedList;

    public Rigidbody myRigidBody;

    public PathFindingNode myCurrentNode;
    public int myDestinationNodeID;

    private BuildingController myDestinationBuilding;



    public BuildingController myHouse;

    public GlobalObject.AgentPerk myPerk;

    public int currentNodeId;

    /// <summary>
    /// La necesidad que en este momento se esta atendiendo
    /// </summary>
    public GlobalObject.NeedScale NeedTakenCare;
    public bool TakingCareOfNeed = false;
    private bool ExecutingBuilding = false;

    private AgentMovement movementController;

    //--- Propiedades
    public float hungerResistance = 1.0f;
    public float healthResistance = 1.0f;
    public float wanderResistance = 1.0f;
    public float educationResistance = 1.0f;
    public float entertainmentResistance = 1.0f;
    public float sleepResistance = 1.0f;
    public float resourceProduction = 1.0f;

    public void InitAgent()
    {
        movementController = gameObject.GetComponent<AgentMovement>();

        movementController.Speed = Speed;

        int rndPerk = Random.Range(1, 5);
        switch (rndPerk)
        {
            case 1:
                myPerk = GlobalObject.AgentPerk.Athletic;
                break;
            case 2:
                myPerk = GlobalObject.AgentPerk.Workaholic;
                break;
            case 3:
                myPerk = GlobalObject.AgentPerk.Extrovert;
                break;
            case 4:
                myPerk = GlobalObject.AgentPerk.Introvert;
                break;
            default:
                myPerk = GlobalObject.AgentPerk.None;
                break;
        }

        WorldManager.instance.GetPerkValues(myPerk, out hungerResistance, out healthResistance, out wanderResistance, out educationResistance, out entertainmentResistance, out sleepResistance, out resourceProduction);

        NeedPercentage _need = new NeedPercentage();
        _need.Need = GlobalObject.NeedScale.Hunger;
        _need.PercentageToCompare = Mathf.RoundToInt(100 * hungerResistance);
        _need.TicValue = 1;
        myNeedList.Add(_need);

        _need = new NeedPercentage();
        _need.Need = GlobalObject.NeedScale.Education;
        _need.PercentageToCompare = Mathf.RoundToInt(100 * educationResistance);
        _need.TicValue = 1;
        myNeedList.Add(_need);

        _need = new NeedPercentage();
        _need.Need = GlobalObject.NeedScale.Entertainment;
        _need.PercentageToCompare = Mathf.RoundToInt(100 * entertainmentResistance);
        _need.TicValue = 1;
        myNeedList.Add(_need);

        _need = new NeedPercentage();
        _need.Need = GlobalObject.NeedScale.HealtCare;
        _need.PercentageToCompare = Mathf.RoundToInt(100 * healthResistance);
        _need.TicValue = 0;
        myNeedList.Add(_need);

        _need = new NeedPercentage();
        _need.Need = GlobalObject.NeedScale.Sleep;
        _need.PercentageToCompare = Mathf.RoundToInt(100 * sleepResistance);
        _need.TicValue = 1;
        myNeedList.Add(_need);

        _need = new NeedPercentage();
        _need.Need = GlobalObject.NeedScale.Wander;
        _need.PercentageToCompare = Mathf.RoundToInt(100 * wanderResistance);
        _need.TicValue = 1;

        StartCoroutine("Life");
    }

    IEnumerator Life()
    {
        float seconds = 0;

        while (true)
        {
            if (!ExecutingBuilding)
            {
                //--- Cada Tik va a ser de 1/4 de segundo
                if (seconds >= 0.25f)
                {
                    //--- Al cumplirse un tik, se suman los porcentajes de acuerdo a las resistencias y habilidades del agente
                    for (int i = 0; i < myNeedList.Count; i++)
                    {
                        if (myNeedList[i].MaxPercentage())
                        {
                            TakeCareOfNeed(myNeedList[i].Need);
                        }
                    }

                    seconds = 0;
                }

                yield return new WaitForFixedUpdate();
                seconds += Time.fixedDeltaTime;
            }
            else
            {
                yield return new WaitForFixedUpdate();
            }
        }
    }
       
    private void TakeCareOfNeed(GlobalObject.NeedScale _need)
    {
        //--- Si ya se esta atendiendo una necesidad, no cambia de necesidad, pero si va 
        //--- sumando porcentaje de necesidad
        if (TakingCareOfNeed)
            return;

        TakingCareOfNeed = true;
        NeedTakenCare = _need;

        if (_need == GlobalObject.NeedScale.Wander)
        {
            //--- Obtiene un id random de camino para deambular
            GetRoad();
        }
        else
        {
            //Debug.LogError("Buscar Edificio para necesidad: " + _need.ToString());

            myDestinationBuilding = WorldAgentController.instance.GetBuilding(_need);

            if (myDestinationBuilding == null)
            {
                _need = GlobalObject.NeedScale.Wander;
                Debug.LogError("El edificio esta lleno, WANDER");
                GetRoad();
            }
            else
            {
                myDestinationNodeID = myDestinationBuilding.AssociatedNode.NodeID;
            }
        }

        //myDestinationNodeID = WorldAgentController.instance.GetBuildingPathfindingNodeID(_need);

        if (myDestinationNodeID < 0)
        {
            //--- No hay edificio disponible para atender la necesidad; debera atender la siguiente necesidad mas alta
            //--- si no tiene necesidad que haya sobrepasado el limite, comienza a diambular, luego pregunta de nuevo
            Debug.LogError("NO HAY EDIFICIO CON NECESIDAD A CUBRIR");
            TakingCareOfNeed = false;
            return;
        }

        movementController.MoveAgent(myDestinationNodeID);
    }

    private void GetRoad()
    {
        myDestinationBuilding = null;

        int max = WorldAgentController.instance.RoadSystem.Count;
        int rnd = Random.Range(0, max);

        myDestinationNodeID = WorldAgentController.instance.RoadSystem[rnd].NodeID;
    }

    public void DestinyReached()
    {
        //Debug.LogError(" SI YA LLEGO A ALGUN LADO");
        StartCoroutine("ExecuteDestiny");
    }


    IEnumerator ExecuteDestiny()
    {
        //--- Ya llego al destino; en caso de que sea Wander; espera un ratito y luego regresa a realizar sus tareas
        if (NeedTakenCare == GlobalObject.NeedScale.Wander)
        {
            yield return new WaitForSeconds(1.5f);
            TakingCareOfNeed = false;
            yield break;
        }


        if (myDestinationBuilding != null)
        {
            if (myDestinationBuilding.CurrentAgentCount == myDestinationBuilding.AgentCapacity)
            {
                //--- En caso de que sea un Building, pero este lleno;  se va a deambular un rato.
                TakingCareOfNeed = false;

                TakeCareOfNeed(GlobalObject.NeedScale.Wander);
                yield break;
            }
            else
            {
                //--- En caso de que sea un Building y haya cupo, entra en el edificio, aumenta los personitas en el edificio, y comienza a esperar el tiempo correspondiente
                //--- Al terminar, sale del edificio y se va a realizar sus tareas normales
                ExecutingBuilding = true;
                myDestinationBuilding.CurrentAgentCount++;
                SetVisibility(false);

                yield return new WaitForSeconds(myDestinationBuilding.TimeToCoverNeed);
                for (int i = 0; i < myNeedList.Count; i++)
                {
                    if (myNeedList[i].Need == NeedTakenCare)
                    {
                        myNeedList[i].CurrentPercentage -= myDestinationBuilding.PercentageRestored;
                        break;
                    }
                }

                ExecutingBuilding = false;
                SetVisibility(true);
                myDestinationBuilding.CurrentAgentCount--;
                myDestinationBuilding = null;
                TakingCareOfNeed = false;
                yield break;
            }
        }
    }

    public void SetVisibility(bool visible)
    {
        AnimationObject.SetActive(visible);       
    }

}