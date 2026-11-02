using System;
using DreamSlayerV2.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace DreamSlayerV2.Scenes
{
    public class BossScene : IScene
    {
        private const int AngelHealAmount = 30;
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
            "Mage:Phew after many adeveture, I finally reached the place where i can escape this dream. I can feel the power of the book in my hand",
            "???: {Roar} Human!  You dare to enter My Domain in Dreamland.",
            "???: You shall not pass!",
            "Mage: I will not let you stop me from my goal. I will defeat you! And Escape from this nightmare.",
            "???: You new born spicies. I will destroy you and take your soul for my Fun Time!",
            "???: Prepare to Die!"
        };

        private readonly string[] _phase2Texts =
        {
            "???: IMPOSSIBLE! How can you defeat me? I am one of the most powerful being in this dream world",
            "???: Let me show you my true Power!",
            "Mage: Bring it on! I do not fear you. I will defeat you and escape from this nightmare.",
            "G???? : Where is Alice?",
            "AHA!: The Phantasmoon Games extend the deepest gratitude for the big boss's generous sponsorship!"

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

        public void LoadContent()
        {
            LoadCurrentDialogue();
        }

        public bool IsPhase2Dialogue => _isPhase2Dialogue;

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

            spriteBatch.Draw(GlobalContentManager.CaveBackground, new Vector2(0, 0), Color.White);
            float approach = _dialogues.Length <= 1
                ? 1f
                : (float)currentIndex / (_dialogues.Length - 1);
            int bossSize = (int)MathHelper.Lerp(180f, 520f, approach);
            int bossX = (int)MathHelper.Lerp(1540f, 1180f, approach);
            int bossY = (int)MathHelper.Lerp(180f, 70f, approach);
            spriteBatch.Draw(GlobalContentManager.BossEnemy, new Rectangle(bossX, bossY, bossSize, bossSize), Color.White);

            Vector2 dialoguePosition = new Vector2(200, 800);
            Vector2 dialogueSize = GlobalContentManager.DefaultFont.MeasureString(displayedText);
            Rectangle dialogueBounds = new Rectangle(
                (int)dialoguePosition.X - 20,
                (int)dialoguePosition.Y - 20,
                Math.Min((int)dialogueSize.X + 40, 1500),
                (int)dialogueSize.Y + 40);
            spriteBatch.Draw(GlobalContentManager.WhitePixel, dialogueBounds, Color.Black * 0.7f);
            spriteBatch.DrawString(GlobalContentManager.DefaultFont, displayedText, dialoguePosition, Color.White);

            if (isTypingFinished)
            {
                spriteBatch.DrawString(GlobalContentManager.DefaultFont, "[Click to Continue]", new Vector2(1500, 1000), Color.Yellow);
            }

            spriteBatch.End();
        }

        public void UnloadContent() { }
    }
}