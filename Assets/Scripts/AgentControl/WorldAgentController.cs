using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldAgentController : MonoBehaviour
{
    public GameObject AgentPrefab;
    public Transform AgentAnchor;

    public int TicCutout;
    int currentTic = 0;

    public float InitialSpeed;
    public float SpeedStep;
    public int SpeedVariationQty;
    public List<float> SpeedList;

    public List<AnimationForPerk> AgentAnimationCollection;

    public float SneezeBaseFrequency = 10f;
    public GameObject SneezePrefab;

    public static WorldAgentController instance;

    public List<BuildingController> Buildings;
    List<BuildingController> houses;

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
    public float TotalPolicyUpkeepCost;
    public float TotalAgentIncome;

    public int initialInfectedDudes = 3;

    [SerializeField]
    List<Policy> ActivePolicies = new List<Policy>();

    DistributionParameterModification foodDistribution = new DistributionParameterModification(DistributionParameter.Food, 0);
    DistributionParameterModification enteDistribution = new DistributionParameterModification(DistributionParameter.Entertainment, 0);
    DistributionParameterModification educDistribution = new DistributionParameterModification(DistributionParameter.Education, 0);

    public List<PerkQuantityForScenario> ScenarioPercentagesForPerks;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        SpeedList = new List<float>();
        float _sp = InitialSpeed;
        for (int i = 0; i < SpeedVariationQty; i++)
        {
            SpeedList.Add(_sp);
            _sp += SpeedStep;
        }

        StartCoroutine(DelayedStart());
        currentTic = 0;
    }

    private void TicReceived()
    {
        currentTic++;
        if (currentTic > TicCutout)
        {
            currentTic = 0;
            AddHouseWarehouseSupplies();
        }
    }

    public GameObject GetAnimationForPerk(GlobalObject.AgentPerk _perk)
    {
        GameObject obj = null;

        for (int i = 0; i < AgentAnimationCollection.Count; i++)
        {
            if (AgentAnimationCollection[i].Perk == _perk)
            {
                obj = AgentAnimationCollection[i].AnimationPrefab;
                break;
            }
        }

        return obj;
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
    void PoblateHouses()
    {
        houses = Buildings.FindAll(x => x.BaseNeedCovered == GlobalObject.NeedScale.Sleep);
    }
    IEnumerator DelayedStart()
    {
        yield return null;
        WorldManager.TicDelegate += TicReceived;
        yield return null;
        PoblateRoadSystem();
        PoblateBuildings();
        PoblateHouses();
        List<BuildingController> auxHouses = Buildings.FindAll(x => x.BaseNeedCovered == GlobalObject.NeedScale.Sleep);
        for (int i = 0; i < auxHouses.Count; i++)
        {
            for (int a = 0; a < auxHouses[i].BaseAgentCapacity; a++)
            {
                GameObject obj = Instantiate(AgentPrefab, AgentAnchor);
                AgentController _agent = obj.GetComponent<AgentController>();

                obj.transform.position = auxHouses[i].transform.position;

                _agent.AgentID = a;
                _agent.myCurrentNode = auxHouses[i].AssociatedNode;
                _agent.currentNodeId = auxHouses[i].AssociatedNode.NodeID;

                int rnd = Random.Range(0, SpeedList.Count);

                _agent.Speed = SpeedList[rnd];

                _agent.myHouse = auxHouses[i];
                //_agent.InitAgent();

                AgentCollection.Add(_agent);
            }
        }

        //--- Calcula las cantidades de personas por tipo de Perk segun los porcentajes establecidos

        List<GlobalObject.AgentPerk> _perksToCreate = new List<GlobalObject.AgentPerk>();

        for (int i = 0; i < ScenarioPercentagesForPerks.Count; i++)
        {
            ScenarioPercentagesForPerks[i].Qty = Mathf.RoundToInt(AgentCollection.Count * ScenarioPercentagesForPerks[i].Percentage);

            for (int k = 0; k < ScenarioPercentagesForPerks[i].Qty; k++)
            {
                _perksToCreate.Add(ScenarioPercentagesForPerks[i].Perk);
            }

            Debug.LogWarning("Para Perk " + ScenarioPercentagesForPerks[i].Perk.ToString() + " tantos monitos " + ScenarioPercentagesForPerks[i].Qty);
        }

        Debug.LogError("TotalAgentes: " + AgentCollection.Count + " >>> TotalPerks " + _perksToCreate.Count);

        //--- Randomiza la lista de perks
        for (int i = 0; i < _perksToCreate.Count; i++)
        {
            GlobalObject.AgentPerk _tmpPerk = _perksToCreate[i];
            int rnd = Random.Range(i, _perksToCreate.Count);
            _perksToCreate[i] = _perksToCreate[rnd];
            _perksToCreate[rnd] = _tmpPerk;
        }


       // List<GlobalObject.AgentPerk> _perksToCreate = new List<GlobalObject.AgentPerk>();
       /*
        int cPerk = 0;
        int killswitch = 0;
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
            killswitch++;
            if (killswitch > 1000)
            {
                Debug.LogError("Got out via killswitch [WorldAgentController.DelayedStart()]");
                break;
            }
        }
        */
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

        InfectAgents(initialInfectedDudes);

        //AgentCollection[0].myStatus = GlobalObject.AgentStatus.Mild_Case;
        ////AgentCollection[0].myStatus = GlobalObject.AgentStatus.Serious_Case;
        //AgentCollection[0].SickIndicator.SetActive(true);
        CalculateBuildingUpkeepCost();
        CalculatePolicyUpkeepCost();
        CalculateAgentIncome();

        //SetHouseWarehouseSupplies(750f);
    }
    void SetHouseWarehouseSupplies(float qty)
    {
        foreach (BuildingController bc in houses)
        {
            bc.myWarehouse.StoreGoods(GlobalObject.NeedScale.Hunger,        qty);
            bc.myWarehouse.StoreGoods(GlobalObject.NeedScale.Education,     qty);
            bc.myWarehouse.StoreGoods(GlobalObject.NeedScale.Entertainment, qty);
        }
    }
    void AddHouseWarehouseSupplies()
    {
        foreach (BuildingController bc in houses)
        {
            bc.myWarehouse.StoreGoods(GlobalObject.NeedScale.Hunger, foodDistribution.Units);
            bc.myWarehouse.StoreGoods(GlobalObject.NeedScale.Education, educDistribution.Units);
            bc.myWarehouse.StoreGoods(GlobalObject.NeedScale.Entertainment, enteDistribution.Units);
        }
    }
    public void ReceivePolicies(List<Policy> policies)
    {
        ActivePolicies.Clear();
        ActivePolicies = policies;

        ResetDistributionPolicies();
        RefreshBuildingsPolicies();
        CalculatePolicyUpkeepCost();
    }
    public void RefreshBuildingsPolicies()
    {
        ClearAllBuildingsPolicies();
        SetNeedChangePolicies();
        SetBuildingPolicies();
        SetDistributionPolicies();
    }
    void ClearAllBuildingsPolicies()
    {
        foreach (BuildingController build in Buildings)
        {
            build.ResetMods();
        }
    }
    void ResetDistributionPolicies()
    {
        foodDistribution.ResetUnits();
        enteDistribution.ResetUnits();
        educDistribution.ResetUnits();
    }
    void SetNeedChangePolicies()
    {
        foreach (Policy pol in ActivePolicies)
        {
            TransformBuilding auxTB = pol.TransformBuildingSection;
            if (auxTB.Enabled)
            {
                float percentage = auxTB.Percentage;
                GlobalObject.NeedScale toChange = auxTB.BuildingToChange;
                GlobalObject.NeedScale newNeed = auxTB.TargetBuilding;
                List<BuildingController> toChangeBuildings = Buildings.FindAll(x => x.BaseNeedCovered == toChange && x.BaseNeedCovered == x.MainNeedCovered);

                float floatQtyToChange = (float)toChangeBuildings.Count * percentage;

                int buildingQuantityToChange = Mathf.RoundToInt(floatQtyToChange);
                int changedBuildings = 0;
                int killswitch = 0;
                while (changedBuildings < buildingQuantityToChange && toChangeBuildings.Count > 0)
                {
                    killswitch++;

                    int randomIndex = Random.Range(0, toChangeBuildings.Count - 1);
                    toChangeBuildings[randomIndex].SetTemporaryNewNeed(newNeed);
                    changedBuildings++;
                    toChangeBuildings.RemoveAt(randomIndex);

                    if (killswitch > 1000)
                    {
                        break;
                    }
                }
            }
        }
    }
    void SetBuildingPolicies()
    {
        foreach (Policy pol in ActivePolicies)
        {
            foreach (BuildingController build in Buildings)
            {
                BuildingEffectivity auxBE = pol.BuildingEffectivitySection;
                LimitBuilding auxLB = pol.LimitBuildingSection;

                if (auxBE.Enabled)
                {
                    for (int i = 0; i < auxBE.ParameterMods.Count; i++)
                    {
                        if (auxBE.ParameterMods[i].BuildingType == build.MainNeedCovered && auxBE.ParameterMods[i].BuildingType != GlobalObject.NeedScale.None)
                        {
                            build.AddModPercentageRestored(auxBE.ParameterMods[i].Percentage);
                            //Debug.Log("Set some property to a building!");
                        }
                    }
                }
                if (auxLB.Enabled)
                {
                    if (build.MainNeedCovered == auxLB.BuildingToChange)
                    {
                        build.AddModAgentCapacity(auxLB.Percentage);
                    }
                }
            }
        }
    }
    void SetDistributionPolicies()
    {
        foreach (Policy pol in ActivePolicies)
        {
            if (pol.DistributionEffectivitySection.Enabled)
            {
                foreach (DistributionParameterModification dpm in pol.DistributionEffectivitySection.ParameterMods)
                {
                    AddDistribution(dpm.Parameter, dpm.Units);
                }
            }
        }
    }
    void AddDistribution(DistributionParameter distPara, int units)
    {
        switch (distPara)
        {
            case DistributionParameter.Food:
                foodDistribution.Units += units;
                break;
            case DistributionParameter.Entertainment:
                enteDistribution.Units += units;
                break;
            case DistributionParameter.Education:
                educDistribution.Units += units;
                break;
        }
    }
    void InfectAgents(int limit)
    {
        int count = 0;
        int killswitch = 0;
        while (count < limit)
        {
            int randomIndex = Random.Range(0, AgentCollection.Count - 1);
            if (AgentCollection[randomIndex].myStatus == GlobalObject.AgentStatus.Healty)
            {
                AgentCollection[randomIndex].myStatus = GlobalObject.AgentStatus.Mild_Case;
                AgentCollection[randomIndex].StatusChanged();
                //AgentCollection[randomIndex].SickIndicator.SetActive(true);
                count++;
            }
            killswitch++;
            if (killswitch >= 1000)
            {
                Debug.LogError("Got out via killswitch [WorldAgentController.InfectAgents()]");
                break;
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
    public void CalculatePolicyUpkeepCost()
    {
        TotalPolicyUpkeepCost = 0;
        foreach (Policy pol in ActivePolicies)
        {
            TotalPolicyUpkeepCost += pol.UpkeepCost;
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

            if (_building.MainNeedCovered == forNeed && _building.CurrentAgentCount < _building.BaseAgentCapacity)
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

        if (ElegibleBuildings.Count == 0)
            return null;

        if (ElegibleBuildings.Count == 1)
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
