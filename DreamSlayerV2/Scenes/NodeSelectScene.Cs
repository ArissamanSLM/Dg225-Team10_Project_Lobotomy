using DreamSlayerV2.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;

namespace DreamSlayerV2.Scenes
{
    public class NodeSelectScene : IScene
    {
        private SpriteFont _font;
        private Vector2 CameraPosition = Vector2.Zero;
        private Vector2 scroll_factor = new Vector2(1.0f, 1f);
        private Texture2D _pixelTexture;

        private Texture2D _shopTexture;
        private Texture2D _eliteTexture;
        private Texture2D _bossTexture;
        private Texture2D _eventTexture;
        private Texture2D _encounterTexture;
        

        private Song NodeMusic;
        private MouseState _previousMouseState;

        private static List<MapNode> allNodes = new List<MapNode>();
        private static MapNode _currentNode = null;

        public void Initialize()
        {
            _previousMouseState = Mouse.GetState();

            if (allNodes.Count == 0)
            {
                allNodes = MapGenerator.GenerateLovecraftianMap(stages: 13, startY: 540);

                if (_currentNode == null)
                {
                    _currentNode = allNodes[0];
                    _currentNode.IsAvailable = true;
                }
            }
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

            try
            {
                _shopTexture = content.Load<Texture2D>("Shop_logo");
                _eliteTexture = content.Load<Texture2D>("E_enemy2");
                _bossTexture = content.Load<Texture2D>("Boss_logo");
                _eventTexture = content.Load<Texture2D>("EventIcon");
                _encounterTexture = content.Load<Texture2D>("enermy_logo");
            }
            catch { }
        }

        public void Update(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.G))
            {
                SceneManager.ChangeScene(new GameOverScene(5, 3, 24, true, false ,0));
            }
            if (Keyboard.GetState().IsKeyDown(Keys.T))
            {
                SceneManager.ChangeScene(new TitleScene());
            }

            MouseState currentMouseState = Mouse.GetState();

            // Camera scroll
            int scrollDelta = currentMouseState.ScrollWheelValue - _previousMouseState.ScrollWheelValue;
            if (scrollDelta > 0) CameraPosition.X -= 60f;
            else if (scrollDelta < 0) CameraPosition.X += 60f;

            CameraPosition.X = MathHelper.Clamp(CameraPosition.X, 0f, 4000f);

            // Node Click Selection
            bool isClicked = currentMouseState.LeftButton == ButtonState.Pressed && _previousMouseState.LeftButton == ButtonState.Released;
            if (isClicked)
            {
                Vector2 mouseWorldPos = new Vector2(currentMouseState.X, currentMouseState.Y) + (CameraPosition * scroll_factor);

                foreach (var node in allNodes)
                {
                    Rectangle nodeRect = new Rectangle((int)node.Position.X - 20, (int)node.Position.Y - 20, 40, 40);

                    if (nodeRect.Contains(mouseWorldPos))
                    {
                        // Check if node is available to travel to
                        if (node.IsAvailable)
                        {
                            _currentNode.IsVisited = true;
                            _currentNode.IsAvailable = false;

                            // Mark all nodes in current stage unavailable
                            foreach (var n in allNodes) n.IsAvailable = false;

                            // Set chosen node as current and mark its connected paths as available
                            _currentNode = node;
                            _currentNode.IsVisited = true;

                            foreach (var connected in _currentNode.ConnectedNodes)
                            {
                                connected.IsAvailable = true;
                            }

                            // Scene Switching
                            switch (node.Type)
                            {
                                case NodeType.Victory:
                                    SceneManager.ChangeScene(new GameOverScene(13,3,5,true,false,1));
                                    break;
                                case NodeType.Boss:
                                case NodeType.Elite:
                                    SceneManager.ChangeScene(new BattleScene(true, null));
                                    break;
                                case NodeType.Shop:
                                    SceneManager.ChangeScene(new ShopRestScene());
                                    break;
                                case NodeType.Event:
                                    SceneManager.ChangeScene(EventSceneFactory.CreateRandomEvent());
                                    break;
                                case NodeType.Encounter:
                                case NodeType.Start:
                                default:
                                    SceneManager.ChangeScene(new BattleScene(false, null));
                                    break;
                            }
                            break;
                        }
                    }
                }
            }

            _previousMouseState = currentMouseState;
        }

        private void DrawLine(SpriteBatch spriteBatch, Vector2 start, Vector2 end, Color color, float thickness = 3f)
        {
            Vector2 edge = end - start;
            float angle = (float)Math.Atan2(edge.Y, edge.X);
            float length = edge.Length();

            spriteBatch.Draw(_pixelTexture, start, null, color, angle, Vector2.Zero, new Vector2(length, thickness), SpriteEffects.None, 0f);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin();

            spriteBatch.DrawString(_font, "Tower Ascent Map", new Vector2(50, 50) - CameraPosition * scroll_factor, Color.White);
            spriteBatch.DrawString(_font, "Use Mouse Scroll Wheel to view full map!", new Vector2(50, 85) - CameraPosition * scroll_factor, Color.Yellow);

            // Draw Paths
            foreach (var node in allNodes)
            {
                foreach (var nextNode in node.ConnectedNodes)
                {
                    Vector2 startPos = node.Position - CameraPosition * scroll_factor;
                    Vector2 endPos = nextNode.Position - CameraPosition * scroll_factor;

                    Color lineCol = (node.IsVisited && nextNode.IsAvailable) ? Color.Gold : Color.DarkSlateBlue;
                    DrawLine(spriteBatch, startPos, endPos, lineCol, 2.5f);
                }
            }

            // Draw Nodes
            foreach (var node in allNodes)
            {
                Vector2 drawPos = node.Position - CameraPosition * scroll_factor;
                Rectangle nodeRect = new Rectangle((int)drawPos.X - 20, (int)drawPos.Y - 20, 40, 40);

                Color tintColor = Color.DarkGray; // Default locked/unreachable state
                if (node.IsVisited)
                {
                    tintColor = Color.Gray;
                }
                else if (node == _currentNode)
                {
                    tintColor = Color.Cyan;
                }
                else if (node.IsAvailable)
                {
                    tintColor = Color.LightGreen; // Ready to click!
                }

                Texture2D currentTexture = node.Type switch
                {
                    NodeType.Shop => _shopTexture,
                    NodeType.Elite => _eliteTexture,
                    NodeType.Boss => _bossTexture,
                    NodeType.Event => _eventTexture,
                    NodeType.Encounter => _encounterTexture,
                    _ => null
                };

                if (currentTexture != null)
                    spriteBatch.Draw(currentTexture, nodeRect, tintColor);
                else
                    spriteBatch.Draw(_pixelTexture, nodeRect, tintColor);

                string alertText = node.Type switch
                {
                    NodeType.Shop => "[ REST / SHOP ]",
                    NodeType.Elite => "!! WARNING: ELITE !!",
                    NodeType.Boss => "*** FINAL BOSS ***",
                    NodeType.Event => "? EVENT ?",
                    NodeType.Start => "START",
                    _ => node.Name
                };

                Color textColor = node.IsAvailable ? Color.Gold : Color.Gray;

                Vector2 textSize = _font.MeasureString(alertText);
                Vector2 textPosition = new Vector2(drawPos.X - (textSize.X / 2), drawPos.Y - 45);

                spriteBatch.DrawString(_font, alertText, textPosition, textColor);
            }

            spriteBatch.End();
        }

        public void UnloadContent() { }
    }
}