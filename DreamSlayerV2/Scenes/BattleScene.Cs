using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using DreamSlayerV2.Core;
using DreamSlayerV2.Scenes;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;

namespace DreamSlayerV2.Scenes
{
    public class BattleScene : IScene
    {
        private SpriteFont _font;
        private Texture2D EnemyTexture;
        private Texture2D[] BgTexture = new Texture2D[3];
        private Texture2D CardTexture;
        private SoundEffect[] DrawcardEffect;
        private Texture2D ScreenTexture;
        private float _redFlashAlpha = 0f;
        private float _purpleFlashAlpha = 0f;
        private List<ScaryFloatingText> _scaryTexts = new List<ScaryFloatingText>();
        private float _scarySpawnTimer = 0f;
        private Random _scaryRnd = new Random();
        private Texture2D _pixelTexture;
        
        // UI Texture list (0: Sanity_bar, 1: Sheid_bar, 2: Enemies_health_bar)
        private Texture2D[] UITexture = new Texture2D[3];
        private Texture2D Healthbar;
        private Texture2D BlankHealthBar;
        private Texture2D HealthEffect;
        
        // Audio Storage
        private List<SoundEffect> _soundEffects = new List<SoundEffect>();
        private SoundEffectInstance _debuffSoundInstance;

        // Combat Stats
        private readonly BaseEnemy _enemy;
        private readonly bool isBossRoom;
        private bool _initialized;
        private bool _contentLoaded;

        // Input States
        private KeyboardState currentKeyboardState;
        private KeyboardState _previousKeyboardState;
        private MouseState currentMouseState;
        private MouseState _previousMouseState;

        // Player instance
        private PlayerController _player;
        private MapNode targetNode;

        public BattleScene(bool bossRoom = false, MapNode nodeToComplete = null)
        {
            isBossRoom = bossRoom;
            _enemy = nodeToComplete != null && nodeToComplete.Type == NodeType.Boss
                ? new BossBattle()
                : new MonsterBattle(bossRoom);
            targetNode = nodeToComplete;
        }

        public void Initialize()
        {
            if (_initialized) return;

            _player = DreamSlayerV2.Core.GameState.CurrentPlayer ?? new PlayerController(CharacterClassV2.Human);
            _player.PlayerTurn();
            _enemy.RollNextAction(_player.Sanity < 30);
            _initialized = true;
        }

        public void LoadContent(ContentManager content)
        {
            if (_contentLoaded) return;
            var graphicsDevice = ((IGraphicsDeviceService)content.ServiceProvider.GetService(typeof(IGraphicsDeviceService))).GraphicsDevice;

            ScreenTexture = new Texture2D(graphicsDevice, 1, 1);
            ScreenTexture.SetData(new[] { Color.White });
            _pixelTexture = ScreenTexture;
            _font = content.Load<SpriteFont>("BattleUIFont");   
            EnemyTexture = content.Load<Texture2D>("UnrealWolf");
            BgTexture[0] = content.Load<Texture2D>("HorrorForestDreamV2");
            BgTexture[1] = content.Load<Texture2D>("HorrorForestDreamV2");
            BgTexture[2] = content.Load<Texture2D>("TheDarkCave");
            CardTexture = content.Load<Texture2D>("All16Card (1)");
            Healthbar = content.Load<Texture2D>("HealthBar");
            BlankHealthBar = content.Load<Texture2D>("HealthBar");

            // โหลด UI Textures ตามลำดับ
            UITexture[0] = content.Load<Texture2D>("Sanity_bar");       //[cite: 16]
            UITexture[1] = content.Load<Texture2D>("Sheid_bar");     //IncludingEffect   //[cite: 15]
            UITexture[2] = content.Load<Texture2D>("Enemies_health_bar"); //[cite: 14]
            HealthEffect = content.Load<Texture2D>("HealthEffect"); //Including Effect //

            // Load SFX (.wav)
            SoundEffect badEffect = content.Load<SoundEffect>("Bad_Effect_gain");
            _soundEffects.Add(badEffect);

            _debuffSoundInstance = badEffect.CreateInstance();
            _debuffSoundInstance.Volume = 1.0f;
            _debuffSoundInstance.Pitch = -0.509f;
            _contentLoaded = true;
        }

        public void Update(GameTime gameTime)
        {
            float ft = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (_redFlashAlpha > 0f)
            {
                _redFlashAlpha -= ft * 2.0f;
                if (_redFlashAlpha < 0f) _redFlashAlpha = 0f;
            }

            if (_purpleFlashAlpha > 0f)
            {
                _purpleFlashAlpha -= ft * 2.0f;
                if (_purpleFlashAlpha < 0f) _purpleFlashAlpha = 0f;
            }

            _previousKeyboardState = currentKeyboardState;
            currentKeyboardState = Keyboard.GetState();

            _previousMouseState = currentMouseState;
            currentMouseState = Mouse.GetState();

            if (currentKeyboardState.IsKeyDown(Keys.I))
            {
                SceneManager.ChangeScene(new TitleScene());
                return;
            }

            if (_enemy is BossBattle boss && boss.CurrentPhase == 1 && boss.HP <= 0)
            {
                boss.TransitionToPhase2();
                boss.RollNextAction(_player.Sanity < 30);
                SceneManager.ChangeScene(new BossScene(_player, this));
                return;
            }

            if (_enemy.HP <= 0)
            {
                bool isFinalBossNode = targetNode != null &&
                                       targetNode.Name == "13-Boss" &&
                                       targetNode.Type == NodeType.Boss;

                if (isFinalBossNode)
                {
                    SceneManager.ChangeScene(new GameOverScene(5, 3, 24, true, false, 6));
                }
                else
                {
                    int rewardTier = (targetNode != null && targetNode.Type == NodeType.Elite) ? 1 : 0;
                    SceneManager.ChangeScene(new RewardScene(rewardTier));
                }
                return;
            }

            if (_player.PlayerHP <= 0 || _player.Sanity <= 0)
            {
                SceneManager.ChangeScene(new GameOverScene(0, 0, 0, false, true, 0));
                return;
            }

            bool isMouseClicked = currentMouseState.LeftButton == ButtonState.Pressed &&
                                  _previousMouseState.LeftButton == ButtonState.Released;

            Keys[] cardKeys = { Keys.D1, Keys.D2, Keys.D3, Keys.D4, Keys.D5 };
            float startX = 500;
            int cardWidth = 140;
            int cardHeight = 200;

            for (int i = 0; i < _player.Hand.Length; i++)
            {
                if (_player.Hand[i] == null) continue;

                bool keyPressed = currentKeyboardState.IsKeyDown(cardKeys[i]) && _previousKeyboardState.IsKeyUp(cardKeys[i]);

                float cardX = startX + (i * 200);
                float cardY = 800;
                Rectangle cardBounds = new Rectangle((int)cardX, (int)cardY, cardWidth, cardHeight);
                bool mouseClickedCard = isMouseClicked && cardBounds.Contains(currentMouseState.Position);

                if (keyPressed || mouseClickedCard)
                {
                    TryPlayCard(i);
                }
            }

            Rectangle endTurnButtonBounds = new Rectangle(1500, 890, 300, 50);
            bool endTurnClicked = isMouseClicked && endTurnButtonBounds.Contains(currentMouseState.Position);
            bool endTurnKeyPressed = currentKeyboardState.IsKeyDown(Keys.E) && _previousKeyboardState.IsKeyUp(Keys.E);

            if (endTurnKeyPressed || endTurnClicked) CompleteTurn();

            if (currentKeyboardState.IsKeyDown(Keys.P) && _previousKeyboardState.IsKeyUp(Keys.P))
            {
                SceneManager.ChangeScene(new PlayerStatsScene(_player, this));
                return;
            }

            if (_player.Sanity < 30)
            {
                float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
                _scarySpawnTimer += dt;

                if (_scaryTexts.Count < 4 && _scarySpawnTimer > 1.2f)
                {
                    _scarySpawnTimer = 0f;
                    SpawnScaryText();
                }

                for (int i = _scaryTexts.Count - 1; i >= 0; i--)
                {
                    var item = _scaryTexts[i];
                    item.Position += item.Velocity * dt;
                    item.LifeSpan -= dt;

                    if (item.LifeSpan < 1.0f)
                    {
                        item.Alpha = item.LifeSpan;
                    }

                    if (item.LifeSpan <= 0f)
                    {
                        _scaryTexts.RemoveAt(i);
                    }
                }
            }
            else
            {
                _scaryTexts.Clear();
            }
        }

        private void TryPlayCard(int cardIndex)
        {
            var selectedCard = _player.Hand[cardIndex];
            if (selectedCard == null) return;

            if (selectedCard.IsUnplayable) return;

            int sanityCost = selectedCard.Cost * 10;

            if (LeavesTwentySanity(selectedCard))
            {
                if (selectedCard.Name == "Mighty Defend Attack")
                {
                    ApplyDamageToEnemy(_player.Defense);
                }
                else
                {
                    ApplyDamageToEnemy(50);
                }

                _player.Sanity = 20;
                _player.UseCard(cardIndex);
                return;
            }

            if (_player.Sanity >= sanityCost)
            {
                _player.Sanity -= sanityCost;

                switch (selectedCard.Name)
                {
                    case "Strike":
                        ApplyDamageToEnemy(selectedCard.InHandDamage);
                        break;

                    case "Piece":
                        ApplyDamageToEnemy(8);
                        break;

                    case "All in One":
                        int strikeCount = 1;
                        for (int i = 0; i < _player.Hand.Length; i++)
                        {
                            if (_player.Hand[i] != null && _player.Hand[i].Name == "Strike")
                            {
                                strikeCount++;
                                _player.UseCard(i);
                            }
                        }
                        ApplyDamageToEnemy(6 * strikeCount);
                        break;

                    case "Defend":
                        _player.Defense += 6;
                        break;

                    case "No Fight":
                        int defendCount = 1;
                        for (int i = 0; i < _player.Hand.Length; i++)
                        {
                            if (_player.Hand[i] != null && _player.Hand[i].Name == "Defend")
                            {
                                defendCount++;
                                _player.UseCard(i);
                            }
                        }
                        _player.Defense += (6 * defendCount);
                        break;

                    case "Mirror Shield":
                        _player.Defense = (_player.Defense * 2) + 10;
                        break;

                    case "Inspection":
                        _player.DrawCard();
                        break;

                    case "Powerup":
                        break;

                    case "Restore":
                    case "Heal":
                        _player.PlayerHP = Math.Min(_player.MaxHP, _player.PlayerHP + 8);
                        break;

                    case "Focus":
                        _player.Sanity = Math.Min(100, _player.Sanity + 20);
                        _player.Hand[cardIndex] = null; 
                        return;

                    case "Aura Heal":
                        _player.Sanity = Math.Min(100, _player.Sanity + 30);
                        _player.PlayerHP = Math.Min(_player.MaxHP, _player.PlayerHP + 15);
                        _player.Hand[cardIndex] = null;
                        return;

                    default:
                        if (selectedCard.Type == CardManager.CardType.Attack)
                        {
                            ApplyDamageToEnemy(selectedCard.InHandDamage);
                        }
                        else if (selectedCard.Type == CardManager.CardType.Defense)
                        {
                            _player.Defense += 6;
                        }
                        break;
                }

                if (_player.Hand[cardIndex] != null)
                {
                    _player.UseCard(cardIndex);
                }
            }
        }

        private static bool LeavesTwentySanity(CardManager card)
        {
            return card.Name == "Omega" || card.Name == "Mighty Defend Attack";
        }

        private void ApplyDamageToEnemy(int damage)
        {
            if (_enemy.Defense > 0)
            {
                if (damage <= _enemy.Defense)
                {
                    _enemy.Defense -= damage;
                }
                else
                {
                    damage -= _enemy.Defense;
                    _enemy.Defense = 0;
                    _enemy.HP -= damage;
                }
            }
            else
            {
                _enemy.HP -= damage;
            }
        }

        private void CompleteTurn()
        {
            _enemy.Defense = 0;

            foreach (EnemyAction action in _enemy.NextActions)
            {
                if (action.Type == EnemyActionType.Attack)
                {
                    _redFlashAlpha = 0.55f;
                }
                else if (action.Type == EnemyActionType.EldritchDebuff)
                {
                    _purpleFlashAlpha = 0.65f;
                    _debuffSoundInstance.Volume = 1.0f;
                    _debuffSoundInstance.Pitch = -0.509f;
                    _debuffSoundInstance.Play();
                }
            }

            _enemy.ExecuteTurn(_player);

            _player.EndTurn();
            _player.PlayerTurn();
            _enemy.RollNextAction(_player.Sanity < 30); 
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin();

            // 1. Background
            int bgIndex = isBossRoom ? 2 : 1;
            spriteBatch.Draw(BgTexture[bgIndex], new Rectangle(0, 0, 1920, 1080), Color.White);
            int MoveY = (int)(Math.Sin(DateTime.Now.TimeOfDay.TotalSeconds * 3.5) * 10);

            // 2. Enemy & Intent Display
            spriteBatch.Draw(EnemyTexture, new Rectangle(1200, 300 + MoveY, 300, 300), Color.White);

            // --- RENDER ENEMY HEALTH BAR (Enemies_health_bar.png) ---
            if (UITexture[2] != null)
            {
                float enemyBarX = 1200f;
                float enemyBarY = 240f;
                float enemyBarWidth = 300f;
                int enemyBarHeight = 22;

                // พื้นหลังหลอดเลือดศัตรู
                spriteBatch.Draw(UITexture[2], new Rectangle((int)enemyBarX, (int)enemyBarY, (int)enemyBarWidth, enemyBarHeight), Color.DarkGray * 0.5f);

                // สัดส่วนเลือดศัตรู
                float enemyHpPercent = MathHelper.Clamp((float)_enemy.HP / _enemy.MaxHP, 0f, 1f);
                int enemyFilledWidth = (int)(enemyBarWidth * enemyHpPercent);
                Rectangle enemySourceRect = new Rectangle(0, 0, (int)(UITexture[2].Width * enemyHpPercent), UITexture[2].Height);
                Rectangle enemyDestRect = new Rectangle((int)enemyBarX, (int)enemyBarY, enemyFilledWidth, enemyBarHeight);

                spriteBatch.Draw(UITexture[2], enemyDestRect, enemySourceRect, Color.White);

                // ข้อความเลือดศัตรูตรงกลางหลอด
                string enemyHpText = $"HP: {_enemy.HP} / {_enemy.MaxHP}";
                if (_enemy.Defense > 0) enemyHpText += $" (+{_enemy.Defense} DEF)";
                Vector2 enemyTextSize = _font.MeasureString(enemyHpText);
                Vector2 enemyTextPos = new Vector2(
                    enemyBarX + (enemyBarWidth - enemyTextSize.X) / 2f,
                    enemyBarY + (enemyBarHeight - enemyTextSize.Y) / 2f
                );
                spriteBatch.DrawString(_font, enemyHpText, enemyTextPos + Vector2.One, Color.Black);
                spriteBatch.DrawString(_font, enemyHpText, enemyTextPos, Color.Red);
            }

            string intentText = "INTENT: ";
            Color intentColor = Color.Yellow;
            for (int i = 0; i < _enemy.NextActions.Count; i++)
            {
                EnemyAction action = _enemy.NextActions[i];
                if (i > 0) intentText += " + ";
                intentText += action.Type switch
                {
                    EnemyActionType.Attack => $"Attacking for {action.Value} DMG",
                    EnemyActionType.Defend => $"Gaining {action.Value} Defense",
                    EnemyActionType.EldritchDebuff => $"Eldritch Gaze (-{action.Value} Sanity)",
                    _ => "Doing nothing"
                };

                if (i == 0)
                {
                    intentColor = action.Type switch
                    {
                        EnemyActionType.Attack => Color.OrangeRed,
                        EnemyActionType.Defend => Color.LightBlue,
                        EnemyActionType.EldritchDebuff => Color.MediumPurple,
                        _ => Color.Yellow
                    };
                }
            }

            spriteBatch.DrawString(_font, intentText, new Vector2(1200, 190), intentColor);

            // 3. Player Stats UI (ปรับตำแหน่ง Shield ซ้าย, Health Bar กลาง, Sanity ขวา)
            
            if (UITexture[1] != null) // Sheid_bar.png
            {
                // ขนาดเดิม 45x45 คูณ 2 เป็น 90x90 พิกเซล
                Rectangle shieldIconRect = new Rectangle(400, 715, 90, 90); 
                spriteBatch.Draw(UITexture[1], shieldIconRect, Color.White);
                
                // ตัวเลข Defense (ปรับขนาดฟอนต์หรือตำแหน่งตามความเหมาะสม)
                string defenseText = $"{_player.Defense}";
                Vector2 defTextSize = _font.MeasureString(defenseText);
                Vector2 defTextPos = new Vector2(
                    shieldIconRect.Center.X - (defTextSize.X / 2f),
                    shieldIconRect.Center.Y - (defTextSize.Y / 2f)
                );
                spriteBatch.DrawString(_font, defenseText, defTextPos + Vector2.One, Color.Black);
                spriteBatch.DrawString(_font, defenseText, defTextPos, Color.White);
            }

            // --- B. LONG HEALTH BAR (ตรงกลาง) ---
            if (Healthbar != null)
            {
                float barX = 500f;
                float barY = 740f; 
                float barWidth = 940f;
                int barHeight = 22;

                if (BlankHealthBar != null)
                {
                    spriteBatch.Draw(BlankHealthBar, new Rectangle((int)barX, (int)barY, (int)barWidth, barHeight), Color.DarkGray * 0.8f);
                }

                float hpPercent = MathHelper.Clamp((float)_player.PlayerHP / _player.MaxHP, 0f, 1f);
                int filledWidth = (int)(barWidth * hpPercent);

                Rectangle sourceRectBar = new Rectangle(0, 0, (int)(Healthbar.Width * hpPercent), Healthbar.Height);
                Rectangle destRectBar = new Rectangle((int)barX, (int)barY, filledWidth, barHeight);

                spriteBatch.Draw(Healthbar, destRectBar, sourceRectBar, Color.White);

                string hpText = $"HP: {_player.PlayerHP} / {_player.MaxHP}";
                Vector2 textSize = _font.MeasureString(hpText);
                Vector2 textPos = new Vector2(
                    barX + (barWidth - textSize.X) / 2f,
                    barY + (barHeight - textSize.Y) / 2f
                );

                spriteBatch.DrawString(_font, hpText, textPos + Vector2.One, Color.Black);
                spriteBatch.DrawString(_font, hpText, textPos, Color.White);
            }

           if (UITexture[0] != null) // Sanity_bar.png
            {
                // ขนาดเดิม 48x48 คูณ 2 เป็น 96x96 พิกเซล
                Rectangle sanityIconRect = new Rectangle(1500, 710, 96, 96); 
                spriteBatch.Draw(UITexture[0], sanityIconRect, Color.White);

                // ตัวเลข Sanity ตรงกลางไอคอนวงกลม
                string sanityText = $"{_player.Sanity}";
                Vector2 sanityTextSize = _font.MeasureString(sanityText);
                Vector2 sanityTextPos = new Vector2(
                    sanityIconRect.Center.X - (sanityTextSize.X / 2f),
                    sanityIconRect.Center.Y - (sanityTextSize.Y / 2f)
                );
                spriteBatch.DrawString(_font, sanityText, sanityTextPos + Vector2.One, Color.Black);
                spriteBatch.DrawString(_font, sanityText, sanityTextPos, Color.Cyan);
            }

            // สถานะเตือน Sanity ต่ำ
            if (_player.Sanity <= 30)
            {
                spriteBatch.DrawString(_font, "Sanity is critically low!", new Vector2(810, 700), Color.Red);
            }
            else
            {
                spriteBatch.DrawString(_font, "Sanity is stable.", new Vector2(850, 700), Color.LightGreen);
            }

            // สถิติอื่นๆ ด้านบน (Energy, Deck, Discard)
            spriteBatch.DrawString(_font, $"Energy: {_player.Energy}", new Vector2(80, 80), Color.Yellow);
            spriteBatch.DrawString(_font, $"Deck: {_player.Deck.Count}  |  Discard: {_player.DiscardPile.Count}", new Vector2(80, 120), Color.LightGray);

            float startX = 500;
            int cardWidth = 140;
            int cardHeight = 200;
            CardManager hoveredCard = null;

            int sourceWidth = CardTexture.Width / 4;
            int sourceHeight = CardTexture.Height / 4;

            for (int i = 0; i < _player.Hand.Length; i++)
            {
                if (_player.Hand[i] != null)
                {
                    var card = _player.Hand[i];
                    float cardX = startX + (i * 200);
                    float cardY = 800;

                    Rectangle cardBounds = new Rectangle((int)cardX, (int)cardY, cardWidth, cardHeight);
                    bool isHovered = cardBounds.Contains(currentMouseState.Position);

                    if (isHovered)
                    {
                        hoveredCard = card;
                        cardY -= 25; 
                    }

                    int col = card.CardID % 4;
                    int row = card.CardID / 4;

                    Rectangle sourceRect = new Rectangle(
                        col * sourceWidth, 
                        row * sourceHeight, 
                        sourceWidth, 
                        sourceHeight
                    );

                    spriteBatch.Draw(CardTexture, new Rectangle((int)cardX, (int)cardY, cardWidth, cardHeight), sourceRect, Color.White);
                    
                    spriteBatch.DrawString(_font, $"[{i + 1}]", new Vector2(cardX + 10, cardY + 10), Color.Gold);
                    
                    bool leavesTwentySanity = LeavesTwentySanity(card);
                    spriteBatch.DrawString(_font, leavesTwentySanity ? "20" : $"{card.Cost * 10}", new Vector2(cardX + 10, cardY + 160), Color.Cyan);
                }
            }

            // 5. Tooltip Rendering
            if (hoveredCard != null && _pixelTexture != null)
            {
                string nameText = $"--- {hoveredCard.Name} ---";
                string descText = $"Description: {hoveredCard.Description}";
                string doesText = $"Effect: {hoveredCard.Does}";
                
                bool leavesTwentySanity = LeavesTwentySanity(hoveredCard);
                string costText = leavesTwentySanity 
                    ? "Sanity Cost: 20 (Special)" 
                    : $"Sanity Cost: {hoveredCard.Cost * 10}";

                string fullTooltipText = $"{nameText}\n{descText}\n{doesText}\n{costText}";

                Vector2 textSize = _font.MeasureString(fullTooltipText);
                
                int padding = 12;
                Vector2 tooltipPos = new Vector2(currentMouseState.X + 20, currentMouseState.Y - 100);

                if (tooltipPos.X + textSize.X + (padding * 2) > 1920)
                {
                    tooltipPos.X = 1920 - textSize.X - (padding * 2) - 10;
                }
                if (tooltipPos.Y < 10)
                {
                    tooltipPos.Y = 10;
                }

                Rectangle frameRect = new Rectangle(
                    (int)tooltipPos.X - padding,
                    (int)tooltipPos.Y - padding,
                    (int)textSize.X + (padding * 2),
                    (int)textSize.Y + (padding * 2)
                );

                spriteBatch.Draw(_pixelTexture, frameRect, Color.Black * 0.85f);
                
                float lineSpacing = _font.LineSpacing;
                Vector2 textDrawPos = tooltipPos;

                spriteBatch.DrawString(_font, nameText, textDrawPos, Color.Gold);
                textDrawPos.Y += lineSpacing;

                spriteBatch.DrawString(_font, descText, textDrawPos, Color.White);
                textDrawPos.Y += lineSpacing;

                spriteBatch.DrawString(_font, doesText, textDrawPos, Color.LightGreen);
                textDrawPos.Y += lineSpacing;

                spriteBatch.DrawString(_font, costText, textDrawPos, Color.Cyan);
            }

            // 6. End Turn Prompt/Button
            spriteBatch.DrawString(_font, "Click or Press [E] to End Turn", new Vector2(1500, 900), Color.Yellow);
            spriteBatch.DrawString(_font, "Press [P] to Pause / Player Stats", new Vector2(1500, 950), Color.LightBlue);

            // 7. Draw Scary Floating Texts if Sanity < 30
            if (_player.Sanity < 30)
            {
                foreach (var item in _scaryTexts)
                {
                    Color scaryColor = new Color(180, 0, 20) * item.Alpha;
                    spriteBatch.DrawString(_font, item.Text, item.Position, scaryColor);
                }
            }

            // 8. Draw Screen Flash Overlay (Red & Purple Flash)
            if (ScreenTexture != null)
            {
                if (_redFlashAlpha > 0f)
                {
                    spriteBatch.Draw(
                        ScreenTexture, 
                        new Rectangle(0, 0, 1920, 1080), 
                        Color.Red * _redFlashAlpha
                    );
                }

                if (_purpleFlashAlpha > 0f)
                {
                    spriteBatch.Draw(
                        ScreenTexture, 
                        new Rectangle(0, 0, 1920, 1080), 
                        Color.Purple * _purpleFlashAlpha
                    );
                }
            }
        
            spriteBatch.End();
        }

        private void SpawnScaryText()
        {
            string[] madnessQuotes = {
                "THEY ARE WATCHING",
                "CANNOT ESCAPE",
                "MIND COLLAPSING",
                "ENDLESS VOID",
                "MADNESS AWAITS",
                "WHO IS THERE?",
                "FORGET YOUR NAME",
                "DREAMS BLEEDING",
                "IT HURTS",
                "NO WAY OUT"
            };

            string selectedText = madnessQuotes[_scaryRnd.Next(madnessQuotes.Length)];

            bool movingRight = _scaryRnd.Next(2) == 0;
            float startX = movingRight ? -300f : 2100f;
            float startY = _scaryRnd.Next(150, 1000);
            float speedX = movingRight ? _scaryRnd.Next(250, 450) : _scaryRnd.Next(-450, -250);

            _scaryTexts.Add(new ScaryFloatingText
            {
                Text = selectedText,
                Position = new Vector2(startX, startY),
                Velocity = new Vector2(speedX, 0),
                LifeSpan = 5.0f
            });
        }

        public void UnloadContent() { }
    }

    public class ScaryFloatingText
    {
        public string Text { get; set; }
        public Vector2 Position { get; set; }
        public Vector2 Velocity { get; set; }
        public float Alpha { get; set; } = 1.0f;
        public float LifeSpan { get; set; } = 5.0f;
    }
}