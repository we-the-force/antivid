using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldAgentController : MonoBehaviour
{
    public GameObject AgentPrefab;
    public Transform AgentAnchor;

    public static WorldAgentController instance;

    public List<BuildingController> Buildings;
    public List<PathFindingNode> RoadSystem;

    public List<AgentController> AgentCollection;


    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        for (int i = 0; i < Buildings.Count; i++)
        {
            if (Buildings[i].MainNeedCovered == GlobalObject.NeedScale.Sleep)
            {
                for (int a = 0; a < Buildings[i].AgentCapacity; a++)
                {
                    GameObject obj = Instantiate(AgentPrefab, AgentAnchor);
                    AgentController _agent = obj.GetComponent<AgentController>();

                    obj.transform.position = Buildings[i].transform.position;

                    _agent.AgentID = a;
                    _agent.myCurrentNode = Buildings[i].AssociatedNode;
                    _agent.currentNodeId = Buildings[i].AssociatedNode.NodeID;

                    _agent.Speed = Random.Range(3f, 4f);

                    _agent.myHouse = Buildings[i];
                    _agent.InitAgent();                    

                    AgentCollection.Add(_agent);
                }
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

    public BuildingController GetBuilding(GlobalObject.NeedScale forNeed)
    {
        for (int i = 0; i < Buildings.Count; i++)
        {
            BuildingController _building = Buildings[i];

            if (_building.MainNeedCovered == forNeed && _building.CurrentAgentCount < _building.AgentCapacity)
            {
                return _building;
            }
        }

        return null;
    }


}
