using System;
using DreamSlayerV2.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DreamSlayerV2.Scenes
{
    internal static class SceneUi
    {
        public static void DrawBackground(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(GlobalContentManager.CaveBackground, new Rectangle(0, 0, 1920, 1080), Color.White);
            spriteBatch.Draw(GlobalContentManager.WhitePixel, new Rectangle(0, 0, 1920, 1080), Color.Black * 0.48f);
        }

        public static void DrawPanel(SpriteBatch spriteBatch, Rectangle bounds, Color tint)
        {
            spriteBatch.Draw(GlobalContentManager.WhitePixel, bounds, Color.Black * 0.78f);
            spriteBatch.Draw(GlobalContentManager.WhitePixel, new Rectangle(bounds.X, bounds.Y, bounds.Width, 3), tint);
            spriteBatch.Draw(GlobalContentManager.WhitePixel, new Rectangle(bounds.X, bounds.Bottom - 3, bounds.Width, 3), tint);
            spriteBatch.Draw(GlobalContentManager.WhitePixel, new Rectangle(bounds.X, bounds.Y, 3, bounds.Height), tint);
            spriteBatch.Draw(GlobalContentManager.WhitePixel, new Rectangle(bounds.Right - 3, bounds.Y, 3, bounds.Height), tint);
        }

        public static void DrawButton(SpriteBatch spriteBatch, Rectangle bounds, string text, bool hovered, bool enabled = true)
        {
            Color color = !enabled
                ? Color.DimGray
                : hovered ? Color.DarkSlateBlue : new Color(28, 35, 48);
            Color border = enabled ? (hovered ? Color.LightYellow : Color.SlateGray) : Color.Gray;
            spriteBatch.Draw(GlobalContentManager.WhitePixel, bounds, color * 0.94f);
            spriteBatch.Draw(GlobalContentManager.WhitePixel, new Rectangle(bounds.X, bounds.Y, bounds.Width, 2), border);
            spriteBatch.Draw(GlobalContentManager.WhitePixel, new Rectangle(bounds.X, bounds.Bottom - 2, bounds.Width, 2), border);

            SpriteFont font = GlobalContentManager.DefaultFont;
            Vector2 size = font.MeasureString(text);
            float scale = Math.Min(1f, (bounds.Width - 32f) / Math.Max(1f, size.X));
            Vector2 position = new Vector2(
                bounds.X + (bounds.Width - size.X * scale) / 2f,
                bounds.Y + (bounds.Height - size.Y * scale) / 2f);
            spriteBatch.DrawString(font, text, position, enabled ? Color.White : Color.LightGray, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
        }

        public static void DrawPlayerStatus(SpriteBatch spriteBatch, PlayerController player)
        {
            if (player == null) return;

            DrawPanel(spriteBatch, new Rectangle(1450, 35, 420, 120), Color.Cyan);
            spriteBatch.DrawString(GlobalContentManager.DefaultFont, $"Soul Coins: {player.SoulCoins}", new Vector2(1475, 55), Color.Cyan);
            spriteBatch.DrawString(GlobalContentManager.DefaultFont, $"HP: {player.PlayerHP}/{player.MaxHP}    Sanity: {player.Sanity}", new Vector2(1475, 100), Color.LightGreen);
        }

        public static void DrawWrappedText(SpriteBatch spriteBatch, string text, Vector2 position, float maxWidth, Color color, float lineHeight = 38f)
        {
            SpriteFont font = GlobalContentManager.DefaultFont;
            string[] paragraphs = text.Split('\n');
            float y = position.Y;

            foreach (string paragraph in paragraphs)
            {
                string line = "";
                foreach (string word in paragraph.Split(' ', StringSplitOptions.RemoveEmptyEntries))
                {
                    string candidate = line.Length == 0 ? word : line + " " + word;
                    if (line.Length > 0 && font.MeasureString(candidate).X > maxWidth)
                    {
                        spriteBatch.DrawString(font, line, new Vector2(position.X, y), color);
                        y += lineHeight;
                        line = word;
                    }
                    else
                    {
                        line = candidate;
                    }
                }

                if (line.Length > 0)
                {
                    spriteBatch.DrawString(font, line, new Vector2(position.X, y), color);
                    y += lineHeight;
                }
            }
        }
    }
}
