using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using DreamSlayerV2.Core;
using DreamSlayerV2.Managers;
using DreamSlayerV2.Scenes;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;

namespace DreamSlayerV2.Scenes
{
    public class BattleScene : IScene
    {
        private float _redFlashAlpha = 0f;
        private float _purpleFlashAlpha = 0f;
        private List<ScaryFloatingText> _scaryTexts = new List<ScaryFloatingText>();
        private float _scarySpawnTimer = 0f;
        private Random _scaryRnd = new Random();
        private SoundEffectInstance _debuffSoundInstance;
        private float _enemyFlashTimer = 0f;
private float _healEffectTimer = 0f;
private float _shieldEffectTimer = 0f;

        // Combat Stats
        private readonly BaseEnemy _enemy;
        private readonly bool isBossRoom;
        private bool _initialized;
        private bool _contentLoaded;
        private bool _alphaLocked;
        private int _baseAttackDamageBonus;
        private int _oneTurnPowerupBonus;
        private readonly List<int> _decayingPowerupBonuses = new List<int>();
        private bool _clockArmed;
        private float _clockBonusMultiplier;
        private int _clockStoredDamage;
        private int _damageDealtThisTurn;
        private bool _isReleasingClockDamage;

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
                ? new BossBattle(GameState.CurrentDepth, GameState.CurrentPlayer.IsNightmareMode)
                : new MonsterBattle(bossRoom, GameState.CurrentDepth, GameState.CurrentPlayer.IsNightmareMode);
            targetNode = nodeToComplete;
        }

        public bool IsBossRoom => isBossRoom;
        public bool IsEliteBattle => targetNode?.Type == NodeType.Elite;

        public void Initialize()
        {
            if (_initialized) return;

            _player = DreamSlayerV2.Core.GameState.CurrentPlayer ?? new PlayerController(CharacterClassV2.Human);
            _baseAttackDamageBonus = _player.IncreaseInHandDamageValue;
            _player.StartEncounterTurn();
            if (_player.Hand[0] != null)
            {
                GlobalContentManager.DrawCardsSound.Play();
            }
            _enemy.RollNextAction(_player.Sanity < 30);
            _initialized = true;
        }

        public void LoadContent()
        {
            if (_contentLoaded) return;
            if (!isBossRoom && targetNode == null)
            {
                MusicManager.PlayLoopingMusic(GlobalContentManager.NodeMusic);
            }
            _debuffSoundInstance = GlobalContentManager.BadEffectSound.CreateInstance();
            _debuffSoundInstance.Volume = 1.0f;
            _debuffSoundInstance.Pitch = -0.509f;
            _contentLoaded = true;
        }

        public void Update(GameTime gameTime)
        {
            float ft = (float)gameTime.ElapsedGameTime.TotalSeconds;
            

// Decrement effect timers
if (_enemyFlashTimer > 0f) _enemyFlashTimer -= ft;
if (_healEffectTimer > 0f) _healEffectTimer -= ft;
if (_shieldEffectTimer > 0f) _shieldEffectTimer -= ft;
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
                ClearPowerupEffects();
                GameState.RecordEnemyDefeated();
                bool isBossNode = targetNode?.Type == NodeType.Boss;
                if (isBossNode) GameState.CompleteDepth();

                int rewardTier = isBossNode ? 2 : targetNode?.Type == NodeType.Elite ? 1 : 0;
                SceneManager.ChangeScene(new RewardScene(rewardTier, _player.IsNightmareMode, GameState.CurrentDepth));
                return;
            }

            if (_player.PlayerHP <= 0 || _player.Sanity <= 0)
            {
                ClearPowerupEffects();
                SceneManager.ChangeScene(new GameOverScene(false, true));
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
        private void PlayCardSound()
        {
            GlobalContentManager.PlayCardSound.Play();
        }

        private void PlayCardEffectSound(bool isAttack)
        {
            var sound = isAttack ? GlobalContentManager.AttackSound : GlobalContentManager.ShieldBlockSound;
            sound.Play();
        }
        private void TryPlayCard(int cardIndex)
        {
            var selectedCard = _player.Hand[cardIndex];
            if (selectedCard == null) return;

            if (_alphaLocked && selectedCard.Name == "Alpha") return;
            if (selectedCard.Name.EndsWith("+", StringComparison.Ordinal))
            {
                TryPlayPlusCard(cardIndex);
                return;
            }
            if (selectedCard.IsUnplayable) return;

            int sanityCost = selectedCard.Cost * 10;

            if (LeavesTwentySanity(selectedCard))
            {
                GameState.RecordCardPlayed();
                PlayCardSound();
                PlayCardEffectSound(isAttack: true);

                if (selectedCard.Name == "Mighty Defend Attack")
                {
                    
                    ApplyDamageToEnemy(_player.Defense);
                    _enemyFlashTimer = 0.4f;
                }
                else
                {
                    if (selectedCard.Name == "Omega")
                    {
                        _alphaLocked = true;
                    }

                    ApplyDamageToEnemy(50);
                    _enemyFlashTimer = 0.4f;
                }

                _player.Sanity = 20;
                if (selectedCard.Name == "Omega")
                {
                    _player.Hand[cardIndex] = null;
                }
                else
                {
                    _player.UseCard(cardIndex);
                }
                return;
            }

            if (_player.Sanity >= sanityCost)
            {
                GameState.RecordCardPlayed();
                _player.Sanity -= sanityCost;
                PlayCardSound();

                switch (selectedCard.Name)
                {
                    case "Alpha":
                        _player.TryAddCard(new CardManager(0, "Beta", 0, CardManager.CardType.Special), out _);
                        _player.UseCard(cardIndex);
                        GlobalContentManager.DrawCardSound.Play();
                        return;

                    case "Beta":
                        _player.Sanity = Math.Min(100, _player.Sanity + 100);
                        _player.TryAddCard(new CardManager(0, "Omega", 0, CardManager.CardType.Special), out _);
                        _player.Hand[cardIndex] = null;
                        _healEffectTimer = 0.4f;
                        GlobalContentManager.HealSound.Play();
                        GlobalContentManager.DrawCardSound.Play();
                        return;

                    case "Strike":
                        ApplyDamageToEnemy(GetAttackDamage(selectedCard, selectedCard.InHandDamage));
                        _enemyFlashTimer = 0.4f;
                        PlayCardEffectSound(isAttack: true);
                        break;

                    case "Piece":
                        ApplyDamageToEnemy(GetAttackDamage(selectedCard, 8));
                        _enemyFlashTimer = 0.4f;
                        PlayCardEffectSound(isAttack: true);
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
                        _enemyFlashTimer = 0.4f;
                        PlayCardEffectSound(isAttack: true);
                        ApplyDamageToEnemy(GetAttackDamage(selectedCard, 6) * strikeCount);
                        break;

                    case "Defend":
                    _shieldEffectTimer = 0.4f;
                    PlayCardEffectSound(isAttack: false);
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
                        _shieldEffectTimer = 0.4f;
                        PlayCardEffectSound(isAttack: false);
                        _player.Defense += (6 * defendCount);
                        break;

                    case "Mirror Shield":
                        _player.Defense = (_player.Defense * 2) + 10;
                        _shieldEffectTimer = 0.4f;
                        PlayCardEffectSound(isAttack: false);
                        break;

                    case "Inspection":
                        bool drewCard = _player.Deck.Count > 0;
                        _player.DrawCard();
                        if (drewCard) GlobalContentManager.DrawCardSound.Play();
                        break;

                    case "Powerup":
                        _oneTurnPowerupBonus++;
                        UpdatePowerupDamageBonus();
                        break;

                    case "Clock":
                        _clockArmed = true;
                        _clockBonusMultiplier = 0.2f;
                        _clockStoredDamage = _damageDealtThisTurn;
                        break;

                    case "Restore":
                    case "Heal":
                        _player.PlayerHP = Math.Min(_player.MaxHP, _player.PlayerHP + 8);
                        _healEffectTimer = 0.4f;
                        GlobalContentManager.HealSound.Play();
                        break;

                    case "Focus":
                        _player.Sanity = Math.Min(100, _player.Sanity + 20);
                        _healEffectTimer = 0.4f;
                        GlobalContentManager.HealSound.Play();
                        _player.Hand[cardIndex] = null; 
                        return;

                    case "Aura Heal":
                        _player.Sanity = Math.Min(100, _player.Sanity + 30);
                        _player.PlayerHP = Math.Min(_player.MaxHP, _player.PlayerHP + 15);
                        _healEffectTimer = 0.4f;
                        GlobalContentManager.HealSound.Play();
                        _player.Hand[cardIndex] = null;
                        return;

                    default:
                        if (selectedCard.Type == CardManager.CardType.Attack)
                        {
                            ApplyDamageToEnemy(GetAttackDamage(selectedCard, selectedCard.InHandDamage));
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
        private void TryPlayPlusCard(int cardIndex)
{
    var selectedCard = _player.Hand[cardIndex];
    if (selectedCard == null) return;

    if (selectedCard.IsUnplayable) return;

    // ตรวจสอบเงื่อนไขราคา Sanity (บางการ์ด Plus เช่น Inspection+ หรือ Focus+ อาจใช้ 0 Sanity)
    int sanityCost = selectedCard.Cost * 10;

    // กรณีการ์ดพิเศษที่เหลือ Sanity ไว้ 35 (เช่น Mighty Defend Attack+ หรือ Omega+)
    if (LeavesThirtyFiveSanity(selectedCard))
    {
        GameState.RecordCardPlayed();
        PlayCardSound();
        if (selectedCard.Name == "Mighty Defend Attack+")
        {
            ApplyDamageToEnemy((int)(_player.Defense * 1.2f)); // ตัวคูณ 1.2 ตามบาลานซ์ใหม่
            PlayCardEffectSound(isAttack: true);
        }
        else if (selectedCard.Name == "Omega+")
        {
            ApplyDamageToEnemy(65);
            PlayCardEffectSound(isAttack: true);
            // เพิ่มเอฟเฟกต์พิเศษ: ทำให้การ์ดถัดไปทำผลลัพธ์เป็น 2 เท่า
        }

        _player.Sanity = 35;
        _player.UseCard(cardIndex);
        return;
    }

    if (_player.Sanity >= sanityCost)
    {
        GameState.RecordCardPlayed();
        _player.Sanity -= sanityCost;
        PlayCardSound();

        switch (selectedCard.Name)
        {
            case "Strike+":
                ApplyDamageToEnemy(GetAttackDamage(selectedCard, 9));
                _enemyFlashTimer = 0.4f;
                PlayCardEffectSound(isAttack: true);// ดาเมจตามบาลานซ์ใหม่
                break;

            case "Piece+":
                ApplyDamageToEnemy(GetAttackDamage(selectedCard, 10));
                _enemyFlashTimer = 0.4f;
                PlayCardEffectSound(isAttack: true);// เพิ่มระบบ Status Effect (Valuable) ตามต้องการ
                break;

            case "All in One+":
                int strikeCount = 1;
                for (int i = 0; i < _player.Hand.Length; i++)
                {
                    if (_player.Hand[i] != null && (_player.Hand[i].Name == "Strike" || _player.Hand[i].Name == "Strike+"))
                    {
                        strikeCount++;
                        _player.UseCard(i);
                    }
                }
                _enemyFlashTimer = 0.4f;
                PlayCardEffectSound(isAttack: true);
                ApplyDamageToEnemy(GetAttackDamage(selectedCard, 8) * strikeCount); // 8 damage per attack card
                break;

            case "Defend+":
                _player.Defense += 10; // เกราะ 10
                _shieldEffectTimer = 0.4f;
                PlayCardEffectSound(isAttack: false);
                break;

            case "No Fight+":
                int defendCount = 1;
                for (int i = 0; i < _player.Hand.Length; i++)
                {
                    if (_player.Hand[i] != null && (_player.Hand[i].Name == "Defend" || _player.Hand[i].Name == "Defend+"))
                    {
                        defendCount++;
                        _player.UseCard(i);
                    }
                }
                _shieldEffectTimer = 0.4f;
                PlayCardEffectSound(isAttack: false);
                _player.Defense += (10 * defendCount);
                break;

            case "Mirror Shield+":
                _player.Defense = (_player.Defense * 2) + 20; // โบนัสเกราะ 20
                _shieldEffectTimer = 0.4f;
                PlayCardEffectSound(isAttack: false);
                break;

            case "Inspection+":
                bool drewCards = _player.Deck.Count > 0;
                _player.DrawCards(3); // จั่ว 3 ใบฟรี Cost 0
                if (drewCards) GlobalContentManager.DrawCardsSound.Play();
                break;

            case "Powerup+":
                _decayingPowerupBonuses.Add(3);
                UpdatePowerupDamageBonus();
                break;

            case "Clock+":
                _clockArmed = true;
                _clockBonusMultiplier = 0.5f;
                _clockStoredDamage = _damageDealtThisTurn;
                break;

            case "Restore+":
            case "Heal+":
                _player.PlayerHP = Math.Min(_player.MaxHP, _player.PlayerHP + 12);
                _player.Sanity = Math.Min(100, _player.Sanity + 10); // ฟื้นทั้ง HP และ Sanity
                _healEffectTimer = 0.4f;
                GlobalContentManager.HealSound.Play();
                break;

            case "Focus+":
                _player.Sanity = Math.Min(100, _player.Sanity + 35);
                bool drewCard = _player.Deck.Count > 0;
                _player.DrawCard();
                if (drewCard) GlobalContentManager.DrawCardSound.Play();
                _healEffectTimer = 0.4f;
                GlobalContentManager.HealSound.Play();
                _player.Hand[cardIndex] = null; 
                return;

            case "Aura Heal+":
                _player.Sanity = Math.Min(100, _player.Sanity + 50);
                _player.PlayerHP = Math.Min(_player.MaxHP, _player.MaxHP + 25);
                _healEffectTimer = 0.4f;
                GlobalContentManager.HealSound.Play();
                _player.Hand[cardIndex] = null;
                return;

            default:
                if (selectedCard.Type == CardManager.CardType.Attack)
                {
                    ApplyDamageToEnemy(GetAttackDamage(selectedCard, selectedCard.InHandDamage));
                }
                else if (selectedCard.Type == CardManager.CardType.Defense)
                {
                    _player.Defense += 10;
                }
                break;
        }

        if (_player.Hand[cardIndex] != null)
        {
            _player.UseCard(cardIndex);
        }
    }
}

// ฟังก์ชันเช็คการ์ด Plus ที่ใช้กลไกเหลือ Sanity ไว้ 35
private static bool LeavesThirtyFiveSanity(CardManager card)
{
    return card.Name == "Omega+" || card.Name == "Mighty Defend Attack+";
}

        private static bool LeavesTwentySanity(CardManager card)
        {
            return card.Name == "Omega" || card.Name == "Mighty Defend Attack";
        }

        private int GetAttackDamage(CardManager card, int baseDamage)
        {
            return card.Type == CardManager.CardType.Attack
                ? baseDamage + _player.IncreaseInHandDamageValue
                : baseDamage;
        }

        private void UpdatePowerupDamageBonus()
        {
            int decayingBonus = 0;
            foreach (int bonus in _decayingPowerupBonuses)
            {
                decayingBonus += bonus;
            }

            _player.IncreaseInHandDamageValue = _baseAttackDamageBonus + _oneTurnPowerupBonus + decayingBonus;
        }

        private void ClearPowerupEffects()
        {
            _oneTurnPowerupBonus = 0;
            _decayingPowerupBonuses.Clear();
            UpdatePowerupDamageBonus();
        }

        private void AdvancePowerupEffects()
        {
            _oneTurnPowerupBonus = 0;

            for (int i = _decayingPowerupBonuses.Count - 1; i >= 0; i--)
            {
                int nextBonus = _decayingPowerupBonuses[i] - 1;
                if (nextBonus <= 0)
                {
                    _decayingPowerupBonuses.RemoveAt(i);
                }
                else
                {
                    _decayingPowerupBonuses[i] = nextBonus;
                }
            }

            UpdatePowerupDamageBonus();
        }

        private void ApplyDamageToEnemy(int damage)
        {
            int startingHp = _enemy.HP;
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

            int hpDamage = Math.Max(0, startingHp - _enemy.HP);
            _damageDealtThisTurn += hpDamage;
            if (_clockArmed && !_isReleasingClockDamage)
            {
                _clockStoredDamage += hpDamage;
            }
        }

        private void CompleteTurn()
        {
            if (_enemy.HP <= 0) return;

            if (_clockArmed)
            {
                int releaseDamage = (int)Math.Ceiling(_clockStoredDamage * _clockBonusMultiplier);
                _clockArmed = false;
                _clockStoredDamage = 0;
                if (releaseDamage > 0)
                {
                    _isReleasingClockDamage = true;
                    ApplyDamageToEnemy(releaseDamage);
                    _isReleasingClockDamage = false;
                    _enemyFlashTimer = 0.4f;
                }

                if (_enemy.HP <= 0) return;
            }

            _damageDealtThisTurn = 0;
            _enemy.Defense = 0;

            foreach (EnemyAction action in _enemy.NextActions)
            {
                if (action.Type == EnemyActionType.Attack)
                {
                    _redFlashAlpha = 0.55f;
                    GlobalContentManager.EnemyAttackSound.Play();
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
            AdvancePowerupEffects();
            bool hasCardsToDraw = _player.Deck.Count > 0;
            _player.PlayerTurn();
            if (hasCardsToDraw && _player.Hand[0] != null)
            {
                GlobalContentManager.DrawCardsSound.Play();
            }
            _enemy.RollNextAction(_player.Sanity < 30); 
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin();

            // 1. Background
            int bgIndex = isBossRoom ? 2 : 1;
            spriteBatch.Draw(GlobalContentManager.BattleBackgrounds[bgIndex], new Rectangle(0, 0, 1920, 1080), Color.White);
            int MoveY = (int)(Math.Sin(DateTime.Now.TimeOfDay.TotalSeconds * 3.5) * 10);

            // 2. Enemy & Intent Display
            Texture2D enemyTexture = targetNode?.Type switch
            {
                NodeType.Elite => GlobalContentManager.EliteEnemy,
                NodeType.Boss => GlobalContentManager.BossEnemy,
                _ => GlobalContentManager.Enemy
            };

            // --- RENDER ENEMY HEALTH BAR (Enemies_health_bar.png) ---
            if (GlobalContentManager.BattleUiTextures[2] != null)
            {
                float enemyBarX = 1200f;
                float enemyBarY = 240f;
                float enemyBarWidth = 300f;
                int enemyBarHeight = 22;

                // พื้นหลังหลอดเลือดศัตรู
                spriteBatch.Draw(GlobalContentManager.BattleUiTextures[2], new Rectangle((int)enemyBarX, (int)enemyBarY, (int)enemyBarWidth, enemyBarHeight), Color.DarkGray * 0.5f);

                // สัดส่วนเลือดศัตรู
                float enemyHpPercent = MathHelper.Clamp((float)_enemy.HP / _enemy.MaxHP, 0f, 1f);
                int enemyFilledWidth = (int)(enemyBarWidth * enemyHpPercent);
                Rectangle enemySourceRect = new Rectangle(0, 0, (int)(GlobalContentManager.BattleUiTextures[2].Width * enemyHpPercent), GlobalContentManager.BattleUiTextures[2].Height);
                Rectangle enemyDestRect = new Rectangle((int)enemyBarX, (int)enemyBarY, enemyFilledWidth, enemyBarHeight);

                spriteBatch.Draw(GlobalContentManager.BattleUiTextures[2], enemyDestRect, enemySourceRect, Color.White);

                // ข้อความเลือดศัตรูตรงกลางหลอด
                string enemyHpText = $"HP: {_enemy.HP} / {_enemy.MaxHP}";
                if (_enemy.Defense > 0) enemyHpText += $" (+{_enemy.Defense} DEF)";
                Vector2 enemyTextSize = GlobalContentManager.BattleFont.MeasureString(enemyHpText);
                Vector2 enemyTextPos = new Vector2(
                    enemyBarX + (enemyBarWidth - enemyTextSize.X) / 2f,
                    enemyBarY + (enemyBarHeight - enemyTextSize.Y) / 2f
                );
                spriteBatch.DrawString(GlobalContentManager.BattleFont, enemyHpText, enemyTextPos + Vector2.One, Color.Black);
                spriteBatch.DrawString(GlobalContentManager.BattleFont, enemyHpText, enemyTextPos, Color.Red);
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

            spriteBatch.DrawString(GlobalContentManager.BattleFont, intentText, new Vector2(1200, 190), intentColor);

            // 3. Player Stats UI (ปรับตำแหน่ง Shield ซ้าย, Health Bar กลาง, Sanity ขวา)
            
            if (GlobalContentManager.BattleUiTextures[1] != null) // Sheid_bar.png
            {
                // ขนาดเดิม 45x45 คูณ 2 เป็น 90x90 พิกเซล
                Rectangle shieldIconRect = new Rectangle(400, 715, 90, 90); 
                spriteBatch.Draw(GlobalContentManager.BattleUiTextures[1], shieldIconRect, Color.White);
                
                // ตัวเลข Defense (ปรับขนาดฟอนต์หรือตำแหน่งตามความเหมาะสม)
                string defenseText = $"{_player.Defense}";
                Vector2 defTextSize = GlobalContentManager.BattleFont.MeasureString(defenseText);
                Vector2 defTextPos = new Vector2(
                    shieldIconRect.Center.X - (defTextSize.X / 2f),
                    shieldIconRect.Center.Y - (defTextSize.Y / 2f)
                );
                spriteBatch.DrawString(GlobalContentManager.BattleFont, defenseText, defTextPos + Vector2.One, Color.Black);
                spriteBatch.DrawString(GlobalContentManager.BattleFont, defenseText, defTextPos, Color.White);
            }

            // --- B. LONG HEALTH BAR (ตรงกลาง) ---
            if (GlobalContentManager.HealthBar != null)
            {
                float barX = 500f;
                float barY = 740f; 
                float barWidth = 940f;
                int barHeight = 22;

                if (GlobalContentManager.HealthBar != null)
                {
                    spriteBatch.Draw(GlobalContentManager.HealthBar, new Rectangle((int)barX, (int)barY, (int)barWidth, barHeight), Color.DarkGray * 0.8f);
                }

                float hpPercent = MathHelper.Clamp((float)_player.PlayerHP / _player.MaxHP, 0f, 1f);
                int filledWidth = (int)(barWidth * hpPercent);

                Rectangle sourceRectBar = new Rectangle(0, 0, (int)(GlobalContentManager.HealthBar.Width * hpPercent), GlobalContentManager.HealthBar.Height);
                Rectangle destRectBar = new Rectangle((int)barX, (int)barY, filledWidth, barHeight);

                spriteBatch.Draw(GlobalContentManager.HealthBar, destRectBar, sourceRectBar, Color.White);

                string hpText = $"HP: {_player.PlayerHP} / {_player.MaxHP}";
                Vector2 textSize = GlobalContentManager.BattleFont.MeasureString(hpText);
                Vector2 textPos = new Vector2(
                    barX + (barWidth - textSize.X) / 2f,
                    barY + (barHeight - textSize.Y) / 2f
                );

                spriteBatch.DrawString(GlobalContentManager.BattleFont, hpText, textPos + Vector2.One, Color.Black);
                spriteBatch.DrawString(GlobalContentManager.BattleFont, hpText, textPos, Color.White);
            }

           if (GlobalContentManager.BattleUiTextures[0] != null) // Sanity_bar.png
            {
                // ขนาดเดิม 48x48 คูณ 2 เป็น 96x96 พิกเซล
                Rectangle sanityIconRect = new Rectangle(1500, 710, 96, 96); 
                spriteBatch.Draw(GlobalContentManager.BattleUiTextures[0], sanityIconRect, Color.White);

                // ตัวเลข Sanity ตรงกลางไอคอนวงกลม
                string sanityText = $"{_player.Sanity}";
                Vector2 sanityTextSize = GlobalContentManager.BattleFont.MeasureString(sanityText);
                Vector2 sanityTextPos = new Vector2(
                    sanityIconRect.Center.X - (sanityTextSize.X / 2f),
                    sanityIconRect.Center.Y - (sanityTextSize.Y / 2f)
                );
                spriteBatch.DrawString(GlobalContentManager.BattleFont, sanityText, sanityTextPos + Vector2.One, Color.Black);
                spriteBatch.DrawString(GlobalContentManager.BattleFont, sanityText, sanityTextPos, Color.MintCream);
            }

            // สถานะเตือน Sanity ต่ำ
            if (_player.Sanity <= 30)
            {
                spriteBatch.DrawString(GlobalContentManager.BattleFont, "Sanity is critically low!", new Vector2(810, 700), Color.Red);
            }
            else
            {
                spriteBatch.DrawString(GlobalContentManager.BattleFont, "Sanity is stable.", new Vector2(850, 700), Color.LightGreen);
            }

            // สถิติอื่นๆ ด้านบน (Energy, Deck, Discard)
            spriteBatch.DrawString(GlobalContentManager.BattleFont, $"Energy: {_player.Energy}", new Vector2(80, 80), Color.Yellow);
            spriteBatch.DrawString(GlobalContentManager.BattleFont, $"Deck: {_player.Deck.Count}  |  Discard: {_player.DiscardPile.Count}", new Vector2(80, 120), Color.LightGray);

            float startX = 500;
            int cardWidth = 140;
            int cardHeight = 200;
            CardManager hoveredCard = null;

            int sourceWidth = GlobalContentManager.CardSheet.Width / 4;
            int sourceHeight = GlobalContentManager.CardSheet.Height / 4;

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

                    bool isAlphaLocked = _alphaLocked && card.Name == "Alpha";
                    var renderedCardBounds = new Rectangle((int)cardX, (int)cardY, cardWidth, cardHeight);
                    spriteBatch.Draw(GlobalContentManager.CardSheet, renderedCardBounds, sourceRect, isAlphaLocked ? Color.Gray : Color.White);

                    if (isAlphaLocked)
                    {
                        spriteBatch.Draw(GlobalContentManager.WhitePixel, renderedCardBounds, Color.Black * 0.45f);
                        spriteBatch.DrawString(
                            GlobalContentManager.BattleFont,
                            "LOCKED",
                            new Vector2(cardX + 25, cardY + 85),
                            Color.Red
                        );
                    }

                    if (card.Name.EndsWith("+", StringComparison.Ordinal))
                    {
                        spriteBatch.Draw(
                            card.PlusSymbolTexture,
                            new Rectangle((int)cardX + cardWidth - 36, (int)cardY + 8, 28, 28),
                            Color.White
                        );
                    }
                    
                    spriteBatch.DrawString(GlobalContentManager.BattleFont, $"[{i + 1}]", new Vector2(cardX + 10, cardY + 10), Color.Gold);
                    
                    bool leavesTwentySanity = LeavesTwentySanity(card);
                    spriteBatch.DrawString(GlobalContentManager.BattleFont, leavesTwentySanity ? "20" : $"{card.Cost * 10}", new Vector2(cardX + 10, cardY + 160), Color.Cyan);
                }
            }

            // 5. Tooltip Rendering
            Color enemyColor = (_enemyFlashTimer > 0f) ? Color.Red : Color.White;
            spriteBatch.Draw(enemyTexture, new Rectangle(1200, 300 + MoveY, 300, 300), enemyColor);

// 2. Draw Healing Plus Symbol Effect near player/health bar if active
if (_healEffectTimer > 0f && GlobalContentManager.PlusCard != null)
{
    // Floats upward slightly over the 0.4 seconds
    float floatOffset = (0.4f - _healEffectTimer) * 50f;
    spriteBatch.Draw(GlobalContentManager.PlusCard, new Rectangle(920, (int)(700 - floatOffset), 60, 60), Color.White);
}

// 3. Draw Shield Bar Highlight Effect if active
if (_shieldEffectTimer > 0f && GlobalContentManager.BattleUiTextures[1] != null)
{
    Rectangle shieldIconRect = new Rectangle(400, 715, 90, 90);
    // Draw a bright golden/white outline or scaled version to indicate activation
    spriteBatch.Draw(GlobalContentManager.BattleUiTextures[1], shieldIconRect, Color.Gold);
}
            if (hoveredCard != null && GlobalContentManager.WhitePixel != null)
            {
                string nameText = $"--- {hoveredCard.Name} ---";
                string descText = $"Description: {hoveredCard.Description}";
                string doesText = $"Effect: {hoveredCard.Does}";
                if (_alphaLocked && hoveredCard.Name == "Alpha")
                {
                    doesText += " Locked until this encounter ends.";
                }
                
                bool leavesTwentySanity = LeavesTwentySanity(hoveredCard);
                string costText = leavesTwentySanity 
                    ? "Sanity Cost: 20 (Special)" 
                    : $"Sanity Cost: {hoveredCard.Cost * 10}";

                string fullTooltipText = $"{nameText}\n{descText}\n{doesText}\n{costText}";

                Vector2 textSize = GlobalContentManager.BattleFont.MeasureString(fullTooltipText);
                
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

                spriteBatch.Draw(GlobalContentManager.WhitePixel, frameRect, Color.Black * 0.85f);
                
                float lineSpacing = GlobalContentManager.BattleFont.LineSpacing;
                Vector2 textDrawPos = tooltipPos;

                spriteBatch.DrawString(GlobalContentManager.BattleFont, nameText, textDrawPos, Color.Gold);
                textDrawPos.Y += lineSpacing;

                spriteBatch.DrawString(GlobalContentManager.BattleFont, descText, textDrawPos, Color.White);
                textDrawPos.Y += lineSpacing;

                spriteBatch.DrawString(GlobalContentManager.BattleFont, doesText, textDrawPos, Color.LightGreen);
                textDrawPos.Y += lineSpacing;

                spriteBatch.DrawString(GlobalContentManager.BattleFont, costText, textDrawPos, Color.Cyan);
            }

            // 6. End Turn Prompt/Button
            spriteBatch.DrawString(GlobalContentManager.BattleFont, "Click or Press [E] to End Turn", new Vector2(1500, 900), Color.Yellow);
            spriteBatch.DrawString(GlobalContentManager.BattleFont, "Press [P] to Pause / Player Stats", new Vector2(1500, 950), Color.LightBlue);

            // 7. Draw Scary Floating Texts if Sanity < 30
            if (_player.Sanity < 30)
            {
                foreach (var item in _scaryTexts)
                {
                    Color scaryColor = new Color(180, 0, 20) * item.Alpha;
                    spriteBatch.DrawString(GlobalContentManager.BattleFont, item.Text, item.Position, scaryColor);
                }
            }

            // 8. Draw Screen Flash Overlay (Red & Purple Flash)
            if (GlobalContentManager.WhitePixel != null)
            {
                if (_redFlashAlpha > 0f)
                {
                    spriteBatch.Draw(
                        GlobalContentManager.WhitePixel,
                        new Rectangle(0, 0, 1920, 1080),
                        Color.Red * _redFlashAlpha
                    );
                }

                if (_purpleFlashAlpha > 0f)
                {
                    spriteBatch.Draw(
                        GlobalContentManager.WhitePixel,
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