using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using MonoGame.Extended.Graphics;
using MonoGame.Extended.Content.Pipeline;
using System;

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
    
    bool Shopevent = false;
    
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

        // Card Display
        for (int i = 0; i < playerManager.HandCards.Count && i < playerManager.HoldCard; i++)
        {
            int xPos = 100 + (i * 100);
            CardData card = playerManager.HandCards[i];
            Color cardColor = card.Type == "Attack" ? Color.LightCoral : card.Type == "Defense" ? Color.LightBlue : card.Type == "Counter" ? Color.Goldenrod : card.Type == "PowerUp" ? Color.Orange : card.Type == "Protection" ? Color.MediumPurple : card.Type == "duplicate" ? Color.LightGray : card.Type == "inpection" ? Color.LightYellow : card.Type == "Forcefield" ? Color.LightCyan : Color.LightGreen;
            _spriteBatch.Draw(_whitePixel, new Rectangle(xPos, 600, 80, 120), cardColor);
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
            if (isBossActive)
            {
                enemyManager.BossHp -= damage;
                if (enemyManager.BossHp <= 0)
                {
                    isEnermyDead = true;
                    coinManager.Coins += 15;
                }
            }
            else
            {
                enemyManager.EnemyHp -= damage;
                if (enemyManager.EnemyHp <= 0)
                {
                    isEnermyDead = true;
                    coinManager.Coins += 15;
                }
            }
        }
        else if (card.Type == "Defense")
        {
            playerManager.GainDefense(card.Value);
            playerManager.Sanity += 4;
        }
        else if (card.Type == "Heal")
        {
            playerManager.Heal(card.Value);
            playerManager.Sanity += 5;
        }
        else if (card.Type == "Counter")
        {
            playerManager.GainDefense(4);
            playerManager.Sanity += 2;
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
            playerManager.Sanity += 2;
        }
        else if (card.Type == "Protection")
        {
            playerManager.DefenseBoostTurns = 1;
            playerManager.GainDefense(card.Value);
            playerManager.Sanity += 2;
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
        playerManager.Sanity += 10; //Regain some sanity each turn
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
                roomManager.Shoproom(ref coinManager.Coins, ref playerManager.Hp, ref playerManager.Sanity, playerManager.HandCards);
                Shopevent = false;
            }

            isEnermyDead = true;
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