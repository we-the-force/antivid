using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class BuildingController : MonoBehaviour
{
    public Transform ocupantAnchor;
    public GameObject prefab;
    public List<GameObject> ocupantIconList = new List<GameObject>();

    public GlobalObject.NeedScale BaseNeedCovered;
    public float TimeToCoverNeed;

    [FormerlySerializedAs("TicsToCoverNeed")]
    public int BaseTicsToCoverNeed;
    [FormerlySerializedAs("PercentageRestored")]
    public float BasePercentageRestored;

    [SerializeField]
    float ModTicsToCoverNeed = 1;
    [SerializeField]
    float ModPercentageRestored = 1;
    [SerializeField]
    float ModAgentCapacity = 1;

    [FormerlySerializedAs("AgentCapacity")]
    public int BaseAgentCapacity;
    public int CurrentAgentCount;
    public PathFindingNode AssociatedNode;

    public float UpkeepCost;

    public Warehouse myWarehouse;
    public BuildingRandomizer myBuildingRandomizer;

    [SerializeField]
    GlobalObject.NeedScale _currentNeedCovered;
    [SerializeField]
    int _ticsToCoverNeed;
    [SerializeField]
    float _percentageRecovered;
    [SerializeField]
    int _agentCapacity;

    public int TicsToCoverNeed
    {
        get { return _ticsToCoverNeed; }
    }
    public float PercentageRestored
    {
        get { return _percentageRecovered; }
    }
    public int AgentCapacity
    {
        get { return _agentCapacity; }
    }
    public GlobalObject.NeedScale MainNeedCovered
    {
        get { return _currentNeedCovered; }
    }

    public void ResetOccupants()
    {
        int diff = Mathf.Abs(CurrentAgentCount - ocupantIconList.Count);

        if (CurrentAgentCount > ocupantIconList.Count)
        {
            //--- Agregar ocupantes
            for (int i = 0; i < diff; i++)
            {
                GameObject obj = Instantiate(prefab, ocupantAnchor);
                obj.transform.localPosition = new Vector3(0, ocupantIconList.Count * 0.5f, 0);

                ocupantIconList.Insert(0, obj);
            }

        }
        else if (CurrentAgentCount < ocupantIconList.Count)
        {
            //--- Eliminar Ocupantes
            for (int i = 0; i < diff; i++)
            {
                GameObject obj = ocupantIconList[0];
                ocupantIconList.RemoveAt(0);
                Destroy(obj);
            }
        }
    }



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
        ResetMods();

        ResetMainNeed();
        AddModTicsToCoverNeed(1);
        AddModPercentageRestored(1);
        AddModAgentCapacity(1);
        //StartCoroutine(DelayedStart());
    }

    public void NewBuilding()
    {
        Start();
    }

    IEnumerator DelayedStart()
    {
        yield return null;
        //yield return null;
        AssociatedNode = GetComponent<PathFindingNode>().ConnectedNodes[0];
    }
    public void ResetMods()
    {
        ModTicsToCoverNeed = 1;
        ModPercentageRestored = 1;
        ModAgentCapacity = 1;
        ResetMainNeed();
        RecalcValues();
    }

    public void RecalcValues()
    {
        _ticsToCoverNeed = (int)(BaseTicsToCoverNeed * ModTicsToCoverNeed);
        _percentageRecovered = BasePercentageRestored * ModPercentageRestored;
        _agentCapacity = (int)(BaseAgentCapacity * ModAgentCapacity);
    }
    public void AddModTicsToCoverNeed(float amount)
    {
        ModTicsToCoverNeed += (amount - 1);
        _ticsToCoverNeed = (int)(BaseTicsToCoverNeed * ModTicsToCoverNeed);
    }
    public void AddModPercentageRestored(float amount)
    {
        ModPercentageRestored += (amount - 1);
        _percentageRecovered = BasePercentageRestored * ModPercentageRestored;
    }
    public void AddModAgentCapacity(float amount)
    {
        ModAgentCapacity += (amount - 1);
        _agentCapacity = (int)(BaseAgentCapacity * ModAgentCapacity); 
    }
    public void SetTemporaryNewNeed(GlobalObject.NeedScale newNeed)
    {
        _currentNeedCovered = newNeed;
    }
    public void ResetMainNeed()
    {
        _currentNeedCovered = BaseNeedCovered;
    }
}
