using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentMovement : MonoBehaviour
{
    private AgentController Agent;

    public int CurrentHeading { get; set; }
    public float Speed = 1f;
    public bool ShipIsMoving = false;
    public Vector3 movementVector;
    private Rigidbody myRigidBody;

    public int CurrentTileID; // { get; set; }
    public int ObjectiveNodeID;
    public float currentDistance;

    public PathFindingNode NextTile;

    private void Start()
    {
        Agent = gameObject.GetComponent<AgentController>();
        myRigidBody = GetComponent<Rigidbody>();
    }


    private List<int> PathToFollow = new List<int>();
    private int NextTileToFollow = 0;

    public void MoveAgent(int _objectiveNodeID)
    {
        StopCoroutine("MovementAction");

        CurrentTileID = Agent.myCurrentNode.NodeID;
        ObjectiveNodeID = _objectiveNodeID;

        NextTileToFollow = 0;
        PathToFollow = new List<int>();

        string _path = WorldManager.instance.GetCompletePath(_objectiveNodeID, Agent.myCurrentNode);
        string[] pathIdCollection = _path.Split(',');
        
        int tot = pathIdCollection.Length - 1;
        for (int i = tot; i >= 0; i--)
        {
            int id = 0;
            int.TryParse(pathIdCollection[i], out id);
            PathToFollow.Add(id);
        }

        if (PathToFollow.Count > 0)
        {            
            NextTile = WorldManager.instance.GetNodeFromID(PathToFollow[NextTileToFollow]);
        }
        else
        {
            return;
        }
        //NextTile = WorldManager.instance.GetNextTileInRoute(CurrentTileID, ObjectiveNodeID, Agent.myCurrentNode);

        movementVector = new Vector3(0, 0, 0);
        getRotation();

        StartCoroutine("MovementAction");
    }

    private float CurrentSpeed;
    private float currentSeconds;
    private float secondsToNode;

    IEnumerator MovementAction()
    {
        CurrentSpeed = Speed * WorldManager.instance.TicScale;

        secondsToNode = currentDistance / CurrentSpeed;
        currentSeconds = 0;

        Agent.AnimatorController.SetInteger("CurrentState", 1);

        while (true)
        {
            if (WorldManager.instance.TicScale == 0)
            {
                // es pausa
                myRigidBody.velocity = Vector3.zero;
                yield return new WaitForFixedUpdate();
            }
            else
            {
                if (currentSeconds >= secondsToNode)
                {
                    Vector3 _pos = transform.position;
                    _pos.x = NextTile.transform.position.x;
                    _pos.z = NextTile.transform.position.z;
                    transform.position = _pos;

                    if (NextTile.NodeID == ObjectiveNodeID)
                    {
                        //--- YA DEBE REGRESAR
                        Agent.myCurrentNode = NextTile;
                        myRigidBody.velocity = Vector3.zero;
                        Agent.AnimatorController.SetInteger("CurrentState", 0);
                        Agent.DestinyReached();
                        break;
                    }

                    CurrentTileID = NextTile.NodeID;
                    Agent.myCurrentNode = NextTile;

                    NextTileToFollow++;
                    NextTile = WorldManager.instance.GetNodeFromID(PathToFollow[NextTileToFollow]);
                    //NextTile = WorldManager.instance.GetNextTileInRoute(CurrentTileID, ObjectiveNodeID, Agent.myCurrentNode);

                    getRotation();

                    CurrentSpeed = Speed * WorldManager.instance.TicScale;
                    secondsToNode = currentDistance / CurrentSpeed;
                    currentSeconds = 0;
                }

                yield return new WaitForFixedUpdate();

                myRigidBody.velocity = (movementVector * CurrentSpeed);// * Time.fixedDeltaTime;
                currentSeconds += Time.fixedDeltaTime;
            }
        }
    }

    public void TicReceived()
    {
        if (WorldManager.instance.TicScale == 0)
            return;

        float _speed = Speed * WorldManager.instance.TicScale;

        if (CurrentSpeed != _speed)
        {
            CurrentSpeed = _speed;

            float percentage = currentSeconds / secondsToNode;
            secondsToNode = currentDistance / CurrentSpeed;
            currentSeconds = secondsToNode * percentage;
        }
    }

    public void StopMovement()
    {
        StopCoroutine("MovementAction");
        Agent.AnimatorController.SetInteger("CurrentState", 1);
        myRigidBody.velocity = Vector3.zero;
    }

    private void getRotation()
    {
        transform.LookAt(NextTile.transform);
        Vector3 _rotation = transform.eulerAngles;
        _rotation.x = 0;
        _rotation.z = 0;
        transform.eulerAngles = _rotation;

        currentDistance = Vector3.Distance(NextTile.transform.position, Agent.myCurrentNode.transform.position);
        movementVector = (NextTile.transform.position - Agent.myCurrentNode.transform.position).normalized;
    }
}
