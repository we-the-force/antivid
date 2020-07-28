using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class NodeComponent : MonoBehaviour
{
    [SerializeField, Range(0.01f, 0.05f), Tooltip("The length of the ray to be casted")]
    float rayLength = 0.01f;
    [SerializeField, Range(-0.001f, 0.001f), Tooltip("The distance to offset the start position of the raycasts (to avoid hitting the piece that sent it)")]
    float rayOffset = 0.001f;
    BoxCollider boxColl;
    public NodeType nodeType;

    [SerializeField]
    List<Ray> rays = new List<Ray>();

    [SerializeField]
    List<NodeComponent> adjacentNodes = new List<NodeComponent>();

    public bool Primed
    {
        get { return (boxColl != null && rays.Count == 4); }
    }
    private void Awake()
    {
        if (!Primed)
        {
            boxColl = GetComponent<BoxCollider>();
            PrimeRays();
        }
    }
    private void Start()
    {
        SetupAdjacentNodes();
    }
    void SetupAdjacentNodes()
    {
        adjacentNodes.Clear();
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
                Debug.Log($"Hit something! ({hit.transform.name})");
                NodeComponent auxComp = hit.transform.GetComponent<NodeComponent>();
                if (auxComp != null)
                {
                    adjacentNodes.Add(auxComp);
                }
            }
            else
            {
                Debug.Log("Hit nothing unu");
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
        SetupAdjacentNodes();
    }
    public void ForcePrime()
    {
        PrimeGameObject();
    }
}
#if UNITY_EDITOR
[CustomEditor(typeof(NodeComponent), true)]
[CanEditMultipleObjects]
public class NodeComponentEditor : Editor
{
    private NodeComponent nodeComp { get { return (target as NodeComponent); } }
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
