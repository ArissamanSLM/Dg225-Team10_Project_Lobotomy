using System.Collections.Generic;
using Microsoft.Xna.Framework;

public enum NodeType { Combat, Elite, Shop, Rest, Boss }

public class MapNode
{
	public Vector2 Position; // Where it sits on your 4000px map
	public NodeType Type;
	public List<MapNode> ConnectedNodes = new List<MapNode>(); // The "branches" leading forward
	public bool IsVisited = false;
	public bool IsAvailable = false; // Can the player click this next?
}