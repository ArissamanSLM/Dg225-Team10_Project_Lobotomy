using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using MonoGame.Extended.Graphics;
using MonoGame.Extended.Content.Pipeline;

namespace Starter;

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
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

        // TODO: use this.Content to load your game content here
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

        // TODO: Add your drawing code here

        base.Draw(gameTime);
    }
}
