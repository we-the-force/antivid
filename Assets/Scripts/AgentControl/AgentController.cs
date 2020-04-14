using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentController : MonoBehaviour
{
    public AgentIcon IconController;

    public float TotalCellsInBody;
    public float CellsCuredPerTic;
    private float CurrentInfectedCells;

    public float ContagioPerTic;

    private float HappinessCoeficient;

    public float FactorContagio = 1.0f;
    public float PorcentageContagio { get; set; }

    public int HasTraveledQuantity { get; set; }

    private bool Sneezing;

    public float Speed;

    public int AgentID;

    public GameObject AnimationObject;
    public Animator AnimatorController;

    public GlobalObject.AgentStatus myStatus;
    public GlobalObject.AgentStatus previousStatus;

    public List<NeedPercentage> myNeedList;

    public Rigidbody myRigidBody;

    public PathFindingNode myCurrentNode;
    public int myDestinationNodeID;

    private BuildingController myDestinationBuilding;

    public BuildingController myHouse;

    public GlobalObject.AgentPerk myPerk;

    public int currentNodeId;

    private int TicCounter;

    /// <summary>
    /// La necesidad que en este momento se esta atendiendo
    /// </summary>
    public GlobalObject.NeedScale NeedTakenCare;
    public bool TakingCareOfNeed = false;
    private bool ExecutingBuilding = false;

    private AgentMovement movementController;

    public float travelResistance = 1.0f;

    public GameObject SickIndicator;

    public float resourceProduction = 1.0f;

    public float percentageHospitalOnMildCase = 0;
    public bool visitedHospitalOnMildCase;

    public float percentageHospitalRandomCase = 0;
    public bool visitedHospitalOnRandomCase;


    public void InitAgent()
    {
        WorldManager.TicDelegate += TicReceived;

        CurrentInfectedCells = 0;

        HasTraveledQuantity = 0;

        movementController = gameObject.GetComponent<AgentMovement>();

        movementController.Speed = Speed;

        int rndPerk = Random.Range(1, 8);
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
            case 5:
                myPerk = GlobalObject.AgentPerk.LeasureTraveler;
                break;
            case 6:
                myPerk = GlobalObject.AgentPerk.Executive;
                break;
            case 7:
                myPerk = GlobalObject.AgentPerk.Gamer;
                break;
            default:
                myPerk = GlobalObject.AgentPerk.None;
                break;
        }

        for (int i = 0; i < WorldManager.instance.NeedPercentageBaseCollection.Count; i++)
        {
            if (WorldManager.instance.NeedPercentageBaseCollection[i].Perk == myPerk)
            {
                PerkPercentages pp = WorldManager.instance.NeedPercentageBaseCollection[i];

                myNeedList = pp.NeedPercentageCollection;
                resourceProduction = pp.resourceProduction;

                if (pp.WillAttendHospitalOnMildCase)
                    percentageHospitalOnMildCase = Random.Range(2.0f, 6.0f);

                if (pp.WillRandomlyAttendHospital)
                    percentageHospitalRandomCase = Random.Range(46.0f, 49.8f);

                break;
            }
        }

        previousStatus = myStatus;

        SetVisibility(false);

        StartCoroutine("Life");
    }

    private void TicReceived()
    {
        movementController.TicReceived();

        if (PorcentageContagio > 0 && PorcentageContagio <= 100)
        {
            AddContagion(ContagioPerTic, false);
        }

        if (myStatus == GlobalObject.AgentStatus.Out_of_circulation)
        {
            return;
        }

        //-- Sumar porcentaje a la cantidad de enfermedad
        switch (myStatus)
        {
            case GlobalObject.AgentStatus.Mild_Case:
                AddInfectedCells(false);

                float cellTreshold = TotalCellsInBody * 0.75f;

                if (CurrentInfectedCells > cellTreshold)
                {
                    myStatus = GlobalObject.AgentStatus.Serious_Case;
                }

                break;

            case GlobalObject.AgentStatus.Serious_Case:
                AddInfectedCells(false);
                if (CurrentInfectedCells > TotalCellsInBody)
                {
                    myStatus = GlobalObject.AgentStatus.Out_of_circulation;
                    movementController.StopMovement();

                    ExecutingBuilding = false;
                    SetVisibility(true);

                    if (myDestinationBuilding != null)
                    {
                        myDestinationBuilding.CurrentAgentCount--;
                        myDestinationBuilding = null;
                    }

                    TakingCareOfNeed = false;

                    TakeCareOfNeed(GlobalObject.NeedScale.Sleep);

                    return;
                }
                break;

            case GlobalObject.AgentStatus.BeingTreated:
                //--- En este caso, ya esta en el hospital, y no saldra de ahi hasta que la cantidad de sus celulas curadas sea la 
                //--- totalidad de las celulas en el cuerpo, la cura progresara dependiendo de el coeficiente de 
                //--- felicidad total del agente en ese momento
                CurrentInfectedCells -= (CellsCuredPerTic + (CellsCuredPerTic * HappinessCoeficient));


                if (myStatus == GlobalObject.AgentStatus.Mild_Case)
                {
                    CurrencyManager.Instance.CurrentCurrency -= 200;
                }
                else if (myStatus == GlobalObject.AgentStatus.Serious_Case)
                {
                    CurrencyManager.Instance.CurrentCurrency -= 400;
                }
                else
                {
                    CurrencyManager.Instance.CurrentCurrency -= 100;
                }


                if (CurrentInfectedCells <= 0)
                {
                    //--- ya puede salir del hospital ya tiene inmunidad, ya puede hacer lo que sea
                    myStatus = GlobalObject.AgentStatus.Inmune;

                    ExecutingBuilding = false;
                    SetVisibility(true);
                    myDestinationBuilding.CurrentAgentCount--;
                    myDestinationBuilding = null;
                    TakingCareOfNeed = false;
                }
                break;
        }

        //--- Le avisa al Controller que actualice la cantidad de recursos que se obtienen, ya que esta infectado/inmunizado
        if (previousStatus != myStatus)
        {
            //Debug.LogError($"Ay valiendo queso cambie de status     ({previousStatus}, {myStatus})");
            if (myStatus == GlobalObject.AgentStatus.Mild_Case || myStatus == GlobalObject.AgentStatus.Serious_Case || myStatus == GlobalObject.AgentStatus.Out_of_circulation || myStatus == GlobalObject.AgentStatus.Inmune)
            {
                WorldAgentController.instance.CalculateAgentIncome();
            }
        }


        previousStatus = myStatus;

        if (ExecutingBuilding)
        {
            TicCounter++;
            return;
        }

        if (Sneezing)
        {
            return;
        }

        //--- Con esto se sustituye lo de la corutina de la vida
        //if (!ExecutingBuilding && !Sneezing)
        //{
            if (myStatus != GlobalObject.AgentStatus.Out_of_circulation && myStatus != GlobalObject.AgentStatus.BeingTreated)
                TakeCareOfNeed(NextNeed());
        //}                     
    }

    IEnumerator Life()
    {
        //--- Inicializa cada agente con un tiempo diferente en IDLE, y despues comienza a deambular.
        float time = Random.Range(0.5f, 3.5f);
        yield return new WaitForSeconds(time);
        TakeCareOfNeed(GlobalObject.NeedScale.Wander);
    }

    private void SetHappiness()
    {
        float _coefficient = 0;
        float _happiness = myNeedList.Count;

        for (int i = 0; i < myNeedList.Count; i++)
        {
            float currPercentage = Mathf.Clamp(myNeedList[i].CurrentPercentage, 0, 200);
            _coefficient = currPercentage / 200.0f;

            _happiness -= _coefficient;
        }

        HappinessCoeficient = _happiness / myNeedList.Count;
    }

    private GlobalObject.NeedScale NextNeed()
    {
        GlobalObject.NeedScale _need = GlobalObject.NeedScale.None;

        float _tmpPercentage;
        float _maxPercentage = -999;

        for (int i = 0; i < myNeedList.Count; i++)
        {
            _tmpPercentage = myNeedList[i].PercentageDifference();
            if (_tmpPercentage > _maxPercentage)
            {
                _maxPercentage = _tmpPercentage;
                _need = myNeedList[i].Need;
            }
        }

        if (_maxPercentage <= 0)
        {
            _need = GlobalObject.NeedScale.None;
        }

        if (myStatus == GlobalObject.AgentStatus.Serious_Case)
        {
            if (_maxPercentage < WorldAgentController.instance.NeedTreshhold)
            {
                _need = GlobalObject.NeedScale.HealtCare;
            }
        }

        return _need;
    }

    private void TakeCareOfNeed(GlobalObject.NeedScale _need)
    {
        //--- Si ya se esta atendiendo una necesidad, no cambia de necesidad, pero si va 
        //--- sumando porcentaje de necesidad
        SetVisibility(true);

        if (TakingCareOfNeed)
            return;

        if (_need == GlobalObject.NeedScale.None)
            return;

        TakingCareOfNeed = true;
        NeedTakenCare = _need;

        if (_need == GlobalObject.NeedScale.Wander)
        {
            //--- Obtiene un id random de camino para deambular
            GetRoad();
        }
        else if (_need == GlobalObject.NeedScale.Sleep)
        {
            myDestinationBuilding = myHouse;
            myDestinationNodeID = myDestinationBuilding.AssociatedNode.NodeID;
        }
        else
        {
            //Debug.LogError("Buscar Edificio para necesidad: " + _need.ToString());

            myDestinationBuilding = WorldAgentController.instance.GetBuilding(_need, myCurrentNode);

            if (myDestinationBuilding == null)
            {
                //Debug.LogError($"Building was null! Need: {_need}");
                _need = GlobalObject.NeedScale.Wander;
                NeedTakenCare = _need;
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

        IconController.ShowIconFor(NeedTakenCare);

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
        int _ticCounter = 0;
        NeedPercentage _myNeedPercentage = null;
        for (int i = 0; i < myNeedList.Count; i++)
        {
            if (myNeedList[i].Need == NeedTakenCare)
            {
                _ticCounter = Mathf.RoundToInt(myNeedList[i].TicsToCoverNeed);
                _myNeedPercentage = myNeedList[i];
                break;
            }
        }
        
        if (NeedTakenCare == GlobalObject.NeedScale.Wander)
        {
            if(_myNeedPercentage != null) _myNeedPercentage.CurrentPercentage = 0;

            if (myStatus == GlobalObject.AgentStatus.Mild_Case || myStatus == GlobalObject.AgentStatus.Serious_Case)
            {
                StartCoroutine("Sneeze");
            }
            else
            {
                ExecutingBuilding = true;
                TicCounter = 0;
                while (true)
                {
                    if (TicCounter == _ticCounter)
                    {
                        break;
                    }

                    yield return new WaitForFixedUpdate();
                }
            }
            TakingCareOfNeed = false;
            ExecutingBuilding = false;
            yield break;
        }

        if (myDestinationBuilding != null)
        {
            if (myDestinationBuilding.CurrentAgentCount == myDestinationBuilding.AgentCapacity)
            {
                TakingCareOfNeed = false;

                //-- En caso de que este lleno el edificio pero este muy enfermo el agente
                //--- debera regresar a casa a dormir un rato
                if (myStatus == GlobalObject.AgentStatus.Serious_Case)
                {
                    TakeCareOfNeed(GlobalObject.NeedScale.Sleep);
                    yield break;
                }
                //--- En caso de que sea un Building, pero este lleno;  se va a deambular un rato.
                TakeCareOfNeed(GlobalObject.NeedScale.Wander);
                yield break;
            }
            else
            {
                //--- En caso de que sea un Building y haya cupo, entra en el edificio, aumenta los personitas en el edificio, y comienza a esperar el tiempo correspondiente
                //--- Al terminar, sale del edificio y se va a realizar sus tareas normales
                ExecutingBuilding = true;
                myDestinationBuilding.CurrentAgentCount++;

                if (myStatus == GlobalObject.AgentStatus.Mild_Case || myStatus == GlobalObject.AgentStatus.Serious_Case)
                {
                    for (int i = 0; i < WorldAgentController.instance.AgentCollection.Count; i++)
                    {
                        AgentController _agent = WorldAgentController.instance.AgentCollection[i];

                        if (_agent.ExecutingBuilding)
                        {
                            _agent.AddContagion(WorldManager.instance.buildingInfectionPercentage, false);
                        }
                    }
                }

                SetVisibility(false);

                if (myStatus == GlobalObject.AgentStatus.Out_of_circulation)
                {
                    //--- En el caso de que el agente esta fuera de circulacion, detiene todas sus acciones.
                    WorldManager.TicDelegate -= TicReceived;
                    IconController.gameObject.SetActive(false);
                    StopAllCoroutines();
                    yield break;
                }

                if (myDestinationBuilding.MainNeedCovered == GlobalObject.NeedScale.HealtCare)
                {
                    myStatus = GlobalObject.AgentStatus.BeingTreated;
                    yield break;
                }

                TicCounter = 0;
                while (true)
                {
                    if (TicCounter >= _ticCounter) // myDestinationBuilding.TicsToCoverNeed)
                    {

                        //{
                        CurrencyManager.Instance.CurrentCurrency += 500;
                        //}
                        break;
                    }
                    yield return new WaitForFixedUpdate();
                }

                if(_myNeedPercentage != null) _myNeedPercentage.CurrentPercentage -= myDestinationBuilding.PercentageRestored;

                //yield return new WaitForSeconds(myDestinationBuilding.TimeToCoverNeed);

                //  for (int i = 0; i < myNeedList.Count; i++)
                // {
                // if (myNeedList[i].Need == NeedTakenCare)
                //{


                // myNeedList[i].CurrentPercentage -= myDestinationBuilding.PercentageRestored;
                //  break;
                // }
                //  }

                ExecuteBuildingAction(myDestinationBuilding.MainNeedCovered);

                ExecutingBuilding = false;
                SetVisibility(true);
                myDestinationBuilding.CurrentAgentCount--;
                myDestinationBuilding = null;

                if (myStatus == GlobalObject.AgentStatus.Mild_Case || myStatus == GlobalObject.AgentStatus.Serious_Case)
                {
                    StartCoroutine("Sneeze");
                }

                TakingCareOfNeed = false;
                yield break;
            }
        }
    }

    public void ExecuteBuildingAction(GlobalObject.NeedScale _needCovered)
    {
        switch (_needCovered)
        {
            case GlobalObject.NeedScale.Travel:
                Debug.LogError("HA REALIZADO VIAJE");
                HasTraveledQuantity++;

                if (myStatus == GlobalObject.AgentStatus.Healty)
                {
                    if (PorcentageContagio == 0)
                    {
                        //--- Dependiendo del escenario; se estima la probabilidad de contagio 
                        if (HasTraveledQuantity >= WorldManager.instance.InfectionGuaranteedAfterNumberOfTravel)
                        {
                            AddContagion(WorldManager.instance.buildingInfectionPercentage, false);
                            Debug.LogError("Agregado contagio por que ya ha viajado muchas veces " + PorcentageContagio);
                        }
                        else
                        {
                            float _rand = Random.Range(0.0f, 100.0f);
                            if (_rand < WorldManager.instance.TravelInfectionPercentage)
                            {
                                AddContagion(WorldManager.instance.buildingInfectionPercentage, false);
                                Debug.LogError("Agregado contagio por que le ha tocado la suerte " + PorcentageContagio);
                            }
                        }
                    }
                    else
                    {
                        AddContagion(WorldManager.instance.buildingInfectionPercentage, false);
                        Debug.LogError("Ya tiene bichos, agregado su dosis adicional " + PorcentageContagio);
                    }
                }
                break;
        }
    }
    
    public void SetVisibility(bool visible)
    {
        AnimationObject.SetActive(visible);
    }

    private void AddInfectedCells(bool fromSneeze)
    {
        float _infected = WorldAgentController.instance.InfectedCellPerTic + (WorldAgentController.instance.InfectedCellPerTic * (1.0f - HappinessCoeficient));

        if (fromSneeze)
            _infected *= 2;

        if (myStatus == GlobalObject.AgentStatus.Serious_Case)
        {
            _infected = _infected * WorldAgentController.instance.SeriousIllnessInfectionFactor;
        }

        CurrentInfectedCells += _infected;
    }

    IEnumerator Sneeze()
    {
        movementController.StopMovement();
        Sneezing = true;
        TakingCareOfNeed = false;

        yield return new WaitForFixedUpdate();
        AnimatorController.SetInteger("CurrentState", 0);

        yield return new WaitForSeconds(0.25f);
        GameObject obj = Instantiate(WorldAgentController.instance.SneezePrefab, WorldAgentController.instance.AgentAnchor);
        obj.transform.position = transform.position;

        //--- Especifica que tipo de estornudo es
        obj.GetComponent<SneezeController>().SneezeStatus = myStatus;
        obj.GetComponent<SneezeController>().Init();

        yield return new WaitForSeconds(0.35f);
        Sneezing = false;
    }

    public void AddContagion(float _percentage, bool fromSneeze)
    {
        if (myStatus == GlobalObject.AgentStatus.Inmune)
            return;

        if (fromSneeze) AddInfectedCells(true);

        if (PorcentageContagio > 100)
            return;

        PorcentageContagio += _percentage * FactorContagio;

        if (PorcentageContagio >= 100)
        {
            SickIndicator.SetActive(fromSneeze);
            myStatus = GlobalObject.AgentStatus.Mild_Case;
        }
    }
    public void RightClick()
    {
        CameraController.Instance.SetObjectToFollow(gameObject);
    }
}
