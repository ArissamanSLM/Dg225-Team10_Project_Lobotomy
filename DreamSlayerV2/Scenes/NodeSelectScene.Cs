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
    // Node class properly isolated inside its namespace
    public class Node
    {
        public Vector2 Position { get; set; }
        public List<Node> ConnectedNodes { get; set; } = new List<Node>();
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
        private MouseState _previousMouseState;

        // Keep track of the currently active/current node the player is standing on
        private Node _currentNode;

        public void Initialize()
        {
            _previousMouseState = Mouse.GetState();

            // 1. Create all map nodes for your 6-node demo + final Boss
            Node node1 = new Node { Position = new Vector2(300, 540) };     // Start

            Node node2M = new Node { Position = new Vector2(800, 540) };    // Part 2
            Node node2 = new Node { Position = new Vector2(800, 350) };
            Node node3 = new Node { Position = new Vector2(800, 730) };

            Node node4 = new Node { Position = new Vector2(1300, 540) };    // Part 3
            Node node4U = new Node { Position = new Vector2(1300, 350) };

            Node node5M = new Node { Position = new Vector2(1750, 540) };   // Part 4
            Node node5D = new Node { Position = new Vector2(1750, 730) };

            Node node6 = new Node { Position = new Vector2(2100, 540) };     // Part 5 (Rest/Shop)
            Node node7 = new Node { Position = new Vector2(2550, 540) };     // Part 6 (Boss)

            // 2. Link them together based on your branch paths
            node1.ConnectedNodes.Add(node2);
            node1.ConnectedNodes.Add(node2M);
            node1.ConnectedNodes.Add(node3);

            node2.ConnectedNodes.Add(node4);
            node2.ConnectedNodes.Add(node4U);
            node2M.ConnectedNodes.Add(node4);
            node3.ConnectedNodes.Add(node4U);

            node4.ConnectedNodes.Add(node5M);
            node4U.ConnectedNodes.Add(node5M);
            node4.ConnectedNodes.Add(node5D);

            node5M.ConnectedNodes.Add(node6);
            node5D.ConnectedNodes.Add(node6);

            node6.ConnectedNodes.Add(node7); // Final link to Boss

            // 3. Add all nodes to the master list so they render
            allNodes.Add(node1);
            allNodes.Add(node2M);
            allNodes.Add(node2);
            allNodes.Add(node3);
            allNodes.Add(node4);
            allNodes.Add(node4U);
            allNodes.Add(node5M);
            allNodes.Add(node5D);
            allNodes.Add(node6);
            allNodes.Add(node7);

            // Set starting active node
            _currentNode = node1;
        }

        public void LoadContent(ContentManager content)
        {
            _font = content.Load<SpriteFont>("File");
            NodeMusic = content.Load<Song>("Start_music (1)");

            if (MediaPlayer.State != MediaState.Playing)
            {
                MediaPlayer.Play(NodeMusic);
                MediaPlayer.IsRepeating = true;
            }

            var graphicsDevice = ((IGraphicsDeviceService)content.ServiceProvider.GetService(typeof(IGraphicsDeviceService))).GraphicsDevice;
            _pixelTexture = new Texture2D(graphicsDevice, 1, 1);
            _pixelTexture.SetData(new[] { Color.White });
        }

        public void Update(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.G))
            {
                SceneManager.ChangeScene(new GameOverScene(5, 3, 24, true, false, 1));
            }
            if (Keyboard.GetState().IsKeyDown(Keys.T))
            {
                SceneManager.ChangeScene(new TitleScene());
            }

            MouseState currentMouseState = Mouse.GetState();

            // Camera Scrolling via Scroll Wheel
            int scrollDelta = currentMouseState.ScrollWheelValue - _previousMouseState.ScrollWheelValue;
            if (scrollDelta > 0) CameraPosition.X -= 50f;
            else if (scrollDelta < 0) CameraPosition.X += 50f;

            CameraPosition.X = MathHelper.Clamp(CameraPosition.X, 0f, 4000f);

            // Handle Node Selection via Mouse Click
            bool isClicked = currentMouseState.LeftButton == ButtonState.Pressed && _previousMouseState.LeftButton == ButtonState.Released;
            if (isClicked)
            {
                Vector2 mouseWorldPos = new Vector2(currentMouseState.X, currentMouseState.Y) + (CameraPosition * scroll_factor);

                foreach (var node in allNodes)
                {
                    // Define bounding box for each node on screen
                    Rectangle nodeRect = new Rectangle((int)node.Position.X - 15, (int)node.Position.Y - 15, 32, 32);

                    if (nodeRect.Contains(mouseWorldPos))
                    {
                        // Check if the clicked node is a valid connected path from the current node
                        if (_currentNode.ConnectedNodes.Contains(node) || _currentNode == node)
                        {
                            _currentNode = node;
                            _currentNode.IsCompleted = true;

                            // Transition based on node type position
                            if (node == allNodes.Last()) // Node 7 (Boss)
                            {
                                SceneManager.ChangeScene(new BattleScene());
                            }
                            else
                            {
                                SceneManager.ChangeScene(new BattleScene());
                            }
                            break;
                        }
                    }
                }
            }

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
                new Vector2(length, thickness),
                SpriteEffects.None,
                0f
            );
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin();

            // Draw HUD text
            spriteBatch.DrawString(_font, "Current Floor: Tower Ascent (Node Choice)", new Vector2(50, 50) - CameraPosition * scroll_factor, Color.White);
            spriteBatch.DrawString(_font, "Press G for Game Over | Press T for Title", new Vector2(50, 90) - CameraPosition * scroll_factor, Color.White);
            spriteBatch.DrawString(_font, "Click connected white nodes to travel!", new Vector2(50, 130) - CameraPosition * scroll_factor, Color.Yellow);
            spriteBatch.DrawString(_font, "Boss Room (End)", new Vector2(2500, 480) - CameraPosition * scroll_factor, Color.Red);

            // 1. Draw connection lines first
            foreach (var node in allNodes)
            {
                foreach (var nextNode in node.ConnectedNodes)
                {
                    Vector2 startPos = node.Position - CameraPosition * scroll_factor;
                    Vector2 endPos = nextNode.Position - CameraPosition * scroll_factor;

                    DrawLine(spriteBatch, startPos, endPos, Color.Purple, 2f);
                }
            }

            // 2. Draw nodes on top
            foreach (var node in allNodes)
            {
                Vector2 drawPos = node.Position - CameraPosition * scroll_factor;
                Rectangle nodeRect = new Rectangle((int)drawPos.X - 15, (int)drawPos.Y - 15, 32, 32);

                // Highlight current or available nodes differently
                Color nodeColor = Color.White;
                if (node == _currentNode) nodeColor = Color.Cyan;
                else if (_currentNode.ConnectedNodes.Contains(node)) nodeColor = Color.LightGreen; // Available choices!

                spriteBatch.Draw(_pixelTexture, nodeRect, nodeColor);
            }

            spriteBatch.End();
        }

        public void UnloadContent() { }
    }
}