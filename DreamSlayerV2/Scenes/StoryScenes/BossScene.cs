using System;
using DreamSlayerV2.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace DreamSlayerV2.Scenes
{
    public class BossScene : IScene
    {
        private const int AngelHealAmount = 30;

        private SpriteFont _font;
        private MouseState _currentMouseState;
        private MouseState _previousMouseState;
        private readonly MapNode _targetNode;
        private readonly PlayerController _player;
        private readonly BattleScene _battleToResume;
        private readonly string[] _dialogues;
        private readonly bool _isPhase2Dialogue;
        private bool _angelBuffGranted;

        private readonly string[] _bossTexts =
        {
            "Mage: Phew after 13 Long Nodes, I finally reached the final node. I can feel the power of the book in my hand.",
            "???: You Dare to Enter my Domain of Dreamland.",
            "???: You shall not pass!",
            "Mage: I will not let you stop me from my goal. I will defeat you! And Escape from this nightmare.",
            "???: Hahaha, You are just a weak mage. I will destroy you and take your power for myself.",
            "???: Prepare to Die!"
        };

        private readonly string[] _phase2Texts =
        {
            "???: IMPOSSIBLE! How can you defeat me? I am the most powerful being in this world.",
            "???: Let me show you my Real Power!",
            "Mage: Bring it on! I do not fear you. I will defeat you and escape from this nightmare.",
            "Angel: Player, you are here! Take my help and defeat this evil being.",
            "Cthulhu: Perish!!!"
        };

        private int currentIndex = 0;
        private string displayedText = "";
        private float timer = 0f;
        private const float CharInterval = 0.03f;
        private int charIndex = 0;
        private bool isTypingFinished = false;

        public BossScene(MapNode targetNode = null)
        {
            _targetNode = targetNode;
            _dialogues = _bossTexts;
        }

        public BossScene(PlayerController player, BattleScene battleToResume)
        {
            _player = player;
            _battleToResume = battleToResume;
            _dialogues = _phase2Texts;
            _isPhase2Dialogue = true;
        }

        public void Initialize()
        {
            _previousMouseState = Mouse.GetState();
            _currentMouseState = _previousMouseState;
        }

        public void LoadContent(ContentManager content)
        {
            _font = content.Load<SpriteFont>("File");
            LoadCurrentDialogue();
        }

        private void LoadCurrentDialogue()
        {
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

            if (!isTypingFinished)
            {
                timer += dt;
                string targetText = _dialogues[currentIndex];

                if (timer >= CharInterval)
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

                if (isMouseClicked)
                {
                    displayedText = targetText;
                    charIndex = targetText.Length;
                    isTypingFinished = true;
                    return;
                }
            }

            if (isTypingFinished && isMouseClicked)
            {
                if (_isPhase2Dialogue && currentIndex == 3 && !_angelBuffGranted)
                {
                    _player.PlayerHP = Math.Min(_player.MaxHP, _player.PlayerHP + AngelHealAmount);
                    _angelBuffGranted = true;
                }

                currentIndex++;

                if (currentIndex < _dialogues.Length)
                {
                    LoadCurrentDialogue();
                }
                else
                {
                    if (_isPhase2Dialogue)
                    {
                        SceneManager.ChangeScene(_battleToResume);
                    }
                    else
                    {
                        SceneManager.ChangeScene(new BattleScene(bossRoom: true, nodeToComplete: _targetNode));
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