using DreamSlayerV2.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;

namespace DreamSlayerV2.Scenes
{
    
    public class TitleScene : IScene
    {
        private enum MenuState
        {
            Main,
            Options,
            Upgrades,
            Credits
        }

        private MenuState _currentState = MenuState.Main;

        private Random _rnd = new Random();
        private SpriteFont _font;
        private SpriteFont NatureFont;
        private Song NightmareSong;
        private Texture2D menuTexture;

        private string titleText = "DREAM SLAYER V2";

        // Menu items
        private readonly string[] _mainMenuItems = { "New Game", "Continue", "Upgrades", "Options", "Exit" };
        private int _selectedMenuIndex = 0;

        // Input States
        private MouseState _currentMouse;
        private MouseState _previousMouse;
        private KeyboardState _currentKeyboard;
        private KeyboardState _previousKeyboard;

        // Audio & Settings State
        private float _masterVolume = 1.0f;
        private bool _sfxEnabled = true;
        private bool _isFullscreen = false;

        // Nightmare Floating Texts
        private List<FloatingNightmareText> _floatingTexts = new List<FloatingNightmareText>();
        private float _spawnTimer = 0f;

        public void Initialize() { }

        public void LoadContent(ContentManager content)
        {
            _font = content.Load<SpriteFont>("File");
            NatureFont = content.Load<SpriteFont>("NatureFonts");
            NightmareSong = content.Load<Song>("leberch-nightmare-611631");
            menuTexture = content.Load<Texture2D>("Blankmenu"); // Blank menu background

            MediaPlayer.IsRepeating = true;
            MediaPlayer.Volume = _masterVolume;
            MediaPlayer.Play(NightmareSong);
        }

        public void Update(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            _previousKeyboard = _currentKeyboard;
            _currentKeyboard = Keyboard.GetState();

            _previousMouse = _currentMouse;
            _currentMouse = Mouse.GetState();

            bool isLeftClicked = _currentMouse.LeftButton == ButtonState.Pressed &&
                                 _previousMouse.LeftButton == ButtonState.Released;

            // 1. Floating Nightmare Text Animation
            UpdateNightmareTexts(dt);

            // 2. State-Based Input Handling
            switch (_currentState)
            {
                case MenuState.Main:
                    HandleMainMenuInput(isLeftClicked);
                    break;

                case MenuState.Options:
                    HandleOptionsInput(isLeftClicked);
                    break;

                case MenuState.Upgrades:
                case MenuState.Credits:
                    if (isLeftClicked || IsKeyPressed(Keys.Escape) || IsKeyPressed(Keys.Back))
                    {
                        _currentState = MenuState.Main;
                    }
                    break;
            }
        }

        private void HandleMainMenuInput(bool isLeftClicked)
        {
            // Keyboard Navigation
            if (IsKeyPressed(Keys.Up) || IsKeyPressed(Keys.W))
            {
                _selectedMenuIndex = (_selectedMenuIndex - 1 + _mainMenuItems.Length) % _mainMenuItems.Length;
            }
            if (IsKeyPressed(Keys.Down) || IsKeyPressed(Keys.S))
            {
                _selectedMenuIndex = (_selectedMenuIndex + 1) % _mainMenuItems.Length;
            }

            // Mouse Navigation & Click Handling
            int menuStartX = 810;
            int menuStartY = 380;
            int buttonWidth = 300;
            int buttonHeight = 55;
            int buttonSpacing = 20;

            for (int i = 0; i < _mainMenuItems.Length; i++)
            {
                Rectangle bounds = new Rectangle(menuStartX, menuStartY + i * (buttonHeight + buttonSpacing), buttonWidth, buttonHeight);

                if (bounds.Contains(_currentMouse.Position))
                {
                    _selectedMenuIndex = i;

                    if (isLeftClicked)
                    {
                        ExecuteMenuChoice(i);
                        return;
                    }
                }
            }

            // Confirm Selection with Enter/Space
            if (IsKeyPressed(Keys.Enter) || IsKeyPressed(Keys.Space))
            {
                ExecuteMenuChoice(_selectedMenuIndex);
            }
        }

        private void ExecuteMenuChoice(int index)
        {
            switch (index)
            {
                case 0: // New Game
                    NodeSelectScene.Reset();
                    SceneManager.ChangeScene(new SceneStory1());
                    break;

                case 1: // Continue
                    // If active player instance exists, resume game; otherwise start new run
                    if (GameState.CurrentPlayer != null)
                    {
                        SceneManager.ChangeScene(new NodeSelectScene());
                    }
                    else
                    {
                        NodeSelectScene.Reset();
                        SceneManager.ChangeScene(new SceneStory1());
                    }
                    break;

                case 2: // Upgrades
                    _currentState = MenuState.Upgrades;
                    break;

                case 3: // Options
                    _currentState = MenuState.Options;
                    break;

                case 4: // Exit
                    System.Environment.Exit(0);
                    break;
            }
        }

        private void HandleOptionsInput(bool isLeftClicked)
        {
            // Back button or ESC
            Rectangle backButton = new Rectangle(810, 750, 300, 50);
            if ((backButton.Contains(_currentMouse.Position) && isLeftClicked) || IsKeyPressed(Keys.Escape))
            {
                _currentState = MenuState.Main;
                return;
            }

            // Master Volume Slider Bounds
            Rectangle volDown = new Rectangle(1100, 420, 40, 40);
            Rectangle volUp = new Rectangle(1220, 420, 40, 40);

            if (isLeftClicked)
            {
                if (volDown.Contains(_currentMouse.Position))
                {
                    _masterVolume = MathHelper.Clamp(_masterVolume - 0.1f, 0f, 1f);
                    MediaPlayer.Volume = _masterVolume;
                }
                if (volUp.Contains(_currentMouse.Position))
                {
                    _masterVolume = MathHelper.Clamp(_masterVolume + 0.1f, 0f, 1f);
                    MediaPlayer.Volume = _masterVolume;
                }

                // SFX Toggle
                Rectangle sfxBox = new Rectangle(1100, 480, 160, 40);
                if (sfxBox.Contains(_currentMouse.Position))
                {
                    _sfxEnabled = !_sfxEnabled;
                }
            }
        }

        private bool IsKeyPressed(Keys key)
        {
            return _currentKeyboard.IsKeyDown(key) && _previousKeyboard.IsKeyUp(key);
        }

        private void UpdateNightmareTexts(float dt)
        {
            _spawnTimer += dt;
            if (_floatingTexts.Count < 4 && _spawnTimer > 1.5f)
            {
                _spawnTimer = 0f;
                SpawnRandomNightmareText();
            }

            for (int i = _floatingTexts.Count - 1; i >= 0; i--)
            {
                var item = _floatingTexts[i];
                item.Position += item.Velocity * dt;
                item.LifeSpan -= dt;

                if (item.LifeSpan < 1.0f)
                {
                    item.Alpha = item.LifeSpan;
                }

                if (item.LifeSpan <= 0f)
                {
                    _floatingTexts.RemoveAt(i);
                }
            }
        }

        private void SpawnRandomNightmareText()
        {
            Array values = Enum.GetValues(typeof(StringNightmareText));
            StringNightmareText randomEnum = (StringNightmareText)values.GetValue(_rnd.Next(values.Length));
            string selectedText = randomEnum.ToString();

            float startX = 960f + _rnd.Next(-800, 800);
            float startY = 540f + _rnd.Next(-80, 80);

            float speedX = _rnd.Next(-100, 100);
            float speedY = _rnd.Next(-50, 50);

            float randomDegrees = _rnd.Next(-40, 41);
            float rotationAngle = MathHelper.ToRadians(randomDegrees);

            _floatingTexts.Add(new FloatingNightmareText
            {
                Text = selectedText,
                Position = new Vector2(startX, startY),
                Velocity = new Vector2(speedX, speedY),
                Rotation = rotationAngle,
                LifeSpan = 5.0f
            });
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin();

            // 1. Draw Nightmare Texts
            foreach (var item in _floatingTexts)
            {
                Color nightmareColor = Color.DarkRed * item.Alpha;
                Vector2 origin = NatureFont.MeasureString(item.Text) / 2f;

                spriteBatch.DrawString(
                    NatureFont,
                    item.Text,
                    item.Position,
                    nightmareColor,
                    item.Rotation,
                    origin,
                    1.0f,
                    SpriteEffects.None,
                    0f
                );
            }

            // 2. Draw Game Title Header
            Vector2 titleSize = _font.MeasureString(titleText);
            Vector2 titlePos = new Vector2(960f - (titleSize.X / 2f), 100f);
            spriteBatch.DrawString(_font, titleText, titlePos, Color.Crimson);

            // 3. Render Current View State
            switch (_currentState)
            {
                case MenuState.Main:
                    DrawMainMenu(spriteBatch);
                    break;

                case MenuState.Options:
                    DrawOptionsMenu(spriteBatch);
                    break;

                case MenuState.Upgrades:
                    DrawUpgradesView(spriteBatch);
                    break;

                case MenuState.Credits:
                    DrawCreditsView(spriteBatch);
                    break;
            }

            spriteBatch.End();
        }

        private void DrawMainMenu(SpriteBatch spriteBatch)
        {
            int menuStartX = 810;
            int menuStartY = 380;
            int buttonWidth = 300;
            int buttonHeight = 55;
            int buttonSpacing = 20;

            int srcWidth = menuTexture.Width / 2;
            int srcHeight = menuTexture.Height / 5;

            for (int i = 0; i < _mainMenuItems.Length; i++)
            {
                Rectangle destRect = new Rectangle(menuStartX, menuStartY + i * (buttonHeight + buttonSpacing), buttonWidth, buttonHeight);
                bool isSelected = (i == _selectedMenuIndex);

                // Source Rectangle for Blank Texture 2-Column Split (Orange Active / Dark Inactive)
                Rectangle srcRect = new Rectangle(
                    isSelected ? 0 : srcWidth,
                    i * srcHeight,
                    srcWidth,
                    srcHeight
                );

                // Draw background slice from blank texture
                spriteBatch.Draw(menuTexture, destRect, srcRect, Color.White);

                // Overlay Text
                string text = _mainMenuItems[i];
                Vector2 textSize = _font.MeasureString(text);
                Vector2 textPos = new Vector2(
                    destRect.X + (destRect.Width - textSize.X) / 2f,
                    destRect.Y + (destRect.Height - textSize.Y) / 2f
                );

                Color textColor = isSelected ? Color.Black : Color.Gold;
                spriteBatch.DrawString(_font, text, textPos, textColor);
            }
        }

        private void DrawOptionsMenu(SpriteBatch spriteBatch)
        {
            spriteBatch.DrawString(_font, "--- OPTIONS ---", new Vector2(850, 320), Color.Gold);

            // Master Volume
            spriteBatch.DrawString(_font, $"Master Volume: {(int)(_masterVolume * 100)}%", new Vector2(750, 420), Color.White);
            spriteBatch.DrawString(_font, "[ - ]", new Vector2(1100, 420), Color.Yellow);
            spriteBatch.DrawString(_font, "[ + ]", new Vector2(1220, 420), Color.Yellow);

            // SFX Toggle
            string sfxText = _sfxEnabled ? "ON" : "OFF";
            Color sfxColor = _sfxEnabled ? Color.LightGreen : Color.Red;
            spriteBatch.DrawString(_font, $"Sound Effects: {sfxText}", new Vector2(750, 480), sfxColor);

            // Back Button
            Rectangle backRect = new Rectangle(810, 750, 300, 50);
            bool isHovered = backRect.Contains(_currentMouse.Position);
            spriteBatch.DrawString(_font, "< BACK TO MENU >", new Vector2(810, 750), isHovered ? Color.Gold : Color.White);
        }

        private void DrawUpgradesView(SpriteBatch spriteBatch)
        {
            spriteBatch.DrawString(_font, "--- PERMANENT UPGRADES ---", new Vector2(760, 320), Color.Gold);
            spriteBatch.DrawString(_font, "Soul Coins Collected: 0", new Vector2(780, 400), Color.Cyan);
            spriteBatch.DrawString(_font, "1. Max HP Boost (+10 Max HP) - 100 Coins", new Vector2(700, 480), Color.LightGray);
            spriteBatch.DrawString(_font, "2. Starting Sanity (+15 Sanity) - 150 Coins", new Vector2(700, 540), Color.LightGray);

            spriteBatch.DrawString(_font, "Click or Press ESC to Return", new Vector2(780, 750), Color.Yellow);
        }

        private void DrawCreditsView(SpriteBatch spriteBatch)
        {
            spriteBatch.DrawString(_font, "--- CREDITS ---", new Vector2(870, 320), Color.Gold);
            spriteBatch.DrawString(_font, "Project Lobotomy Team 10", new Vector2(800, 420), Color.White);
            spriteBatch.DrawString(_font, "Music: Leberch Nightmare", new Vector2(810, 480), Color.LightGray);

            spriteBatch.DrawString(_font, "Click or Press ESC to Return", new Vector2(780, 750), Color.Yellow);
        }

        public void UnloadContent()
        {
            MediaPlayer.Stop();
        }
    }
    public enum StringNightmareText
    {
        Despair,
        Nightmare,
        Fear,
        Hopeless,
        Darkness,
        Suffering,
        Doom,
        Trapped,
        Pain,
        Paralyzed,
        Void,
        Gloom,
        Terror,
        Collapse,
        NoEscape
    }

    public class FloatingNightmareText
    {
        public string Text { get; set; }
        public Vector2 Position { get; set; }
        public Vector2 Velocity { get; set; }
        public float Rotation { get; set; }
        public float Alpha { get; set; } = 1.0f;
        public float LifeSpan { get; set; } = 5.0f;
    }
}
