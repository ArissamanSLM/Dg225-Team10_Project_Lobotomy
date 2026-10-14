using DreamSlayerV2.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace DreamSlayerV2.Scenes
{
    public class BossScene : IScene
    {
        private SpriteFont _font;
        private MouseState _currentMouseState;
        private MouseState _previousMouseState;

        // บทสนทนาเฟส 1
        private string[] bossTexts = new string[]
        {
            "Mage: Phew after 13 Long Nodes, I finally reached the final node. I can feel the power of the book in my hand.",
            "???: You Dare to Enter my Domain of Dreamland.",
            "???: You shall not pass!",
            "Mage: I will not let you stop me from my goal. I will defeat you! And Escape from this nightmare.",
            "???: Hahaha, You are just a weak mage. I will destroy you and take your power for myself.",
            "???: Prepare to Die!"
        };

        // บทสนทนาเฟส 2
        private string[] phase2Texts = new string[]
        {
            "???: IMPOSSIBLE! How can you defeat me? I am the most powerful being in this world.",
            "???: Let me show you my Real Power!",
            "Mage: Bring it on! I do not fear you. I will defeat you and escape from this nightmare.",
            "Angel: Player you are Here! Here take my help And defeat this evil being.",
            "Catulu: Perish!!!"
        };

        private int currentIndex = 0;
        private bool isPhase2 = false; // ตัวแปรบอกสถานะว่าอยู่เฟส 2 หรือยัง

        // ตัวแปรสำหรับทำเอฟเฟกต์พิมพ์ดีด (Typewriter)
        private string displayedText = "";
        private float timer = 0f;
        private float charInterval = 0.03f; // ความเร็วในการพิมพ์แต่ละตัวอักษร
        private int charIndex = 0;
        private bool isTypingFinished = false;

        public void Initialize() { }

        public void LoadContent(ContentManager content)
        {
            _font = content.Load<SpriteFont>("File");
            LoadCurrentDialogue(); // โหลดข้อความประโยคแรก
        }

        private void LoadCurrentDialogue()
        {
            string currentFullText = isPhase2 ? phase2Texts[currentIndex] : bossTexts[currentIndex];
            displayedText = "";
            charIndex = 0;
            timer = 0f;
            isTypingFinished = false;
        }

        public void Update(GameTime gameTime)
        {
            _previousMouseState = _currentMouseState;
            _currentMouseState = Mouse.GetState();

            bool isMouseClicked = _currentMouseState.LeftButton == ButtonState.Pressed &&
                                  _previousMouseState.LeftButton == ButtonState.Released;

            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // 1. จัดการเอฟเฟกต์พิมพ์ทีละตัวอักษร
            if (!isTypingFinished)
            {
                timer += dt;
                string targetText = isPhase2 ? phase2Texts[currentIndex] : bossTexts[currentIndex];

                if (timer >= charInterval)
                {
                    timer = 0f;
                    charIndex++;
                    if (charIndex <= targetText.Length)
                    {
                        displayedText = targetText.Substring(0, charIndex);
                    }
                    else
                    {
                        isTypingFinished = true;
                    }
                }

                // ถ้าผู้เล่นคลิกตอนกำลังพิมพ์อยู่ ให้แสดงข้อความทั้งหมดทันที
                if (isMouseClicked)
                {
                    displayedText = targetText;
                    charIndex = targetText.Length;
                    isTypingFinished = true;
                    return; // ข้ามการเปลี่ยนบรรทัดรอบนี้ไปก่อน ให้ผู้เล่นคลิกอีกทีเพื่อไปประโยคถัดไป
                }
            }

            // 2. เมื่อพิมพ์เสร็จแล้ว และผู้เล่นคลิกเมาส์ -> ไปประโยคถัดไป หรือเปลี่ยนเฟส / เข้าสู่ฉากสู้จริง
            if (isTypingFinished && isMouseClicked)
            {
                currentIndex++;

                int maxIndex = isPhase2 ? phase2Texts.Length : bossTexts.Length;

                if (currentIndex < maxIndex)
                {
                    LoadCurrentDialogue();
                }
                else
                {
                    // จบบทสนทนาของชุดนั้นๆ
                    if (!isPhase2)
                    {
                        // จบเฟส 1 -> เปลี่ยนไปเริ่มเฟส 2
                        isPhase2 = true;
                        currentIndex = 0;
                        LoadCurrentDialogue();
                    }
                    else
                    {
                        // จบเฟส 2 ทุกประโยคแล้ว -> เข้าสู่หน้าจอต่อสู้บอสจริง (BattleScene ห้องบอส)
                        // SceneManager.ChangeScene(new BattleScene(bossRoom: true, targetNode));
                    }
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin();

            // วาดกล่องข้อความหรือตัวหนังสือบทสนทนา (มีเอฟเฟกต์พิมพ์ดีด)
            spriteBatch.DrawString(_font, displayedText, new Vector2(200, 800), Color.White);

            if (isTypingFinished)
            {
                spriteBatch.DrawString(_font, "▼ Click to Continue...", new Vector2(1500, 1000), Color.Yellow);
            }

            spriteBatch.End();
        }

        public void UnloadContent() { }
    }
}