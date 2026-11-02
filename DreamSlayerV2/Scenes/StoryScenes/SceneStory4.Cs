using System;
using DreamSlayerV2.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DreamSlayerV2.Scenes
{
    public class SceneStory4 : IScene
    {
        private float _appearingEffect;

        private readonly StoryDialogue _dialogue = new StoryDialogue(
            "As the book opened, power burst out from it. Ah... I am 100% sure it had a \"DO NOT OPEN\" tape and I missed it. Oh well.");

        public void Initialize()
        {
            _dialogue.Initialize();
            _appearingEffect = 0f;
            if (SceneStory1._soundInstance != null)
            {
                SceneStory1._soundInstance.Pitch = -0.2f;
            }
        }

        public void LoadContent() { }

        public void Update(GameTime gameTime)
        {
            if (_appearingEffect < 1f)
            {
                _appearingEffect = Math.Min(
                    1f,
                    _appearingEffect + (float)gameTime.ElapsedGameTime.TotalSeconds * 2f);
                return;
            }

            if (_dialogue.Update(gameTime))
            {
                SceneManager.ChangeScene(new SceneStory5());
            }
        }

public void Draw(SpriteBatch spriteBatch)
{
    GraphicsDevice device = spriteBatch.GraphicsDevice;
    device.Clear(Color.Black);

    // 1. วาด Background ปกติผ่าน StoryDialogue
    _dialogue.Draw(spriteBatch, GlobalContentManager.SceneStoryBackground4, _appearingEffect);

    // 2. เพิ่มเลเยอร์สีดำทับ (Overlay) เพื่อให้ฉากดูมืด/ทึบลงตามเอฟเฟกต์
    // (ปรับค่า 0.7f หรือ 0.9f ตามความมืดที่คุณต้องการ)
    if (_appearingEffect > 0f && GlobalContentManager.WhitePixel != null)
    {
        spriteBatch.Begin();
        // ใช้ _appearingEffect มาคูณเพื่อให้ความมืดจางหายไปพร้อมกับอนิเมชันเปิดฉาก
        float overlayAlpha = (1f - _appearingEffect) * 0.8f; 
        spriteBatch.Draw(
            GlobalContentManager.WhitePixel,
            new Rectangle(0, 0, 1920, 1080),
            Color.Black * overlayAlpha);
        spriteBatch.End();
    }

    // 3. วาด Portal ตรงกลางจอตามเดิม
    float portalOpacity = 1f - _appearingEffect;
    if (portalOpacity > 0f && GlobalContentManager.Portal != null)
    {
        float portalSize = 520f - 340f * _appearingEffect;
        var portalBounds = new Rectangle(
            (int)((1920f - portalSize) / 2f),
            (int)((1080f - portalSize) / 2f),
            (int)portalSize,
            (int)portalSize);

        spriteBatch.Begin();
        spriteBatch.Draw(GlobalContentManager.Portal, portalBounds, Color.White * portalOpacity);
        spriteBatch.End();
    }
}
        public void UnloadContent() { }
    }
}
