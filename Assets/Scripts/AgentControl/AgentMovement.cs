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
    
    public void MoveAgent(int _objectiveNodeID)
    {
        CurrentTileID = Agent.myCurrentNode.NodeID;
        ObjectiveNodeID = _objectiveNodeID;

        NextTile = WorldManager.instance.GetNextTileInRoute(CurrentTileID, ObjectiveNodeID);

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

        while (true)
        {
            if (currentSeconds >= secondsToNode)
            {
                if (NextTile.NodeID == ObjectiveNodeID)
                {
                    //--- YA DEBE REGRESAR
                    Agent.myCurrentNode = NextTile;
                    myRigidBody.velocity = Vector3.zero;
                    Agent.DestinyReached();
                    break;
                }

                Vector3 _pos = transform.position;
                _pos.x = NextTile.transform.position.x;
                _pos.z = NextTile.transform.position.z;
                transform.position = _pos;
                
                CurrentTileID = NextTile.NodeID;
                Agent.myCurrentNode = NextTile;
                NextTile = WorldManager.instance.GetNextTileInRoute(CurrentTileID, ObjectiveNodeID);

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

    public void TicReceived()
    {
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
