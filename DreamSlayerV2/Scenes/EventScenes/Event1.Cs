using DreamSlayerV2.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace DreamSlayerV2.Scenes
{
    public static class EventSceneFactory
    {
        private static readonly Random Rng = new Random();

        public static IEvent CreateRandomEvent()
        {
            switch (Rng.Next(5))
            {
                case 0:
                    return new Event1();
                case 1:
                    return new Event4();
                case 2:
                    return new Event5();
                case 3:
                    return new Event6();
                default:
                    return new Event7();
            }
        }
    }

    public abstract class EventChoiceSceneBase : IEvent
    {
        private readonly List<string> _choices;
        protected SpriteFont Font;
        protected MouseState PreviousMouseState;
        protected Vector2 DialogPos = new Vector2(50, 50);
        protected Vector2 ChoicesStartPos = new Vector2(50, 220);

        protected EventChoiceSceneBase(EventTag tag, string dialogText, params string[] choices)
        {
            Tag = tag;
            DialogText = dialogText;
            _choices = new List<string>(choices);
        }

        public EventTag Tag { get; }
        public string DialogText { get; }

        public void Initialize()
        {
            PreviousMouseState = Mouse.GetState();
        }

        public void LoadContent(ContentManager content)
        {
            Font = content.Load<SpriteFont>("File");
        }

        public void Update(GameTime gameTime)
        {
            MouseState currentMouseState = Mouse.GetState();

            for (int i = 0; i < _choices.Count; i++)
            {
                Vector2 choiceSize = Font.MeasureString(_choices[i]);
                Vector2 choicePos = new Vector2(ChoicesStartPos.X, ChoicesStartPos.Y + (i * 60));
                Rectangle choiceBounds = new Rectangle((int)choicePos.X, (int)choicePos.Y, (int)choiceSize.X, (int)choiceSize.Y);

                bool isHovered = choiceBounds.Contains(currentMouseState.Position);
                bool isClicked = isHovered && currentMouseState.LeftButton == ButtonState.Pressed && PreviousMouseState.LeftButton == ButtonState.Released;

                if (isClicked)
                {
                    ExecuteConsequence(i);
                    return;
                }
            }

            PreviousMouseState = currentMouseState;
        }

        public abstract void ExecuteConsequence(int choiceIndex);

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin();
            spriteBatch.DrawString(Font, DialogText, DialogPos, Color.Red);

            MouseState currentMouseState = Mouse.GetState();
            for (int i = 0; i < _choices.Count; i++)
            {
                Vector2 choicePos = new Vector2(ChoicesStartPos.X, ChoicesStartPos.Y + (i * 60));
                Vector2 choiceSize = Font.MeasureString(_choices[i]);
                Rectangle choiceBounds = new Rectangle((int)choicePos.X, (int)choicePos.Y, (int)choiceSize.X, (int)choiceSize.Y);
                Color choiceColor = choiceBounds.Contains(currentMouseState.Position) ? Color.LightYellow : Color.Yellow;
                spriteBatch.DrawString(Font, _choices[i], choicePos, choiceColor);
            }

            spriteBatch.End();
        }

        public void UnloadContent() { }
    }

    public class Event1 : EventChoiceSceneBase
    {
        public Event1() : base(
            EventTag.Lucky,
            "You found a Fountain while exploring this tower!\nYou may make a wish and receive one of three blessings:",
            "1. Toss a coin: Gain 50 Soul Coins",
            "2. Drink deeply: Heal until Max HP",
            "3. Take the Foundation Ring: Gain a permanent tower buff")
        {
        }

        public override void ExecuteConsequence(int choiceIndex)
        {
            PlayerController player = GameState.CurrentPlayer;

            switch (choiceIndex)
            {
                case 0:
                    player.SoulCoins += 50;
                    break;
                case 1:
                    player.PlayerHP = player.MaxHP;
                    break;
                case 2:
                    player.MaxHP += 5;
                    player.PlayerHP = player.MaxHP;
                    player.Sanity = Math.Min(100, player.Sanity + 10);
                    break;
            }

            SceneManager.ChangeScene(new NodeSelectScene());
        }
    }

    public class Event4 : EventChoiceSceneBase
    {
        public Event4() : base(
            EventTag.Normal,
            "A tiny shrine glows in the dark. Three offerings are laid before you:",
            "1. Leave an offering: Gain 25 Soul Coins",
            "2. Pray for protection: Recover 20 HP",
            "3. Take the blessed charm: Gain +5 Max HP for the rest of this tower")
        {
        }

        public override void ExecuteConsequence(int choiceIndex)
        {
            PlayerController player = GameState.CurrentPlayer;

            switch (choiceIndex)
            {
                case 0:
                    player.SoulCoins += 25;
                    break;
                case 1:
                    player.PlayerHP = Math.Min(player.MaxHP, player.PlayerHP + 20);
                    break;
                case 2:
                    player.MaxHP += 5;
                    player.PlayerHP = player.MaxHP;
                    break;
            }

            SceneManager.ChangeScene(new NodeSelectScene());
        }
    }

    public class Event5 : EventChoiceSceneBase
    {
        public Event5() : base(
            EventTag.Normal,
            "A whispering alcove hides a strange omen. The air feels heavier than a nightmare:",
            "1. Follow the whisper: Gain 30 Soul Coins",
            "2. Listen and breathe: Restore 15 Sanity",
            "3. Break the omen: Gain a small shield and recover 10 HP")
        {
        }

        public override void ExecuteConsequence(int choiceIndex)
        {
            PlayerController player = GameState.CurrentPlayer;

            switch (choiceIndex)
            {
                case 0:
                    player.SoulCoins += 30;
                    break;
                case 1:
                    player.Sanity = Math.Min(100, player.Sanity + 15);
                    break;
                case 2:
                    player.PlayerHP = Math.Min(player.MaxHP, player.PlayerHP + 10);
                    player.Sanity = Math.Min(100, player.Sanity + 10);
                    break;
            }

            SceneManager.ChangeScene(new NodeSelectScene());
        }
    }

    public class Event6 : EventChoiceSceneBase
    {
        public Event6() : base(
            EventTag.Normal,
            "A tired merchant waits in the shadows with an unusual deal:",
            "1. Buy a charm: Gain 20 Soul Coins and +2 Max HP",
            "2. Trade your fear: Heal to full HP",
            "3. Refuse the deal: Gain 15 Sanity and a safer route forward")
        {
        }

        public override void ExecuteConsequence(int choiceIndex)
        {
            PlayerController player = GameState.CurrentPlayer;

            switch (choiceIndex)
            {
                case 0:
                    player.SoulCoins += 20;
                    player.MaxHP += 2;
                    player.PlayerHP = player.MaxHP;
                    break;
                case 1:
                    player.PlayerHP = player.MaxHP;
                    break;
                case 2:
                    player.Sanity = Math.Min(100, player.Sanity + 15);
                    break;
            }

            SceneManager.ChangeScene(new NodeSelectScene());
        }
    }

    public class Event7 : EventChoiceSceneBase
    {
        public Event7() : base(
            EventTag.Nightmare,
            "You found something you were not supposed to see.\nSuddenly you faint, and when you wake, you are trapped in a cage.\nFour doors stand before you; only one leads to survival:",
            "1. Red Door: Terrible curse — lose 25 HP and 20 Sanity",
            "2. Black Door: The cage opens and you lose 30 Soul Coins",
            "3. Silver Door: A shadow emerges and you must fight an Elite",
            "4. White Door: Survive and escape with 15 Soul Coins and a heal")
        {
        }

        public override void ExecuteConsequence(int choiceIndex)
        {
            PlayerController player = GameState.CurrentPlayer;

            switch (choiceIndex)
            {
                case 0:
                    player.PlayerHP = Math.Max(0, player.PlayerHP - 25);
                    player.Sanity = Math.Max(0, player.Sanity - 20);
                    SceneManager.ChangeScene(new NodeSelectScene());
                    return;
                case 1:
                    player.SoulCoins = Math.Max(0, player.SoulCoins - 30);
                    SceneManager.ChangeScene(new NodeSelectScene());
                    return;
                case 2:
                    SceneManager.ChangeScene(new BattleScene(true, null));
                    return;
                case 3:
                    player.SoulCoins += 15;
                    player.PlayerHP = Math.Min(player.MaxHP, player.PlayerHP + 20);
                    SceneManager.ChangeScene(new NodeSelectScene());
                    return;
            }

            SceneManager.ChangeScene(new NodeSelectScene());
        }
    }
}