using DreamSlayerV2.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Linq;

namespace DreamSlayerV2.Scenes
{
    public class PlayerStatsScene : IScene
    {
        private static readonly Rectangle ResumeButtonBounds = new Rectangle(600, 930, 320, 68);
        private static readonly Rectangle QuitButtonBounds = new Rectangle(1000, 930, 320, 68);

        private MouseState _previousMouseState;
        private KeyboardState _previousKeyboardState;
        private readonly PlayerController _player;
        private readonly IScene _previousScene;

        public PlayerStatsScene(PlayerController player, IScene previousScene)
        {
            _player = player;
            _previousScene = previousScene;
        }

        public void Initialize()
        {
            _previousMouseState = Mouse.GetState();
            _previousKeyboardState = Keyboard.GetState();
        }

        public void LoadContent() { }

        public void Update(GameTime gameTime)
        {
            MouseState currentMouseState = Mouse.GetState();
            KeyboardState currentKeyboardState = Keyboard.GetState();
            bool isLeftClicked = currentMouseState.LeftButton == ButtonState.Pressed &&
                                 _previousMouseState.LeftButton == ButtonState.Released;
            bool resumeKeyPressed =
                (currentKeyboardState.IsKeyDown(Keys.P) && _previousKeyboardState.IsKeyUp(Keys.P)) ||
                (currentKeyboardState.IsKeyDown(Keys.Escape) && _previousKeyboardState.IsKeyUp(Keys.Escape));

            if (resumeKeyPressed ||
                (isLeftClicked && ResumeButtonBounds.Contains(currentMouseState.Position)))
            {
                ResumeGame();
            }
            else if (isLeftClicked && QuitButtonBounds.Contains(currentMouseState.Position))
            {
                SceneManager.ChangeScene(new TitleScene());
            }

            _previousMouseState = currentMouseState;
            _previousKeyboardState = currentKeyboardState;
        }

        private void ResumeGame()
        {
            SceneManager.ChangeScene(_previousScene ?? new NodeSelectScene());
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin();
            SceneUi.DrawBackground(spriteBatch);
            spriteBatch.Draw(GlobalContentManager.WhitePixel, new Rectangle(0, 0, 1920, 1080), Color.Black * 0.3f);

            DrawHeader(spriteBatch);
            DrawCharacterPanel(spriteBatch);
            DrawRunAndDeckPanel(spriteBatch);
            DrawActions(spriteBatch);

            spriteBatch.End();
        }

        private void DrawHeader(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(GlobalContentManager.WhitePixel, new Rectangle(0, 0, 1920, 8), Color.MediumPurple);
            DrawCenteredText(spriteBatch, "PAUSED", new Rectangle(0, 55, 1920, 56), Color.White, 1.4f);
            DrawCenteredText(
                spriteBatch,
                "PLAYER STATUS  /  RUN SUMMARY",
                new Rectangle(0, 120, 1920, 36),
                Color.LightSteelBlue,
                0.75f);
        }

        private void DrawCharacterPanel(SpriteBatch spriteBatch)
        {
            Rectangle panel = new Rectangle(160, 190, 720, 700);
            SceneUi.DrawPanel(spriteBatch, panel, Color.MediumPurple);

            DrawPanelHeading(spriteBatch, "CHARACTER", panel.X + 36, panel.Y + 28);
            if (_player == null)
            {
                spriteBatch.DrawString(
                    GlobalContentManager.DefaultFont,
                    "No active player data found.",
                    new Vector2(panel.X + 36, panel.Y + 100),
                    Color.IndianRed);
                return;
            }

            spriteBatch.DrawString(
                GlobalContentManager.DefaultFont,
                _player.SelectedClass.ToString().ToUpperInvariant(),
                new Vector2(panel.X + 36, panel.Y + 88),
                Color.White);
            string modeText = _player.IsNightmareMode ? "NIGHTMARE MODE" : "STANDARD MODE";
            spriteBatch.DrawString(
                GlobalContentManager.DefaultFont,
                modeText,
                new Vector2(panel.X + 36, panel.Y + 128),
                _player.IsNightmareMode ? Color.OrangeRed : Color.LightBlue);

            DrawStatBar(
                spriteBatch,
                "HEALTH",
                _player.PlayerHP,
                _player.MaxHP,
                panel.X + 36,
                panel.Y + 200,
                panel.Width - 72,
                Color.IndianRed);
            DrawStatBar(
                spriteBatch,
                "SANITY",
                _player.Sanity,
                100,
                panel.X + 36,
                panel.Y + 292,
                panel.Width - 72,
                Color.MediumPurple);

            int gridTop = panel.Y + 410;
            int cellWidth = 300;
            int cellHeight = 88;
            DrawMetricCard(spriteBatch, new Rectangle(panel.X + 36, gridTop, cellWidth, cellHeight), "LEVEL", _player.Level.ToString(), Color.Gold);
            DrawMetricCard(spriteBatch, new Rectangle(panel.X + 360, gridTop, cellWidth, cellHeight), "ENERGY", _player.Energy.ToString(), Color.Cyan);
            DrawMetricCard(spriteBatch, new Rectangle(panel.X + 36, gridTop + 112, cellWidth, cellHeight), "DEFENSE", _player.Defense.ToString(), Color.LightBlue);
            DrawMetricCard(spriteBatch, new Rectangle(panel.X + 360, gridTop + 112, cellWidth, cellHeight), "HONOR", _player.Honor.ToString(), Color.LightGoldenrodYellow);
            DrawMetricCard(spriteBatch, new Rectangle(panel.X + 36, gridTop + 224, panel.Width - 72, 76), "SOUL COINS", _player.SoulCoins.ToString(), Color.Gold);
        }

        private void DrawRunAndDeckPanel(SpriteBatch spriteBatch)
        {
            Rectangle panel = new Rectangle(920, 190, 840, 700);
            SceneUi.DrawPanel(spriteBatch, panel, Color.Cyan);
            DrawPanelHeading(spriteBatch, "RUN & DECK", panel.X + 36, panel.Y + 28);

            DrawMetricCard(spriteBatch, new Rectangle(panel.X + 36, panel.Y + 88, 232, 82), "DEPTH", GameState.CurrentDepth.ToString(), Color.MediumPurple);
            DrawMetricCard(spriteBatch, new Rectangle(panel.X + 304, panel.Y + 88, 232, 82), "RUN SCORE", GameState.CalculateCurrentScore().ToString("N0"), Color.Gold);
            DrawMetricCard(spriteBatch, new Rectangle(panel.X + 572, panel.Y + 88, 232, 82), "ENEMIES", GameState.EnemiesDefeated.ToString(), Color.IndianRed);

            DrawPanelHeading(spriteBatch, "CARD COLLECTION", panel.X + 36, panel.Y + 205);
            int deckCount = _player?.Deck?.Count ?? 0;
            int discardCount = _player?.DiscardPile?.Count ?? 0;
            int handCount = _player?.Hand?.Count(card => card != null) ?? 0;
            spriteBatch.DrawString(
                GlobalContentManager.DefaultFont,
                $"Deck {deckCount}     Hand {handCount}     Discard {discardCount}",
                new Vector2(panel.X + 36, panel.Y + 250),
                Color.LightSteelBlue);

            if (_player?.Deck == null || _player.Deck.Count == 0)
            {
                spriteBatch.DrawString(
                    GlobalContentManager.DefaultFont,
                    "No cards remaining in the draw pile.",
                    new Vector2(panel.X + 36, panel.Y + 310),
                    Color.LightGray);
                return;
            }

            var cards = _player.Deck
                .Where(card => card != null)
                .OrderBy(card => card.Name)
                .Take(10)
                .ToArray();

            for (int i = 0; i < cards.Length; i++)
            {
                int column = i / 5;
                int row = i % 5;
                Rectangle cardBounds = new Rectangle(
                    panel.X + 36 + column * 382,
                    panel.Y + 310 + row * 64,
                    360,
                    52);
                Color cardTint = cards[i].Type switch
                {
                    CardManager.CardType.Attack => new Color(90, 38, 48),
                    CardManager.CardType.Defense => new Color(38, 64, 92),
                    CardManager.CardType.Heal => new Color(38, 82, 62),
                    CardManager.CardType.Utility => new Color(78, 64, 38),
                    _ => new Color(68, 46, 88)
                };
                spriteBatch.Draw(GlobalContentManager.WhitePixel, cardBounds, cardTint * 0.86f);
                spriteBatch.DrawString(
                    GlobalContentManager.DefaultFont,
                    cards[i].Name ?? "Unnamed card",
                    new Vector2(cardBounds.X + 14, cardBounds.Y + 12),
                    Color.White);
                string typeText = cards[i].Type.ToString().ToUpperInvariant();
                Vector2 typeSize = GlobalContentManager.DefaultFont.MeasureString(typeText);
                spriteBatch.DrawString(
                    GlobalContentManager.DefaultFont,
                    typeText,
                    new Vector2(cardBounds.Right - typeSize.X - 12, cardBounds.Y + 14),
                    Color.LightSteelBlue);
            }

            if (deckCount > cards.Length)
            {
                spriteBatch.DrawString(
                    GlobalContentManager.DefaultFont,
                    $"+ {deckCount - cards.Length} more cards",
                    new Vector2(panel.X + 36, panel.Bottom - 42),
                    Color.LightGray);
            }
        }

        private void DrawActions(SpriteBatch spriteBatch)
        {
            bool resumeHovered = ResumeButtonBounds.Contains(Mouse.GetState().Position);
            bool quitHovered = QuitButtonBounds.Contains(Mouse.GetState().Position);
            SceneUi.DrawButton(spriteBatch, ResumeButtonBounds, "RESUME  [P / ESC]", resumeHovered);
            SceneUi.DrawButton(spriteBatch, QuitButtonBounds, "QUIT TO TITLE", quitHovered);
        }

        private void DrawPanelHeading(SpriteBatch spriteBatch, string text, int x, int y)
        {
            spriteBatch.DrawString(GlobalContentManager.DefaultFont, text, new Vector2(x, y), Color.LightSteelBlue);
            spriteBatch.Draw(GlobalContentManager.WhitePixel, new Rectangle(x, y + 34, 120, 2), Color.MediumPurple);
        }

        private void DrawMetricCard(SpriteBatch spriteBatch, Rectangle bounds, string label, string value, Color accent)
        {
            spriteBatch.Draw(GlobalContentManager.WhitePixel, bounds, new Color(18, 22, 34) * 0.92f);
            spriteBatch.Draw(GlobalContentManager.WhitePixel, new Rectangle(bounds.X, bounds.Y, 3, bounds.Height), accent);
            spriteBatch.DrawString(
                GlobalContentManager.DefaultFont,
                label,
                new Vector2(bounds.X + 14, bounds.Y + 8),
                Color.LightGray);
            spriteBatch.DrawString(
                GlobalContentManager.DefaultFont,
                value,
                new Vector2(bounds.X + 14, bounds.Y + 40),
                accent);
        }

        private void DrawStatBar(SpriteBatch spriteBatch, string label, int value, int maximum, int x, int y, int width, Color color)
        {
            int safeMaximum = Math.Max(1, maximum);
            float ratio = MathHelper.Clamp(value / (float)safeMaximum, 0f, 1f);
            spriteBatch.DrawString(GlobalContentManager.DefaultFont, label, new Vector2(x, y), Color.LightGray);
            string valueText = $"{value} / {maximum}";
            Vector2 valueSize = GlobalContentManager.DefaultFont.MeasureString(valueText);
            spriteBatch.DrawString(
                GlobalContentManager.DefaultFont,
                valueText,
                new Vector2(x + width - valueSize.X, y),
                Color.White);

            Rectangle bar = new Rectangle(x, y + 38, width, 18);
            spriteBatch.Draw(GlobalContentManager.WhitePixel, bar, new Color(18, 22, 34));
            if (ratio > 0f)
            {
                spriteBatch.Draw(
                    GlobalContentManager.WhitePixel,
                    new Rectangle(bar.X, bar.Y, (int)(bar.Width * ratio), bar.Height),
                    color);
            }
        }

        private void DrawCenteredText(SpriteBatch spriteBatch, string text, Rectangle bounds, Color color, float scale)
        {
            Vector2 size = GlobalContentManager.DefaultFont.MeasureString(text) * scale;
            Vector2 position = new Vector2(
                bounds.Center.X - size.X / 2f,
                bounds.Center.Y - size.Y / 2f);
            spriteBatch.DrawString(
                GlobalContentManager.DefaultFont,
                text,
                position,
                color,
                0f,
                Vector2.Zero,
                scale,
                SpriteEffects.None,
                0f);
        }

        public void UnloadContent() { }
    }
}
