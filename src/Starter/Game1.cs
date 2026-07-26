using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;

namespace Starter;

public class RoomData 
{
    public string RoomType { get; set; } 

    public RoomData(string roomType) 
    {
        RoomType = roomType;
    }
}

public enum EventType
{
    Cursed,
    Blessed,
    Shop
}

public class CardRectangle
{
    public CardData Data { get; set; }
    public Rectangle Bounds { get; set; }
    public bool IsSelected { get; set; }

    public CardRectangle(CardData data, Rectangle bounds)
    {
        Data = data;
        Bounds = bounds;
        IsSelected = false;
    }

    public void Draw(SpriteBatch spriteBatch, Texture2D whitePixel)
    {
        Color cardColor = Color.White;
        if (Data.Type == "Attack") cardColor = Color.LightCoral;
        else if (Data.Type == "Defense") cardColor = Color.LightBlue;
        else if (Data.Type == "Heal") cardColor = Color.LightGreen;
        else if (Data.Type == "Counter") cardColor = Color.Goldenrod;
        else if (Data.Type == "PowerUp") cardColor = Color.Orange;
        else if (Data.Type == "Protection") cardColor = Color.MediumPurple;
        else if (Data.Type == "duplicate") cardColor = Color.LightGray;
        else if (Data.Type == "inpection") cardColor = Color.LightYellow;
        else if (Data.Type == "Forcefield") cardColor = Color.LightCyan;

        spriteBatch.Draw(whitePixel, Bounds, cardColor);
    }
}

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private Texture2D _whitePixel;
    Texture2D _cardTextureAttack;
    Texture2D _cardTextureDefense;
    Texture2D _cardTextureHeal;
    Texture2D _cardTextureCounter;
    Texture2D _cardTexturePowerUp;
    Texture2D _cardTextureProtection;
    Texture2D _cardTextureDuplicate;
    Texture2D _cardTextureInspection;
    Texture2D _cardTextureForcefield;
    private Song _battleMusic;
private Song _bossMusic1;
private SoundEffect _badEffectGain;

    bool Shopevent = false;
    bool isShopActive = false;
    bool isEnermyDead = false;
    bool isBossActive = false; 
    bool Skipturn = false; // skip turn when player has no Sanity left to play
    private string currentRoomType = "Encounter";

    private MouseState previousMouse;
    private MouseState currentMouse;
    
    private RoomManager roomManager = new RoomManager();
    private EnemyManager enemyManager = new EnemyManager();
    private PlayerManager playerManager = new PlayerManager();
    private CoinManager coinManager = new CoinManager();

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        _graphics.PreferredBackBufferWidth = 1280;
        _graphics.PreferredBackBufferHeight = 768;
        _graphics.ApplyChanges();
    }

    protected override void Initialize()
    {
        playerManager.ResetHand();
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        _whitePixel = new Texture2D(GraphicsDevice, 1, 1);
        _whitePixel.SetData(new[] { Color.White });
        _cardTextureAttack = Content.Load<Texture2D>("strike");
        _cardTextureDefense = Content.Load<Texture2D>("defend");
        _cardTextureHeal = Content.Load<Texture2D>("heal");
        _cardTextureCounter = Content.Load<Texture2D>("counter");
        _cardTexturePowerUp = Content.Load<Texture2D>("Power up");
        _cardTextureProtection = Content.Load<Texture2D>("Protection");
        _cardTextureInspection = Content.Load<Texture2D>("Inspection");
    }

    protected override void Update(GameTime gameTime)
    {
        previousMouse = currentMouse;
        currentMouse = Mouse.GetState();

        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();
        
        if (currentMouse.LeftButton == ButtonState.Pressed && previousMouse.LeftButton == ButtonState.Released)
        {
            Point mousePos = new Point(currentMouse.X, currentMouse.Y);
            Rectangle skipRect = new Rectangle(1000, 600, 80, 40);
            if (isShopActive)
            {
                // Shop Item Rectangles (3 items)
                Rectangle item1Rect = new Rectangle(300, 230, 150, 150); // Buy Card (50 coins)
                Rectangle item2Rect = new Rectangle(500, 230, 150, 150); // Big Heal (20 coins)
                Rectangle item3Rect = new Rectangle(700, 230, 150, 150); // Small Heal (10 coins)
                Rectangle exitShopRect = new Rectangle(500, 430, 150, 50); // Exit Shop button

                if (item1Rect.Contains(mousePos))
                {
                    roomManager.TryBuyItem(1, ref coinManager.Coins, ref playerManager.Hp, ref playerManager.Sanity, playerManager.HandCards);
                }
                else if (item2Rect.Contains(mousePos))
                {
                    roomManager.TryBuyItem(2, ref coinManager.Coins, ref playerManager.Hp, ref playerManager.Sanity, playerManager.HandCards);
                }
                else if (item3Rect.Contains(mousePos))
                {
                    roomManager.TryBuyItem(3, ref coinManager.Coins, ref playerManager.Hp, ref playerManager.Sanity, playerManager.HandCards);
                }
                else if (exitShopRect.Contains(mousePos))
                {
                    isShopActive = false;
                    // Go back to normal room selection doors after leaving shop
                }
            }

            if (!isEnermyDead)
            {
                if (Skipturn)
                {
                    AdvanceTurn();
                    Skipturn = false;
                }
                else if (skipRect.Contains(mousePos))
                {
                    Skipturn = true;
                }
                else if (playerManager.HandCards.Count > 0 && playerManager.ClickLimit > 0)
                {
                    for (int i = 0; i < playerManager.HandCards.Count && i < playerManager.HoldCard; i++)
                    {
                        int xPos = 100 + (i * 100);
                        Rectangle cardRect = new Rectangle(xPos, 600, 80, 120);

                        if (cardRect.Contains(mousePos))
                        {
                            UseCard(i);
                            break;
                        }
                    }
                }
                else
                {
                    Skipturn = true;
                }
            }
            else
            {
                for (int i = 0; i < 3; i++)
                {
                    int xPos = 300 + (i * 300);
                    Rectangle doorRect = new Rectangle(xPos, 230, 128, 128);

                    if (doorRect.Contains(mousePos))
                    {
                        string chosenType = i == 1 ? roomManager.GetNextYellowDoorType() : roomManager.GetNextBrownDoorType();
                        roomManager.RegisterRoomCompletion(chosenType);
                        EnterRoom(chosenType);
                        break;
                    }
                }
            }
        }
        
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        _spriteBatch.Begin();
        
        // Background
        _spriteBatch.Draw(_whitePixel, new Rectangle(0, 0, 1280, 768), Color.Pink);
      
        // Render Active Enemy or Boss
        if (!isEnermyDead)
        {
            int enemyHpBarMaxWidth = 256;
            int enemyHpWidth = Math.Max(0, (int)(enemyHpBarMaxWidth * (enemyManager.EnemyHp / 30f)));
            int bossHpWidth = Math.Max(0, (int)(enemyHpBarMaxWidth * (enemyManager.BossHp / 120f)));

            _spriteBatch.Draw(_whitePixel, new Rectangle(512, 100, enemyHpBarMaxWidth, 32), Color.DarkGray);
            if (isBossActive)
            {
                _spriteBatch.Draw(_whitePixel, new Rectangle(512, 100, bossHpWidth, 32), Color.DarkRed);
            }
            else
            {
                _spriteBatch.Draw(_whitePixel, new Rectangle(512, 100, enemyHpWidth, 32), Color.Red);
            }

            Color enemyColor = isBossActive ? Color.DarkRed : Color.Red;
            Rectangle enemyRect = isBossActive ? new Rectangle(512, 200, 150, 150) : new Rectangle(512, 230, 128, 128);
            _spriteBatch.Draw(_whitePixel, enemyRect, enemyColor);
        }

        // Brown Doors Test (Appears when Enemy/Boss HP <= 0)
        if (isEnermyDead)
        {
            for (int i = 0; i < 3; i++)
            {
                int xPos = 300 + (i * 300); 
                _spriteBatch.Draw(_whitePixel, new Rectangle(xPos, 230, 128, 128), Color.LightSkyBlue);
            }
        }
        if (isShopActive)
        {
            // Draw 3 shop item boxes
            _spriteBatch.Draw(_whitePixel, new Rectangle(300, 230, 150, 150), Color.Gold);      // Item 1: Card
            _spriteBatch.Draw(_whitePixel, new Rectangle(500, 230, 150, 150), Color.LightGreen); // Item 2: Big Heal
            _spriteBatch.Draw(_whitePixel, new Rectangle(700, 230, 150, 150), Color.DarkSeaGreen); // Item 3: Small Heal

            // Exit Shop Button
            _spriteBatch.Draw(_whitePixel, new Rectangle(500, 430, 150, 50), Color.Gray);        // Exit Button
        }

       for (int i = 0; i < playerManager.HandCards.Count && i < playerManager.HoldCard; i++)
        {
            int xPos = 100 + (i * 100);
            CardData card = playerManager.HandCards[i];
            
            // Default to white pixel if texture isn't found
            Texture2D cardTexture = _whitePixel; 
            Color tintColor = Color.White;

            // Match card type to the texture you loaded in LoadContent
            if (card.Type == "Attack") 
            {
                cardTexture = _cardTextureAttack;
            }
            else if (card.Type == "Defense") 
            {
                cardTexture = _cardTextureDefense;
            }
            else if (card.Type == "Heal") 
            {
                cardTexture = _cardTextureHeal;
            }
            else if (card.Type == "Counter") 
            {
                cardTexture = _cardTextureCounter;
            }
            else if (card.Type == "PowerUp") 
            {
                cardTexture = _cardTexturePowerUp;
            }
            else if (card.Type == "Protection") 
            {
                cardTexture = _cardTextureProtection;
            }
            else if (card.Type == "duplicate") 
            {
                cardTexture = _cardTextureDuplicate;
            }
            else if (card.Type == "inpection") 
            {
                cardTexture = _cardTextureInspection;
            }
            else if (card.Type == "Forcefield") 
            {
                cardTexture = _cardTextureForcefield;
            }

            // Draw the actual loaded texture instead of just the white pixel rectangle
            if (cardTexture == null)
            {
                cardTexture = _whitePixel;
            }

            _spriteBatch.Draw(cardTexture, new Rectangle(xPos, 600, 80, 120), tintColor);       
        }
        // SkipButton Display
        Color skipButtonColor = Skipturn ? Color.Goldenrod : Color.Gray;
        _spriteBatch.Draw(_whitePixel, new Rectangle(1000, 600, 80, 40), skipButtonColor);

        // Player UI Status Bars
        int hpBarMaxWidth = 256;
        int sanityBarMaxWidth = 192;
        int hpBarWidth = Math.Min(hpBarMaxWidth, (int)(hpBarMaxWidth * (playerManager.Hp / 25f)));
        int sanityBarWidth = Math.Min(sanityBarMaxWidth, (int)(sanityBarMaxWidth * (playerManager.Sanity / 100f)));

        _spriteBatch.Draw(_whitePixel, new Rectangle(64, 64, hpBarMaxWidth, 32), Color.DarkGray);
        _spriteBatch.Draw(_whitePixel, new Rectangle(64, 64, hpBarWidth, 32), Color.Green);

        _spriteBatch.Draw(_whitePixel, new Rectangle(64, 104, sanityBarMaxWidth, 24), Color.DarkGray);
        _spriteBatch.Draw(_whitePixel, new Rectangle(64, 104, sanityBarWidth, 24), Color.LightBlue);

        int defenseBarMaxWidth = 160;
        int defenseBarWidth = Math.Min(defenseBarMaxWidth, playerManager.Defense * 10);
        _spriteBatch.Draw(_whitePixel, new Rectangle(64, 144, defenseBarMaxWidth, 20), Color.DarkGray);
        _spriteBatch.Draw(_whitePixel, new Rectangle(64, 144, defenseBarWidth, 20), Color.CornflowerBlue);

        _spriteBatch.End();

        base.Draw(gameTime);
    }

    private void UseCard(int cardIndex)
    {
        if (cardIndex < 0 || cardIndex >= playerManager.HandCards.Count)
            return;

        CardData card = playerManager.HandCards[cardIndex];
        playerManager.HandCards.RemoveAt(cardIndex);
        playerManager.ClickLimit--;

        if (card.Type == "Attack")
        {
            int damage = card.Value + playerManager.PowerBoost;
            playerManager.Sanity -= 5;
            if (isBossActive)
            {
                enemyManager.BossHp -= damage;
                if (enemyManager.BossHp <= 0)
                {
                    isEnermyDead = true;
                    coinManager.Coins += 15;
                    playerManager.PowerBoost = 0; // Reset power boost after defeating the boss
                }
            }
            else
            {
                enemyManager.EnemyHp -= damage;
                if (enemyManager.EnemyHp <= 0)
                {
                    isEnermyDead = true;
                    coinManager.Coins += 15;
                    playerManager.PowerBoost = 0; // Reset power boost after defeating the enemy
                }
            }
        }
        else if (card.Type == "Defense")
        {
            playerManager.GainDefense(card.Value);
            playerManager.Sanity -= 7;
        }
        else if (card.Type == "Heal")
        {
            playerManager.Heal(card.Value);
            playerManager.Sanity -= 5;
        }
        else if (card.Type == "Counter")
        {
            playerManager.GainDefense(4);
            playerManager.Sanity -= 5;
            int damage = card.Value + playerManager.PowerBoost;
            if (isBossActive)
            {
                enemyManager.BossHp -= damage;
            }
            else
            {
                enemyManager.EnemyHp -= damage;
            }
        }
        else if (card.Type == "PowerUp")
        {
            playerManager.PowerBoost += card.Value;
            playerManager.Sanity -= 8;
        }
        else if (card.Type == "Protection")
        {
            playerManager.DefenseBoostTurns = 1;
            playerManager.GainDefense(card.Value);
            playerManager.Sanity -= 4;
        }
        else if (card.Type == "inpection")
        {
            // Draw 3 extra cards (or however many cards you want to pull)
            for (int k = 0; k < 3; k++)
            {
                // Pull a random card from your starting/current deck pool
                List<CardData> availableCards = CardData.MakeStartingDeck(); 
                if (availableCards.Count > 0)
                {
                    int randomIndex = new Random().Next(availableCards.Count);
                    playerManager.HandCards.Add(availableCards[randomIndex]);
                }
            }
            playerManager.Sanity += 20; // Optional sanity regain
        }
        else if (card.Type == "duplicate")
        {
            // Duplicate a random card from your hand (if any)
            if (playerManager.HandCards.Count > 0)
            {
                int randomIndex = new Random().Next(playerManager.HandCards.Count);
                CardData cardToDuplicate = playerManager.HandCards[randomIndex];
                playerManager.HandCards.Add(new CardData(cardToDuplicate.Name, cardToDuplicate.SanityCost, cardToDuplicate.Value, cardToDuplicate.Type));
            }
            playerManager.Sanity -= 20; // Optional sanity regain
        }
        

        if (playerManager.ClickLimit <= 0 || playerManager.HandCards.Count == 0)
        {
            Skipturn = true;
        }
    }

    private void AdvanceTurn()
    {
        if (isBossActive)
        {
            enemyManager.BossAction(playerManager);
        }
        else
        {
            enemyManager.MonsterAction(playerManager);
        }
        playerManager.Sanity += 25; //Regain some sanity each turn
        playerManager.LimitPlayerStats();
        playerManager.ResetHand();
    }

    private void EnterRoom(string chosenType)
    {
        currentRoomType = chosenType;

        if (roomManager.IsBossRoomReady())
        {
            currentRoomType = "Boss";
            isBossActive = true;
            enemyManager.BossHp = 200;
            isEnermyDead = false;
        }
        else if (chosenType == "Event")
        {
            isBossActive = false;
            roomManager.EventRoom(ref playerManager.Hp, ref playerManager.Sanity, ref Shopevent, EventType.Cursed);

            if (Shopevent)
            {
                isShopActive = true;    // Open shop UI state
                isEnermyDead = true;    // Keeps combat inactive while shopping
                Shopevent = false;
            }
            else
            {
                isEnermyDead = true;    // Normal event resolution back to doors
            }
        }
        else
        {
            isBossActive = false;
            enemyManager.EnemyHp = 30;
            isEnermyDead = false;
        }

        playerManager.ResetHand();
    }
}