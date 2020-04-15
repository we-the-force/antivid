using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldAgentController : MonoBehaviour
{
    public GameObject AgentPrefab;

    public List<GameObject> AgentPrefabCollection;

    public Transform AgentAnchor;

    public float SneezeBaseFrequency = 10f;
    public GameObject SneezePrefab;

    public static WorldAgentController instance;

    public List<BuildingController> Buildings;
    public List<PathFindingNode> RoadSystem;

    public List<AgentController> AgentCollection;

    public float NeedTreshhold;

    //--- Propiedades relacionadas con la historia de la enfermedad
    public float InfectedCellPerTic;
    public float SeriousIllnessInfectionFactor;

    //Propiedades del sistema de currency
    [Tooltip("How much resources each agent will give you each period")]
    public float IncomePerAgent;

    public float TotalBuildingUpkeepCost;
    public float TotalAgentIncome;

    public int initialInfectedDudes = 3;

    public List<PerkQuantityForScenario> ScenarioPercentagesForPerks;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        StartCoroutine(DelayedStart());
    }

    void PoblateRoadSystem()
    {
        RoadSystem.Clear();
        RoadSystem = WorldManager.instance.NodeCollection.FindAll(x => x.nodeType == NodeType.Road);
    }
    void PoblateBuildings()
    {
        Buildings.Clear();
        List<NodeType> auxList = new List<NodeType>() { NodeType.HealthCare, NodeType.House, NodeType.Education, NodeType.Food, NodeType.Workplace, NodeType.Entertainment, NodeType.Buyable, NodeType.Border };
        List<PathFindingNode> nodeList = WorldManager.instance.NodeCollection.FindAll(x => auxList.Contains(x.nodeType));
        foreach (PathFindingNode pfn in nodeList)
        {
            BuildingController auxBuildCont = pfn.GetComponent<BuildingController>();
            if (auxBuildCont != null)
            {
                Buildings.Add(auxBuildCont);
            }
        }
    }

    IEnumerator DelayedStart()
    {
        yield return null;
        yield return null;
        PoblateRoadSystem();
        PoblateBuildings();
        for (int i = 0; i < Buildings.Count; i++)
        {
            if (Buildings[i].MainNeedCovered == GlobalObject.NeedScale.Sleep)
            {
                for (int a = 0; a < Buildings[i].AgentCapacity; a++)
                {
                    int rnd = Random.Range(0, AgentPrefabCollection.Count);


                    GameObject obj = Instantiate(AgentPrefabCollection[rnd], AgentAnchor);
                    AgentController _agent = obj.GetComponent<AgentController>();

                    obj.transform.position = Buildings[i].transform.position;

                    _agent.AgentID = a;
                    _agent.myCurrentNode = Buildings[i].AssociatedNode;
                    _agent.currentNodeId = Buildings[i].AssociatedNode.NodeID;

                    _agent.Speed = Random.Range(0.8f, 1.4f);

                    _agent.myHouse = Buildings[i];
                    //_agent.InitAgent();

                    AgentCollection.Add(_agent);
                }
            }
        }

        //--- Calcula las cantidades de personas por tipo de Perk segun los porcentajes establecidos
        for (int i = 0; i < ScenarioPercentagesForPerks.Count; i++)
        {
            ScenarioPercentagesForPerks[i].Qty = Mathf.RoundToInt(AgentCollection.Count * ScenarioPercentagesForPerks[i].Percentage);

            //Debug.LogError("Para Perk " + ScenarioPercentagesForPerks[i].Perk.ToString() + " tantos monitos " + ScenarioPercentagesForPerks[i].Qty);
        }

        List<GlobalObject.AgentPerk> _perksToCreate = new List<GlobalObject.AgentPerk>();
        int cPerk = 0;
        while (true)
        {
            if (ScenarioPercentagesForPerks.Count == 0)
                break;

            if (cPerk == ScenarioPercentagesForPerks.Count)
                cPerk = 0;

            _perksToCreate.Add(ScenarioPercentagesForPerks[cPerk].Perk);

            ScenarioPercentagesForPerks[cPerk].Qty--;

            if (ScenarioPercentagesForPerks[cPerk].Qty == 0)
            {
                ScenarioPercentagesForPerks.RemoveAt(cPerk);
            }
            else
            {
                cPerk++;
            }            
        }

      //  Debug.LogError(">>> TOTAL PERKS " + _perksToCreate.Count + " >>> Agents " + AgentCollection.Count);

        for (int i = 0; i < AgentCollection.Count; i++)
        {
            if (_perksToCreate.Count == i)
            {
                AgentCollection[i].InitAgent(GlobalObject.AgentPerk.Random);
            }
            else
            {
                AgentCollection[i].InitAgent(_perksToCreate[i]);
            }
        }


        //--- Si el escenario tiene una cantidad de agentes infectados desde el inicio, aqui se realiza el calculo de cuales son los infectados
        InfectAgents(initialInfectedDudes);

        //AgentCollection[0].myStatus = GlobalObject.AgentStatus.Mild_Case;
        ////AgentCollection[0].myStatus = GlobalObject.AgentStatus.Serious_Case;
        //AgentCollection[0].SickIndicator.SetActive(true);
        CalculateBuildingUpkeepCost();
        CalculateAgentIncome();
    }

    void InfectAgents(int limit)
    {
        int count = 0;
        while(count < limit)
        {
            int randomIndex = Random.Range(0, AgentCollection.Count - 1);
            if (AgentCollection[randomIndex].myStatus == GlobalObject.AgentStatus.Healty)
            {
                AgentCollection[randomIndex].myStatus = GlobalObject.AgentStatus.Mild_Case;
                AgentCollection[randomIndex].SickIndicator.SetActive(true);
                count++;
            }
        }
    }

    public void CalculateBuildingUpkeepCost()
    {
        TotalBuildingUpkeepCost = 0;
        foreach (BuildingController bc in Buildings)
        {
            TotalBuildingUpkeepCost += bc.UpkeepCost;
        }
    }
    public void CalculateAgentIncome()
    {
        TotalAgentIncome = 0;
        List<GlobalObject.AgentStatus> invalidStatus = new List<GlobalObject.AgentStatus>() { GlobalObject.AgentStatus.Serious_Case, GlobalObject.AgentStatus.Out_of_circulation };
        foreach (AgentController ac in AgentCollection)
        {
            if (ac.myStatus == GlobalObject.AgentStatus.Mild_Case)
            {
                TotalAgentIncome += IncomePerAgent / 1.5f;
            }
            else if (!invalidStatus.Contains(ac.myStatus))
            {
                TotalAgentIncome += IncomePerAgent;
            }
        }
    }
    public int GetBuildingPathfindingNodeID(GlobalObject.NeedScale forNeed)
    {
        int id = -1;

        for (int i = 0; i < Buildings.Count; i++)
        {
            BuildingController _building = Buildings[i];

            if (_building.MainNeedCovered == forNeed && _building.CurrentAgentCount < _building.AgentCapacity)
            {
                id = _building.AssociatedNode.NodeID;
                break;
            }
        }
               
        return id;
    }

    public BuildingController GetBuilding(GlobalObject.NeedScale forNeed, PathFindingNode fromTile)
    {
        List<BuildingController> ElegibleBuildings = new List<BuildingController>();
        BuildingController _building;

        for (int i = 0; i < Buildings.Count; i++)
        {
            _building = Buildings[i];

            if (_building.MainNeedCovered == forNeed && _building.CurrentAgentCount < _building.AgentCapacity)
            {
                ElegibleBuildings.Add(_building);
            }
        }

        if(ElegibleBuildings.Count == 0)
            return null;

        if(ElegibleBuildings.Count == 1)
            return ElegibleBuildings[0];

        int _cost = 99999;
        _building = null;
        for (int i = 0; i < ElegibleBuildings.Count; i++)
        {
            for (int k = 0; k < fromTile.PathCostCollection.Count; k++)
            {
                if (fromTile.PathCostCollection[k].TileID == ElegibleBuildings[i].AssociatedNode.NodeID)
                {
                    if (_cost > fromTile.PathCostCollection[k].Cost)
                    {
                        _cost = fromTile.PathCostCollection[k].Cost;
                        _building = ElegibleBuildings[i];
                    }
                }
            }
        }

        return _building;
    }
}
