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
}

public class RoomData 
{
    public string RoomType { get; set; } 

    public RoomData(string roomType) 
    {
        RoomType = roomType;
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
}

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private Texture2D _whitePixel;
    
    int hp = 30;
    int mp = 100;
    int sanity = 100;
    int holdCard = 6;
    int CardMax = 12;
    int Enermyhp = 30;
    int Bosshp = 120;
    int ClickLimit = 0;
    
    bool isEnermyDead = false;
    bool isBossActive = false; // Tracks if we are fighting the boss

    private MouseState previousMouse;
    private MouseState currentMouse;
    
    private RoomManager roomManager = new RoomManager();

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
        
        // Left Click Logic
        if (currentMouse.LeftButton == ButtonState.Pressed && previousMouse.LeftButton == ButtonState.Released)
        {
            Point mousePos = new Point(currentMouse.X, currentMouse.Y);

            // 1. If enemy/boss is alive, click to damage them
            if (!isEnermyDead)
            {
                if (isBossActive)
                {
                    Bosshp -= 10; // Deal damage to boss
                    if (Bosshp <= 0)
                    {
                        isEnermyDead = true; // Win condition reached
                    }
                }
                else
                {
                    Enermyhp -= 10; // Deal damage to standard enemy
                    if (Enermyhp <= 0)
                    {
                        isEnermyDead = true;
                    }
                }
            }
            // 2. If enemy is dead, check clicks on the 3 Brown Doors
            else
            {
                for (int i = 0; i < 3; i++)
                {
                    int xPos = 300 + (i * 200);
                    Rectangle doorRect = new Rectangle(xPos, 230, 128, 128);

                    if (doorRect.Contains(mousePos))
                    {
                        // Simulate choosing a door
                        string chosenType = roomManager.GetNextBrownDoorType();
                        roomManager.RegisterRoomCompletion(chosenType);

                        // Check if boss room is ready after completion
                        if (roomManager.IsBossRoomReady())
                        {
                            isBossActive = true;
                            Bosshp = 120; // Reset boss HP
                        }
                        else
                        {
                            Enermyhp = 30; // Reset normal enemy HP
                            isBossActive = false;
                        }

                        isEnermyDead = false; // Reset encounter state to fight again
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
                _spriteBatch.Draw(_whitePixel, new Rectangle(xPos, 230, 128, 128), Color.Brown);
            }
        }

        // Player UI Status Bars
        _spriteBatch.Draw(_whitePixel, new Rectangle(64, 64, 256, 32), Color.Green);
        _spriteBatch.Draw(_whitePixel, new Rectangle(64, 96, 192, 24), Color.LightBlue);

        _spriteBatch.End();

        base.Draw(gameTime);
    }
}