using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine.Tilemaps;

[Serializable]
[CreateAssetMenu(fileName = "New Node Tile", menuName = "Tiles/Node Tile")]
public class NodeTile : Tile
{
    [SerializeField]
    public Sprite[] m_Sprites;

    public override void RefreshTile(Vector3Int position, ITilemap tilemap)
    {
        base.RefreshTile(position, tilemap);

    }

    public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
    {
        //base.GetTileData(position, tilemap, ref tileData);
        //tileData.sprite = sprite;
        //tileData.color = Color.white;
        //var m = tileData.transform;
        //m.SetTRS(Vector3.zero, new Quaternion(), Vector3.one);
        //tileData.transform = m;
        //tileData.flags = TileFlags.LockTransform;
        //tileData.colliderType = ColliderType.None;
        base.GetTileData(position, tilemap, ref tileData);
        if ((m_Sprites != null) && (m_Sprites.Length > 0))
        {
            long hash = position.x;
            hash = (hash + 0xabcd1234) + (hash << 15);
            hash = (hash + 0x0987efab) ^ (hash >> 11);
            hash ^= position.y;
            hash = (hash + 0x46ac12fd) + (hash << 7);
            hash = (hash + 0xbe9730af) ^ (hash << 11);
            Random.InitState((int)hash);
            tileData.sprite = m_Sprites[(int)(m_Sprites.Length * Random.value)];
        }
    }

    bool HasNodeTile(ITilemap tilemap, Vector3Int position)
    {
        return tilemap.GetTile(position) == this;
    }

    int GetIndex(byte mask)
    {
        switch (mask)
        {
            case 0: return 0;
            case 12: return 1;
            case 8: return 2;
            case 14: return 3;
            case 15: return 4;
        }
        return -1;
    }

    Quaternion GetRotation(byte mask)
    {
        switch (mask)
        {
            case 8: return Quaternion.Euler(0, 0, -90f);
            case 14: return Quaternion.Euler(0, 0, -180f);
            case 13: return Quaternion.Euler(0, 0, -270f);
        }
        return Quaternion.Euler(0, 0, 0);
    }

#if UNITY_EDITOR
#endif
}

#if UNITY_EDITOR
[CustomEditor(typeof(NodeTile), true)]
[CanEditMultipleObjects]
public class NodeTileEditor : Editor
{
    private NodeTile tile { get { return (target as NodeTile); } }

    public override void OnInspectorGUI()
    {
        EditorGUI.BeginChangeCheck();
        int count = EditorGUILayout.DelayedIntField("Number of Sprites", tile.m_Sprites != null ? tile.m_Sprites.Length : 0);
        if (count < 0)
            count = 0;
        if (tile.m_Sprites == null || tile.m_Sprites.Length != count)
        {
            Array.Resize<Sprite>(ref tile.m_Sprites, count);
        }
        if (count == 0)
            return;




        EditorGUILayout.LabelField("Place random sprites");
        EditorGUILayout.Separator();

        for (int i = 0; i < count; i++)
        {
            tile.m_Sprites[i] = (Sprite)EditorGUILayout.ObjectField($"Sprite {i + 1}", tile.m_Sprites[i], typeof(Sprite), false, null);
        }



        //EditorGUILayout.DropdownButton();
        if (EditorGUI.EndChangeCheck())
            EditorUtility.SetDirty(tile);
    }
}
#endif