using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public enum NodeType { Road, House, HealthCare, Food, Workplace, Education, Entertainment, Buyable, None };
public class PathFindingNode : MonoBehaviour
{
    public int NodeID;

    public List<PathFindingNode> ConnectedNodes = new List<PathFindingNode>();
    public List<int> HeadingToConnectedNode;

    public List<PathFindingNodeConnection> NodeConnection;// { get; set; }

    public List<string> ShortestPathCollection = new List<string>();

    public List<PathCost> PathCostCollection = new List<PathCost>();   


    [SerializeField, Range(0.01f, 0.05f), Tooltip("The length of the ray to be casted")]
    float rayLength = 0.01f;
    [SerializeField, Range(-0.001f, 0.001f), Tooltip("The distance to offset the start position of the raycasts (to avoid hitting the piece that sent it)")]
    float rayOffset = 0.001f;
    BoxCollider boxColl;
    public NodeType nodeType;

    [SerializeField]
    List<Ray> rays = new List<Ray>();

    public bool Primed
    {
        get { return (boxColl != null && rays.Count == 4); }
    }

    private void Awake()
    {
        if (!Primed)
        {
            boxColl = GetComponent<BoxCollider>();

            if (boxColl == null)
            {
                gameObject.AddComponent<BoxCollider>();
                boxColl = GetComponent<BoxCollider>();
            }

            PrimeRays();
        }
    }
    private void Start()
    {
        SetupConnectedNodes();
    }
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
    void SetupConnectedNodes()
    {
        ConnectedNodes.Clear();

        for (int i = 0; i < rays.Count; i++)
        {
            Color col = Color.black;
            switch (i)
            {
                case 0:
                    col = Color.red;
                    break;
                case 1:
                    col = Color.yellow;
                    break;
                case 2:
                    col = Color.green;
                    break;
                case 3:
                    col = Color.blue;
                    break;
            }
            Debug.DrawRay(rays[i].origin, rays[i].direction * rayLength, col, 2f);
            if (Physics.Raycast(rays[i], out RaycastHit hit, rayLength, 1 << LayerMask.NameToLayer("PathfindingNode")))
            {
                //Debug.Log($"Hit something! ({hit.transform.name})");
                PathFindingNode auxComp = hit.transform.GetComponent<PathFindingNode>();
                if (auxComp != null)
                {
                    ConnectedNodes.Add(auxComp);
                }
            }
            else
            {
                //Debug.Log("Hit nothing unu");
            }
        }
    }
    void PrimeRays()
    {
        rays.Clear();
        rays.Add(new Ray(new Vector3(boxColl.bounds.center.x, boxColl.bounds.center.y, boxColl.bounds.max.z + rayOffset), Vector3.forward));    //up
        rays.Add(new Ray(new Vector3(boxColl.bounds.max.x + rayOffset, boxColl.bounds.center.y, boxColl.bounds.center.z), Vector3.right));      //right
        rays.Add(new Ray(new Vector3(boxColl.bounds.center.x, boxColl.bounds.center.y, boxColl.bounds.min.z - rayOffset), Vector3.back));       //down
        rays.Add(new Ray(new Vector3(boxColl.bounds.min.x - rayOffset, boxColl.bounds.center.y, boxColl.bounds.center.z), Vector3.left));       //left
    }
    void PrimeGameObject()
    {
        boxColl = GetComponent<BoxCollider>();
        PrimeRays();
    }
    public void SetupNode()
    {
        if (!Primed)
        {
            PrimeGameObject();
        }
        SetupConnectedNodes();
        //InitConnections();
    }
    public void ForcePrime()
    {
        PrimeGameObject();
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(PathFindingNode), true)]
[CanEditMultipleObjects]
public class PathFindingNodeEditor : Editor
{
    private PathFindingNode nodeComp { get { return (target as PathFindingNode); } }
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        EditorGUILayout.Space();
        if (GUILayout.Button("Do the thing with the laser"))
        {
            nodeComp.SetupNode();
        }
        if (GUILayout.Button("Force Prime"))
        {
            nodeComp.ForcePrime();
        }
    }
}
#endif