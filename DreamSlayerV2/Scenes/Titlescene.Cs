using DreamSlayerV2.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;

namespace DreamSlayerV2.Scenes
{
    public class TitleScene : IScene
    {
        private Random _rnd = new Random();
        private Texture2D BgTitle;
        private SpriteFont _font;
        private SpriteFont NatureFont;
        public static SoundEffectInstance _soundInstance;

        private string titleText = "DREAM SLAYER";
        private string startText = "Start Game";

        // รายการเก็บข้อความลอยไม่เกิน 4 ข้อความ
        private List<FloatingHopeText> _floatingTexts = new List<FloatingHopeText>();
        private float _spawnTimer = 0f;

        public void Initialize() { }

        public void LoadContent(ContentManager content)
        {
            _font = content.Load<SpriteFont>("File");
            NatureFont = content.Load<SpriteFont>("NatureFonts");
            SoundEffect badEffect = content.Load<SoundEffect>("TitleMusic");
            _soundInstance = badEffect.CreateInstance();
            _soundInstance.Volume = 1.0f;
            _soundInstance.Pitch = 0.03f;
            _soundInstance.IsLooped = true;
            _soundInstance.Play();
            MediaPlayer.IsRepeating = true;
        }

        public void Update(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // 1. สุ่มสร้างข้อความใหม่เมื่อมีน้อยกว่า 4 ข้อความ
            _spawnTimer += dt;
            if (_floatingTexts.Count < 4 && _spawnTimer > 1.5f)
            {
                _spawnTimer = 0f;
                SpawnRandomHopeText();
            }

            // 2. อัปเดตตำแหน่งและความโปร่งใส (Fade away) ของแต่ละข้อความ
            for (int i = _floatingTexts.Count - 1; i >= 0; i--)
            {
                var item = _floatingTexts[i];
                item.Position += item.Velocity * dt;
                item.LifeSpan -= dt;

                // ค่อยๆ ลด Alpha ลงเมื่อใกล้หมดอายุ
                if (item.LifeSpan < 1.0f)
                {
                    item.Alpha = item.LifeSpan; 
                }

                // ลบออกเมื่อหมดเวลาหรือลอยออกนอกจอ
                if (item.LifeSpan <= 0f)
                {
                    _floatingTexts.RemoveAt(i);
                }
            }

            // เช็คปุ่มเริ่มเกมปกติ
            Vector2 startSize = _font.MeasureString(startText);
            Vector2 startPos = new Vector2(960f - (startSize.X / 2f), 918f);
            MouseState mouse = Mouse.GetState();
            Rectangle startButtonBounds = new Rectangle((int)startPos.X, (int)startPos.Y, (int)startSize.X, (int)startSize.Y);

            if (startButtonBounds.Contains(mouse.Position) && mouse.LeftButton == ButtonState.Pressed)
            {
                
                SceneManager.ChangeScene(new SceneStory1());
            }
        }

        private void SpawnRandomHopeText()
        {
            // ดึงค่าจาก Enum สุ่มมาแสดง
            Array values = Enum.GetValues(typeof(StringScrollingText));
            StringScrollingText randomEnum = (StringScrollingText)values.GetValue(_rnd.Next(values.Length));
            string selectedText = randomEnum.ToString();

            // สุ่มฝั่งเกิด (ซ้ายไปขวา หรือ ขวาไปซ้าย)
            bool movingRight = _rnd.Next(2) == 0;
            float startX = movingRight ? -200f : 2100f;
            float startY = _rnd.Next(200, 700); // สุ่มความสูงในจอ

            // ** ปรับความเร็วตรงนี้ให้มากขึ้น (เช่น 300 ถึง 500) **
            float speedX = movingRight ? _rnd.Next(200, 400) : _rnd.Next(-400, -200);

            _floatingTexts.Add(new FloatingHopeText
            {
                Text = selectedText,
                Position = new Vector2(startX, startY),
                Velocity = new Vector2(speedX, 0),
                LifeSpan = 5.0f
            });
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin();

            // วาดข้อความลอยทั้งหมดด้วย foreach (สีเขียว และรองรับ Fade Alpha)
            foreach (var item in _floatingTexts)
            {
                Color Black = Color.Black * item.Alpha;
                spriteBatch.DrawString(NatureFont, item.Text, item.Position, Black);
            }

            // วาดหัวข้อและปุ่ม Start
            Vector2 titleSize = _font.MeasureString(titleText);
            Vector2 titlePos = new Vector2(960f - (titleSize.X / 2f), 50f);
            Vector2 startSize = _font.MeasureString(startText);
            Vector2 startPos = new Vector2(960f - (startSize.X / 2f), 918f);

            spriteBatch.DrawString(_font, titleText, titlePos, Color.White);
            spriteBatch.DrawString(_font, startText, startPos, Color.White);

            spriteBatch.End();
        }

        public void UnloadContent() { }
    }

    enum StringScrollingText
    {
        Life,
        Youcandoit,
        DontGiveUp,
        KeepGoing,
        NeverSurrender,
        StayStrong,
        BelieveInYourself,
        YouAreCapable,
        Hope,
        Courage,
        Morale,
        Determination,
        JustKeepPushing,
        ConquerTheDreamland,
        DreamSlayer
    }
    public class FloatingHopeText
{
    public string Text { get; set; }
    public Vector2 Position { get; set; }
    public Vector2 Velocity { get; set; }
    public float Alpha { get; set; } = 1.0f;
    public float LifeSpan { get; set; } = 5.0f; // อยู่ได้กี่วินาทีก่อนจะจางหาย
}
}