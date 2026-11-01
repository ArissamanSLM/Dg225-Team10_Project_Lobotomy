using DreamSlayerV2.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

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
        protected SpriteFont _font;
        protected MouseState _prevMouse;
        protected Texture2D _pixel;
        protected readonly bool _isRest;
        protected StructureType _currentStructure;
        
        protected bool _primaryUsed = false;
        protected bool _secondaryUsed = false;
        protected string _interactionMessage = "";
        protected float _messageTimer = 0f;

        protected ShopRestScene(bool isRest, StructureType structure)
        {
            _isRest = isRest;
            _currentStructure = structure;
        }

        public void Initialize()
        {
            _prevMouse = Mouse.GetState();
        }

        public void LoadContent(ContentManager content)
        {
            _font = content.Load<SpriteFont>("File");
            var graphicsDevice = ((IGraphicsDeviceService)content.ServiceProvider.GetService(typeof(IGraphicsDeviceService))).GraphicsDevice;
            _pixel = new Texture2D(graphicsDevice, 1, 1);
            _pixel.SetData(new[] { Color.White });
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
                Rectangle rPrimaryRest = new Rectangle(200, 420, 500, 40);
                Rectangle rSecondaryAction = new Rectangle(200, 480, 500, 40);

                if (clicked)
                {
                    if (rPrimaryRest.Contains(ms.Position) && !_primaryUsed)
                    {
                        _primaryUsed = true;
                        float healMultiplier = (_currentStructure == StructureType.SafeHouse || _currentStructure == StructureType.Hospital) ? 0.30f : 0.15f;
                        int heal = Math.Max(1, (int)(player.MaxHP * healMultiplier));
                        
                        player.PlayerHP = Math.Min(player.MaxHP, player.PlayerHP + heal);
                        player.Sanity = 100;
                        _interactionMessage = $"Rested well! Healed {heal} HP and fully restored Sanity.";
                        _messageTimer = 3f;
                    }

                    if (rSecondaryAction.Contains(ms.Position) && !_secondaryUsed)
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
                                _secondaryUsed = true;
                                _interactionMessage = "Upgraded a card successfully at the anvil!";
                                break;

                            case StructureType.StorageArea:
                                _secondaryUsed = true;
                                double roll = new Random().NextDouble();
                                string cardRarity = roll < 0.05 ? "Ultra Rare (UR)" : "Rare";
                                _interactionMessage = $"Scavenged the storage and found a {cardRarity} card!";
                                break;

                            case StructureType.Hospital:
                                _secondaryUsed = true;
                                player.MaxHP += 5;
                                player.PlayerHP += 5;
                                _interactionMessage = "Injected Vaccine! Max HP permanently increased by 5.";
                                break;
                        }
                        _messageTimer = 3f;
                    }
                }
            }

            if (ms.RightButton == ButtonState.Pressed && _prevMouse.RightButton == ButtonState.Released)
            {
                SceneManager.ChangeScene(new NodeSelectScene());
            }

            _prevMouse = ms;
        }

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin();

            if (_isRest)
            {
                spriteBatch.DrawString(_font, $"Rest Area: {_currentStructure}", new Vector2(200, 200), Color.Gold);
                spriteBatch.DrawString(_font, GetStructureDialogue(), new Vector2(200, 250), Color.LightGray);

                string restLabel = _primaryUsed ? "Rest (Already Used)" : GetPrimaryRestLabel();
                spriteBatch.DrawString(_font, restLabel, new Vector2(200, 420), _primaryUsed ? Color.Gray : Color.LightGreen);

                string secondaryLabel = _secondaryUsed ? "Secondary Action (Already Used)" : GetSecondaryActionLabel();
                spriteBatch.DrawString(_font, secondaryLabel, new Vector2(200, 480), _secondaryUsed ? Color.Gray : Color.Cyan);

                if (_messageTimer > 0f)
                {
                    spriteBatch.DrawString(_font, _interactionMessage, new Vector2(200, 540), Color.Orange);
                }
            }

            spriteBatch.DrawString(_font, "Right-click to leave", new Vector2(200, 620), Color.AntiqueWhite);

            var player = GameState.CurrentPlayer;
            if (player != null)
            {
                spriteBatch.DrawString(_font, $"Soul Coins: {player.SoulCoins}", new Vector2(50, 50), Color.Cyan);
                spriteBatch.DrawString(_font, $"Current HP: {player.PlayerHP}/{player.MaxHP}", new Vector2(50, 80), Color.LightGreen);
            }

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
            StructureType.Blacksmith => "2. Use Anvil to Upgrade a Card",
            StructureType.StorageArea => "2. Search Cargo (95% Rare, 5% UR Card)",
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