﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingController : MonoBehaviour
{
    public GlobalObject.NeedScale MainNeedCovered;
    public float TimeToCoverNeed;
    public int PercentageRestored;

    public int AgentCapacity;
    public int CurrentAgentCount;
    public PathFindingNode AssociatedNode;


    private void Start()
    {
        List<PathFindingNode> auxList = GetComponent<PathFindingNode>().ConnectedNodes;
        if (auxList.Count > 0)
        {
            AssociatedNode = GetComponent<PathFindingNode>().ConnectedNodes[0];
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