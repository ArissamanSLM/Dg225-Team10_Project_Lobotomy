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
        private Texture2D _nodeCircleTexture;

        private Texture2D _shopTexture;
        private Texture2D _eliteTexture;
        private Texture2D _bossTexture;
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

            _nodeCircleTexture = CreateCircleTexture(graphicsDevice, 64);
            _shopTexture = content.Load<Texture2D>("Shop_logo");
            _eliteTexture = content.Load<Texture2D>("E_enemy2");
            _bossTexture = content.Load<Texture2D>("Boss_logo");
            _encounterTexture = content.Load<Texture2D>("enemy_logo");
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
                    Rectangle nodeRect = new Rectangle((int)node.Position.X - 32, (int)node.Position.Y - 32, 64, 64);

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
                                case NodeType.Boss:
                                case NodeType.Elite:
                                    SceneManager.ChangeScene(new BattleScene(true, null));
                                    break;
                                case NodeType.Shop:
                                case NodeType.Rest:
                                    SceneManager.ChangeScene(new ShopRestScene());
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

        private static Texture2D CreateCircleTexture(GraphicsDevice graphicsDevice, int diameter)
        {
            var texture = new Texture2D(graphicsDevice, diameter, diameter);
            var pixels = new Color[diameter * diameter];
            float center = (diameter - 1) / 2f;
            float radius = diameter / 2f - 1f;

            for (int y = 0; y < diameter; y++)
            {
                for (int x = 0; x < diameter; x++)
                {
                    float distance = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                    byte coverage = (byte)(MathHelper.Clamp(radius + 0.5f - distance, 0f, 1f) * 255f);
                    pixels[y * diameter + x] = new Color(coverage, coverage, coverage, coverage);
                }
            }

            texture.SetData(pixels);
            return texture;
        }

        private static Color GetNodeColor(NodeType type) => type switch
        {
            NodeType.Start => Color.LightSkyBlue,
            NodeType.Encounter => Color.Silver,
            NodeType.Elite => Color.OrangeRed,
            NodeType.Shop => Color.Gold,
            NodeType.Rest => Color.MediumSeaGreen,
            NodeType.Boss => Color.Crimson,
            _ => Color.White
        };

        private static string GetNodeGlyph(NodeType type) => type switch
        {
            NodeType.Start => "S",
            NodeType.Rest => "+",
            _ => "?"
        };

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

                    Color lineCol = (node.IsVisited && nextNode.IsAvailable) ? Color.Gold : new Color(78, 88, 91);
                    DrawLine(spriteBatch, startPos, endPos, lineCol, 3f);
                }
            }

            // Draw Nodes
            foreach (var node in allNodes)
            {
                Vector2 drawPos = node.Position - CameraPosition * scroll_factor;
                Color nodeColor = GetNodeColor(node.Type);
                Color ringColor = node.IsVisited
                    ? Color.Lerp(nodeColor, Color.Black, 0.5f)
                    : node.IsAvailable ? Color.White : nodeColor * 0.45f;

                Rectangle outerRect = new Rectangle((int)drawPos.X - 30, (int)drawPos.Y - 30, 60, 60);
                Rectangle innerRect = new Rectangle((int)drawPos.X - 24, (int)drawPos.Y - 24, 48, 48);
                spriteBatch.Draw(_nodeCircleTexture, outerRect, ringColor);
                spriteBatch.Draw(_nodeCircleTexture, innerRect, new Color(22, 29, 32));

                Texture2D currentTexture = node.Type switch
                {
                    NodeType.Shop => _shopTexture,
                    NodeType.Elite => _eliteTexture,
                    NodeType.Boss => _bossTexture,
                    NodeType.Encounter => _encounterTexture,
                    _ => null
                };

                if (currentTexture != null)
                {
                    float scale = Math.Min(30f / currentTexture.Width, 30f / currentTexture.Height);
                    Vector2 iconSize = new Vector2(currentTexture.Width * scale, currentTexture.Height * scale);
                    Vector2 iconPosition = drawPos - iconSize / 2f;
                    Color iconTint = node.IsVisited ? Color.Gray : node.IsAvailable ? Color.White : Color.LightGray * 0.55f;
                    spriteBatch.Draw(currentTexture, iconPosition, null, iconTint, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
                }
                else
                {
                    string glyph = GetNodeGlyph(node.Type);
                    Vector2 glyphSize = _font.MeasureString(glyph);
                    Vector2 glyphPosition = drawPos - glyphSize / 2f;
                    Color glyphColor = node.IsAvailable ? nodeColor : node.IsVisited ? Color.Gray : Color.LightGray * 0.65f;
                    spriteBatch.DrawString(_font, glyph, glyphPosition, glyphColor);
                }

                string label = node.GetArknightsLabel();
                Vector2 textSize = _font.MeasureString(label);
                Vector2 textPosition = new Vector2(drawPos.X - textSize.X / 2f, drawPos.Y + 36);
                Color textColor = node.IsVisited ? Color.Gray : node.IsAvailable ? Color.Gold : Color.LightGray;
                spriteBatch.DrawString(_font, label, textPosition + Vector2.One, Color.Black);
                spriteBatch.DrawString(_font, label, textPosition, textColor);
            }

            spriteBatch.End();
        }

        public void UnloadContent() { }
    }
}