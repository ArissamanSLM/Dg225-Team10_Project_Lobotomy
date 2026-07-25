using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using MonoGame.Extended.Graphics;
using MonoGame.Extended.Content.Pipeline;
using System;

namespace Starter;

public class CardData 
{
    public string Name { get; set; }
    public int SanityCost { get; set; }
    public int Value { get; set; } 
    public string Type { get; set; } 

    public CardData(string name, int cost, int value, string type) 
    {
        Name = name;
        SanityCost = cost;
        Value = value;
        Type = type;
    }

    public enum CardType
    {
        Attack,
        Defense,
        Heal
    }

    // Fixed: Returns a list of cards instead of multiple unreachable returns
    public static List<CardData> MakeStartingDeck()
    {
        List<CardData> deck = new List<CardData>();
        deck.Add(new CardData("Strike", 10, 6, "Attack"));
        deck.Add(new CardData("Strike", 10, 6, "Attack"));
        deck.Add(new CardData("Defend", 5, 4, "Defense"));
        deck.Add(new CardData("Defend", 5, 4, "Defense"));
        deck.Add(new CardData("Heal", 8, 5, "Heal"));
        deck.Add(new CardData("Heal", 8, 5, "Heal"));
        return deck;
    }
}

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
    Blessed
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

        spriteBatch.Draw(whitePixel, Bounds, cardColor);
    }
}

public class RoomManager
{
    private int roomCount = 0;
    private int recentEncounters = 0;
    private int recentEvents = 0;

    private int encounterChance = 50;
    private int eventChance = 50;

    private static Random rand = new Random();

    public string GetNextBrownDoorType()
    {
        if (recentEncounters == recentEvents)
        {
            encounterChance = 50;
            eventChance = 50;
        }
        else if (recentEncounters > recentEvents)
        {
            encounterChance = 30;
            eventChance = 70;
        }
        else
        {
            encounterChance = 70;
            eventChance = 30;
        }

        int roll = rand.Next(0, 100);

        if (roll < encounterChance)
        {
            return "Encounter"; 
        }
        else
        {
            return "Event"; 
        }
    }

    public void RegisterRoomCompletion(string chosenType)
    {
        roomCount++;

        if (chosenType == "Encounter")
        {
            recentEncounters++;
        }
        else if (chosenType == "Event")
        {
            recentEvents++;
        }
    }

    public bool IsBossRoomReady()
    {
        return roomCount >= 5;
    }

    public void ResetRun()
    {
        roomCount = 0;
        recentEncounters = 0;
        recentEvents = 0;
    }

    public void EventRoom(ref int hp, ref int sanity, EventType eventType)
    {
        int roll = rand.Next(0, 100);
        if (roll < 50)
        {
            hp -= 5;
            sanity -= 10;
        }
        else
        {
            // Implement the effects of a blessed event here
            hp += 10;
            sanity += 20;
        }
    }
}

// Fixed: Methods are now properly separated outside of each other
public class MonsterHandler
{
    public int EnemyHp = 30;
    public int BossHp = 120;
    public int EnemyActionCount = 4;
    public int BossActionCount = 3;

    public void MonsterAction(ref int playerHp)
    {
        int enemyDamage = 3;
        if (EnemyActionCount == 0)
        {
            playerHp -= enemyDamage;
            EnemyActionCount = 4;
        }
        else
        {
            EnemyActionCount--;
        }
    }

    public void BossAction(ref int playerHp)
    {
        int bossDamage = 5;
        if (BossActionCount == 0)
        {
            playerHp -= bossDamage;
            BossActionCount = 3;
        }
        else
        {
            BossActionCount--;
        }
    }
}

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private Texture2D _whitePixel;
    
    int hp = 15;
    int sanity = 100;
    int holdCard = 6;
    int ClickLimit = 6;
    
    bool isEnermyDead = false;
    bool isBossActive = false; 
    bool Skipturn = false; // skip turn when player has no Sanity left to play

    private MouseState previousMouse;
    private MouseState currentMouse;
    
    private RoomManager roomManager = new RoomManager();
    private MonsterHandler monsterHandler = new MonsterHandler();
    private List<CardData> handCards = new List<CardData>();

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
        handCards = CardData.MakeStartingDeck();
        ClickLimit = 6;
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
                else if (handCards.Count > 0 && ClickLimit > 0)
                {
                    for (int i = 0; i < handCards.Count && i < holdCard; i++)
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
                    int xPos = 300 + (i * 200);
                    Rectangle doorRect = new Rectangle(xPos, 230, 128, 128);

                    if (doorRect.Contains(mousePos))
                    {
                        string chosenType = roomManager.GetNextBrownDoorType();
                        roomManager.RegisterRoomCompletion(chosenType);

                        if (roomManager.IsBossRoomReady())
                        {
                            isBossActive = true;
                            monsterHandler.BossHp = 120; 
                        }
                        else
                        {
                            monsterHandler.EnemyHp = 30; 
                            isBossActive = false;
                        }

                        isEnermyDead = false;
                        ClickLimit = 6;
                        handCards = CardData.MakeStartingDeck();
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
            Color enemyColor = isBossActive ? Color.DarkRed : Color.Red;
            Rectangle enemyRect = isBossActive ? new Rectangle(512, 200, 150, 150) : new Rectangle(512, 230, 128, 128);
            _spriteBatch.Draw(_whitePixel, enemyRect, enemyColor);
        }

        // Brown Doors Test (Appears when Enemy/Boss HP <= 0)
        if (isEnermyDead)
        {
            for (int i = 0; i < 3; i++)
            {
                int xPos = 300 + (i * 200); 
                _spriteBatch.Draw(_whitePixel, new Rectangle(xPos, 230, 128, 128), Color.Black);
            }
        }

        // Card Display
        for (int i = 0; i < handCards.Count && i < holdCard; i++)
        {
            int xPos = 100 + (i * 100);
            CardData card = handCards[i];
            Color cardColor = card.Type == "Attack" ? Color.LightCoral : card.Type == "Defense" ? Color.LightBlue : Color.LightGreen;
            _spriteBatch.Draw(_whitePixel, new Rectangle(xPos, 600, 80, 120), cardColor);
        }
        // SkipButton Display
        Color skipButtonColor = Skipturn ? Color.Goldenrod : Color.Gray;
        _spriteBatch.Draw(_whitePixel, new Rectangle(1000, 600, 80, 40), skipButtonColor);
        // Player UI Status Bars
        _spriteBatch.Draw(_whitePixel, new Rectangle(64, 64, 256, 32), Color.Green);
        _spriteBatch.Draw(_whitePixel, new Rectangle(64, 96, 192, 24), Color.LightBlue);

        _spriteBatch.End();

        base.Draw(gameTime);
    }

    private void UseCard(int cardIndex)
    {
        if (cardIndex < 0 || cardIndex >= handCards.Count)
            return;

        CardData card = handCards[cardIndex];
        handCards.RemoveAt(cardIndex);
        ClickLimit--;

        if (card.Type == "Attack")
        {
            if (isBossActive)
            {
                monsterHandler.BossHp -= card.Value;
                if (monsterHandler.BossHp <= 0)
                    isEnermyDead = true;
            }
            else
            {
                monsterHandler.EnemyHp -= card.Value;
                if (monsterHandler.EnemyHp <= 0)
                    isEnermyDead = true;
            }
        }
        else if (card.Type == "Defense")
        {
            hp += card.Value;
            sanity += 2;
        }
        else if (card.Type == "Heal")
        {
            hp += card.Value;
            sanity += 5;
        }

        if (ClickLimit <= 0 || handCards.Count == 0)
        {
            Skipturn = true;
        }
    }

    private void AdvanceTurn()
    {
        if (isBossActive)
        {
            monsterHandler.BossAction(ref hp);
        }
        else
        {
            monsterHandler.MonsterAction(ref hp);
        }
        sanity += 10; //Regain some sanity each turn
        ClickLimit = 6;
        handCards = CardData.MakeStartingDeck();
    }
}