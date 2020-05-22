using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentController : MonoBehaviour
{
    public AgentIcon IconController;

    public agentAnimController CanvasAnim;

    public float TotalCellsInBody;
    public float CellsCuredPerTic;
    [SerializeField]
    private float CurrentInfectedCells;

    public float ContagioPerTic;

    public float HappinessCoeficient;

    public float FactorContagio = 1.0f;
    //public float PorcentageContagio { get; set; }
    public float PorcentageContagio;

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

    public List<WarehouseItemObject> BackPack;
    public float PercentageForTheBackPack;

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

    private void OnDestroy()
    {
        WorldManager.TicDelegate -= TicReceived;
    }

    public void InitAgent(GlobalObject.AgentPerk useThisPerk = GlobalObject.AgentPerk.Random)
    {
        WorldManager.TicDelegate += TicReceived;

        CanvasAnim.myCanvas.worldCamera = CameraController.Instance.Cam;

        CurrentInfectedCells = 0;

        HasTraveledQuantity = 0;

        movementController = gameObject.GetComponent<AgentMovement>();

        movementController.Speed = Speed;

        if (useThisPerk == GlobalObject.AgentPerk.Random)
        {
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
        }
        else
        {
            myPerk = useThisPerk;
        }



        GameObject animObj = Instantiate(WorldAgentController.instance.GetAnimationForPerk(myPerk), AnimationObject.transform);
        AnimatorController = animObj.GetComponent<Animator>();
        animObj.transform.localPosition = Vector3.zero;

        //--- Asigna los porcentajes de necesidades de acuerdo al Perk establecido para el agente
        for (int i = 0; i < WorldManager.instance.NeedPercentageBaseCollection.Count; i++)
        {
            if (WorldManager.instance.NeedPercentageBaseCollection[i].Perk == myPerk)
            {
                PerkPercentages pp = WorldManager.instance.NeedPercentageBaseCollection[i];

                //myNeedList = pp.NeedPercentageCollection;
                myNeedList = pp.CloneNeedPercentage();
                resourceProduction = pp.resourceProduction;
                PercentageForTheBackPack = pp.PercentageForBackPack;

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
        //--- Avienta un tic al controlador de movimiento, con lo cual en caso de estar avanzando controla la velocidad relativa a la escala de tiempo
        movementController.TicReceived();

        //--- Un agente que esta fuera de circulacion, debe regresar a casa y apagarse
        //--- es por eso que todas las acciones del TIC ya no se ejecutan, solo el movimiento
        //--- para que pueda regresar a su casa
        if (myStatus == GlobalObject.AgentStatus.Out_of_circulation)
            return;

        SetHappiness();

        //--- Ejecuta la logica de contagio, solo aplica en caso de tener 
        //--- Porcentage de Contagio mayor a 0, pero menor que 100. Llegando a 100, ya pasa a la siguiente etapa de contagio (Caso Leve de enfermedad)
        AddContagion(ContagioPerTic, false);

        //--- Sumar porcentaje a la cantidad de enfermedad. Esto solo aplicara cuando el agente tenga ya una enfermedad
        //--- activada, y el metodo regresa verdadero en caso de que el agente queda fuera de circulacion, a partir de lo cual ya no debe 
        //--- continuar con las acciones del TIC
        if (HandleBodyCells())
            return;

        //--- Le avisa al Controller que actualice la cantidad de recursos que se obtienen, ya que esta infectado/inmunizado
        if (previousStatus != myStatus)
        {
            StatusChanged();
            //Debug.LogError($"Ay valiendo queso cambie de status     ({previousStatus}, {myStatus})");
            //if (myStatus == GlobalObject.AgentStatus.Mild_Case || myStatus == GlobalObject.AgentStatus.Serious_Case || myStatus == GlobalObject.AgentStatus.Out_of_circulation || myStatus == GlobalObject.AgentStatus.Inmune)
            //{
                WorldAgentController.instance.CalculateAgentIncome();
            //}
        }
        previousStatus = myStatus;

        //--- Cuando se estan ejecutando los edificios, se suman los un contador de Tic para tener control del tiempo
        //--- de ejecucion de los edificios, debe esperar a que termine de ejecutar el edificio para calcular la 
        //--- siguiente necesidad
        if (ExecutingBuilding)
        {
            TicCounter++;
            return;
        }

        //--- Si estornuda hace una pausa, para no moverse de ese lugar hasta que termine el estornudo
        if (Sneezing)
            return;

        //--- Determina la siguiente necesidad que tiene que cubrir el agente, y en caso de que si exista 
        //--- esa necesidad, solicita la ruta hacia el edificio y comienza a ejecutar
        if (myStatus != GlobalObject.AgentStatus.Out_of_circulation && myStatus != GlobalObject.AgentStatus.BeingTreated)
        {
            //--- Si hay cuarentena, se ejecuta el comportamiento de cuarentena.
            //--- Si no, se ejecuta el comportamiento normalon.
            if (PolicyManager.Instance.IsOnQuarantine)
            {
                HandleQuarantine();
            }
            else
            {
                TakeCareOfNeed(NextNeed());
            }
        }
        //}

    }

    IEnumerator Life()
    {
        //--- Inicializa cada agente con un tiempo diferente en IDLE, y despues comienza a deambular.
        float time = Random.Range(2.5f, 3.5f);
        yield return new WaitForSeconds(time);
        TakeCareOfNeed(GlobalObject.NeedScale.Wander);
    }

    /// <summary>
    /// Regresa verdadero solo cuando queda fuera de circulacion
    /// </summary>
    /// <returns></returns>
    private bool HandleBodyCells()
    {
        bool isOutOfCirculation = false;
        switch (myStatus)
        {
            case GlobalObject.AgentStatus.Mild_Case:
                AddInfectedCells(false);
                float cellTreshold = TotalCellsInBody * 0.55f;

                //--- En caso de que las celulas infectadas en el cuerpo del agente sobrepasen
                //--- un limite establecido, el agente obtiene un caso serio de enfermedad
                //--- Si el limite es mas pequeño, es mas probable que sobreviva (tiene mayor oportunidad 
                //--- de llegar al hospital antes de quedar fuera de circulacion
                if (CurrentInfectedCells > cellTreshold)
                {
                    myStatus = GlobalObject.AgentStatus.Serious_Case;
                }
                break;

            case GlobalObject.AgentStatus.Serious_Case:
                AddInfectedCells(false);

                //--- Cuando la cantidad de celulas infectadas en el cuerpo sobrepasa la cantidad total
                //--- de celulas, ese agente queda fuera de circulacion, y ya no puede ser atendido en el hospital 
                //--- lo unico que le queda hacer es regresar a casa y apagarse, ya tampoco contribuye a la economia
                if (CurrentInfectedCells > TotalCellsInBody)
                {
                    myStatus = GlobalObject.AgentStatus.Out_of_circulation;
                    movementController.StopMovement();

                    if (ExecutingBuilding && myDestinationBuilding != null)
                    {
                        myDestinationBuilding.CurrentAgentCount--;
                        //myDestinationBuilding = null;
                    }

                    ExecutingBuilding = false;
                    SetVisibility(true);
                    TakingCareOfNeed = false;

                    TakeCareOfNeed(GlobalObject.NeedScale.Sleep);

                    isOutOfCirculation = true;
                }
                break;

            case GlobalObject.AgentStatus.BeingTreated:
                //--- En este caso, ya esta en el hospital, y no saldra de ahi hasta que la cantidad de sus celulas curadas sea la 
                //--- totalidad de las celulas en el cuerpo, la cura progresara dependiendo de el coeficiente de 
                //--- felicidad total del agente en ese momento

                //CurrentInfectedCells -= (CellsCuredPerTic * HappinessCoeficient); // (CellsCuredPerTic + (CellsCuredPerTic * HappinessCoeficient));
                CurrentInfectedCells -= (myDestinationBuilding.PercentageRestored * HappinessCoeficient); // (CellsCuredPerTic + (CellsCuredPerTic * HappinessCoeficient));

                if (myStatus == GlobalObject.AgentStatus.Mild_Case)
                {
                    //CurrencyManager.Instance.CurrentCurrency -= 0.01f;
                    CurrencyManager.Instance.UseBuilding(GlobalObject.NeedScale.HealtCare, GlobalObject.AgentStatus.Mild_Case);
                }
                else if (myStatus == GlobalObject.AgentStatus.Serious_Case)
                {
                    //CurrencyManager.Instance.CurrentCurrency -= 0.03f;
                    CurrencyManager.Instance.UseBuilding(GlobalObject.NeedScale.HealtCare, GlobalObject.AgentStatus.Serious_Case);
                }
                else
                {
                    //CurrencyManager.Instance.CurrentCurrency -= 0.01f;
                    CurrencyManager.Instance.UseBuilding(GlobalObject.NeedScale.HealtCare, GlobalObject.AgentStatus.Mild_Case);
                }

                if (CurrentInfectedCells <= 0)
                {
                    //--- ya puede salir del hospital ya tiene inmunidad, ya puede hacer lo que sea
                    //myStatus = GlobalObject.AgentStatus.Inmune;
                    //--- Que siempre no tienen inmunidad
                    myStatus = GlobalObject.AgentStatus.Healty;

                    ExecutingBuilding = false;
                    SetVisibility(true);
                    myDestinationBuilding.CurrentAgentCount--;
                    myDestinationBuilding = null;
                    TakingCareOfNeed = false;
                    if (!VaccineManager.Instance.IsVaccineGenerated)
                    {
                        //CurrentInfectedCells = 1;
                        PorcentageContagio = 0.1f;
                    }
                }
                break;
        }

        return isOutOfCirculation;
    }

    public void HandleCellsWithVaccine()
    {
        if (myStatus == GlobalObject.AgentStatus.Out_of_circulation)
            return;

        CurrentInfectedCells = 0;
        if (myStatus == GlobalObject.AgentStatus.BeingTreated)
        {
            ExecutingBuilding = false;
            SetVisibility(true);
            myDestinationBuilding.CurrentAgentCount--;
            myDestinationBuilding = null;
            TakingCareOfNeed = false;
        }

        myStatus = GlobalObject.AgentStatus.Inmune;
    }

    private void SetHappiness()
    {
        float _coefficient = 0;

        float _coefficientTotal = 0;
        float _happiness = 0;//  myNeedList.Count;

        for (int i = 0; i < myNeedList.Count; i++)
        {
            if (myNeedList[i].Need != GlobalObject.NeedScale.Wander)
           {
                float currPercentage = Mathf.Clamp(myNeedList[i].CurrentPercentage, 0, 200);
                _coefficient = currPercentage / 200.0f;

                if (_coefficient < 0.55f) _coefficient = 0;

                _coefficientTotal += _coefficient;

                _happiness++;

                //_happiness -= _coefficient;
            }
        }

        float totHappiness = _happiness;
         _happiness -= _coefficientTotal;

        //HappinessCoeficient = _happiness / myNeedList.Count;
        HappinessCoeficient = _happiness / totHappiness;
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

    /// <summary>
    /// Cuando hay cuarentena se mete a este chavo
    /// Primero checa si esta en su casita; si no, lo manda a mimir.
    /// Despues, si ya esta en su casita, pone invisible al chavo, le incrementa sus necesidades
    /// si su status es Serious_Case lo manda al hospital. Si su hambre sube mucho tambien lo manda a comers.
    /// </summary>
    private void HandleQuarantine()
    {
        //Debug.LogError("Quarantine :C");
        if (myDestinationBuilding != myHouse && !ExecutingBuilding)
        {
            TakeCareOfNeed(GlobalObject.NeedScale.Sleep);
        }
        else if (myCurrentNode.ConnectedNodes.Contains(myHouse.GetComponent<PathFindingNode>()))
        {
            SetVisibility(false);
            for (int i = 0; i < myNeedList.Count; i++)
            {
                myNeedList[i].PercentageDifference();
            }

            myNeedList.Find(x => x.Need == GlobalObject.NeedScale.Sleep).CurrentPercentage = 0;

            foreach (NeedPercentage np in myNeedList)
            {
                if (np.Need == GlobalObject.NeedScale.Hunger || np.Need == GlobalObject.NeedScale.Entertainment || np.Need == GlobalObject.NeedScale.Education)
                {
                    if (np.CurrentPercentage > np.PercentageToCompare)
                    {
                        float quantity = myHouse.myWarehouse.UseGoods(np.Need, 20f);
                        np.CurrentPercentage -= quantity;
                        if (myHouse.GetComponent<PathFindingNode>().NodeID == 1)
                        {
                            Debug.Log($"Agent{AgentID} took {quantity} from houseID: {1}");
                        }
                    }
                }
            }

            List<NeedPercentage> CriticalNeeds = myNeedList.FindAll(x => (x.CurrentPercentage > (x.PercentageToCompare * 2)) && (x.Need == GlobalObject.NeedScale.Hunger || x.Need == GlobalObject.NeedScale.HealtCare));

            if (myStatus == GlobalObject.AgentStatus.Serious_Case)
            {
                Debug.Log("Should be leaving Quarantine now :'c (Gotta go to the hospital)");
                SetVisibility(true);
                TakeCareOfNeed(GlobalObject.NeedScale.HealtCare);
            }
            if (CriticalNeeds.Count > 0)
            {
                Debug.LogError($"Should be leaving Quarantine now (qty: {CriticalNeeds.Count}, [0]: {CriticalNeeds[0].Need}, Perk: {myPerk}) :'c");
                SetVisibility(true);
                TakeCareOfNeed(GlobalObject.NeedScale.Hunger);
            }
        }
    }

    private void TakeCareOfNeed(GlobalObject.NeedScale _need)
    {
        //--- Si ya se esta atendiendo una necesidad, no cambia de necesidad, pero si va 
        //--- sumando porcentaje de necesidad
        if (TakingCareOfNeed)
            return;

        if (_need == GlobalObject.NeedScale.None)
        {
            TakeCareOfNeed(GlobalObject.NeedScale.Wander);
            return;
        }      

        SetVisibility(true);

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
                Debug.LogError($"Building was null! Need: {_need}");
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
            if (_myNeedPercentage != null) _myNeedPercentage.CurrentPercentage = 0;

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

                /*
                //-- En caso de que este lleno el edificio pero este muy enfermo el agente
                //--- debera regresar a casa a dormir un rato
                if (myStatus == GlobalObject.AgentStatus.Serious_Case)
                {
                    TakeCareOfNeed(GlobalObject.NeedScale.Sleep);
                    yield break;
                }
                //--- En caso de que sea un Building, pero este lleno;  se va a deambular un rato.
                Debug.LogError($"Building was full Need: {NeedTakenCare} [AgentController.ExecuteDestiny()]");
                TakeCareOfNeed(GlobalObject.NeedScale.Wander);
                */
                yield break;
            }
            else
            {
                //--- En caso de que sea un Building y haya cupo, entra en el edificio, aumenta los personitas en el edificio, y comienza a esperar el tiempo correspondiente
                //--- Al terminar, sale del edificio y se va a realizar sus tareas normales
                ExecutingBuilding = true;
                myDestinationBuilding.CurrentAgentCount++;

                myDestinationBuilding.ResetOccupants();

                //--- Agrega contagio a la gente que esta en el edificio
                //--- En caso de que existan politicas de distanciamiento y asi, aqui se colocara 
                //--- un porcentaje de que suceda un contagio (Pero solo si no es hospital)
                if (myDestinationBuilding.MainNeedCovered != GlobalObject.NeedScale.HealtCare)
                {
                    if (myStatus == GlobalObject.AgentStatus.Mild_Case || myStatus == GlobalObject.AgentStatus.Serious_Case)
                    {
                        for (int i = 0; i < WorldAgentController.instance.AgentCollection.Count; i++)
                        {
                            AgentController _agent = WorldAgentController.instance.AgentCollection[i];

                            if (_agent.ExecutingBuilding && _agent.currentNodeId == currentNodeId)
                            {
                                _agent.AddContagion(WorldManager.instance.buildingInfectionPercentage, false);
                            }
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

                _ticCounter = myDestinationBuilding.TicsToCoverNeed;
                TicCounter = 0;
                while (true)
                {
                    if (myStatus == GlobalObject.AgentStatus.Serious_Case)
                    {
                        //--- Ya se enfermo bien pasado, necesita irse al hospital de una vez
                        ExecutingBuilding = false;

                        SetVisibility(true);
                        myDestinationBuilding.CurrentAgentCount--;
                        myDestinationBuilding.ResetOccupants();
                        StartCoroutine("Sneeze");
                        TakingCareOfNeed = false;
                        yield break;
                    }

                    if (TicCounter >= _ticCounter) // myDestinationBuilding.TicsToCoverNeed)
                    {
                        CurrencyManager.Instance.UseBuilding(NeedTakenCare);
                        //CurrencyManager.Instance.CurrentCurrency += 0.075f;// 500;
                        break;
                    }
                    yield return new WaitForFixedUpdate();
                }                

                if (NeedTakenCare == GlobalObject.NeedScale.Sleep)
                {
                    for (int i = 0; i < BackPack.Count; i++)
                    {
                        myHouse.myWarehouse.StoreGoods(BackPack[i].Need, BackPack[i].CurrentQty * 1.5f);
                        BackPack[i].CurrentQty = 0;
                    }
                }
                else
                {
                    for (int i = 0; i < BackPack.Count; i++)
                    {
                        if (BackPack[i].Need == NeedTakenCare)
                        {
                            BackPack[i].CurrentQty += myDestinationBuilding.PercentageRestored * PercentageForTheBackPack;
                            if (BackPack[i].CurrentQty > BackPack[i].CurrentMaxQty)
                                BackPack[i].CurrentQty = BackPack[i].CurrentMaxQty;
                            break;
                        }
                    }
                }

                if (_myNeedPercentage != null)
                {
                    if (_myNeedPercentage.CurrentPercentage - myDestinationBuilding.PercentageRestored < 0)
                    {
                        _myNeedPercentage.CurrentPercentage = 0;
                    }
                    else
                    {
                        _myNeedPercentage.CurrentPercentage -= myDestinationBuilding.PercentageRestored;

                    }
                }

                ExecuteBuildingAction(myDestinationBuilding.MainNeedCovered);

                ExecutingBuilding = false;

                SetVisibility(true);
                myDestinationBuilding.CurrentAgentCount--;
                myDestinationBuilding.ResetOccupants();
                //   myDestinationBuilding = null;

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
                //Debug.LogError("HA REALIZADO VIAJE");
                HasTraveledQuantity++;

                if (myStatus == GlobalObject.AgentStatus.Healty)
                {
                    if (PorcentageContagio == 0)
                    {
                        //--- Dependiendo del escenario; se estima la probabilidad de contagio 
                        if (HasTraveledQuantity >= WorldManager.instance.InfectionGuaranteedAfterNumberOfTravel)
                        {
                            AddContagion(WorldManager.instance.buildingInfectionPercentage, false);
                            //Debug.LogError("Agregado contagio por que ya ha viajado muchas veces " + PorcentageContagio);
                        }
                        else
                        {
                            float _rand = Random.Range(0.0f, 100.0f);
                            if (_rand < WorldManager.instance.TravelInfectionPercentage)
                            {
                                AddContagion(WorldManager.instance.buildingInfectionPercentage, false);
                                //Debug.LogError("Agregado contagio por que le ha tocado la suerte " + PorcentageContagio);
                            }
                        }
                    }
                    else
                    {
                        AddContagion(WorldManager.instance.buildingInfectionPercentage, false);
                        //Debug.LogError("Ya tiene bichos, agregado su dosis adicional " + PorcentageContagio);
                    }
                }
                break;
        }
    }

    public void SetVisibility(bool visible)
    {
        IconController.gameObject.SetActive(visible);
        AnimationObject.SetActive(visible);
    }

    
    public float totalInfectedCellsThisCycle { get; set; }
    private void AddInfectedCells(bool fromSneeze)
    {
        //float _infected = WorldAgentController.instance.InfectedCellPerTic + (WorldAgentController.instance.InfectedCellPerTic * (1.0f - HappinessCoeficient));

        float _infected = WorldAgentController.instance.InfectedCellPerTic / HappinessCoeficient;
        totalInfectedCellsThisCycle += _infected;

        if (fromSneeze)
            _infected *= 1.5f;

        if (myStatus == GlobalObject.AgentStatus.Serious_Case)
        {
            _infected = _infected * WorldAgentController.instance.SeriousIllnessInfectionFactor;
        }

        CurrentInfectedCells += _infected;
    }


    public void StatusChanged()
    {
        if (myStatus == GlobalObject.AgentStatus.Mild_Case || myStatus == GlobalObject.AgentStatus.Serious_Case)
        {
            SickIndicator.SetActive(true);
        }
        else
        {
            SickIndicator.SetActive(false);
        }
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


    public float TotalContagionPerCycle { get; set; }
    public void AddContagion(float _percentage, bool fromSneeze)
    {
        if (myStatus == GlobalObject.AgentStatus.Inmune)
            return;

        if (fromSneeze) AddInfectedCells(true);

        if (PorcentageContagio < 0 || PorcentageContagio > 100)
            return;


        float addContagio = _percentage * FactorContagio;
        TotalContagionPerCycle += addContagio;
        PorcentageContagio += addContagio;

        if (PorcentageContagio < 0)
            PorcentageContagio = 0;

        if (PorcentageContagio >= 100)
        {
            //SickIndicator.SetActive(fromSneeze);
            myStatus = GlobalObject.AgentStatus.Mild_Case;

            //--- Ejecuta el evento especial de el primer infectado en el escenario
            if (!WorldManager.instance.FirstInfectionDetected)
            {
                WorldManager.instance.FirstInfectionDetected = true;
                CanvasControl.instance._announcementWindow.SpecialEvent(GlobalObject.SpecialEventName.FirstInfected);
            }

            //CanvasControl.instance.ShowVaccineIcon(true);
            //VaccineManager.Instance.ShouldTic = true;
        }
    }
    public void RightClick()
    {
        CameraController.Instance.SetObjectToFollow(gameObject);
    }
}
