using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingController : MonoBehaviour
{
    public GlobalObject.NeedScale MainNeedCovered;
    public float TimeToCoverNeed;

    public int TicsToCoverNeed;

    public int PercentageRestored;

    public int AgentCapacity;
    public int CurrentAgentCount;
    public PathFindingNode AssociatedNode;

    public float UpkeepCost;

    public Warehouse myWarehouse;
    public BuildingRandomizer myBuildingRandomizer;

    private void Start()
    {
        List<PathFindingNode> auxList = GetComponent<PathFindingNode>().ConnectedNodes;
        if (auxList.Count > 0)
        {
            AssociatedNode = GetComponent<PathFindingNode>().ConnectedNodes[0];
        }
        
        myWarehouse = gameObject.GetComponent<Warehouse>();
        if (myWarehouse == null)
        {
            myWarehouse = gameObject.AddComponent<Warehouse>();
        }

        myBuildingRandomizer = gameObject.GetComponent<BuildingRandomizer>();

        if (myBuildingRandomizer != null)
        {
            myBuildingRandomizer.InitBuilding();
        }
        //StartCoroutine(DelayedStart());
    }

    IEnumerator DelayedStart()
    {
        yield return null;
        //yield return null;
        AssociatedNode = GetComponent<PathFindingNode>().ConnectedNodes[0];
    }
}
