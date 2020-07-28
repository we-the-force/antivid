using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBehaviour : MonoBehaviour
{
    public int ShipID { get; set; }

    public int LifePoints = 10;

    /// <summary>
    /// 0 > FULLSTOP. 1 > XAxis+1. 2 > XAxis-1. 3 > ZAxis+1. 4 > ZAxis-1. 
    /// </summary>
    /// 
    public int ShipCurrentHeading { get; set; }
    public float Speed = 1f;
    public bool ShipIsMoving = false;
    private Vector3 movementVector;
    private Rigidbody myRigidBody;

    public int CurrentTileID; // { get; set; }
    public int ObjectiveNodeID;

    public PathFindingNode NextTile;
    private PathFindingNode CurrentTile;

    public void InitShip(int _heading)
    {
        myRigidBody = GetComponent<Rigidbody>();
        ShipCurrentHeading = _heading;

        movementVector = new Vector3(0, 0, 0);
        switch (ShipCurrentHeading)
        {
            case 1:
                movementVector.x = 1;
                break;
            case 2:
                movementVector.x = -1;
                break;
            case 3:
                movementVector.z = 1;
                break;
            case 4:
                movementVector.z = -1;
                break;
        }


       // Debug.LogError(" >>> Haciendo heading inicial = " + _heading);
        transform.eulerAngles = getRotation();
      //  Debug.LogError(" >>> ROTACION INCIAL : " + transform.eulerAngles.ToString());

        NextTile = WorldManager.instance.GetNextTileInRoute(CurrentTileID, ObjectiveNodeID);

      //  Debug.LogError(" >>> NEXT TILE ID " + NextTile.NodeID);

        StartCoroutine("MovementAction");
    }

    public void DoDamage(int _damage, out bool isDead)
    {
        isDead = false;
        LifePoints -= _damage;

        if (LifePoints <= 0)
        {
            isDead = true;
            RemoveUnit();
        }
    }

    IEnumerator MovementAction()
    {
        bool switchNode = false;
        Vector3 myPos;

        while (true)
        {
            //--- Validar si ya estoy localizado en el area cercana al siguiente nodo en el camino
            myPos = transform.position;

            /// 0 > FULLSTOP. 1 > XAxis+1. 2 > XAxis-1. 3 > ZAxis+1. 4 > ZAxis-1. 
            switch (ShipCurrentHeading)
            {
                case 1:
                    if (myPos.x >= NextTile.transform.position.x)
                        switchNode = true;
                    break;

                case 2:
                    if (myPos.x <= NextTile.transform.position.x)
                        switchNode = true;
                    break;

                case 3:
                    if (myPos.z >= NextTile.transform.position.z)
                        switchNode = true;
                    break;

                case 4:
                    if (myPos.z <= NextTile.transform.position.z)
                        switchNode = true;
                    break;
            }

            if (switchNode)
            {
                if (NextTile.NodeID == ObjectiveNodeID)
                {
                    //--- YA SE TIENE QUE SALIR; LLEGO A LA META
                    RemoveUnit();
                    break;
                }
                               
                switchNode = false;
                CurrentTileID = NextTile.NodeID;
                CurrentTile = NextTile;
                NextTile = WorldManager.instance.GetNextTileInRoute(CurrentTileID, ObjectiveNodeID);

                for (int i = 0; i < CurrentTile.ConnectedNodes.Count; i++)
                {
                    if (CurrentTile.ConnectedNodes[i].NodeID == NextTile.NodeID)
                    {
                        ShipCurrentHeading = CurrentTile.HeadingToConnectedNode[i];
                    }
                }

              //  Debug.LogError(" > > El siguiente nodo ahora es : " + NextTile.NodeID);
              //  Debug.LogError(" >>> Cambiando ahora el Heading es : " + ShipCurrentHeading);

                transform.eulerAngles = getRotation();
                movementVector = Vector3.zero;

                switch (ShipCurrentHeading)
                {
                    case 1:
                        movementVector.x = 1;
                        break;
                    case 2:
                        movementVector.x = -1;
                        break;
                    case 3:
                        movementVector.z = 1;
                        break;
                    case 4:
                        movementVector.z = -1;
                        break;
                }

              //  Debug.LogError(" >> Vector de movimiento : " + movementVector.ToString());
            }


            yield return new WaitForFixedUpdate();
            myRigidBody.velocity = (movementVector * Speed) * Time.fixedDeltaTime;
        }
    }

    private void RemoveUnit()
    {
        StopAllCoroutines();
        Destroy(gameObject);
    }


    private Vector3 getRotation()
    {
        Vector3 endingRotation = transform.rotation.eulerAngles;
        switch (ShipCurrentHeading)
        {
            case 1:
                endingRotation.y = 90;
                break;
            case 2:
                endingRotation.y = 270;
                break;
            case 3:
                endingRotation.y = 0;
                break;
            case 4:
                endingRotation.y = 180;
                break;
        }
        return endingRotation;
    }

    public void MoveShip(int _heading = -1)
    {
        if (ShipIsMoving)
            return;

        if (_heading == -1)
            _heading = ShipCurrentHeading;
        
        if (ShipCurrentHeading != _heading)
        {
            ShipCurrentHeading = _heading;
            StartCoroutine("rotateShip");
            return;
        }

        ShipCurrentHeading = _heading;
        if (ShipCurrentHeading != 0)
        {
            ShipIsMoving = true;
            StartCoroutine("performingMovement");
        }
    }

    IEnumerator rotateShip()
    {
        //-- Corresponding rotation angles for ShipCurrentHeadong
        //-- ShipCurrentHeading = 1 >>  Y_Angle = 90 
        //-- ShipCurrentHeading = 2 >>  Y_Angle = 270
        //-- ShipCurrentHeading = 3 >>  Y_Angle = 0 
        //-- ShipCurrentHeading = 4 >>  Y_Angle = 180

        float currentTime = 0;
        Vector3 startingRotation = transform.rotation.eulerAngles;
        Vector3 endingRotation = getRotation();
        Vector3 currentRotation = transform.rotation.eulerAngles;

        float _operator = 1;
        if (endingRotation.y - startingRotation.y < 0)
            _operator = -1;

        float _difference = Mathf.Abs(endingRotation.y - startingRotation.y);

        float totalTime = 1.0f; //  WorldManager.instance.TurnDuration;
        float _percent = Time.fixedDeltaTime / totalTime;

        while (currentTime < totalTime)
        {
            yield return new WaitForFixedUpdate();
            currentTime += Time.fixedDeltaTime;

            currentRotation.y += ((_difference * _percent) * _operator);
            transform.eulerAngles = currentRotation;
        }
        transform.eulerAngles = endingRotation;

        ShipIsMoving = false;
    }

    IEnumerator performingMovement()
    {
        float currentTime = 0;
        Vector3 startPos = transform.position;
        Vector3 endPos = transform.position;
        Vector3 currentPos = transform.position;
        switch (ShipCurrentHeading)
        {
            case 1:
                endPos.x += Speed;
                break;
            case 2:
                endPos.x -= Speed;
                break;
            case 3:
                endPos.z += Speed;
                break;
            case 4:
                endPos.z -= Speed;
                break;
        }

        float totalTime = 1.0f; // WorldManager.instance.TurnDuration;
        float _percent = Time.fixedDeltaTime / totalTime;

        while (currentTime < totalTime)
        {
            yield return new WaitForFixedUpdate();
            currentTime += Time.fixedDeltaTime;
                        
            //Debug.LogError(_percent);

            switch (ShipCurrentHeading)
            {
                case 1:
                    currentPos.x += (Speed * _percent);
                    break;
                case 2:
                    currentPos.x -= (Speed * _percent);
                    break;
                case 3:
                    currentPos.z += (Speed * _percent);

                    // Debug.LogError("pos Z " + currentPos.z);
                    break;
                case 4:
                    currentPos.z -= (Speed * _percent);
                    break;
            }

            transform.position = currentPos;
        }
        transform.position = endPos;

        ShipIsMoving = false;
    }




}
