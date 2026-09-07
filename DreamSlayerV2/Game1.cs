using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace DreamSlayerV2
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private Texture2D MapTexture;
        private SpriteFont _gameFont;

        private RoomManager _roomManager;
        private ItemManager _itemManager;
        private PlayerController _playerController;

        private KeyboardState _previousKeyboardState;
        private MouseState _currentMouseState;
        private MouseState _previousMouseState;

        private List<CardManager> _playerHand = new List<CardManager>();

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            _graphics.PreferredBackBufferWidth = 1280;
            _graphics.PreferredBackBufferHeight = 720;
            _graphics.ApplyChanges();
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            _roomManager = new RoomManager();
            _itemManager = new ItemManager();
            _playerController = new PlayerController(CharacterClassV2.Human);
            // Initialize sample cards in hand
            _playerHand.Add(new CardManager(1, "Strike", 1, CardManager.CardType.Attack));
            _playerHand.Add(new CardManager(2, "Defend", 1, CardManager.CardType.Defense));
            _playerHand.Add(new CardManager(3, "Slime Hazard", 1, CardManager.CardType.Status, CardManager.HazardSubtype.Slime));

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            MapTexture = Content.Load<Texture2D>("HorrorForestDream");
            _gameFont = Content.Load<SpriteFont>("File");
        }

        protected override void Update(GameTime gameTime)
        {
            _previousKeyboardState = Keyboard.GetState();
            _previousMouseState = _currentMouseState;
            
            KeyboardState currentKeyboardState = Keyboard.GetState();
            _currentMouseState = Mouse.GetState();

            if (currentKeyboardState.IsKeyDown(Keys.Escape))
                Exit();

            if (currentKeyboardState.IsKeyDown(Keys.Space) && _previousKeyboardState.IsKeyUp(Keys.Space))
            {
                _roomManager.ChooseNode();
            }

            if (currentKeyboardState.IsKeyDown(Keys.N) && _previousKeyboardState.IsKeyUp(Keys.N))
            {
                _roomManager.NightmareMode = !_roomManager.NightmareMode;
            }

            // Update hand card logic (clicks)
            for (int i = _playerHand.Count - 1; i >= 0; i--)
            {
                int cardX = 150 + (i * 140);
                Rectangle cardBounds = new Rectangle(cardX, 500, 120, 180);

                bool isHovered = cardBounds.Contains(_currentMouseState.Position);
                bool isClicked = isHovered && 
                                 _currentMouseState.LeftButton == ButtonState.Pressed && 
                                 _previousMouseState.LeftButton == ButtonState.Released;

                if (isClicked)
                {
                    // If card is unplayable (like Slime/Rock), prevent playing it
                    if (!_playerHand[i].IsUnplayable)
                    {
                        _playerHand.RemoveAt(i);
                    }
                    break;
                }
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            _spriteBatch.Begin();
            _spriteBatch.Draw(MapTexture, new Vector2(0, 0), Color.White);

            // UI Status Strings
            _spriteBatch.DrawString(_gameFont, "Current Room: " + _roomManager.CurrentRoom, new Vector2(50, 50), Color.White);
            _spriteBatch.DrawString(_gameFont, $"Floor: {_roomManager.FloorDifficulty} | Node: {_roomManager.RoomCount}", new Vector2(50, 90), Color.White);
            _spriteBatch.DrawString(_gameFont, "Nightmare Mode: " + (_roomManager.NightmareMode ? "ON" : "OFF"), new Vector2(50, 130), _roomManager.NightmareMode ? Color.Red : Color.White);

            // Render Cards in Hand
            for (int i = 0; i < _playerHand.Count; i++)
            {
                CardManager card = _playerHand[i];
                
                int cardX = 150 + (i * 140);
                int cardY = 500;
                
                Rectangle cardBounds = new Rectangle(cardX, cardY, 120, 180);
                bool isHovered = cardBounds.Contains(_currentMouseState.Position);

                if (isHovered)
                {
                    cardBounds.Y -= 20; // Lift card on hover
                }

                // Draw Card Text/Stats
                _spriteBatch.DrawString(_gameFont, card.Name, new Vector2(cardBounds.X + 10, cardBounds.Y + 10), Color.Black);
                _spriteBatch.DrawString(_gameFont, $"Cost: {card.Cost}", new Vector2(cardBounds.X + 10, cardBounds.Y + 35), Color.DarkBlue);
                _spriteBatch.DrawString(_gameFont, card.Does, new Vector2(cardBounds.X + 10, cardBounds.Y + 60), Color.DarkGreen);

                if (card.IsUnplayable)
                {
                    _spriteBatch.DrawString(_gameFont, "[UNPLAYABLE]", new Vector2(cardBounds.X + 10, cardBounds.Y + 100), Color.Red);
                }

                // Draw Tooltip on Hover
                if (isHovered)
                {
                    Vector2 tooltipPos = new Vector2(_currentMouseState.X + 15, _currentMouseState.Y - 15);
                    _spriteBatch.DrawString(_gameFont, card.Description, tooltipPos, Color.Yellow);
                }
            }

            _spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}