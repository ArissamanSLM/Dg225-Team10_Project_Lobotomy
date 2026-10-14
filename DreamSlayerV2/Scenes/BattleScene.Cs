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

        private List<ScaryFloatingText> _scaryTexts = new List<ScaryFloatingText>();
        private float _scarySpawnTimer = 0f;
        private Random _scaryRnd = new Random();
        
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

    _font = content.Load<SpriteFont>("File");
    EnemyTexture = content.Load<Texture2D>("UnrealWolf");
    BgTexture[0] = content.Load<Texture2D>("HorrorForestDreamV2");
    BgTexture[1] = content.Load<Texture2D>("HorrorForestDreamV2");
    BgTexture[2] = content.Load<Texture2D>("TheDarkCave");
    CardTexture = content.Load<Texture2D>("All16Card");

    // Load SFX (.wav)
    SoundEffect badEffect = content.Load<SoundEffect>("Bad_Effect_gain");
    _soundEffects.Add(badEffect);

    // Create and configure the sound instance
    _debuffSoundInstance = badEffect.CreateInstance();
    _debuffSoundInstance.Volume = 1.0f;    // Max volume
    _debuffSoundInstance.Pitch = -0.509f;  // ~0.491x pitch / speed lowering
    _contentLoaded = true;
}
        public void Update(GameTime gameTime)
        {
            _previousKeyboardState = currentKeyboardState;
            currentKeyboardState = Keyboard.GetState();

            _previousMouseState = currentMouseState;
            currentMouseState = Mouse.GetState();

            // Press 'I' to return to Title
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

            // Check for Win Condition (Enemy HP <= 0)
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

            // Check Defeat Condition
            if (_player.PlayerHP <= 0 || _player.Sanity <= 0)
            {
                SceneManager.ChangeScene(new GameOverScene(0, 0, 0, false, true, 0));
                return;
            }

            bool isMouseClicked = currentMouseState.LeftButton == ButtonState.Pressed &&
                                  _previousMouseState.LeftButton == ButtonState.Released;

            // 1. PLAY CARDS (Keyboard 1-5 OR Left Mouse Click)
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

            // 2. END TURN (Keyboard [E] or Click End Turn Button)
            Rectangle endTurnButtonBounds = new Rectangle(1500, 890, 300, 50);
            bool endTurnClicked = isMouseClicked && endTurnButtonBounds.Contains(currentMouseState.Position);
            bool endTurnKeyPressed = currentKeyboardState.IsKeyDown(Keys.E) && _previousKeyboardState.IsKeyUp(Keys.E);

            if (endTurnKeyPressed || endTurnClicked) CompleteTurn();
            // Check if 'P' key was pressed to open the Pause Scene
            if (currentKeyboardState.IsKeyDown(Keys.P) && _previousKeyboardState.IsKeyUp(Keys.P))
            {
                // Passes the current active player instance and 'this' scene so it can resume seamlessly
                SceneManager.ChangeScene(new PlayerStatsScene(_player, this));
                return;
            }
            if (_player.Sanity < 30)
{
    float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
    _scarySpawnTimer += dt;

    // สุ่มเกิดข้อความใหม่ทุกๆ 1.2 วินาที (จำกัดไม่เกิน 4 ข้อความพร้อมกัน)
    if (_scaryTexts.Count < 4 && _scarySpawnTimer > 1.2f)
    {
        _scarySpawnTimer = 0f;
        SpawnScaryText();
    }

    // อัปเดตตำแหน่งและการจางหายของข้อความหลอน
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
    _scaryTexts.Clear(); // เคลียร์ข้อความทิ้งถ้า Sanity กลับมา >= 30
}
        }

        private void TryPlayCard(int cardIndex)
        {
            var selectedCard = _player.Hand[cardIndex];
            if (selectedCard == null) return;

            int sanityCost = selectedCard.Cost * 10;

            if (_player.Sanity >= sanityCost)
            {
                _player.Sanity -= sanityCost;

                if (selectedCard.Type == CardManager.CardType.Attack)
                {
                    int damage = selectedCard.InHandDamage;

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
                else if (selectedCard.Type == CardManager.CardType.Heal)
                {
                    int healAmount = 8;
                    _player.PlayerHP = Math.Min(_player.MaxHP, _player.PlayerHP + healAmount);
                }
                else if (selectedCard.Type == CardManager.CardType.Defense)
                {
                    int increaseDefense = 6;
                    _player.Defense += increaseDefense;
                }
                // else if (selectedCard.Type == CardManager.CardType.Utility)
                // {
                    
                // }

                _player.UseCard(cardIndex);
            }
        }

        private void CompleteTurn()
        {
            _enemy.Defense = 0;
            _enemy.ExecuteTurn(_player);

            foreach (EnemyAction action in _enemy.NextActions)
            {
                if (action.Type == EnemyActionType.EldritchDebuff)
                {
                    _debuffSoundInstance.Volume = 1.0f;
                    _debuffSoundInstance.Pitch = -0.509f;
                    _debuffSoundInstance.Play();
                    break;
                }
            }

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

            string enemyDefenseText = _enemy.Defense > 0 ? $" (+{_enemy.Defense} DEF)" : "";
            spriteBatch.DrawString(_font, $"Enemy HP: {_enemy.HP} / {_enemy.MaxHP}{enemyDefenseText}", new Vector2(1200, 240), Color.Red);

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

            // 3. Player Stats UI
            spriteBatch.DrawString(_font, $"HP: {_player.PlayerHP} / {_player.MaxHP}", new Vector2(80, 80), Color.White);
            spriteBatch.DrawString(_font, $"Defense: {_player.Defense}", new Vector2(80, 130), Color.SkyBlue);
            spriteBatch.DrawString(_font, $"Sanity: {_player.Sanity}", new Vector2(80, 180), Color.Cyan);
            if (_player.Sanity <= 30)
            {
                spriteBatch.DrawString(_font, "Sanity is critically low!", new Vector2(200, 180), Color.Red);
            }
            else
            {
                spriteBatch.DrawString(_font, "Sanity is stable.", new Vector2(200, 180), Color.LightGreen);
            }
            spriteBatch.DrawString(_font, $"Energy: {_player.Energy}", new Vector2(80, 230), Color.Yellow);
            spriteBatch.DrawString(_font, $"Deck: {_player.Deck.Count}  |  Discard: {_player.DiscardPile.Count}", new Vector2(80, 280), Color.LightGray);

            // 4. Cards in Hand
            float startX = 500;
            int cardWidth = 140;
            int cardHeight = 200;
            CardManager hoveredCard = null;

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

                    int textureIndex = card.Type switch
                    {
                        CardManager.CardType.Attack => 0,
                        CardManager.CardType.Defense => 1,
                        CardManager.CardType.Utility => 2,
                        CardManager.CardType.Heal => 3,
                        _ => 3
                    };

                    int sourceWidth = CardTexture.Width / 4;
                    Rectangle sourceRect = new Rectangle(textureIndex * sourceWidth, 0, sourceWidth, CardTexture.Height / 4);

                    spriteBatch.Draw(CardTexture, new Rectangle((int)cardX, (int)cardY, cardWidth, cardHeight), sourceRect, Color.White);
                    spriteBatch.DrawString(_font, $"[{i + 1}]", new Vector2(cardX + 10, cardY + 10), Color.Gold);
                }
            }

            // 5. Tooltip Rendering
            if (hoveredCard != null)
            {
                Vector2 tooltipPos = new Vector2(currentMouseState.X + 20, currentMouseState.Y - 80);
                spriteBatch.DrawString(_font,
                    $"--- {hoveredCard.Name} ---\n" +
                    $"Description: {hoveredCard.Description}\n" +
                    $"Effect: {hoveredCard.Does}\n" +
                    $"Sanity Cost: {hoveredCard.Cost * 10}",
                    tooltipPos, Color.Gold);
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
    float startY = _scaryRnd.Next(150, 1000); // สุ่มความสูงในจอ
    // ความเร็วสูงและสั่นประสาท
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
    public float Alpha { get; set; } = 1.0f; // แก้ไขตรงนี้ให้ถูกต้อง
    public float LifeSpan { get; set; } = 5.0f;
}
}