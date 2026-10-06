using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace DreamSlayerV2.Core
{
    // Restricting strictly to your implemented node types
    public enum NodeType 
    { 
        Start,      // Starting Node
        Encounter,  // Standard Combat (Combat Ops)
        Elite,      // Hard Combat (Emergency Ops)
        Shop,       // Rogue Trader
        Rest,       // Safe House / Rest (Not fully implemented)
        Boss        // Dreadful Foe / Final Stage
    }

    public class MapNode
    {
        public string Name { get; set; } = "Node";
        public Vector2 Position;
        public NodeType Type;
        public List<MapNode> ConnectedNodes = new List<MapNode>();
        public bool IsVisited = false;
        public bool IsAvailable = false;

        // Arknights-style UI labels for active node types
        public string GetArknightsLabel() => Type switch
        {
            NodeType.Start => "START",
            NodeType.Encounter => "Combat Ops",
            NodeType.Elite => "Emergency Ops",
            NodeType.Shop => "Rogue Trader",
            NodeType.Rest => "Safe House",
            NodeType.Boss => "Dreadful Foe",
            _ => "Unknown"
        };

        // Helper to convert NodeType to RoomType for RoomManager compatibility
        public RoomType ToRoomType() => Type switch
        {
            NodeType.Elite => RoomType.Elite,
            NodeType.Shop => RoomType.Shop,
            NodeType.Rest => RoomType.Event, // Fallback placeholder since Rest isn't fully implemented
            NodeType.Boss => RoomType.Boss,
            _ => RoomType.Encounter
        };
    }

    public class MapGenerator
    {
        private static readonly Random Rng = new Random();

        public static List<MapNode> GenerateLovecraftianMap(int stages = 13, int startY = 540)
        {
            List<MapNode> generatedNodes = new List<MapNode>();
            List<List<MapNode>> mapColumns = new List<List<MapNode>>();

            // 1. STAGE 1: Start Node
            MapNode startNode = new MapNode
            {
                Name = "1-Start",
                Position = new Vector2(300, startY),
                Type = NodeType.Start,
                IsAvailable = true
            };
            generatedNodes.Add(startNode);
            mapColumns.Add(new List<MapNode> { startNode });

            int currentX = 650;
            int xSpacing = 350;

            // 2. STAGES 2 TO 12
            for (int stage = 2; stage <= 12; stage++)
            {
                List<MapNode> currentColumn = new List<MapNode>();

                if (stage == 6)
                {
                    // Mid-run Shop
                    MapNode shopNode = new MapNode 
                    { 
                        Name = $"{stage}-Shop", 
                        Position = new Vector2(currentX, startY), 
                        Type = NodeType.Shop 
                    };
                    currentColumn.Add(shopNode);
                }
                else if (stage == 7)
                {
                    // Mid-run Elite encounter
                    MapNode eliteNode = new MapNode 
                    { 
                        Name = $"{stage}-Elite", 
                        Position = new Vector2(currentX, startY), 
                        Type = NodeType.Elite 
                    };
                    currentColumn.Add(eliteNode);
                }
                else if (stage == 12)
                {
                    // Pre-Boss Shop
                    MapNode shopNode2 = new MapNode 
                    { 
                        Name = $"{stage}-Shop", 
                        Position = new Vector2(currentX, startY), 
                        Type = NodeType.Shop 
                    };
                    currentColumn.Add(shopNode2);
                }
                else
                {
                    int laneCount = Rng.Next(2, 4); // 2 or 3 branches
                    int[] possibleYPositions = new int[] { 250, 400, 540, 680, 830 };
                    List<int> chosenYs = GetRandomSample(possibleYPositions, laneCount);
                    chosenYs.Sort();

                    for (int i = 0; i < chosenYs.Count; i++)
                    {
                        NodeType assignedType = RollNodeType();

                        MapNode node = new MapNode
                        {
                            Name = $"{stage}-{assignedType}",
                            Position = new Vector2(currentX, chosenYs[i]),
                            Type = assignedType
                        };
                        currentColumn.Add(node);
                    }
                }

                generatedNodes.AddRange(currentColumn);
                mapColumns.Add(currentColumn);

                List<MapNode> previousColumn = mapColumns[stage - 2];
                ConnectArknightsLanes(previousColumn, currentColumn);

                currentX += xSpacing;
            }

            // 3. STAGE 13: Boss Node
            MapNode bossNode = new MapNode
            {
                Name = "13-Boss",
                Position = new Vector2(currentX, startY),
                Type = NodeType.Boss
            };
            generatedNodes.Add(bossNode);

            foreach (var node in mapColumns[mapColumns.Count - 1])
            {
                node.ConnectedNodes.Add(bossNode);
            }

            return generatedNodes;
        }

        private static void ConnectArknightsLanes(List<MapNode> prevCol, List<MapNode> currCol)
        {
            if (prevCol.Count == 1)
            {
                foreach (var curr in currCol)
                    prevCol[0].ConnectedNodes.Add(curr);
            }
            else if (currCol.Count == 1)
            {
                foreach (var prev in prevCol)
                    prev.ConnectedNodes.Add(currCol[0]);
            }
            else
            {
                for (int i = 0; i < prevCol.Count; i++)
                {
                    int targetIndex = (i * currCol.Count) / prevCol.Count;
                    prevCol[i].ConnectedNodes.Add(currCol[targetIndex]);

                    if (targetIndex + 1 < currCol.Count && Rng.Next(0, 2) == 0)
                    {
                        prevCol[i].ConnectedNodes.Add(currCol[targetIndex + 1]);
                    }
                }

                foreach (var currNode in currCol)
                {
                    bool isReachable = prevCol.Exists(p => p.ConnectedNodes.Contains(currNode));
                    if (!isReachable)
                    {
                        prevCol[Rng.Next(prevCol.Count)].ConnectedNodes.Add(currNode);
                    }
                }
            }
        }

        private static NodeType RollNodeType()
        {
            int roll = Rng.Next(0, 100);
            if (roll < 75) return NodeType.Encounter; // 80% Standard Combat
            if (roll < 95) return NodeType.Elite;     // 10% Elite Combat
            return NodeType.Rest;                     // 10% Rest Area
        }

        private static List<int> GetRandomSample(int[] array, int count)
        {
            List<int> list = new List<int>(array);
            List<int> result = new List<int>();
            for (int i = 0; i < count && list.Count > 0; i++)
            {
                int index = Rng.Next(list.Count);
                result.Add(list[index]);
                list.RemoveAt(index);
            }
            return result;
        }
    }
}