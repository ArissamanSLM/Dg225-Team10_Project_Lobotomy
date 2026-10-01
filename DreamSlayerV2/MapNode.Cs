using System.Collections.Generic;
using Microsoft.Xna.Framework;
using System;

namespace DreamSlayerV2.Core
{
    public enum NodeType { Start, Encounter, Event, Shop, Elite, Boss, Victory }

    public class MapNode
    {
        public string Name { get; set; } = "Node";
        public Vector2 Position; // Where it sits on your map
        public NodeType Type;
        public List<MapNode> ConnectedNodes = new List<MapNode>(); // The "branches" leading forward
        public bool IsVisited = false;
        public bool IsAvailable = false; // Can the player click this next?

        // Helper to convert NodeType to RoomType for RoomManager compatibility
        public RoomType ToRoomType() => Type switch
        {
            NodeType.Elite => RoomType.Elite,
            NodeType.Shop => RoomType.Shop,
            NodeType.Event => RoomType.Event,
            NodeType.Boss => RoomType.Boss,
            NodeType.Victory => RoomType.Boss, // Map to Victory / Game Complete state
            _ => RoomType.Encounter
        };
    }

    public class MapGenerator
    {
        private static readonly Random Rng = new Random();

        // Updated default stages to 14 (Stage 13 = Boss, Stage 14 = Exit / Victory)
        public static List<MapNode> GenerateLovecraftianMap(int stages = 14, int startY = 540)
        {
            List<MapNode> generatedNodes = new List<MapNode>();
            List<List<MapNode>> mapColumns = new List<List<MapNode>>();

            // 1. STAGE 1: Start Node
            MapNode startNode = new MapNode
            {
                Name = "1-Start",
                Position = new Vector2(300, startY),
                Type = NodeType.Start,
                IsAvailable = true // Player starts here!
            };
            generatedNodes.Add(startNode);
            mapColumns.Add(new List<MapNode> { startNode });

            int currentX = 650;
            int xSpacing = 350;

            // 2. STAGES 2 TO 12 (Random & Fixed Nodes)
            for (int stage = 2; stage <= 12; stage++)
            {
                List<MapNode> currentColumn = new List<MapNode>();

                if (stage == 6)
                {
                    MapNode shopNode = new MapNode { Name = $"{stage}-Shop", Position = new Vector2(currentX, startY), Type = NodeType.Shop };
                    currentColumn.Add(shopNode);
                }
                else if (stage == 7)
                {
                    MapNode eliteNode = new MapNode { Name = $"{stage}-Elite", Position = new Vector2(currentX, startY), Type = NodeType.Elite };
                    currentColumn.Add(eliteNode);
                }
                else if (stage == 12)
                {
                    MapNode shopNode2 = new MapNode { Name = $"{stage}-Shop", Position = new Vector2(currentX, startY), Type = NodeType.Shop };
                    currentColumn.Add(shopNode2);
                }
                else
                {
                    int laneCount = Rng.Next(2, 4);
                    int[] possibleYPositions = new int[] { 200, 350, 540, 730, 900 };
                    List<int> chosenYs = GetRandomSample(possibleYPositions, laneCount);

                    for (int i = 0; i < chosenYs.Count; i++)
                    {
                        NodeType assignedType = RollNodeType();

                        // Formats name as "!Encounter!", "!Event!", or "!Elite!" instead of lane codes
                        string typeLabel = $"!{assignedType}!";

                        MapNode node = new MapNode
                        {
                            Name = $"{stage}-{typeLabel}",
                            Position = new Vector2(currentX, chosenYs[i]),
                            Type = assignedType
                        };
                        currentColumn.Add(node);
                    }
                }

                generatedNodes.AddRange(currentColumn);
                mapColumns.Add(currentColumn);

                List<MapNode> previousColumn = mapColumns[stage - 2];
                ConnectColumns(previousColumn, currentColumn);

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

            currentX += xSpacing;

            // 4. STAGE 14: Exit / Victory Star Node
            MapNode victoryNode = new MapNode
            {
                Name = "14-Victory",
                Position = new Vector2(currentX, startY),
                Type = NodeType.Victory
            };
            generatedNodes.Add(victoryNode);

            // Connect Boss to the Victory exit node
            bossNode.ConnectedNodes.Add(victoryNode);

            return generatedNodes;
        }

        private static void ConnectColumns(List<MapNode> prevCol, List<MapNode> currCol)
        {
            foreach (var prevNode in prevCol)
            {
                int connections = Rng.Next(1, Math.Min(3, currCol.Count + 1));
                List<MapNode> targets = GetRandomSampleNodes(currCol, connections);

                foreach (var target in targets)
                {
                    if (!prevNode.ConnectedNodes.Contains(target))
                    {
                        prevNode.ConnectedNodes.Add(target);
                    }
                }
            }

            foreach (var currNode in currCol)
            {
                bool isReachable = false;
                foreach (var prevNode in prevCol)
                {
                    if (prevNode.ConnectedNodes.Contains(currNode))
                    {
                        isReachable = true;
                        break;
                    }
                }

                if (!isReachable)
                {
                    MapNode randomPrev = prevCol[Rng.Next(prevCol.Count)];
                    randomPrev.ConnectedNodes.Add(currNode);
                }
            }
        }

        private static NodeType RollNodeType()
        {
            int roll = Rng.Next(0, 100);
            if (roll < 50) return NodeType.Encounter;
            if (roll < 95) return NodeType.Event;
            return NodeType.Elite;
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

        private static List<MapNode> GetRandomSampleNodes(List<MapNode> list, int count)
        {
            List<MapNode> copy = new List<MapNode>(list);
            List<MapNode> result = new List<MapNode>();
            for (int i = 0; i < count && copy.Count > 0; i++)
            {
                int index = Rng.Next(copy.Count);
                result.Add(copy[index]);
                copy.RemoveAt(index);
            }
            return result;
        }
    }
}