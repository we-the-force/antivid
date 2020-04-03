using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum NodeType { Road, House, Hospital, };
//[Serializable]
public class Node
{
    int _nodeID;
    private List<Node> _connectedNodes = new List<Node>();
    NodeType _type;
    Vector2 _tilePosition;

    public int NodeID
    {
        get { return _nodeID; }
        set { _nodeID = value; }
    }
    public List<Node> ConnectedNodes
    {
        get { return _connectedNodes; }
    }
    public NodeType Type
    {
        get { return _type; }
        set { _type = value; }
    }
    public Vector2 TilePosition
    {
        get { return _tilePosition; }
        set { _tilePosition = value; }
    }
}