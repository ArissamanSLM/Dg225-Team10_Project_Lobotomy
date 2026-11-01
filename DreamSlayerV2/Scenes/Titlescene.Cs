using DreamSlayerV2.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
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

        // ระบบเลือกฉากหลังด้วยปุ่ม [1-4] และระบบเลื่อนกล้อง
        private int _selectedBgIndex = 0; // 0=Forest, 1=Cave, 2=MapLeft, 3=MapRight
        private float _titleCameraX = 0f;
        private float _cameraDirection = 1f;

        private string titleText = "DREAM SLAYER V2";

        private readonly string[] _mainMenuItems = { "New Game", "Continue", "Upgrades", "Options", "Exit" };
        private int _selectedMenuIndex = 0;

        private MouseState _currentMouse;
        private MouseState _previousMouse;
        private KeyboardState _currentKeyboard;
        private KeyboardState _previousKeyboard;

        private float _masterVolume = 1.0f;
        private bool _sfxEnabled = true;
        private string _menuStatusText = string.Empty;

        private List<FloatingNightmareText> _floatingTexts = new List<FloatingNightmareText>();
        private float _spawnTimer = 0f;

        public void Initialize() { }

        public void LoadContent()
        {

            // [1] Horror Forest, [2] Dark Cave

            // [3] Map Left, [4] Map Right (Seamless 2500x1080)

            MediaPlayer.IsRepeating = true;
            MediaPlayer.Volume = _masterVolume;
            MediaPlayer.Play(GlobalContentManager.NightmareSong);
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

            // 1. สลับฉากหลังด้วยปุ่ม [1], [2], [3], [4]
            if (IsKeyPressed(Keys.D1)) _selectedBgIndex = 0;
            if (IsKeyPressed(Keys.D2)) _selectedBgIndex = 1;
            if (IsKeyPressed(Keys.D3)) _selectedBgIndex = 2;
            if (IsKeyPressed(Keys.D4)) _selectedBgIndex = 3;

            // 2. แอนิเมชันกล้องเลื่อนฉากหลังอัตโนมัติ (ขยับไปมาระหว่างช่วง 0 ถึง 580px เพื่อเกลี่ยภาพ 2500px ให้พอดีกับหน้าจอ 1920px)
            _titleCameraX += _cameraDirection * 40f * dt;
            if (_titleCameraX > 580f)
            {
                _titleCameraX = 580f;
                _cameraDirection = -1f;
            }
            else if (_titleCameraX < 0f)
            {
                _titleCameraX = 0f;
                _cameraDirection = 1f;
            }

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
                MoveMenuSelection(-1);
            }
            if (IsKeyPressed(Keys.Down) || IsKeyPressed(Keys.S))
            {
                MoveMenuSelection(1);
            }

            int menuStartX = 810;
            int menuStartY = 380;
            int buttonWidth = 300;
            int buttonHeight = 55;
            int buttonSpacing = 20;

            for (int i = 0; i < _mainMenuItems.Length; i++)
            {
                if (!IsMenuItemEnabled(i)) continue;

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
                if (IsMenuItemEnabled(_selectedMenuIndex))
                {
                    ExecuteMenuChoice(_selectedMenuIndex);
                }
            }
        }

        private void MoveMenuSelection(int direction)
        {
            for (int i = 0; i < _mainMenuItems.Length; i++)
            {
                _selectedMenuIndex = (_selectedMenuIndex + direction + _mainMenuItems.Length) % _mainMenuItems.Length;
                if (IsMenuItemEnabled(_selectedMenuIndex)) return;
            }
        }

        private static bool IsMenuItemEnabled(int index)
        {
            return index != 1 || GameSaveManager.HasSave;
        }

        private void ExecuteMenuChoice(int index)
        {
            switch (index)
            {
                case 0:
                    NodeSelectScene.Reset();
                    GameSaveManager.DeleteSave();
                    
                    // สร้าง PlayerController พร้อมเช็คว่าเปิดโหมด Nightmare อยู่หรือไม่
                    bool isNightmare = UpgradeOrbPermanent.NightmareModeUnlocked;
                    GameState.CurrentPlayer = new PlayerController(CharacterClassV2.Human, isNightmare);

                    SceneManager.ChangeScene(new SceneStory1());
                    break;

                case 1:
                    if (GameSaveManager.TryLoadRun(out SavedRunData savedRun, out string loadError))
                    {
                        GameState.CurrentPlayer = GameSaveManager.RestorePlayer(savedRun.Player);
                        NodeSelectScene.RestoreRun(savedRun);
                        _menuStatusText = string.Empty;
                        SceneManager.ChangeScene(savedRun.PendingStartReward
                            ? new NodeStartScene()
                            : new NodeSelectScene());
                    }
                    else
                    {
                        _menuStatusText = loadError;
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

            // --- 1. DRAW DYNAMIC BACKGROUND (2500x1080 -> 1920x1080) ---
            Texture2D currentBg = _selectedBgIndex switch
            {
                0 => GlobalContentManager.ForestBackground,
                1 => GlobalContentManager.CaveBackground,
                2 => GlobalContentManager.MapLeft,
                3 => GlobalContentManager.MapRight,
                _ => GlobalContentManager.ForestBackground
            };

            if (currentBg != null)
            {
                // เลื่อน X จาก 0 ถึง -580px เพื่อให้เห็นภาพกว้าง 2500px ได้อย่างสมบูรณ์แบบ
                spriteBatch.Draw(
                    currentBg, 
                    new Rectangle((int)(-_titleCameraX), 0, 2500, 1080), 
                    Color.White
                );
            }

            // --- 2. DRAW FLOATING NIGHTMARE TEXTS ---
            foreach (var item in _floatingTexts)
            {
                Color nightmareColor = Color.DarkRed * item.Alpha;
                Vector2 origin = GlobalContentManager.NatureFont.MeasureString(item.Text) / 2f;

                spriteBatch.DrawString(
                    GlobalContentManager.NatureFont,
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

            // --- 3. DRAW TITLE HEADER & BACKGROUND SELECTOR PROMPT ---
            Vector2 titleSize = GlobalContentManager.DefaultFont.MeasureString(titleText);
            Vector2 titlePos = new Vector2(960f - (titleSize.X / 2f), 100f);
            spriteBatch.DrawString(GlobalContentManager.DefaultFont, titleText, titlePos, Color.Crimson);

            // แสดงคำแนะนำปุ่มกดเปลี่ยน Background ที่มุมซ้ายล่าง
            spriteBatch.DrawString(GlobalContentManager.DefaultFont, $"Theme: [{_selectedBgIndex + 1}] (Press 1-4 to change)", new Vector2(30, 1020), Color.Gold * 0.8f);

            // --- 4. RENDER MENU STATES ---
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

            // จับคู่ Index ของเมนูหลักเข้ากับ Texture เฉพาะตัวที่โหลดมาจาก GlobalContentManager
            Texture2D[] menuTextures = new[]
            {
                GlobalContentManager.ThirdMenu,   // 0: START (New Game)
                GlobalContentManager.FourthMenu,  // 1: CONTINUE
                GlobalContentManager.SeventhMenu, // 2: UPGRADES
                GlobalContentManager.SixthMenu,   // 3: OPTIONS
                GlobalContentManager.FifthMenu    // 4: EXIT
            };

            for (int i = 0; i < _mainMenuItems.Length; i++)
            {
                Rectangle destRect = new Rectangle(menuStartX, menuStartY + i * (buttonHeight + buttonSpacing), buttonWidth, buttonHeight);
                bool isSelected = (i == _selectedMenuIndex);
                bool isEnabled = IsMenuItemEnabled(i);

                // ใช้ BlankUI2 เป็นพื้นหลังตอนเลือก (Hovered/Selected) และ BlankUI1 เป็นปุ่มปกติ
                Texture2D buttonBg = isSelected ? GlobalContentManager.SecondMenu : GlobalContentManager.FirstMenu;
                if (buttonBg != null)
                {
                    spriteBatch.Draw(buttonBg, destRect, Color.White);
                }

                // วาดรูปโลโก้/ข้อความของปุ่มนั้นๆ (เช่น START, OPTIONS) ทับลงไป
                Texture2D itemTex = menuTextures[i];
                if (itemTex != null)
                {
                    Color tint = !isEnabled ? Color.DimGray : isSelected ? Color.White : Color.LightGray;
                    spriteBatch.Draw(itemTex, destRect, tint);
                }
                else
                {
                    // Fallback เผื่อรูปไหนยังโหลดไม่ขึ้น ให้แสดงตัวหนังสือแทน
                    string text = _mainMenuItems[i];
                    Vector2 textSize = GlobalContentManager.DefaultFont.MeasureString(text);
                    Vector2 textPos = new Vector2(
                        destRect.X + (destRect.Width - textSize.X) / 2f,
                        destRect.Y + (destRect.Height - textSize.Y) / 2f
                    );
                    Color textColor = isSelected ? Color.Black : Color.Gold;
                    spriteBatch.DrawString(GlobalContentManager.DefaultFont, text, textPos, textColor);
                }

                if (!isEnabled)
                {
                    spriteBatch.Draw(GlobalContentManager.WhitePixel, destRect, Color.Black * 0.55f);
                    Vector2 lockedSize = GlobalContentManager.DefaultFont.MeasureString("LOCKED");
                    spriteBatch.DrawString(
                        GlobalContentManager.DefaultFont,
                        "LOCKED",
                        new Vector2(destRect.Center.X - lockedSize.X / 2f, destRect.Center.Y - lockedSize.Y / 2f),
                        Color.Gray);
                }
            }

            if (!string.IsNullOrEmpty(_menuStatusText))
            {
                spriteBatch.DrawString(GlobalContentManager.DefaultFont, _menuStatusText, new Vector2(700, 800), Color.OrangeRed);
            }
        }

        private void DrawOptionsMenu(SpriteBatch spriteBatch)
        {
            spriteBatch.DrawString(GlobalContentManager.DefaultFont, "--- OPTIONS ---", new Vector2(850, 320), Color.Gold);

            spriteBatch.DrawString(GlobalContentManager.DefaultFont, $"Master Volume: {(int)(_masterVolume * 100)}%", new Vector2(750, 420), Color.White);
            spriteBatch.DrawString(GlobalContentManager.DefaultFont, "[ - ]", new Vector2(1100, 420), Color.Yellow);
            spriteBatch.DrawString(GlobalContentManager.DefaultFont, "[ + ]", new Vector2(1220, 420), Color.Yellow);

            string sfxText = _sfxEnabled ? "ON" : "OFF";
            Color sfxColor = _sfxEnabled ? Color.LightGreen : Color.Red;
            spriteBatch.DrawString(GlobalContentManager.DefaultFont, $"Sound Effects: {sfxText}", new Vector2(750, 480), sfxColor);

            Rectangle backRect = new Rectangle(810, 750, 300, 50);
            bool isHovered = backRect.Contains(_currentMouse.Position);
            spriteBatch.DrawString(GlobalContentManager.DefaultFont, "< BACK TO MENU >", new Vector2(810, 750), isHovered ? Color.Gold : Color.White);
        }

        private void DrawUpgradesView(SpriteBatch spriteBatch)
        {
            spriteBatch.DrawString(GlobalContentManager.DefaultFont, "--- PERMANENT ORB UPGRADES ---", new Vector2(720, 260), Color.Gold);
            spriteBatch.DrawString(GlobalContentManager.DefaultFont, $"Rebirth Coins: {UpgradeOrbPermanent.RebirthCoins}", new Vector2(810, 320), Color.Cyan);

            int hpCost = UpgradeOrbPermanent.GetUpgradeCost(UpgradeOrbPermanent.MaxHPLevel);
            string hpStatus = UpgradeOrbPermanent.MaxHPLevel >= 12 ? "MAX" : $"{hpCost} Coins";
            spriteBatch.DrawString(GlobalContentManager.DefaultFont, $"1. Max HP (+1 HP/Orb) [{UpgradeOrbPermanent.GetStarOrbText(UpgradeOrbPermanent.MaxHPLevel)}] - Cost: {hpStatus}", new Vector2(500, 400), Color.White);

            int coinCost = UpgradeOrbPermanent.GetUpgradeCost(UpgradeOrbPermanent.StartCoinsLevel);
            string coinStatus = UpgradeOrbPermanent.StartCoinsLevel >= 9 ? "MAX" : $"{coinCost} Coins";
            spriteBatch.DrawString(GlobalContentManager.DefaultFont, $"2. Starting Soul Coins (+10 Coins/Orb) [{UpgradeOrbPermanent.GetStarOrbText(UpgradeOrbPermanent.StartCoinsLevel)}] - Cost: {coinStatus}", new Vector2(500, 470), Color.White);

            string diceText = UpgradeOrbPermanent.UnlockDiceMinigame ? "UNLOCKED" : "Cost: 300 Coins";
            spriteBatch.DrawString(GlobalContentManager.DefaultFont, $"3. Dice Minigame Event - {diceText}", new Vector2(500, 540), UpgradeOrbPermanent.UnlockDiceMinigame ? Color.LightGreen : Color.Yellow);

            string altarText = !UpgradeOrbPermanent.EnergyAltarUnlocked ? "Cost: 500 Coins" : (UpgradeOrbPermanent.CanUseEnergyAltar() ? "READY (Free Buff)" : "RECHARGING (24h Cooldown)");
            spriteBatch.DrawString(GlobalContentManager.DefaultFont, $"4. Energy Altar - {altarText}", new Vector2(500, 610), UpgradeOrbPermanent.EnergyAltarUnlocked ? Color.DeepSkyBlue : Color.Yellow);

            string charmText = UpgradeOrbPermanent.CharmsUnlocked ? "COMPLETED" : "Cost: 2500 Coins [BRAVE BUFF: +20 Max Sanity & +5 Regen/turn!]";
            spriteBatch.DrawString(GlobalContentManager.DefaultFont, $"5. Unlock Charms (Challenge) - {charmText}", new Vector2(500, 680), UpgradeOrbPermanent.CharmsUnlocked ? Color.Magenta : Color.Orange);

            string nmText = UpgradeOrbPermanent.NightmareModeUnlocked ? "UNLOCKED" : "Cost: 1000 Coins (Req Depths 10)";
            spriteBatch.DrawString(GlobalContentManager.DefaultFont, $"6. Unlock Nightmare Mode - {nmText}", new Vector2(500, 750), UpgradeOrbPermanent.NightmareModeUnlocked ? Color.Crimson : Color.Red);

            Rectangle backRect = new Rectangle(810, 920, 300, 50);
            bool isHovered = backRect.Contains(_currentMouse.Position);
            spriteBatch.DrawString(GlobalContentManager.DefaultFont, "< BACK TO MENU >", new Vector2(810, 920), isHovered ? Color.Gold : Color.White);
        }

        private void DrawCreditsView(SpriteBatch spriteBatch)
        {
            spriteBatch.DrawString(GlobalContentManager.DefaultFont, "--- CREDITS ---", new Vector2(870, 320), Color.Gold);
            spriteBatch.DrawString(GlobalContentManager.DefaultFont, "Project Lobotomy Team 10", new Vector2(800, 420), Color.White);
            spriteBatch.DrawString(GlobalContentManager.DefaultFont, "Music: Leberch Nightmare", new Vector2(810, 480), Color.LightGray);

            spriteBatch.DrawString(GlobalContentManager.DefaultFont, "Click or Press ESC to Return", new Vector2(780, 750), Color.Yellow);
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
