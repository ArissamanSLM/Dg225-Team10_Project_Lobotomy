using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using DreamSlayerV2.Core;
using DreamSlayerV2.Scenes;
using Microsoft.Xna.Framework.Content;

namespace DreamSlayerV2.Scenes
{
    public class BattleScene : IScene
    {
        public enum EnemyActionType { Attack, Defend, EldritchDebuff }

        private SpriteFont _font;
        private Texture2D EnemyTexture;
        private Texture2D[] BgTexture = new Texture2D[3];
        private Texture2D CardTexture;

        // Combat Stats
        private int enemyHP = 30;
        private int maxEnemyHP = 30;
        private int enemyDefense = 0;
        private bool isBossRoom = false;

        // Enemy Intent (Slay the Spire mechanics)
        private EnemyActionType nextEnemyAction;
        private int nextEnemyActionValue;
        private Random _rng = new Random();

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
            targetNode = nodeToComplete;
            if (isBossRoom)
            {
                enemyHP = 90;
                maxEnemyHP = 90;
            }
        }

        public void Initialize()
        {
            _player = DreamSlayerV2.Core.GameState.CurrentPlayer ?? new PlayerController(CharacterClassV2.Human);
            _player.PlayerTurn();

            // Roll initial intent for turn 1
            RollNextEnemyAction();
        }

        public void LoadContent(ContentManager content)
        {
            _font = content.Load<SpriteFont>("File");
            EnemyTexture = content.Load<Texture2D>("UnrealWolf");
            BgTexture[1] = content.Load<Texture2D>("HorrorForestDreamV2");
            BgTexture[2] = content.Load<Texture2D>("TheDarkCave");
            CardTexture = content.Load<Texture2D>("All16Card");
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

            // Check for Win Condition (Enemy HP <= 0)
            if (enemyHP <= 0)
            {
                // Check if this node is the Stage 13 Final Boss node
                bool isFinalBossNode = targetNode != null &&
                          targetNode.Name == "13-Boss" &&
                          targetNode.Type == NodeType.Boss;

                if (isFinalBossNode)
                {
                    // Victory: 6 arguments (nodesPassed, enemiesCleared, cardsPlayed, isVictory: true, died: false, floorsPassed: 13)
                    SceneManager.ChangeScene(new GameOverScene(5, 3, 24, true, false ,6));
                }
                else
                {
                    // Normal encounter/elite clear -> Proceed to Reward Scene
                    int rewardTier = (targetNode != null && targetNode.Type == NodeType.Elite) ? 1 : 0;
                    SceneManager.ChangeScene(new RewardScene(rewardTier));
                }
                return; // Stop updating battle logic after scene transition
            }

            // Check Defeat Condition
            if (_player.PlayerHP <= 0)
            {
                // Defeat: 6 arguments (nodesPassed, enemiesCleared, cardsPlayed, isVictory: false, died: true, floorsPassed: 1)
                SceneManager.ChangeScene(new GameOverScene(5, 3, 24, false, true ,0));
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

                // Calculate mouse collision bounds
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

            if (endTurnKeyPressed || endTurnClicked)
            {
                ExecuteEnemyTurn();
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

                    // Attack passes through Enemy Defense first
                    if (enemyDefense > 0)
                    {
                        if (damage <= enemyDefense)
                        {
                            enemyDefense -= damage;
                        }
                        else
                        {
                            damage -= enemyDefense;
                            enemyDefense = 0;
                            enemyHP -= damage;
                        }
                    }
                    else
                    {
                        enemyHP -= damage;
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

                _player.UseCard(cardIndex);
            }
        }

        private void RollNextEnemyAction()
        {
            int roll = _rng.Next(0, 100);

            if (roll < 60)
            {
                nextEnemyAction = EnemyActionType.Attack;
                nextEnemyActionValue = isBossRoom ? _rng.Next(12, 20) : _rng.Next(6, 12);
            }
            else if (roll < 85)
            {
                nextEnemyAction = EnemyActionType.Defend;
                nextEnemyActionValue = isBossRoom ? 12 : 8;
            }
            else
            {
                nextEnemyAction = EnemyActionType.EldritchDebuff;
                nextEnemyActionValue = 15; // Drains player Sanity
            }
        }

        private void ExecuteEnemyTurn()
        {
            // 1. Reset enemy shield at start of their turn
            enemyDefense = 0;

            // 2. Execute Telegraphed Intent
            switch (nextEnemyAction)
            {
                case EnemyActionType.Attack:
                    int incomingDamage = nextEnemyActionValue;
                    if (_player.Defense > 0)
                    {
                        if (incomingDamage <= _player.Defense)
                        {
                            _player.Defense -= incomingDamage;
                        }
                        else
                        {
                            incomingDamage -= _player.Defense;
                            _player.Defense = 0;
                            _player.PlayerHP -= incomingDamage;
                        }
                    }
                    else
                    {
                        _player.PlayerHP -= incomingDamage;
                    }
                    break;

                case EnemyActionType.Defend:
                    enemyDefense += nextEnemyActionValue;
                    break;

                case EnemyActionType.EldritchDebuff:
                    _player.Sanity = Math.Max(0, _player.Sanity - nextEnemyActionValue);
                    break;
            }

            // 3. Complete Player End Turn & Start Next Player Turn
            _player.EndTurn();
            _player.PlayerTurn();

            // 4. Roll fresh intent for enemy's upcoming turn
            RollNextEnemyAction();
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin();

            // 1. Background
            int bgIndex = isBossRoom ? 2 : 1;
            spriteBatch.Draw(BgTexture[bgIndex], new Rectangle(0, 0, 1920, 1080), Color.White);

            // 2. Enemy, Stats & Slay the Spire INTENT DISPLAY
            spriteBatch.Draw(EnemyTexture, new Rectangle(1200, 300, 300, 300), Color.White);

            string enemyDefenseText = enemyDefense > 0 ? $" (+{enemyDefense} DEF)" : "";
            spriteBatch.DrawString(_font, $"Enemy HP: {enemyHP} / {maxEnemyHP}{enemyDefenseText}", new Vector2(1200, 240), Color.Red);

            // RENDER NEXT ENEMY ACTION INTENT
            string intentText = nextEnemyAction switch
            {
                EnemyActionType.Attack => $"INTENT: Attacking for {nextEnemyActionValue} DMG",
                EnemyActionType.Defend => $"INTENT: Gaining {nextEnemyActionValue} Defense",
                EnemyActionType.EldritchDebuff => $"INTENT: Eldritch Gaze (-{nextEnemyActionValue} Sanity)",
                _ => ""
            };
            Color intentColor = nextEnemyAction switch
            {
                EnemyActionType.Attack => Color.OrangeRed,
                EnemyActionType.Defend => Color.LightBlue,
                EnemyActionType.EldritchDebuff => Color.MediumPurple,
                _ => Color.Yellow
            };

            spriteBatch.DrawString(_font, intentText, new Vector2(1200, 190), intentColor);

            // 3. Player Stats UI
            spriteBatch.DrawString(_font, $"HP: {_player.PlayerHP} / {_player.MaxHP}", new Vector2(80, 80), Color.White);
            spriteBatch.DrawString(_font, $"Defense: {_player.Defense}", new Vector2(80, 130), Color.SkyBlue);
            spriteBatch.DrawString(_font, $"Sanity: {_player.Sanity}", new Vector2(80, 180), Color.Cyan);
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
                        CardManager.CardType.Heal => 2,
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

            spriteBatch.End();
        }

        public void UnloadContent() { }
    }
}