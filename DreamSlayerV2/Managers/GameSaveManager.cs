using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using DreamSlayerV2.Core;
using Microsoft.Xna.Framework;

namespace DreamSlayerV2
{
    public static class GameSaveManager
    {
        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        private static string SavePath => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "DreamSlayerV2",
            "save.json");

        public static bool HasSave => File.Exists(SavePath);

        public static void DeleteSave()
        {
            if (File.Exists(SavePath))
            {
                File.Delete(SavePath);
            }
        }

        public static void SaveRun(
            PlayerController player,
            IReadOnlyList<MapNode> nodes,
            MapNode currentNode,
            bool pendingStartReward = false)
        {
            if (player == null) throw new ArgumentNullException(nameof(player));
            if (nodes == null) throw new ArgumentNullException(nameof(nodes));

            int currentNodeIndex = -1;
            for (int i = 0; i < nodes.Count; i++)
            {
                if (ReferenceEquals(nodes[i], currentNode))
                {
                    currentNodeIndex = i;
                    break;
                }
            }

            if (currentNodeIndex < 0) throw new InvalidDataException("Cannot save the run without its current map node.");

            var nodeIndices = new Dictionary<MapNode, int>();
            for (int i = 0; i < nodes.Count; i++)
            {
                nodeIndices.Add(nodes[i], i);
            }

            var data = new SavedRunData
            {
                Player = SavePlayer(player),
                CurrentNodeIndex = currentNodeIndex,
                PendingStartReward = pendingStartReward,
                Nodes = nodes.Select(node => new SavedMapNode
                {
                    Name = node.Name,
                    Stage = node.Stage,
                    X = node.Position.X,
                    Y = node.Position.Y,
                    Type = node.Type,
                    IsVisited = node.IsVisited,
                    IsAvailable = node.IsAvailable,
                    ConnectedNodeIndices = node.ConnectedNodes
                        .Where(nodeIndices.ContainsKey)
                        .Select(connectedNode => nodeIndices[connectedNode])
                        .ToList()
                }).ToList()
            };

            string directory = Path.GetDirectoryName(SavePath)
                ?? throw new InvalidOperationException("The save directory could not be determined.");
            Directory.CreateDirectory(directory);

            string temporaryPath = SavePath + ".tmp";
            File.WriteAllText(temporaryPath, JsonSerializer.Serialize(data, JsonOptions));
            File.Move(temporaryPath, SavePath, overwrite: true);
        }

        public static bool TryLoadRun(out SavedRunData run, out string error)
        {
            run = null;
            error = null;

            if (!HasSave)
            {
                error = "No saved run is available yet.";
                return false;
            }

            try
            {
                string json = File.ReadAllText(SavePath);
                run = JsonSerializer.Deserialize<SavedRunData>(json, JsonOptions);
                if (run?.Player == null || run.Nodes == null ||
                    run.CurrentNodeIndex < 0 || run.CurrentNodeIndex >= run.Nodes.Count)
                {
                    throw new InvalidDataException("The saved run is incomplete or invalid.");
                }

                return true;
            }
            catch (Exception exception) when (
                exception is IOException ||
                exception is JsonException ||
                exception is InvalidDataException)
            {
                error = $"Could not load the saved run: {exception.Message}";
                run = null;
                return false;
            }
        }

        public static PlayerController RestorePlayer(SavedPlayer playerData)
        {
            var player = new PlayerController(playerData.SelectedClass, playerData.IsNightmareMode)
            {
                PlayerHP = playerData.PlayerHP,
                MaxHP = playerData.MaxHP,
                Sanity = playerData.Sanity,
                Honor = playerData.Honor,
                Energy = playerData.Energy,
                Defense = playerData.Defense,
                Level = playerData.Level,
                SoulCoins = playerData.SoulCoins,
                IncreaseInHandDamageValue = playerData.IncreaseInHandDamageValue,
                TurnCount = playerData.TurnCount,
                HasStartBonus = playerData.HasStartBonus,
                PassiveRelics = playerData.PassiveRelics ?? new int[5],
                Deck = RestoreCards(playerData.Deck),
                DiscardPile = RestoreCards(playerData.DiscardPile),
                Hand = RestoreHand(playerData.Hand)
            };

            return player;
        }

        public static List<MapNode> RestoreNodes(SavedRunData run)
        {
            List<MapNode> nodes = run.Nodes.Select(node => new MapNode
            {
                Name = node.Name,
                Stage = node.Stage,
                Position = new Vector2(node.X, node.Y),
                Type = node.Type,
                IsVisited = node.IsVisited,
                IsAvailable = node.IsAvailable
            }).ToList();

            for (int i = 0; i < run.Nodes.Count; i++)
            {
                foreach (int connectedIndex in run.Nodes[i].ConnectedNodeIndices)
                {
                    if (connectedIndex < 0 || connectedIndex >= nodes.Count)
                    {
                        throw new InvalidDataException("The saved map contains an invalid node connection.");
                    }

                    nodes[i].ConnectedNodes.Add(nodes[connectedIndex]);
                }
            }

            return nodes;
        }

        private static SavedPlayer SavePlayer(PlayerController player)
        {
            return new SavedPlayer
            {
                SelectedClass = player.SelectedClass,
                IsNightmareMode = player.IsNightmareMode,
                PlayerHP = player.PlayerHP,
                MaxHP = player.MaxHP,
                Sanity = player.Sanity,
                Honor = player.Honor,
                Energy = player.Energy,
                Defense = player.Defense,
                Level = player.Level,
                SoulCoins = player.SoulCoins,
                IncreaseInHandDamageValue = player.IncreaseInHandDamageValue,
                TurnCount = player.TurnCount,
                HasStartBonus = player.HasStartBonus,
                PassiveRelics = (int[])player.PassiveRelics.Clone(),
                Deck = player.Deck.Select(SaveCard).ToList(),
                DiscardPile = player.DiscardPile.Select(SaveCard).ToList(),
                Hand = player.Hand.Select(card => card == null ? null : SaveCard(card)).ToList()
            };
        }

        private static SavedCard SaveCard(CardManager card)
        {
            return new SavedCard
            {
                CardID = card.CardID,
                Name = card.Name,
                Cost = card.Cost,
                Type = card.Type,
                Hazard = card.Hazard,
                IsUnplayable = card.IsUnplayable,
                InHandDamage = card.InHandDamage
            };
        }

        private static CardManager RestoreCard(SavedCard cardData)
        {
            var card = new CardManager(
                cardData.CardID,
                cardData.Name,
                cardData.Cost,
                cardData.Type,
                cardData.Hazard)
            {
                IsUnplayable = cardData.IsUnplayable,
                InHandDamage = cardData.InHandDamage
            };
            return card;
        }

        private static List<CardManager> RestoreCards(List<SavedCard> cards)
        {
            return cards?.Select(RestoreCard).ToList() ?? new List<CardManager>();
        }

        private static CardManager[] RestoreHand(List<SavedCard> cards)
        {
            var hand = new CardManager[5];
            if (cards == null) return hand;

            for (int i = 0; i < Math.Min(hand.Length, cards.Count); i++)
            {
                if (cards[i] != null) hand[i] = RestoreCard(cards[i]);
            }

            return hand;
        }
    }

    public sealed class SavedRunData
    {
        public SavedPlayer Player { get; set; }
        public List<SavedMapNode> Nodes { get; set; } = new List<SavedMapNode>();
        public int CurrentNodeIndex { get; set; }
        public bool PendingStartReward { get; set; }
    }

    public sealed class SavedPlayer
    {
        public CharacterClassV2 SelectedClass { get; set; }
        public bool IsNightmareMode { get; set; }
        public int PlayerHP { get; set; }
        public int MaxHP { get; set; }
        public int Sanity { get; set; }
        public int Honor { get; set; }
        public int Energy { get; set; }
        public int Defense { get; set; }
        public int Level { get; set; }
        public int SoulCoins { get; set; }
        public int IncreaseInHandDamageValue { get; set; }
        public int TurnCount { get; set; }
        public bool HasStartBonus { get; set; }
        public int[] PassiveRelics { get; set; }
        public List<SavedCard> Deck { get; set; } = new List<SavedCard>();
        public List<SavedCard> DiscardPile { get; set; } = new List<SavedCard>();
        public List<SavedCard> Hand { get; set; } = new List<SavedCard>();
    }

    public sealed class SavedCard
    {
        public int CardID { get; set; }
        public string Name { get; set; }
        public int Cost { get; set; }
        public CardManager.CardType Type { get; set; }
        public CardManager.HazardSubtype Hazard { get; set; }
        public bool IsUnplayable { get; set; }
        public int InHandDamage { get; set; }
    }

    public sealed class SavedMapNode
    {
        public string Name { get; set; }
        public int Stage { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public NodeType Type { get; set; }
        public bool IsVisited { get; set; }
        public bool IsAvailable { get; set; }
        public List<int> ConnectedNodeIndices { get; set; } = new List<int>();
    }
}
