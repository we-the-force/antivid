using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathFindingNode : MonoBehaviour
{
    public int NodeID;

    public List<PathFindingNode> ConnectedNodes;
    public List<int> HeadingToConnectedNode;

    public List<PathFindingNodeConnection> NodeConnection;// { get; set; }

    public List<string> ShortestPathCollection = new List<string>();

    public void InitConnections()
    {
        NodeConnection = new List<PathFindingNodeConnection>();
        HeadingToConnectedNode = new List<int>();

        Vector3 pos = transform.position;

        for (int i = 0; i < ConnectedNodes.Count; i++)
        {
            int _heading = 0;

            //--- Obtiene el numero que representa la direccion hacia la que debe ir hacia el nodo
            // 0 > FULLSTOP. 1 > XAxis+1. 2 > XAxis-1. 3 > ZAxis+1. 4 > ZAxis-1. 
            if (pos.x == ConnectedNodes[i].transform.position.x)
            {
                _heading = 3;
                if (pos.z > ConnectedNodes[i].transform.position.z)
                    _heading = 4;
            }
            else if (pos.z == ConnectedNodes[i].transform.position.z)
            {
                _heading = 1;
                if (pos.x > ConnectedNodes[i].transform.position.x)
                    _heading = 2;
            }
            HeadingToConnectedNode.Add(_heading);

            //--- Configura la conexion a cada nodo
            PathFindingNodeConnection conx = new PathFindingNodeConnection();
            conx.ConnectedNode = ConnectedNodes[i];
            conx.Cost = Mathf.RoundToInt(Vector3.Distance(ConnectedNodes[i].transform.position, transform.position));

            NodeConnection.Add(conx);
        }
    }

}
