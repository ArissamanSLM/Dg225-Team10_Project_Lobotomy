using DreamSlayerV2.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace DreamSlayerV2.Scenes
{
    public class NodeSelectScene : IScene
    {
        private Vector2 CameraPosition = Vector2.Zero;
        private Vector2 scroll_factor = new Vector2(1.0f, 1f);
        private int _scrollAccumulator = 0;
        private Vector2 LockPosition;
        private bool _isCameraLocked;
        private int _viewportWidth;
        private MouseState _previousMouseState;
        private KeyboardState _previousKeyboardState;

        private static List<MapNode> allNodes = new List<MapNode>();
        private static MapNode _currentNode = null;

        public static void Reset()
        {
            allNodes.Clear();
            _currentNode = null;
        }

        public static void SaveRun(bool pendingStartReward = false)
        {
            if (_currentNode == null || allNodes.Count == 0) return;
            GameSaveManager.SaveRun(GameState.CurrentPlayer, allNodes, _currentNode, pendingStartReward);
        }

        public static void RestoreRun(SavedRunData run)
        {
            allNodes = GameSaveManager.RestoreNodes(run);
            _currentNode = allNodes[run.CurrentNodeIndex];
            GameState.RestoreRun(run);
        }

        public void Initialize()
        {
            _previousMouseState = Mouse.GetState();
            _previousKeyboardState = Keyboard.GetState();

            if (allNodes.Count == 0)
            {
                allNodes = MapGenerator.GenerateLovecraftianMap(stages: 14, startY: 540);

                if (_currentNode == null)
                {
                    _currentNode = allNodes[0];
                    _currentNode.IsAvailable = true;
                }
            }

            SaveRun();
        }

        public void LoadContent()
        {
            var graphicsDevice = GlobalContentManager.GraphicsDevice;
            _viewportWidth = graphicsDevice.Viewport.Width;
            if (_currentNode != null)
            {
                CameraPosition.X = MathHelper.Clamp(_currentNode.Position.X - _viewportWidth / 2f, 0f, 4000f);
            }
        }

        public void Update(GameTime gameTime)
        {
            KeyboardState currentKeyboardState = Keyboard.GetState();
            if (currentKeyboardState.IsKeyDown(Keys.G))
            {
                SceneManager.ChangeScene(new GameOverScene(true, false));
            }
            if (currentKeyboardState.IsKeyDown(Keys.T))
            {
                SceneManager.ChangeScene(new TitleScene());
            }

            MouseState currentMouseState = Mouse.GetState();

            // Camera scroll
            
            
            int scrollDelta = currentMouseState.ScrollWheelValue - _previousMouseState.ScrollWheelValue;
            _scrollAccumulator += scrollDelta;

            const int ScrollThreshold = 120; // Standard mouse wheel notch value

            if (scrollDelta != 0)
            {
                _isCameraLocked = false;
            }

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

            if (currentKeyboardState.IsKeyDown(Keys.D5) && !_previousKeyboardState.IsKeyDown(Keys.D5))
            {
                LockCameraToStage(5);
            }
            else if (currentKeyboardState.IsKeyDown(Keys.D0) && !_previousKeyboardState.IsKeyDown(Keys.D0))
            {
                LockCameraToStage(10);
            }
            else if (_isCameraLocked)
            {
                CameraPosition = LockPosition;
            }

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

                            if (node.Type != NodeType.Start &&
                                node.Type != NodeType.Escape &&
                                node.Type != NodeType.GoDeeper)
                            {
                                GameState.RecordNodeVisited();
                            }

                            // Scene Switching ใน NodeSelectScene.Cs
                            switch (node.Type)
                            {
                                case NodeType.Escape:
                                    SceneManager.ChangeScene(new GameOverScene(true, false));
                                    return;
                                case NodeType.GoDeeper:
                                    AdvanceDepth();
                                    return;
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
                                    int structureRoll = Random.Shared.Next(100);
                                    IScene restScene = structureRoll switch
                                    {
                                        < 40 => new SafeHouseRest(),
                                        < 60 => new StorageRest(),
                                        < 75 => new BlacksmithRest(),
                                        _ => new HospitalRest()
                                    };
                                    SceneManager.ChangeScene(restScene);
                                    break;
                                case NodeType.Event:
                                    SceneManager.ChangeScene(EventSceneFactory.CreateRandomEvent());
                                    break;
                                case NodeType.Start:
                                    SaveRun(pendingStartReward: true);
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
            _previousKeyboardState = currentKeyboardState;
        }

        private void AdvanceDepth()
        {
            GameState.CurrentDepth++;
            allNodes = MapGenerator.GenerateLovecraftianMap(stages: 14, startY: 540);
            _currentNode = allNodes[0];
            _currentNode.IsVisited = true;
            foreach (MapNode connected in _currentNode.ConnectedNodes)
            {
                connected.IsAvailable = true;
            }

            CameraPosition.X = 0f;
            SaveRun();
            SceneManager.ChangeScene(new NodeSelectScene());
        }

        private void LockCameraToStage(int stage)
        {
            var stageNodes = allNodes.FindAll(node => node.Stage == stage);
            if (stageNodes.Count == 0)
            {
                throw new InvalidOperationException($"The map does not contain stage {stage}.");
            }

            float stageCenterX = 0f;
            foreach (var node in stageNodes)
            {
                stageCenterX += node.Position.X;
            }

            stageCenterX /= stageNodes.Count;
            CameraPosition.X = MathHelper.Clamp(stageCenterX - _viewportWidth / 2f, 0f, 4000f);
            LockPosition = CameraPosition;
            _isCameraLocked = true;
        }
        
            
            
        

        private void DrawLine(SpriteBatch spriteBatch, Vector2 start, Vector2 end, Color color, float thickness = 3f)
        {
            Vector2 edge = end - start;
            float angle = (float)Math.Atan2(edge.Y, edge.X);
            float length = edge.Length();

            spriteBatch.Draw(GlobalContentManager.WhitePixel, start, null, color, angle, Vector2.Zero, new Vector2(length, thickness), SpriteEffects.None, 0f);
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
            NodeType.Escape => Color.LightGreen,
            NodeType.GoDeeper => Color.DeepSkyBlue,
            _ => Color.White
        };

        private static string GetNodeGlyph(NodeType type) => type switch
        {
            NodeType.Start => "S",
            NodeType.Rest => "+",
            NodeType.Escape => "E",
            NodeType.GoDeeper => ">",
            _ => "?"
        };

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin();
            
            // --- 1. Draw Parallax Backgrounds (2 ภาพ ขนาด 2500x1080) ---
            float bgWidth = 2500f;
            float parallaxFactor = 0.7f; // ความเร็วการเลื่อนฉากหลัง    เทียบกับกล้อง

            for (int i = 0;     i < GlobalContentManager.MapBackgrounds.Length; i++)
            {
                if (GlobalContentManager.MapBackgrounds[i] != null)
                {
                    // คำนวณตำแหน่ง X ของภาพแต่ละแผ่นไม่ให้เกิดช่องว่างและเลื่อนตามกล้อง
                    float drawX = (i * bgWidth) - (CameraPosition.X * scroll_factor.X * parallaxFactor);

                    spriteBatch.Draw(
                        GlobalContentManager.MapBackgrounds[i],
                        new Rectangle((int)drawX, 0, (int)bgWidth, 1080),
                        Color.White
                    );
                }
            }
            spriteBatch.DrawString(GlobalContentManager.DefaultFont, "Tower Ascent Map", new Vector2(50, 50) - CameraPosition * scroll_factor, Color.White);
            spriteBatch.DrawString(GlobalContentManager.DefaultFont, "Use Mouse Scroll Wheel to view full map! Press 5/0 to lock on stages 5/10.", new Vector2(50, 85) - CameraPosition * scroll_factor, Color.Yellow);
            string runStatus = $"Depth {GameState.CurrentDepth}  Score {GameState.CalculateCurrentScore()}" +
                (GameState.CurrentPlayer.IsNightmareMode ? "  NIGHTMARE" : "");
            spriteBatch.DrawString(GlobalContentManager.DefaultFont, runStatus, new Vector2(50, 120), Color.Cyan);

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
                    NodeType.Shop => GlobalContentManager.ShopIcon,
                    NodeType.Elite => GlobalContentManager.EliteIcon,
                    NodeType.Boss => GlobalContentManager.BossIcon,
                    NodeType.Encounter => GlobalContentManager.EncounterIcon,
                    NodeType.Escape => GlobalContentManager.Portal,
                    NodeType.GoDeeper => GlobalContentManager.NextFloorLogo,
                    NodeType.Event => null,
                    _ => null
                };
                spriteBatch.Draw(
    GlobalContentManager.NodeCircle,
    drawPos,
    null,
    ringColor,
    0f,
    new Vector2(GlobalContentManager.NodeCircle.Width / 2f, GlobalContentManager.NodeCircle.Height / 2f),
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
                    Vector2 glyphSize = GlobalContentManager.DefaultFont.MeasureString(glyph);
                    Vector2 glyphPosition = drawPos - glyphSize / 2f;
                    Color glyphColor = node.IsAvailable ? nodeColor : node.IsVisited ? Color.Gray : Color.LightGray * 0.65f;
                    spriteBatch.DrawString(GlobalContentManager.DefaultFont, glyph, glyphPosition, glyphColor);
                }

                string label = node.GetArknightsLabel();
                Vector2 textSize = GlobalContentManager.DefaultFont.MeasureString(label);
                Vector2 textPosition = new Vector2(drawPos.X - textSize.X / 2f, drawPos.Y + 36);
                Color textColor = node.IsVisited ? Color.Gray : node.IsAvailable ? Color.Gold : Color.LightGray;
                spriteBatch.DrawString(GlobalContentManager.DefaultFont, label, textPosition + Vector2.One, Color.Black);
                spriteBatch.DrawString(GlobalContentManager.DefaultFont, label, textPosition, textColor);
            }
            
            
            spriteBatch.End();
        }

        public void UnloadContent() { }
    }
}