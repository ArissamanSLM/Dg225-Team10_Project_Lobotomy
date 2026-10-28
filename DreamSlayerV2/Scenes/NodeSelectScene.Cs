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
        private int _scrollAccumulator = 0;
        private Texture2D _shopTexture;
        private Texture2D _eliteTexture;
        private Texture2D _bossTexture;
        private Texture2D _encounterTexture;
        private Texture2D _eventTexture;
        private Texture2D[] BgNodeTexture;
        

        private Song NodeMusic;
        private MouseState _previousMouseState;

        private static List<MapNode> allNodes = new List<MapNode>();
        private static MapNode _currentNode = null;

        public static void Reset()
        {
            allNodes.Clear();
            _currentNode = null;
        }

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
            BgNodeTexture = new Texture2D[2];
BgNodeTexture[0] = content.Load<Texture2D>("MapLeft_DreamSlayer");
BgNodeTexture[1] = content.Load<Texture2D>("MapRight_DreamSlayer");
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
                _encounterTexture = content.Load<Texture2D>("enemy_logo");
                _eventTexture = content.Load<Texture2D>("EventIcon");
                
            }
            catch { }
            _nodeCircleTexture = CreateCircleTexture(graphicsDevice, 64);
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
            _scrollAccumulator += scrollDelta;

            const int ScrollThreshold = 120; // Standard mouse wheel notch value

            if (_scrollAccumulator >= ScrollThreshold)
            {
                CameraPosition.X -= 60f;          // Moves exactly once per notch
                _scrollAccumulator -= ScrollThreshold; // Keeps any leftover smooth-scroll momentum
            }
            else if (_scrollAccumulator <= -ScrollThreshold)
            {
                CameraPosition.X += 60f;          // Moves exactly once per notch
                _scrollAccumulator += ScrollThreshold;
            }

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

                            // Scene Switching ใน NodeSelectScene.Cs
                            switch (node.Type)
                            {
                                case NodeType.Boss:
                                    // เมื่อคลิกโดนโหนด Boss (Stage 13) ให้เปิดฉากคัตซีนสนทนาก่อนสู้
                                    SceneManager.ChangeScene(new BossScene(node));
                                    break;
                                case NodeType.Elite:
                                    SceneManager.ChangeScene(new BattleScene(true, node));
                                    break;
                                case NodeType.Shop:
                                    SceneManager.ChangeScene(new Shop());
                                    break;
                                case NodeType.Rest:
                                    SceneManager.ChangeScene(new Rest());
                                    break;
                                case NodeType.Event:
                                    SceneManager.ChangeScene(EventSceneFactory.CreateRandomEvent());
                                    break;
                                case NodeType.Start:
                                    SceneManager.ChangeScene(new NodeStartScene());
                                    break;
                                case NodeType.Encounter:
                                default:
                                    SceneManager.ChangeScene(new BattleScene(false, null));
                                    break;
                            }
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
            NodeType.Event => Color.MediumPurple,
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
            
            // --- 1. Draw Parallax Backgrounds (2 ภาพ ขนาด 2500x1080) ---
            float bgWidth = 2500f;
            float parallaxFactor = 0.5f; // ความเร็วการเลื่อนฉากหลัง    เทียบกับกล้อง

            for (int i = 0;     i < BgNodeTexture.Length; i++)
            {
                if (BgNodeTexture[i] != null)
                {
                    // คำนวณตำแหน่ง X ของภาพแต่ละแผ่นไม่ให้เกิดช่องว่างและเลื่อนตามกล้อง
                    float drawX = (i * bgWidth) - (CameraPosition.X * scroll_factor.X * parallaxFactor);

                    spriteBatch.Draw(
                        BgNodeTexture[i], 
                        new Rectangle((int)drawX, 0, (int)bgWidth, 1080), 
                        Color.White
                    );
                }
            }
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
                
                Color tintColor = Color.LightGray; // Default locked/unreachable state
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
                    NodeType.Encounter => _encounterTexture,
                    NodeType.Event => _eventTexture,
                    _ => null
                };
                spriteBatch.Draw(
    _nodeCircleTexture,
    drawPos,
    null,
    ringColor,
    0f,
    new Vector2(_nodeCircleTexture.Width / 2f, _nodeCircleTexture.Height / 2f),
    1f,
    SpriteEffects.None,
    0f);
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