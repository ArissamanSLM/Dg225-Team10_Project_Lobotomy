using DreamSlayerV2.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;
using System.IO;

namespace DreamSlayerV2.Scenes
{
    public class TitleScene : IScene
    {
        private enum MenuState
        {
            Main,
            Options,
            Upgrades,
            Credits,
            SaveSlots,
            OverwriteConfirm,
            HardResetConfirm
        }

        private enum SlotAction
        {
            NewGame,
            Continue
        }

        private MenuState _currentState = MenuState.Main;

        private Random _rnd = new Random();

        // ระบบเลือกฉากหลังด้วยปุ่ม [1-4] และระบบเลื่อนกล้อง
        private int _selectedBgIndex = 0; // 0=Forest, 1=Cave, 2=MapLeft, 3=MapRight
        private float _titleCameraX = 0f;
        private float _cameraDirection = 1f;


        private readonly string[] _mainMenuItems = { "New Game", "Continue", "Upgrades", "Options", "Exit", "Nightmare Mode" };
        private int _selectedMenuIndex = 0;
        private bool _nightmareModeSelected;
        private bool _showAscendedUpgrades;
        private SlotAction _slotAction;
        private int _selectedSaveSlot;
        private int _pendingOverwriteSlot;
        private int _confirmationSelection = 1;

        private MouseState _currentMouse;
        private MouseState _previousMouse;
        private KeyboardState _currentKeyboard;
        private KeyboardState _previousKeyboard;

        private float _masterVolume = 1.0f;
        private bool _sfxEnabled = true;
        private string _menuStatusText = string.Empty;

        private List<FloatingNightmareText> _floatingTexts = new List<FloatingNightmareText>();
        private float _spawnTimer = 0f;

        public void Initialize()
        {
            GameSaveManager.LoadPermanentProgress();
        }

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

                case MenuState.SaveSlots:
                    HandleSaveSlotsInput(isLeftClicked);
                    break;

                case MenuState.OverwriteConfirm:
                    HandleOverwriteConfirmationInput(isLeftClicked);
                    break;

                case MenuState.HardResetConfirm:
                    HandleHardResetConfirmationInput(isLeftClicked);
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
            return (index != 1 || GameSaveManager.HasAnySave) &&
                   (index != 5 || UpgradeOrbPermanent.NightmareModeUnlocked);
        }

        private void ExecuteMenuChoice(int index)
        {
            switch (index)
            {
                case 0:
                    BeginSaveSlotSelection(SlotAction.NewGame);
                    break;

                case 1:
                    BeginSaveSlotSelection(SlotAction.Continue);
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

                case 5:
                    _nightmareModeSelected = !_nightmareModeSelected;
                    break;
            }

        }

        private void BeginSaveSlotSelection(SlotAction action)
        {
            _slotAction = action;
            _selectedSaveSlot = 0;
            _menuStatusText = string.Empty;
            _currentState = MenuState.SaveSlots;
        }

        private void HandleSaveSlotsInput(bool isLeftClicked)
        {
            Rectangle firstSlot = new Rectangle(600, 535, 320, 100);
            Rectangle secondSlot = new Rectangle(1000, 535, 320, 100);
            Rectangle backButton = new Rectangle(810, 675, 300, 58);

            if (IsKeyPressed(Keys.Escape) || IsKeyPressed(Keys.Back))
            {
                _currentState = MenuState.Main;
                return;
            }

            if (IsKeyPressed(Keys.Left) || IsKeyPressed(Keys.Right) ||
                IsKeyPressed(Keys.A) || IsKeyPressed(Keys.D))
            {
                _selectedSaveSlot = 1 - _selectedSaveSlot;
            }

            if (firstSlot.Contains(_currentMouse.Position))
            {
                _selectedSaveSlot = 0;
                if (isLeftClicked)
                {
                    SelectSaveSlot(1);
                    return;
                }
            }
            else if (secondSlot.Contains(_currentMouse.Position))
            {
                _selectedSaveSlot = 1;
                if (isLeftClicked)
                {
                    SelectSaveSlot(2);
                    return;
                }
            }
            else if (backButton.Contains(_currentMouse.Position) && isLeftClicked)
            {
                _currentState = MenuState.Main;
                return;
            }

            if (IsKeyPressed(Keys.Enter) || IsKeyPressed(Keys.Space))
            {
                SelectSaveSlot(_selectedSaveSlot + 1);
            }
        }

        private void SelectSaveSlot(int slot)
        {
            GameSaveManager.ActiveSlot = slot;
            if (_slotAction == SlotAction.NewGame)
            {
                if (GameSaveManager.HasSave)
                {
                    _pendingOverwriteSlot = slot;
                    _confirmationSelection = 1;
                    _currentState = MenuState.OverwriteConfirm;
                }
                else
                {
                    StartNewGame(slot);
                }

                return;
            }

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
        }

        private void StartNewGame(int slot)
        {
            GameSaveManager.ActiveSlot = slot;
            NodeSelectScene.Reset();
            GameSaveManager.DeleteSave();
            GameState.ResetRun();
            GameState.CurrentPlayer = new PlayerController(
                CharacterClassV2.Human,
                _nightmareModeSelected && UpgradeOrbPermanent.NightmareModeUnlocked);

            SceneManager.ChangeScene(new SceneStory1());
        }

        private void HandleOverwriteConfirmationInput(bool isLeftClicked)
        {
            Rectangle yesButton = new Rectangle(640, 650, 280, 64);
            Rectangle noButton = new Rectangle(1000, 650, 280, 64);

            if (IsKeyPressed(Keys.Escape) || IsKeyPressed(Keys.Back))
            {
                _currentState = MenuState.SaveSlots;
                return;
            }

            if (IsKeyPressed(Keys.Left) || IsKeyPressed(Keys.Right) ||
                IsKeyPressed(Keys.A) || IsKeyPressed(Keys.D))
            {
                _confirmationSelection = 1 - _confirmationSelection;
            }

            if (yesButton.Contains(_currentMouse.Position))
            {
                _confirmationSelection = 0;
                if (isLeftClicked)
                {
                    StartNewGame(_pendingOverwriteSlot);
                    return;
                }
            }
            else if (noButton.Contains(_currentMouse.Position))
            {
                _confirmationSelection = 1;
                if (isLeftClicked)
                {
                    _currentState = MenuState.SaveSlots;
                    return;
                }
            }

            if (IsKeyPressed(Keys.Enter) || IsKeyPressed(Keys.Space))
            {
                if (_confirmationSelection == 0) StartNewGame(_pendingOverwriteSlot);
                else _currentState = MenuState.SaveSlots;
            }
        }

        private void HandleHardResetConfirmationInput(bool isLeftClicked)
        {
            Rectangle resetButton = new Rectangle(640, 650, 280, 64);
            Rectangle cancelButton = new Rectangle(1000, 650, 280, 64);

            if (IsKeyPressed(Keys.Escape) || IsKeyPressed(Keys.Back))
            {
                _currentState = MenuState.Options;
                return;
            }

            if (IsKeyPressed(Keys.Left) || IsKeyPressed(Keys.Right) ||
                IsKeyPressed(Keys.A) || IsKeyPressed(Keys.D))
            {
                _confirmationSelection = 1 - _confirmationSelection;
            }

            if (resetButton.Contains(_currentMouse.Position))
            {
                _confirmationSelection = 0;
                if (isLeftClicked)
                {
                    PerformHardReset();
                    return;
                }
            }
            else if (cancelButton.Contains(_currentMouse.Position))
            {
                _confirmationSelection = 1;
                if (isLeftClicked)
                {
                    _currentState = MenuState.Options;
                    return;
                }
            }

            if (IsKeyPressed(Keys.Enter) || IsKeyPressed(Keys.Space))
            {
                if (_confirmationSelection == 0) PerformHardReset();
                else _currentState = MenuState.Options;
            }
        }

        private void PerformHardReset()
        {
            try
            {
                GameSaveManager.ResetAllProgress();
                NodeSelectScene.Reset();
                GameState.ResetRun();
                _nightmareModeSelected = false;
                _menuStatusText = "Progress reset. Two rotating backups are kept in AppData.";
                _currentState = MenuState.Options;
            }
            catch (Exception exception) when (
                exception is IOException ||
                exception is UnauthorizedAccessException)
            {
                _menuStatusText = $"Could not reset progress: {exception.Message}";
                _currentState = MenuState.Options;
            }
        }

        private void HandleOptionsInput(bool isLeftClicked)
        {
            Rectangle backButton = new Rectangle(770, 790, 380, 58);
            if ((backButton.Contains(_currentMouse.Position) && isLeftClicked) || IsKeyPressed(Keys.Escape))
            {
                _currentState = MenuState.Main;
                return;
            }

            Rectangle volDown = new Rectangle(990, 400, 56, 52);
            Rectangle volUp = new Rectangle(1220, 400, 56, 52);
            Rectangle sfxToggle = new Rectangle(1030, 500, 220, 58);
            Rectangle hardReset = new Rectangle(730, 640, 460, 58);

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

                if (sfxToggle.Contains(_currentMouse.Position))
                {
                    _sfxEnabled = !_sfxEnabled;
                }

                if (hardReset.Contains(_currentMouse.Position))
                {
                    _confirmationSelection = 1;
                    _currentState = MenuState.HardResetConfirm;
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
            Rectangle orbTab = new Rectangle(600, 350, 320, 48);
            Rectangle ascendedTab = new Rectangle(1000, 350, 320, 48);
            if (orbTab.Contains(mousePos))
            {
                _showAscendedUpgrades = false;
                return;
            }
            if (ascendedTab.Contains(mousePos))
            {
                _showAscendedUpgrades = true;
                return;
            }

            bool changed = _showAscendedUpgrades
                ? PurchaseAscendedUpgrade(mousePos)
                : PurchaseOrbUpgrade(mousePos);
            if (changed) GameSaveManager.SavePermanentProgress();
        }

        private static bool PurchaseOrbUpgrade(Point mousePos)
        {
            bool changed = false;
            if (UpgradeRowBounds(0).Contains(mousePos) && UpgradeOrbPermanent.MaxHPLevel < 12)
            {
                int cost = UpgradeOrbPermanent.GetUpgradeCost(UpgradeOrbPermanent.MaxHPLevel);
                if (UpgradeOrbPermanent.RebirthCoins >= cost)
                {
                    UpgradeOrbPermanent.RebirthCoins -= cost;
                    UpgradeOrbPermanent.MaxHPLevel++;
                    changed = true;
                }
            }
            else if (UpgradeRowBounds(1).Contains(mousePos) && UpgradeOrbPermanent.StartCoinsLevel < 9)
            {
                int cost = UpgradeOrbPermanent.GetUpgradeCost(UpgradeOrbPermanent.StartCoinsLevel);
                if (UpgradeOrbPermanent.RebirthCoins >= cost)
                {
                    UpgradeOrbPermanent.RebirthCoins -= cost;
                    UpgradeOrbPermanent.StartCoinsLevel++;
                    changed = true;
                }
            }
            else if (UpgradeRowBounds(2).Contains(mousePos) && !UpgradeOrbPermanent.UnlockDiceMinigame &&
                     UpgradeOrbPermanent.RebirthCoins >= 300)
            {
                UpgradeOrbPermanent.RebirthCoins -= 300;
                UpgradeOrbPermanent.UnlockDiceMinigame = true;
                changed = true;
            }
            else if (UpgradeRowBounds(3).Contains(mousePos) && !UpgradeOrbPermanent.EnergyAltarUnlocked &&
                     UpgradeOrbPermanent.RebirthCoins >= 500)
            {
                UpgradeOrbPermanent.RebirthCoins -= 500;
                UpgradeOrbPermanent.EnergyAltarUnlocked = true;
                changed = true;
            }
            else if (UpgradeRowBounds(4).Contains(mousePos) && !UpgradeOrbPermanent.CharmsUnlocked &&
                     UpgradeOrbPermanent.RebirthCoins >= 2500)
            {
                UpgradeOrbPermanent.RebirthCoins -= 2500;
                UpgradeOrbPermanent.CharmsUnlocked = true;
                changed = true;
            }
            else if (UpgradeRowBounds(5).Contains(mousePos) && !UpgradeOrbPermanent.NightmareModeUnlocked &&
                     UpgradeOrbPermanent.TotalDepthsCleared >= 10 && UpgradeOrbPermanent.RebirthCoins >= 1000)
            {
                UpgradeOrbPermanent.RebirthCoins -= 1000;
                UpgradeOrbPermanent.NightmareModeUnlocked = true;
                changed = true;
            }

            return changed;
        }

        private static bool PurchaseAscendedUpgrade(Point mousePos)
        {
            if (UpgradeRowBounds(0).Contains(mousePos) && !UpgradeOrbPermanent.ClockPlusUnlocked &&
                UpgradeOrbPermanent.AscendedCoins >= 10)
            {
                UpgradeOrbPermanent.AscendedCoins -= 10;
                UpgradeOrbPermanent.ClockPlusUnlocked = true;
                return true;
            }

            if (UpgradeRowBounds(1).Contains(mousePos))
            {
                if (!UpgradeOrbPermanent.PlusBasicCardsUnlocked && UpgradeOrbPermanent.AscendedCoins >= 10)
                {
                    UpgradeOrbPermanent.AscendedCoins -= 10;
                    UpgradeOrbPermanent.PlusBasicCardsUnlocked = true;
                    UpgradeOrbPermanent.PlusBasicCardsEnabled = true;
                    return true;
                }

                if (UpgradeOrbPermanent.PlusBasicCardsUnlocked)
                {
                    UpgradeOrbPermanent.PlusBasicCardsEnabled = !UpgradeOrbPermanent.PlusBasicCardsEnabled;
                    return true;
                }
            }

            if (UpgradeRowBounds(2).Contains(mousePos) && UpgradeOrbPermanent.StartingHpUpgradeLevel < 3)
            {
                int cost = UpgradeOrbPermanent.GetAscendedUpgradeCost(15, UpgradeOrbPermanent.StartingHpUpgradeLevel);
                if (UpgradeOrbPermanent.AscendedCoins >= cost)
                {
                    UpgradeOrbPermanent.AscendedCoins -= cost;
                    UpgradeOrbPermanent.StartingHpUpgradeLevel++;
                    return true;
                }
            }

            if (UpgradeRowBounds(3).Contains(mousePos) && !UpgradeOrbPermanent.StartingCoinsUpgradeUnlocked &&
                UpgradeOrbPermanent.AscendedCoins >= 15)
            {
                UpgradeOrbPermanent.AscendedCoins -= 15;
                UpgradeOrbPermanent.StartingCoinsUpgradeUnlocked = true;
                return true;
            }

            if (UpgradeRowBounds(4).Contains(mousePos) && UpgradeOrbPermanent.SoulRewardUpgradeLevel < 5)
            {
                int cost = UpgradeOrbPermanent.GetAscendedUpgradeCost(20, UpgradeOrbPermanent.SoulRewardUpgradeLevel);
                if (UpgradeOrbPermanent.AscendedCoins >= cost)
                {
                    UpgradeOrbPermanent.AscendedCoins -= cost;
                    UpgradeOrbPermanent.SoulRewardUpgradeLevel++;
                    return true;
                }
            }

            if (UpgradeRowBounds(5).Contains(mousePos) && !UpgradeOrbPermanent.StartingEnergyUpgradeUnlocked &&
                UpgradeOrbPermanent.AscendedCoins >= 25)
            {
                UpgradeOrbPermanent.AscendedCoins -= 25;
                UpgradeOrbPermanent.StartingEnergyUpgradeUnlocked = true;
                return true;
            }

            return false;
        }

        private static Rectangle UpgradeRowBounds(int row)
        {
            return new Rectangle(360, 415 + (row * 62), 1200, 52);
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
            spriteBatch.Draw(GlobalContentManager.DreamSlayerText, new Vector2(960 - GlobalContentManager.DreamSlayerText.Width / 2f, 50), Color.White);
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

                case MenuState.SaveSlots:
                    DrawSaveSlotSelection(spriteBatch);
                    break;

                case MenuState.OverwriteConfirm:
                    DrawOverwriteConfirmation(spriteBatch);
                    break;

                case MenuState.HardResetConfirm:
                    DrawHardResetConfirmation(spriteBatch);
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
                GlobalContentManager.FifthMenu,   // 4: EXIT
                null                              // 5: Nightmare toggle
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
                    string text = i == 5
                        ? $"Nightmare Mode: {(_nightmareModeSelected ? "ON" : "OFF")}"
                        : _mainMenuItems[i];
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
            Rectangle panel = new Rectangle(520, 285, 880, 560);
            spriteBatch.Draw(GlobalContentManager.WhitePixel, panel, Color.Black * 0.72f);
            spriteBatch.Draw(GlobalContentManager.WhitePixel, new Rectangle(panel.X, panel.Y, panel.Width, 4), Color.MediumPurple);

            DrawCenteredLabel(spriteBatch, "OPTIONS", 335, Color.White, 1.35f);
            spriteBatch.DrawString(GlobalContentManager.DefaultFont, "Music volume", new Vector2(640, 415), Color.Gainsboro);

            Rectangle downButton = new Rectangle(990, 400, 56, 52);
            Rectangle volumeTrack = new Rectangle(1060, 416, 140, 20);
            Rectangle upButton = new Rectangle(1220, 400, 56, 52);
            DrawUiButton(spriteBatch, downButton, "-", downButton.Contains(_currentMouse.Position), Color.SlateBlue);
            spriteBatch.Draw(GlobalContentManager.WhitePixel, volumeTrack, Color.DimGray);
            if (_masterVolume > 0f)
            {
                spriteBatch.Draw(
                    GlobalContentManager.WhitePixel,
                    new Rectangle(volumeTrack.X, volumeTrack.Y, (int)(volumeTrack.Width * _masterVolume), volumeTrack.Height),
                    Color.MediumPurple);
            }
            DrawUiButton(spriteBatch, upButton, "+", upButton.Contains(_currentMouse.Position), Color.SlateBlue);
            spriteBatch.DrawString(
                GlobalContentManager.DefaultFont,
                $"{(int)(_masterVolume * 100)}%",
                new Vector2(1072, 445),
                Color.LightGray);

            spriteBatch.DrawString(GlobalContentManager.DefaultFont, "Sound effects", new Vector2(640, 515), Color.Gainsboro);
            Rectangle sfxButton = new Rectangle(1030, 500, 220, 58);
            DrawUiButton(
                spriteBatch,
                sfxButton,
                _sfxEnabled ? "ON" : "OFF",
                sfxButton.Contains(_currentMouse.Position),
                _sfxEnabled ? Color.SeaGreen : Color.Firebrick);

            Rectangle hardResetButton = new Rectangle(730, 640, 460, 58);
            bool resetHovered = hardResetButton.Contains(_currentMouse.Position);
            spriteBatch.Draw(
                GlobalContentManager.WhitePixel,
                hardResetButton,
                resetHovered ? Color.DarkRed : new Color(90, 28, 38));
            if (GlobalContentManager.EighthMenu != null)
            {
                spriteBatch.Draw(GlobalContentManager.EighthMenu, hardResetButton, Color.White);
            }
            else
            {
                DrawCenteredInButton(spriteBatch, "HARD RESET", hardResetButton, Color.White);
            }

            Rectangle backRect = new Rectangle(770, 790, 380, 58);
            DrawUiButton(spriteBatch, backRect, "BACK TO MENU", backRect.Contains(_currentMouse.Position), Color.SlateBlue);

            if (!string.IsNullOrEmpty(_menuStatusText))
            {
                DrawCenteredLabel(spriteBatch, _menuStatusText, 875, Color.OrangeRed, 0.8f);
            }
        }

        private void DrawSaveSlotSelection(SpriteBatch spriteBatch)
        {
            DrawOverlayPanel(spriteBatch, "CHOOSE A SAVE SLOT", "Select a slot using the mouse or left/right arrow keys.");
            DrawSaveSlotButton(spriteBatch, 1, new Rectangle(600, 535, 320, 100));
            DrawSaveSlotButton(spriteBatch, 2, new Rectangle(1000, 535, 320, 100));
            Rectangle backButton = new Rectangle(810, 675, 300, 58);
            DrawUiButton(spriteBatch, backButton, "BACK", backButton.Contains(_currentMouse.Position), Color.SlateBlue);
            if (!string.IsNullOrEmpty(_menuStatusText))
            {
                DrawCenteredLabel(spriteBatch, _menuStatusText, 700, Color.OrangeRed, 0.8f);
            }
        }

        private void DrawSaveSlotButton(SpriteBatch spriteBatch, int slot, Rectangle bounds)
        {
            bool selected = _selectedSaveSlot == slot - 1 || bounds.Contains(_currentMouse.Position);
            bool hasSave = GameSaveManager.HasSaveForSlot(slot);
            Color background = selected ? new Color(80, 52, 126) : new Color(40, 43, 62);
            spriteBatch.Draw(GlobalContentManager.WhitePixel, bounds, background);
            spriteBatch.Draw(
                GlobalContentManager.WhitePixel,
                new Rectangle(bounds.X, bounds.Y, bounds.Width, 3),
                selected ? Color.MediumPurple : Color.SlateGray);

            DrawCenteredLabel(spriteBatch, $"SLOT {slot}", bounds.Y + 20, Color.White, 1f, bounds);
            string status = hasSave ? "Saved run available" : "Empty";
            Color statusColor = hasSave ? Color.LightGreen : Color.LightGray;
            if (_slotAction == SlotAction.Continue && !hasSave) statusColor = Color.Gray;
            DrawCenteredLabel(spriteBatch, status, bounds.Y + 58, statusColor, 0.75f, bounds);
        }

        private void DrawOverwriteConfirmation(SpriteBatch spriteBatch)
        {
            DrawOverlayPanel(
                spriteBatch,
                "OVERWRITE SLOT?",
                $"Starting a new game will replace Slot {_pendingOverwriteSlot}. Your old progress is backed up.");
            DrawConfirmationButtons(spriteBatch, "OVERWRITE", "CANCEL");
        }

        private void DrawHardResetConfirmation(SpriteBatch spriteBatch)
        {
            DrawOverlayPanel(
                spriteBatch,
                "RESET ALL PROGRESS?",
                "This erases both run slots, upgrades, unlocks, and scores. Two rotating backups are kept.");
            DrawConfirmationButtons(spriteBatch, "RESET EVERYTHING", "CANCEL", true);
        }

        private void DrawOverlayPanel(SpriteBatch spriteBatch, string title, string description)
        {
            spriteBatch.Draw(GlobalContentManager.WhitePixel, new Rectangle(0, 0, 1920, 1080), Color.Black * 0.58f);
            Rectangle panel = new Rectangle(500, 330, 920, 420);
            spriteBatch.Draw(GlobalContentManager.WhitePixel, panel, new Color(22, 24, 40));
            spriteBatch.Draw(GlobalContentManager.WhitePixel, new Rectangle(panel.X, panel.Y, panel.Width, 4), Color.MediumPurple);
            DrawCenteredLabel(spriteBatch, title, 390, Color.White, 1.25f);
            DrawCenteredLabel(spriteBatch, description, 485, Color.Gainsboro, 0.8f);
        }

        private void DrawConfirmationButtons(SpriteBatch spriteBatch, string confirmText, string cancelText, bool destructive = false)
        {
            Rectangle confirm = new Rectangle(640, 650, 280, 64);
            Rectangle cancel = new Rectangle(1000, 650, 280, 64);
            DrawUiButton(
                spriteBatch,
                confirm,
                confirmText,
                _confirmationSelection == 0 || confirm.Contains(_currentMouse.Position),
                destructive ? Color.Firebrick : Color.DarkOrange);
            DrawUiButton(
                spriteBatch,
                cancel,
                cancelText,
                _confirmationSelection == 1 || cancel.Contains(_currentMouse.Position),
                Color.SlateBlue);
        }

        private void DrawUiButton(SpriteBatch spriteBatch, Rectangle bounds, string text, bool selected, Color color)
        {
            spriteBatch.Draw(GlobalContentManager.WhitePixel, bounds, selected ? color : color * 0.72f);
            DrawCenteredInButton(spriteBatch, text, bounds, Color.White);
        }

        private void DrawCenteredInButton(SpriteBatch spriteBatch, string text, Rectangle bounds, Color color)
        {
            Vector2 size = GlobalContentManager.DefaultFont.MeasureString(text);
            spriteBatch.DrawString(
                GlobalContentManager.DefaultFont,
                text,
                new Vector2(bounds.Center.X - size.X / 2f, bounds.Center.Y - size.Y / 2f),
                color);
        }

        private void DrawCenteredLabel(
            SpriteBatch spriteBatch,
            string text,
            int y,
            Color color,
            float scale,
            Rectangle? bounds = null)
        {
            Vector2 size = GlobalContentManager.DefaultFont.MeasureString(text) * scale;
            int centerX = bounds?.Center.X ?? 960;
            spriteBatch.DrawString(
                GlobalContentManager.DefaultFont,
                text,
                new Vector2(centerX - size.X / 2f, y),
                color,
                0f,
                Vector2.Zero,
                scale,
                SpriteEffects.None,
                0f);
        }

        private void DrawUpgradesView(SpriteBatch spriteBatch)
        {
            spriteBatch.DrawString(GlobalContentManager.DefaultFont, "--- PERMANENT UPGRADES ---", new Vector2(790, 250), Color.Gold);
            SceneUi.DrawButton(spriteBatch, new Rectangle(600, 350, 320, 48), "Orb Tree", !_showAscendedUpgrades);
            SceneUi.DrawButton(spriteBatch, new Rectangle(1000, 350, 320, 48), "Nightmare / Ascended", _showAscendedUpgrades);

            if (_showAscendedUpgrades)
            {
                spriteBatch.DrawString(GlobalContentManager.DefaultFont, $"Ascended Coins: {UpgradeOrbPermanent.AscendedCoins}", new Vector2(810, 320), Color.Cyan);
                DrawUpgradeRow(spriteBatch, 0,
                    $"1. Start with Clock+ - {(UpgradeOrbPermanent.ClockPlusUnlocked ? "OWNED" : "Cost: 10 Ascended Coins")}",
                    UpgradeOrbPermanent.ClockPlusUnlocked ? Color.LightGreen : Color.White);
                string plusCards = !UpgradeOrbPermanent.PlusBasicCardsUnlocked
                    ? "Cost: 10 Ascended Coins"
                    : UpgradeOrbPermanent.PlusBasicCardsEnabled ? "ON (click to turn OFF)" : "OFF (click to turn ON)";
                DrawUpgradeRow(spriteBatch, 1, $"2. Plus basic starter cards - {plusCards}",
                    UpgradeOrbPermanent.PlusBasicCardsUnlocked ? Color.LightGreen : Color.White);
                string hpUpgrade = UpgradeOrbPermanent.StartingHpUpgradeLevel >= 3
                    ? "MAX"
                    : $"Level {UpgradeOrbPermanent.StartingHpUpgradeLevel}/3, +5 HP each - Cost: {UpgradeOrbPermanent.GetAscendedUpgradeCost(15, UpgradeOrbPermanent.StartingHpUpgradeLevel)}";
                DrawUpgradeRow(spriteBatch, 2, $"3. Tougher start - {hpUpgrade}", Color.White);
                DrawUpgradeRow(spriteBatch, 3,
                    $"4. Starting Soul Coins +25 - {(UpgradeOrbPermanent.StartingCoinsUpgradeUnlocked ? "OWNED" : "Cost: 15 Ascended Coins")}",
                    UpgradeOrbPermanent.StartingCoinsUpgradeUnlocked ? Color.LightGreen : Color.White);
                string rewardUpgrade = UpgradeOrbPermanent.SoulRewardUpgradeLevel >= 5
                    ? "MAX"
                    : $"Level {UpgradeOrbPermanent.SoulRewardUpgradeLevel}/5, +10% each - Cost: {UpgradeOrbPermanent.GetAscendedUpgradeCost(20, UpgradeOrbPermanent.SoulRewardUpgradeLevel)}";
                DrawUpgradeRow(spriteBatch, 4, $"5. Soul Coin bounty - {rewardUpgrade}", Color.White);
                DrawUpgradeRow(spriteBatch, 5,
                    $"6. +1 starting Energy - {(UpgradeOrbPermanent.StartingEnergyUpgradeUnlocked ? "OWNED" : "Cost: 25 Ascended Coins")}",
                    UpgradeOrbPermanent.StartingEnergyUpgradeUnlocked ? Color.LightGreen : Color.White);
            }
            else
            {
                spriteBatch.DrawString(GlobalContentManager.DefaultFont, $"Rebirth Coins: {UpgradeOrbPermanent.RebirthCoins}", new Vector2(810, 320), Color.Cyan);

                int hpCost = UpgradeOrbPermanent.GetUpgradeCost(UpgradeOrbPermanent.MaxHPLevel);
                string hpStatus = UpgradeOrbPermanent.MaxHPLevel >= 12 ? "MAX" : $"{hpCost} Coins";
                DrawUpgradeRow(spriteBatch, 0,
                    $"1. Max HP (+1 HP/Orb) [{UpgradeOrbPermanent.GetStarOrbText(UpgradeOrbPermanent.MaxHPLevel)}] - Cost: {hpStatus}",
                    Color.White);

                int coinCost = UpgradeOrbPermanent.GetUpgradeCost(UpgradeOrbPermanent.StartCoinsLevel);
                string coinStatus = UpgradeOrbPermanent.StartCoinsLevel >= 9 ? "MAX" : $"{coinCost} Coins";
                DrawUpgradeRow(spriteBatch, 1,
                    $"2. Starting Soul Coins (+10/Orb) [{UpgradeOrbPermanent.GetStarOrbText(UpgradeOrbPermanent.StartCoinsLevel)}] - Cost: {coinStatus}",
                    Color.White);

                DrawUpgradeRow(spriteBatch, 2,
                    $"3. Dice Minigame Event - {(UpgradeOrbPermanent.UnlockDiceMinigame ? "UNLOCKED" : "Cost: 300 Coins")}",
                    UpgradeOrbPermanent.UnlockDiceMinigame ? Color.LightGreen : Color.White);
                string altarText = !UpgradeOrbPermanent.EnergyAltarUnlocked
                    ? "Cost: 500 Coins"
                    : UpgradeOrbPermanent.CanUseEnergyAltar() ? "READY" : "RECHARGING (24h)";
                DrawUpgradeRow(spriteBatch, 3, $"4. Energy Altar - {altarText}", Color.White);
                DrawUpgradeRow(spriteBatch, 4,
                    $"5. Unlock Charms - {(UpgradeOrbPermanent.CharmsUnlocked ? "UNLOCKED" : "Cost: 2500 Coins")}",
                    UpgradeOrbPermanent.CharmsUnlocked ? Color.LightGreen : Color.White);

                string nightmareText = UpgradeOrbPermanent.NightmareModeUnlocked
                    ? "UNLOCKED"
                    : $"Cost: 1000 Coins; requires 10 cleared Depths ({UpgradeOrbPermanent.TotalDepthsCleared}/10)";
                DrawUpgradeRow(spriteBatch, 5, $"6. Unlock Nightmare Mode - {nightmareText}",
                    UpgradeOrbPermanent.NightmareModeUnlocked ? Color.Crimson : Color.White);
            }

            Rectangle backRect = new Rectangle(810, 920, 300, 50);
            bool isHovered = backRect.Contains(_currentMouse.Position);
            SceneUi.DrawButton(spriteBatch, backRect, "< BACK TO MENU >", isHovered);
        }

        private void DrawUpgradeRow(SpriteBatch spriteBatch, int row, string text, Color color)
        {
            Rectangle bounds = UpgradeRowBounds(row);
            bool hovered = bounds.Contains(_currentMouse.Position);
            SceneUi.DrawPanel(spriteBatch, bounds, hovered ? Color.Gold : Color.SlateGray);
            SpriteFont font = GlobalContentManager.DefaultFont;
            Vector2 size = font.MeasureString(text);
            float scale = Math.Min(1f, (bounds.Width - 36f) / Math.Max(1f, size.X));
            spriteBatch.DrawString(
                font,
                text,
                new Vector2(bounds.X + 18, bounds.Y + (bounds.Height - size.Y * scale) / 2f),
                hovered ? Color.Gold : color,
                0f,
                Vector2.Zero,
                scale,
                SpriteEffects.None,
                0f);
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
