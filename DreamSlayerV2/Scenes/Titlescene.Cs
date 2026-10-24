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
        private Song NightmareSong;
        Texture2D menuTexture;
        int currentMenu = 1;
        bool keyActiveUp = false;
        bool keyActiveDown = false;
        

        private string titleText = "DREAM SLAYER";
        private string startText = "Start Game";

        // รายการเก็บข้อความฝันร้ายลอย
        private List<FloatingNightmareText> _floatingTexts = new List<FloatingNightmareText>();
        private float _spawnTimer = 0f;

        public void Initialize() { }

        public void LoadContent(ContentManager content)
        {
            _font = content.Load<SpriteFont>("File");
            NatureFont = content.Load<SpriteFont>("NatureFonts");
            NightmareSong = content.Load<Song>("leberch-nightmare-611631");
            menuTexture = content.Load<Texture2D>("Blankmenu");

            
            MediaPlayer.IsRepeating = true;
            MediaPlayer.Volume = 1.0f;
            MediaPlayer.Play(NightmareSong);
        }

        public void Update(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // 1. สุ่มสร้างข้อความใหม่
            _spawnTimer += dt;
            if (_floatingTexts.Count < 4 && _spawnTimer > 1.5f)
            {
                _spawnTimer = 0f;
                SpawnRandomNightmareText();
            }

            // 2. อัปเดตตำแหน่งและความโปร่งใส (Fade away)
            for (int i = _floatingTexts.Count - 1; i >= 0; i--)
            {
                var item = _floatingTexts[i];
                item.Position += item.Velocity * dt;
                item.LifeSpan -= dt;

                if (item.LifeSpan < 1.0f)
                {
                    item.Alpha = item.LifeSpan; 
                }

                if (item.LifeSpan <= 0f)
                {
                    _floatingTexts.RemoveAt(i);
                }
            }
            KeyboardState keyboard = Keyboard.GetState();
    if (keyboard.IsKeyDown(Keys.Up))
    {
        if (keyActiveUp == true)
        {
            if (currentMenu > 1)
            {
                currentMenu = currentMenu - 1;
                keyActiveUp = false;
            }
        }
    }
    if (keyboard.IsKeyDown(Keys.Down))
    {
        if (keyActiveDown == true)
        {
            if (currentMenu < 3)
            {
                currentMenu = currentMenu + 1; 
                keyActiveDown = false;
            }
        }
    }
    // check keyup
    if (keyboard.IsKeyUp(Keys.Up))
    {
        keyActiveUp = true;
    }
    if (keyboard.IsKeyUp(Keys.Down))
    {
        keyActiveDown = true;
    }

            // เช็คปุ่มเริ่มเกมปกติ
            Vector2 startSize = _font.MeasureString(startText);
            Vector2 startPos = new Vector2(960f - (startSize.X / 2f), 918f);
            MouseState mouse = Mouse.GetState();
            Rectangle startButtonBounds = new Rectangle((int)startPos.X, (int)startPos.Y, (int)startSize.X, (int)startSize.Y);

            if (startButtonBounds.Contains(mouse.Position) && mouse.LeftButton == ButtonState.Pressed)
            {
                NodeSelectScene.Reset();
                SceneManager.ChangeScene(new SceneStory1());
            }
        }

        private void SpawnRandomNightmareText()
        {
            // สุ่มข้อความ Nightmare
            Array values = Enum.GetValues(typeof(StringNightmareText));
            StringNightmareText randomEnum = (StringNightmareText)values.GetValue(_rnd.Next(values.Length));
            string selectedText = randomEnum.ToString();

            // ตำแหน่งกึ่งกลางจอ (960, 540) ปรับแต่งด้วยช่วง +800 และ -80
            float startX = 960f + _rnd.Next(-800, 800);
            float startY = 540f + _rnd.Next(-80, 80);

            float speedX = _rnd.Next(-100, 100);
            float speedY = _rnd.Next(-50, 50);

            // สุ่มมุมหมุนระหว่าง -40 ถึง +40องศา (แปลงเป็น เรเดียน)
            float randomDegrees = _rnd.Next(-40, 41);
            float rotationAngle = MathHelper.ToRadians(randomDegrees);

            _floatingTexts.Add(new FloatingNightmareText
            {
                Text = selectedText,
                Position = new Vector2(startX, startY),
                Velocity = new Vector2(speedX, speedY),
                Rotation = rotationAngle,
                LifeSpan = 5.0f
            });
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin();

            // วาดข้อความ Nightmare แบบหมุนรอบจุดศูนย์กลาง (Centered Origin)
            foreach (var item in _floatingTexts)
            {
                Color nightmareColor = Color.DarkRed * item.Alpha;
                
                // คำนวณจุดกึ่งกลางของข้อความเพื่อให้หมุนจากตรงกลาง
                Vector2 origin = NatureFont.MeasureString(item.Text) / 2f;

                spriteBatch.DrawString(
                    NatureFont, 
                    item.Text, 
                    item.Position, 
                    nightmareColor, 
                    item.Rotation, 
                    origin, 
                    1.0f, 
                    SpriteEffects.None, 
                    0f
                );
            }

            // วาดหัวข้อและปุ่ม Start
            Vector2 titleSize = _font.MeasureString(titleText);
            Vector2 titlePos = new Vector2(960f - (titleSize.X / 2f), 50f);
            

            spriteBatch.DrawString(_font, titleText, titlePos, Color.White);
            if (currentMenu == 1)
    {
        spriteBatch.Draw(menuTexture, new Vector2(300, 200), new Rectangle(192, 0, 192, 40), Color.White);
    }
    else
    {
    spriteBatch.Draw(menuTexture, new Vector2(300, 200), new Rectangle(0, 0, 192, 40), Color.White);
    }
    if (currentMenu == 2)
    {
        spriteBatch.Draw(menuTexture, new Vector2(300, 260), new Rectangle(192, 40, 192, 40), Color.White);
    }
    else
    {
        spriteBatch.Draw(menuTexture, new Vector2(300, 260), new Rectangle(0, 40, 192, 40), Color.White);
    }
    if (currentMenu == 3)
    {
        spriteBatch.Draw(menuTexture, new Vector2(300, 320), new Rectangle(192, 80, 192, 40), Color.White);
    }
    else
    {
        spriteBatch.Draw(menuTexture, new Vector2(300, 320), new Rectangle(0, 80, 192, 40), Color.White);
    }
    


            spriteBatch.End();
        }

        public void UnloadContent()
        {
            MediaPlayer.Stop();
        }
    }

    enum StringNightmareText
    {
        Despair,
        Nightmare,
        Fear,
        Hopeless,
        Darkness,
        Suffering,
        Doom,
        Trapped,
        Pain,
        Paralyzed,
        Void,
        Gloom,
        Terror,
        Collapse,
        NoEscape
    }

    public class FloatingNightmareText
    {
        public string Text { get; set; }
        public Vector2 Position { get; set; }
        public Vector2 Velocity { get; set; }
        public float Rotation { get; set; } // มุมหมุน
        public float Alpha { get; set; } = 1.0f;
        public float LifeSpan { get; set; } = 5.0f;
    }
}