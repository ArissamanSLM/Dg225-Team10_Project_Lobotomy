using DreamSlayerV2.Core;
using DreamSlayerV2.Scenes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System.Collections.Generic;
using System.Linq;

namespace DreamSlayerV2.Core
{
    public class Node
    {
        // Position of the node on the map
        public Vector2 Position { get; set; }

        // Connected nodes you can travel to next
        public List<Node> ConnectedNodes { get; set; } = new List<Node>();

        // Status of the node
        public bool IsCompleted { get; set; } = false;
    }
}

namespace DreamSlayerV2.Scenes
{
    public class NodeSelectScene : IScene 
    {
        private SpriteFont _font;
        private Vector2 CameraPosition = Vector2.Zero;
        private Vector2 scroll_factor = new Vector2(1.0f, 1f);
        private Texture2D _pixelTexture;
        private List<Node> allNodes = new List<Node>();
        private Song NodeMusic;
        // Track the previous mouse state to calculate scroll movement
        private MouseState _previousMouseState;

        public void Initialize()
        {
            _previousMouseState = Mouse.GetState();

            // Create sample nodes so you can test the camera scrolling and lines immediately!
            Node node1 = new Node { Position = new Vector2(300, 540) };
            Node node2M = new Node { Position = new Vector2(800, 540) };
            Node node2 = new Node { Position = new Vector2(800, 350) };
            Node node3 = new Node { Position = new Vector2(800, 730) };
            Node node4 = new Node { Position = new Vector2(1300, 540) };

            // Link them together
            node1.ConnectedNodes.Add(node2);
            node1.ConnectedNodes.Add(node3);
            node2.ConnectedNodes.Add(node4);
            node3.ConnectedNodes.Add(node4);
            node1.ConnectedNodes.Add(node2M); 
            node2M.ConnectedNodes.Add(node4); // Optional: Add a middle node for more complexity

            // Add them to the master list
            allNodes.Add(node1);
            allNodes.Add(node2);
            allNodes.Add(node3);
            allNodes.Add(node2M);
            allNodes.Add(node4);
        }

        public void LoadContent(ContentManager content)
        {
            _font = content.Load<SpriteFont>("File");
            NodeMusic = content.Load<Song>("Start_music (1)");
                MediaPlayer.Play(NodeMusic);
            MediaPlayer.IsRepeating = true;
            var graphicsDevice = ((IGraphicsDeviceService)content.ServiceProvider.GetService(typeof(IGraphicsDeviceService))).GraphicsDevice;
            _pixelTexture = new Texture2D(graphicsDevice, 1, 1);
            _pixelTexture.SetData(new[] { Color.White });
        }

        public void Update(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.G))
            {
                SceneManager.ChangeScene(new GameOverScene());
            }
            if (Keyboard.GetState().IsKeyDown(Keys.T))
            {
                SceneManager.ChangeScene(new TitleScene());
            }

            // 1. Get the current mouse state every frame
            MouseState currentMouseState = Mouse.GetState();

            // 2. Calculate the difference (delta) in the scroll wheel
            int scrollDelta = currentMouseState.ScrollWheelValue - _previousMouseState.ScrollWheelValue;

            // 3. Move camera based on scroll direction
            if (scrollDelta > 0)
            {
                CameraPosition.X -= 50f; // Scrolled up / forward
            }
            else if (scrollDelta < 0)
            {
                CameraPosition.X += 50f; // Scrolled down / backward
            }

            CameraPosition.X = MathHelper.Clamp(CameraPosition.X, 0f, 4000f);

            // 4. Save current state for the next frame
            _previousMouseState = currentMouseState;
        }

        private void DrawLine(SpriteBatch spriteBatch, Vector2 start, Vector2 end, Color color, float thickness = 5f)
        {
            Vector2 edge = end - start;
            float angle = (float)System.Math.Atan2(edge.Y, edge.X);
            float length = edge.Length();

            spriteBatch.Draw(
                _pixelTexture,
                start,
                null,
                color,
                angle,
                Vector2.Zero,
                new Vector2(length, thickness), // Scale X to length, Y to thickness
                SpriteEffects.None,
                0f
            );
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin();

            // Draw HUD text
            spriteBatch.DrawString(_font, "Current Floor: Tower Ascent (Node Choice)", new Vector2(50, 50) - CameraPosition * scroll_factor, Color.White);
            spriteBatch.DrawString(_font, "Press G to go to Game Over Scene", new Vector2(50, 100) - CameraPosition * scroll_factor, Color.White);
            spriteBatch.DrawString(_font, "Press T to go to Title Scene", new Vector2(50, 150) - CameraPosition * scroll_factor, Color.White);
            spriteBatch.DrawString(_font, "Scroll to move camera left/right", new Vector2(50, 200) - CameraPosition * scroll_factor, Color.White);
            spriteBatch.DrawString(_font, "End of Map", new Vector2(3000, 50) - CameraPosition * scroll_factor, Color.White);

            // 1. Draw connection lines first (underneath nodes)
            foreach (var node in allNodes)
            {
                foreach (var nextNode in node.ConnectedNodes)
                {
                    Vector2 startPos = node.Position - CameraPosition * scroll_factor;
                    Vector2 endPos = nextNode.Position - CameraPosition * scroll_factor;

                    DrawLine(spriteBatch, startPos, endPos, Color.Purple, 2f);
                }
            }

            // 2. Draw nodes on top of the lines (using small boxes or circles made with _pixelTexture)
            foreach (var node in allNodes)
            {
                Vector2 drawPos = node.Position - CameraPosition * scroll_factor;

                // Draw a simple 32x32 square placeholder for each node centered at drawPos
                Rectangle nodeRect = new Rectangle((int)drawPos.X - 15, (int)drawPos.Y - 15, 32, 32);
                spriteBatch.Draw(_pixelTexture, nodeRect, Color.White);
            }

            spriteBatch.End();
        }

        public void UnloadContent()
        {
            // Clear textures or memory specific to this floor when leaving
        }
    }
}