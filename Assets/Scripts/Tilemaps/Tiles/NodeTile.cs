using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;
#if UNITY_EDITOR
using UnityEditor;
#endif

[Serializable]
[CreateAssetMenu(fileName = "New Prefab Tile", menuName = "Tiles/Prefab Tile")]
public class NodeTile : TileBase
{
    [SerializeField]
    public Sprite m_Preview;
    [SerializeField]
    public Color m_spriteColor;
    [SerializeField]
    public GameObject[] m_Prefabs;
    [SerializeField]
    public Vector3 m_positionOffset;
    [SerializeField]
    public NodeType m_nodeType;



    public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
    {
        if (m_Preview)
        {
            tileData.sprite = m_Preview;
            tileData.color = m_spriteColor;
            if (m_Prefabs.Length > 0)
            {
                tileData.gameObject = m_Prefabs[Random.Range(0, m_Prefabs.Length)];
                tileData.gameObject.SetActive(false);
            }
        }
        //base.GetTileData(position, tilemap, ref tileData);
    }

    public override bool StartUp(Vector3Int position, ITilemap tilemap, GameObject go)
    {
        if (PrefabExists(0) && go != null && Application.isPlaying)
        {
            var aux = tilemap.GetTransformMatrix(position).rotation.eulerAngles;

            if (m_nodeType == NodeType.None)
            {
                go.transform.parent = GameObject.Find("GrassCollection").transform;
            }
            else
            {
                go.transform.parent = GameObject.Find("NodeCollection").transform;
            }

            //go.transform.parent = GameObject.Find("Objects").transform.Find(m_nodeType.ToString());
            go.transform.localPosition += m_positionOffset;
            go.transform.localRotation = Quaternion.Euler(0, -aux.z, 0);
            go.SetActive(true);
        }
        return base.StartUp(position, tilemap, go);
    }
    public void OnDestroy()
    {
        Debug.Log($"Ayyy got nuked ({m_Preview.texture.name})");
    }
    public bool HasNodeOnLocation(Tilemap tilemap, Vector3Int position)
    {
        return tilemap.GetTile(position) == this;
    }
    public override void RefreshTile(Vector3Int position, ITilemap tilemap)
    {
        Vector3Int aux = new Vector3Int(position.x, position.y, position.z);
        base.RefreshTile(aux, tilemap);
        //Debug.Log($"Currently refreshing: {aux.ToString()}, rot: {tilemap.GetTransformMatrix(position).rotation.eulerAngles.ToString("F2")}");
    }
    bool HasPrefabs()
    {
        return m_Prefabs.Length > 0;
    }
    bool PrefabExists(int index)
    {
        if (m_Prefabs.Length >= index)
        {
            return m_Prefabs[index] != null;
        }
        return false;
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(NodeTile), true)]
[CanEditMultipleObjects]
public class RoadNodeTileEditor : Editor
{
    private NodeTile nodeTile { get { return (target as NodeTile); } }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        EditorGUI.BeginChangeCheck();
        if (EditorGUI.EndChangeCheck())
            EditorUtility.SetDirty(nodeTile);
        //if (GUILayout.Button("Refresh"))
        //{
        //    nodeTile.();
        //}
    }
}
#endif
