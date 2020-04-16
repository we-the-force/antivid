#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Tilemaps;

[CreateAssetMenu(fileName = "Prefab Brush", menuName = "Brushes/Prefab Brush")]
[CustomGridBrush(false, true, false, "Prefab Brush")]
public class PrefabTileBrush : GridBrush
{
    //public DoorOrientation orientation;
    public override void Paint(GridLayout gridLayout, GameObject brushTarget, Vector3Int position)
    {
        base.Paint(gridLayout, brushTarget, position);
    }
    public override void Erase(GridLayout gridLayout, GameObject brushTarget, Vector3Int position)
    {
        base.Erase(gridLayout, brushTarget, position);
    }
    private static Transform GetObjectInCell(GridLayout grid, Transform parent, Vector3Int position)
    {
        int childCount = parent.childCount;
        Vector3 min = grid.LocalToWorld(grid.CellToLocalInterpolated(position));
        Vector3 max = grid.LocalToWorld(grid.CellToLocalInterpolated(position + Vector3Int.one));
        Bounds bounds = new Bounds((max + min) * .5f, max - min);

        for (int i = 0; i < childCount; i++)
        {
            Transform child = parent.GetChild(i);
            if (bounds.Contains(child.position))
                return child;
        }
        return null;
    }

    private static float GetPerlinValue(Vector3Int position, float scale, float offset)
    {
        return Mathf.PerlinNoise((position.x + offset) * scale, (position.y + offset) * scale);
    }
}
#endif