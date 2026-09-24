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
        private SpriteFont _font;
        private Texture2D EnemyTexture;
        private Texture2D[] BgTexture = new Texture2D[3];
        private Texture2D CardTexture;

        // Combat Stats
        private int enemyHP = 50;
        private int maxEnemyHP = 50;
        private bool isBossRoom = false; // Set to true if this is the Node 7 boss encounter

        // Input States
        private KeyboardState currentKeyboardState;
        private KeyboardState _previousKeyboardState;
        private MouseState currentMouseState;

        // Player instance handling stats, deck, and hand
        private PlayerController _player;

        // Constructor
        public BattleScene(bool bossRoom = false, Node nodeToComplete = null)
        {
            isBossRoom = bossRoom;
            targetNode = nodeToComplete; // <--- บันทึกค่า Node เอาไว้
            if (isBossRoom)
            {
                enemyHP = 150;
                maxEnemyHP = 150;
            }
        }

        public void Initialize()
        {
            
            _player = DreamSlayerV2.Core.GameState.CurrentPlayer ?? new PlayerController(CharacterClassV2.HumanDemo);

            // Start the player turn logic to draw initial hand
            _player.PlayerTurn();
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
            currentMouseState = Mouse.GetState();

            // Press 'I' to return to Title
            if (currentKeyboardState.IsKeyDown(Keys.I))
            {
                SceneManager.ChangeScene(new TitleScene());
            }

            // Check for Win Condition (Enemy HP <= 0)
            if (enemyHP <= 0)
            {
                // Pass reward tier: 0 for normal enemy, 2 for boss room
                int rewardTier = isBossRoom ? 2 : 0;
                SceneManager.ChangeScene(new RewardScene(rewardTier));
                return; // Stop updating battle logic once won
            }

            // Play cards using keys [1] through [5]
            Keys[] cardKeys = { Keys.D1, Keys.D2, Keys.D3, Keys.D4, Keys.D5 };
            for (int i = 0; i < cardKeys.Length; i++)
            {
                if (currentKeyboardState.IsKeyDown(cardKeys[i]) && _previousKeyboardState.IsKeyUp(cardKeys[i]))
                {
                    if (i < _player.Hand.Length && _player.Hand[i] != null)
                    {
                        var selectedCard = _player.Hand[i];

                        // Calculate Sanity cost (1 cost = 10 Sanity based on your orb icons)
                        int sanityCost = selectedCard.Cost * 10;

                        if (_player.Sanity >= sanityCost)
                        {
                            // Deduct Sanity
                            _player.Sanity -= sanityCost;

                            // Apply card effect (Attack deals damage to enemyHP)
                            if (selectedCard.Type == CardManager.CardType.Attack)
                            {
                                enemyHP -= selectedCard.InHandDamage;
                            }

                            // Remove card from hand slot (automatically moves it to discard pile)
                            _player.UseCard(i);
                        }
                    }
                }
            }

            // End Turn option via [E] key
            if (currentKeyboardState.IsKeyDown(Keys.E) && _previousKeyboardState.IsKeyUp(Keys.E))
            {
                _player.EndTurn();
                _player.PlayerTurn();
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin();

            // 1. Draw Background scaled to fill the 1920x1080 screen (Index 2 if boss room, 1 if normal)
            int bgIndex = isBossRoom ? 2 : 1;
            spriteBatch.Draw(BgTexture[bgIndex], new Rectangle(0, 0, 1920, 1080), Color.White);

            // 2. Draw Enemy / Boss & Health Text
            spriteBatch.Draw(EnemyTexture, new Rectangle(1200, 300, 300, 300), Color.White);
            spriteBatch.DrawString(_font, $"Enemy HP: {enemyHP} / {maxEnemyHP}", new Vector2(1200, 240), Color.Red);

            // 3. Draw Player Stats UI on the top-left
            spriteBatch.DrawString(_font, $"HP: {_player.PlayerHP} / {_player.MaxHP}", new Vector2(80, 80), Color.White);
            spriteBatch.DrawString(_font, $"Sanity: {_player.Sanity}", new Vector2(80, 130), Color.Cyan);
            spriteBatch.DrawString(_font, $"Energy: {_player.Energy}", new Vector2(80, 180), Color.Yellow);
            spriteBatch.DrawString(_font, $"Deck: {_player.Deck.Count}  |  Discard: {_player.DiscardPile.Count}", new Vector2(80, 230), Color.LightGray);

            // 4. Display current cards in hand with hover tooltips and textures
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

                    // Hover bounds check
                    Rectangle cardBounds = new Rectangle((int)cardX, (int)cardY, cardWidth, cardHeight);
                    bool isHovered = cardBounds.Contains(currentMouseState.Position);

                    if (isHovered)
                    {
                        hoveredCard = card;
                        cardY -= 25; // Pop card up when hovered
                    }

                    // Select correct texture slice from All16Card.png
                    int textureIndex = card.Type switch
                    {
                        CardManager.CardType.Attack => 0,
                        CardManager.CardType.Defense => 1,
                        CardManager.CardType.Utility => 2,
                        _ => 3
                    };

                    int sourceWidth = CardTexture.Width / 4;
                    Rectangle sourceRect = new Rectangle(textureIndex * sourceWidth, 0, sourceWidth, CardTexture.Height);

                    // Draw Card Visual Texture
                    spriteBatch.Draw(CardTexture, new Rectangle((int)cardX, (int)cardY, cardWidth, cardHeight), sourceRect, Color.White);

                    // Draw Card Name & Cost over the visual
                    spriteBatch.DrawString(_font, $"[{i + 1}] {card.Name}\nCost: {card.Cost}", new Vector2(cardX + 10, cardY + 10), Color.Black);
                }
            }

            // 5. Render card description tooltip panel if hovered
            if (hoveredCard != null)
            {
                Vector2 tooltipPos = new Vector2(currentMouseState.X + 20, currentMouseState.Y - 60);
                spriteBatch.DrawString(_font,
                    $"--- {hoveredCard.Name} ---\n" +
                    $"Description: {hoveredCard.Description}\n" +
                    $"Effect: {hoveredCard.Does}",
                    tooltipPos, Color.Gold);
            }

            // 6. Draw End Turn prompt near the bottom right
            spriteBatch.DrawString(_font, "Press [E] to End Turn", new Vector2(1500, 900), Color.White);

            spriteBatch.End();
        }

        public void UnloadContent() { }
    }
}