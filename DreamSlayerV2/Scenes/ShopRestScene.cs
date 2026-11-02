using DreamSlayerV2.Core;
using DreamSlayerV2.Managers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DreamSlayerV2.Scenes
{
    public enum StructureType
    {
        SafeHouse,
        Blacksmith,
        StorageArea,
        Hospital
    }

    public class ShopRestScene : IScene
    {
        protected MouseState _prevMouse;
        protected readonly bool _isRest;
        protected StructureType _currentStructure;
        
        protected bool _primaryUsed = false;
        protected bool _secondaryUsed = false;
        protected string _interactionMessage = "";
        protected float _messageTimer = 0f;
        protected static readonly Rectangle PrimaryActionBounds = new Rectangle(150, 425, 900, 72);
        protected static readonly Rectangle SecondaryActionBounds = new Rectangle(150, 525, 900, 72);
        protected static readonly Rectangle LeaveBounds = new Rectangle(1570, 940, 270, 72);

        protected ShopRestScene(bool isRest, StructureType structure)
        {
            _isRest = isRest;
            _currentStructure = structure;
        }

        public void Initialize()
        {
            _prevMouse = Mouse.GetState();
        }

        public virtual void LoadContent()
        {
            if (_isRest)
            {
                MusicManager.PlaySong(GlobalContentManager.NodeSong, restart: true);
            }
            var graphicsDevice = GlobalContentManager.GraphicsDevice;
        }

        public virtual void Update(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (_messageTimer > 0f) _messageTimer -= dt;

            var ms = Mouse.GetState();
            bool clicked = ms.LeftButton == ButtonState.Pressed && _prevMouse.LeftButton == ButtonState.Released;

            var player = GameState.CurrentPlayer;
            if (player == null) return;

            if (_isRest)
            {
                if (clicked)
                {
                    if (PrimaryActionBounds.Contains(ms.Position) && !_primaryUsed)
                    {
                        _primaryUsed = true;
                        float healMultiplier = (_currentStructure == StructureType.SafeHouse || _currentStructure == StructureType.Hospital) ? 0.30f : 0.15f;
                        int heal = Math.Max(1, (int)(player.MaxHP * healMultiplier));
                        
                        player.PlayerHP = Math.Min(player.MaxHP, player.PlayerHP + heal);
                        player.Sanity = 100;
                        _interactionMessage = $"Rested well! Healed {heal} HP and fully restored Sanity.";
                        _messageTimer = 3f;
                    }

                    if (SecondaryActionBounds.Contains(ms.Position) && !_secondaryUsed)
                    {
                        switch (_currentStructure)
                        {
                            case StructureType.SafeHouse:
                                _secondaryUsed = true;
                                int foundCoins = new Random().Next(15, 36);
                                player.SoulCoins += foundCoins;
                                _interactionMessage = $"Explored the Safe House and found {foundCoins} Soul Coins!";
                                break;

                            case StructureType.Blacksmith:
                                int upgradeIndex = player.Deck.FindIndex(card =>
                                    card.Name == "Strike" || card.Name == "Defend");
                                if (upgradeIndex >= 0)
                                {
                                    CardManager card = player.Deck[upgradeIndex];
                                    player.Deck[upgradeIndex] = new CardManager(
                                        card.CardID,
                                        card.Name + "+",
                                        card.Cost,
                                        card.Type,
                                        card.Hazard);
                                    _secondaryUsed = true;
                                    _interactionMessage = $"Upgraded {card.Name} to {card.Name}+!";
                                }
                                else
                                {
                                    _interactionMessage = "No basic Strike or Defend card is available to upgrade.";
                                }
                                break;

                            case StructureType.StorageArea:
                                _secondaryUsed = true;
                                player.TryAddCard(
                                    new CardManager(1, "Defend+", 1, CardManager.CardType.Defense),
                                    out _);
                                _interactionMessage = "Scavenged storage and added a Defend+ card to your deck!";
                                break;

                            case StructureType.Hospital:
                                _secondaryUsed = true;
                                player.MaxHP += 5;
                                player.PlayerHP = Math.Min(player.MaxHP, player.PlayerHP + 5);
                                _interactionMessage = "Injected Vaccine! Max HP permanently increased by 5.";
                                break;
                        }
                        _messageTimer = 3f;
                    }
                }
            }

            if ((ms.RightButton == ButtonState.Pressed && _prevMouse.RightButton == ButtonState.Released) ||
                (clicked && LeaveBounds.Contains(ms.Position)))
            {
                SceneManager.ChangeScene(new NodeSelectScene());
            }

            _prevMouse = ms;
        }

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin();
            SceneUi.DrawBackground(spriteBatch);
            SceneUi.DrawPanel(spriteBatch, new Rectangle(100, 150, 1120, 540), Color.MediumSeaGreen);

            if (_isRest)
            {
                spriteBatch.DrawString(GlobalContentManager.DefaultFont, $"REST AREA: {_currentStructure}", new Vector2(150, 185), Color.Gold);
                SceneUi.DrawWrappedText(spriteBatch, GetStructureDialogue(), new Vector2(150, 245), 1000, Color.LightGray);

                string restLabel = _primaryUsed ? "Rest (Already Used)" : GetPrimaryRestLabel();
                string secondaryLabel = _secondaryUsed ? "Secondary Action (Already Used)" : GetSecondaryActionLabel();
                MouseState mouse = Mouse.GetState();
                SceneUi.DrawButton(spriteBatch, PrimaryActionBounds, restLabel, PrimaryActionBounds.Contains(mouse.Position), !_primaryUsed);
                SceneUi.DrawButton(spriteBatch, SecondaryActionBounds, secondaryLabel, SecondaryActionBounds.Contains(mouse.Position), !_secondaryUsed);

                if (_messageTimer > 0f)
                {
                    SceneUi.DrawWrappedText(spriteBatch, _interactionMessage, new Vector2(150, 620), 1050, Color.Orange);
                }
            }

            var player = GameState.CurrentPlayer;
            SceneUi.DrawPlayerStatus(spriteBatch, player);
            SceneUi.DrawButton(spriteBatch, LeaveBounds, "Return to Map", LeaveBounds.Contains(Mouse.GetState().Position), true);

            spriteBatch.End();
        }

        private string GetStructureDialogue() => _currentStructure switch
        {
            StructureType.SafeHouse => "\"A quiet room with a surprisingly soft mattress. It feels safe here... for now.\"",
            StructureType.Blacksmith => "\"Sparks fly from an old anvil. The tools here can reshape weapons and cards alike.\"",
            StructureType.StorageArea => "\"Dusty crates and forgotten supplies. Who knows what was left behind?\"",
            StructureType.Hospital => "\"A chilling medical ward, smelling of antiseptics and lingering nightmares.\"",
            _ => "\"A resting place in the abyss.\""
        };

        private string GetPrimaryRestLabel() => (_currentStructure == StructureType.SafeHouse || _currentStructure == StructureType.Hospital)
            ? "1. Rest on Comfort Bed - Heal 30% MaxHP & Full Sanity"
            : "1. Rest - Heal 15% MaxHP & Full Sanity";

        private string GetSecondaryActionLabel() => _currentStructure switch
        {
            StructureType.SafeHouse => "2. Explore the room for hidden Soul Coins",
            StructureType.Blacksmith => "2. Upgrade the first Strike or Defend in your deck",
            StructureType.StorageArea => "2. Scavenge a Defend+ card for your deck",
            StructureType.Hospital => "2. Search Medical Supply for Vaccine Buff (+5 Max HP)",
            _ => "2. Special Action"
        };

        public void UnloadContent() { }
    }

    public class SafeHouseRest : ShopRestScene { public SafeHouseRest() : base(true, StructureType.SafeHouse) { } }
    public class BlacksmithRest : ShopRestScene { public BlacksmithRest() : base(true, StructureType.Blacksmith) { } }
    public class StorageRest : ShopRestScene { public StorageRest() : base(true, StructureType.StorageArea) { } }
    public class HospitalRest : ShopRestScene { public HospitalRest() : base(true, StructureType.Hospital) { } }
}