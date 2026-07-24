using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using MonoGame.Extended.Graphics;
using MonoGame.Extended.Content.Pipeline;
using System;

namespace Starter;

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private Texture2D _whitePixel;
    int hp = 30;
    int mp = 100;
    int snaity = 100;
    int Card = 0;
    int holdCard = 6;
    int CardMax = 12;
    int Enermyhp = 50;
    int Bosshp = 200;
    string[] CardName = new string[12] { "Attack", "Attack", "Attack", "Attack", "Attack", "Defense", "Defense", "Defense", "Defense", "Defense", "Heal", "Heal" };
    private MouseState previousMouse;
    private MouseState currentMouse;
    private Texture2DAtlas _textureAtlas;
    private String[] RoomBAsed = new string[3] { "Event", "Enemy", "Boss"};
    Vector2 Enermy1 = new Vector2(250, 250);
    Vector2 Enermy2 = new Vector2(500 , 250);
    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = false;
        _graphics.PreferredBackBufferWidth = 900;
        _graphics.PreferredBackBufferHeight = 900;
        _graphics.ApplyChanges();
    }

    protected override void Initialize()
    {
        // TODO: Add your initialization logic here

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        // Create a 1x1 white pixel texture at runtime.
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
            // Handle left mouse button click
            // You can add your logic here for what happens when the left mouse button is clicked
        }
        // TODO: Add your update logic here
        
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        _spriteBatch.Begin();
        // Draw a single pixel at (100, 100).
        _spriteBatch.Draw(_whitePixel, new Rectangle(100, 100, 1, 1), Color.White);
        _spriteBatch.End();

        base.Draw(gameTime);
    }
}
