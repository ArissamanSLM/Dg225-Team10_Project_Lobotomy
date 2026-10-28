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

        private readonly string[] _mainMenuItems = { "New Game", "Continue", "Upgrades", "Options", "Exit" };
        private int _selectedMenuIndex = 0;

        private MouseState _currentMouse;
        private MouseState _previousMouse;
        private KeyboardState _currentKeyboard;
        private KeyboardState _previousKeyboard;

        private float _masterVolume = 1.0f;
        private bool _sfxEnabled = true;

        private List<FloatingNightmareText> _floatingTexts = new List<FloatingNightmareText>();
        private float _spawnTimer = 0f;

        public void Initialize() { }

        public void LoadContent(ContentManager content)
        {
            _font = content.Load<SpriteFont>("File");
            NatureFont = content.Load<SpriteFont>("NatureFonts");
            NightmareSong = content.Load<Song>("leberch-nightmare-611631");
            menuTexture = content.Load<Texture2D>("Blankmenu");

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

            UpdateNightmareTexts(dt);

            switch (_currentState)
            {
                case MenuState.Main:
                    HandleMainMenuInput(isLeftClicked);
                    break;

                case MenuState.Options:
                    HandleOptionsInput(isLeftClicked);
                    break;

                case MenuState.Upgrades:
                    HandleUpgradesInput(isLeftClicked);
                    break;

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
            if (IsKeyPressed(Keys.Up) || IsKeyPressed(Keys.W))
            {
                _selectedMenuIndex = (_selectedMenuIndex - 1 + _mainMenuItems.Length) % _mainMenuItems.Length;
            }
            if (IsKeyPressed(Keys.Down) || IsKeyPressed(Keys.S))
            {
                _selectedMenuIndex = (_selectedMenuIndex + 1) % _mainMenuItems.Length;
            }

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

            if (IsKeyPressed(Keys.Enter) || IsKeyPressed(Keys.Space))
            {
                ExecuteMenuChoice(_selectedMenuIndex);
            }
        }

        private void ExecuteMenuChoice(int index)
        {
            switch (index)
            {
                case 0:
                    NodeSelectScene.Reset();
                    SceneManager.ChangeScene(new SceneStory1());
                    break;

                case 1:
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

                case 2:
                    _currentState = MenuState.Upgrades;
                    break;

                case 3:
                    _currentState = MenuState.Options;
                    break;

                case 4:
                    Environment.Exit(0);
                    break;
            }
        }

        private void HandleOptionsInput(bool isLeftClicked)
        {
            Rectangle backButton = new Rectangle(810, 750, 300, 50);
            if ((backButton.Contains(_currentMouse.Position) && isLeftClicked) || IsKeyPressed(Keys.Escape))
            {
                _currentState = MenuState.Main;
                return;
            }

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

                Rectangle sfxBox = new Rectangle(1100, 480, 160, 40);
                if (sfxBox.Contains(_currentMouse.Position))
                {
                    _sfxEnabled = !_sfxEnabled;
                }
            }
        }

        private void HandleUpgradesInput(bool isLeftClicked)
        {
            Rectangle backRect = new Rectangle(810, 920, 300, 50);
            if ((backRect.Contains(_currentMouse.Position) && isLeftClicked) || IsKeyPressed(Keys.Escape))
            {
                _currentState = MenuState.Main;
                return;
            }

            if (!isLeftClicked) return;
            Point mousePos = _currentMouse.Position;

            // 1. Max HP
            Rectangle hpRect = new Rectangle(500, 400, 900, 50);
            if (hpRect.Contains(mousePos) && UpgradeOrbPermanent.MaxHPLevel < 12)
            {
                int cost = UpgradeOrbPermanent.GetUpgradeCost(UpgradeOrbPermanent.MaxHPLevel);
                if (UpgradeOrbPermanent.RebirthCoins >= cost)
                {
                    UpgradeOrbPermanent.RebirthCoins -= cost;
                    UpgradeOrbPermanent.MaxHPLevel++;
                }
            }

            // 2. Starting Coins
            Rectangle coinsRect = new Rectangle(500, 470, 900, 50);
            if (coinsRect.Contains(mousePos) && UpgradeOrbPermanent.StartCoinsLevel < 9)
            {
                int cost = UpgradeOrbPermanent.GetUpgradeCost(UpgradeOrbPermanent.StartCoinsLevel);
                if (UpgradeOrbPermanent.RebirthCoins >= cost)
                {
                    UpgradeOrbPermanent.RebirthCoins -= cost;
                    UpgradeOrbPermanent.StartCoinsLevel++;
                }
            }

            // 3. Dice Minigame
            Rectangle diceRect = new Rectangle(500, 540, 900, 50);
            if (diceRect.Contains(mousePos) && !UpgradeOrbPermanent.UnlockDiceMinigame && UpgradeOrbPermanent.RebirthCoins >= 300)
            {
                UpgradeOrbPermanent.RebirthCoins -= 300;
                UpgradeOrbPermanent.UnlockDiceMinigame = true;
            }

            // 4. Energy Altar
            Rectangle altarRect = new Rectangle(500, 610, 900, 50);
            if (altarRect.Contains(mousePos) && !UpgradeOrbPermanent.EnergyAltarUnlocked && UpgradeOrbPermanent.RebirthCoins >= 500)
            {
                UpgradeOrbPermanent.RebirthCoins -= 500;
                UpgradeOrbPermanent.EnergyAltarUnlocked = true;
            }

            // 5. Charms
            Rectangle charmRect = new Rectangle(500, 680, 900, 50);
            if (charmRect.Contains(mousePos) && !UpgradeOrbPermanent.CharmsUnlocked && UpgradeOrbPermanent.RebirthCoins >= 5000)
            {
                UpgradeOrbPermanent.RebirthCoins -= 5000;
                UpgradeOrbPermanent.CharmsUnlocked = true;
            }

            // 6. Nightmare Mode
            Rectangle nightmareRect = new Rectangle(500, 750, 900, 50);
            if (nightmareRect.Contains(mousePos) && !UpgradeOrbPermanent.NightmareModeUnlocked && UpgradeOrbPermanent.RebirthCoins >= 1000)
            {
                UpgradeOrbPermanent.RebirthCoins -= 1000;
                UpgradeOrbPermanent.NightmareModeUnlocked = true;
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

            Vector2 titleSize = _font.MeasureString(titleText);
            Vector2 titlePos = new Vector2(960f - (titleSize.X / 2f), 100f);
            spriteBatch.DrawString(_font, titleText, titlePos, Color.Crimson);

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

                Rectangle srcRect = new Rectangle(
                    isSelected ? 0 : srcWidth,
                    i * srcHeight,
                    srcWidth,
                    srcHeight
                );

                spriteBatch.Draw(menuTexture, destRect, srcRect, Color.White);

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

            spriteBatch.DrawString(_font, $"Master Volume: {(int)(_masterVolume * 100)}%", new Vector2(750, 420), Color.White);
            spriteBatch.DrawString(_font, "[ - ]", new Vector2(1100, 420), Color.Yellow);
            spriteBatch.DrawString(_font, "[ + ]", new Vector2(1220, 420), Color.Yellow);

            string sfxText = _sfxEnabled ? "ON" : "OFF";
            Color sfxColor = _sfxEnabled ? Color.LightGreen : Color.Red;
            spriteBatch.DrawString(_font, $"Sound Effects: {sfxText}", new Vector2(750, 480), sfxColor);

            Rectangle backRect = new Rectangle(810, 750, 300, 50);
            bool isHovered = backRect.Contains(_currentMouse.Position);
            spriteBatch.DrawString(_font, "< BACK TO MENU >", new Vector2(810, 750), isHovered ? Color.Gold : Color.White);
        }

        private void DrawUpgradesView(SpriteBatch spriteBatch)
        {
            spriteBatch.DrawString(_font, "--- PERMANENT ORB UPGRADES ---", new Vector2(720, 260), Color.Gold);
            spriteBatch.DrawString(_font, $"Rebirth Coins: {UpgradeOrbPermanent.RebirthCoins}", new Vector2(810, 320), Color.Cyan);

            int hpCost = UpgradeOrbPermanent.GetUpgradeCost(UpgradeOrbPermanent.MaxHPLevel);
            string hpStatus = UpgradeOrbPermanent.MaxHPLevel >= 12 ? "MAX" : $"{hpCost} Coins";
            spriteBatch.DrawString(_font, $"1. Max HP (+1 HP/Orb) [{UpgradeOrbPermanent.GetStarOrbText(UpgradeOrbPermanent.MaxHPLevel)}] - Cost: {hpStatus}", new Vector2(500, 400), Color.White);

            int coinCost = UpgradeOrbPermanent.GetUpgradeCost(UpgradeOrbPermanent.StartCoinsLevel);
            string coinStatus = UpgradeOrbPermanent.StartCoinsLevel >= 9 ? "MAX" : $"{coinCost} Coins";
            spriteBatch.DrawString(_font, $"2. Starting Soul Coins (+10 Coins/Orb) [{UpgradeOrbPermanent.GetStarOrbText(UpgradeOrbPermanent.StartCoinsLevel)}] - Cost: {coinStatus}", new Vector2(500, 470), Color.White);

            string diceText = UpgradeOrbPermanent.UnlockDiceMinigame ? "UNLOCKED" : "Cost: 300 Coins";
            spriteBatch.DrawString(_font, $"3. Dice Minigame Event - {diceText}", new Vector2(500, 540), UpgradeOrbPermanent.UnlockDiceMinigame ? Color.LightGreen : Color.Yellow);

            string altarText = !UpgradeOrbPermanent.EnergyAltarUnlocked ? "Cost: 500 Coins" : (UpgradeOrbPermanent.CanUseEnergyAltar() ? "READY (Free Buff)" : "RECHARGING (24h Cooldown)");
            spriteBatch.DrawString(_font, $"4. Energy Altar - {altarText}", new Vector2(500, 610), UpgradeOrbPermanent.EnergyAltarUnlocked ? Color.DeepSkyBlue : Color.Yellow);

            string charmText = UpgradeOrbPermanent.CharmsUnlocked ? "COMPLETED" : "Cost: 5000 Coins [BRAVE BUFF: +20 Max Sanity & +5 Regen/turn!]";
            spriteBatch.DrawString(_font, $"5. Unlock Charms (Challenge) - {charmText}", new Vector2(500, 680), UpgradeOrbPermanent.CharmsUnlocked ? Color.Magenta : Color.Orange);

            string nmText = UpgradeOrbPermanent.NightmareModeUnlocked ? "UNLOCKED" : "Cost: 1000 Coins (Req Depths 10)";
            spriteBatch.DrawString(_font, $"6. Unlock Nightmare Mode - {nmText}", new Vector2(500, 750), UpgradeOrbPermanent.NightmareModeUnlocked ? Color.Crimson : Color.Red);

            Rectangle backRect = new Rectangle(810, 920, 300, 50);
            bool isHovered = backRect.Contains(_currentMouse.Position);
            spriteBatch.DrawString(_font, "< BACK TO MENU >", new Vector2(810, 920), isHovered ? Color.Gold : Color.White);
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