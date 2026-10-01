using System.Collections.Generic;
using System;

namespace DreamSlayerV2
{
    public enum CharacterClassV2
    {
        Human,
        HumanDemo, 
        ShadowBind,
        PactBinder,
        TheOrbMaster,
        TheMixer,
        DreamSlayer
    }

    public class PlayerController
    {
        public int PlayerHP { get; set; }
        public int MaxHP { get; set; }
        public int Sanity { get; set; }
        public int Honor { get; set; } = 0;
        public int Energy { get; private set; }
        public CharacterClassV2 SelectedClass { get; set; }
        public List<CardManager> Deck { get; set; } = new List<CardManager>();
        public CardManager[] Hand { get; set; } = new CardManager[5];
        public List<CardManager> DiscardPile { get; set; } = new List<CardManager>(); // <-- ADD THIS
        public int Level { get; set; } = 1;
        public int SoulCoins { get; set; } = 0;

        public int[] PassiveRelics { get; set; } = new int[5];
        private readonly Random _rand = new Random();

        public PlayerController(CharacterClassV2 characterClass)
        {
            SelectedClass = characterClass;
            InitializeCharacterStats();
            Deck = CardStarterDeck.GetStarterDeck(SelectedClass);
            Shuffle();
        }

        private void InitializeCharacterStats()
        {
            switch (SelectedClass)
            {
                case CharacterClassV2.HumanDemo:
                    PlayerHP = 60; MaxHP = 60; Sanity = 100; Honor = 0; break;
                case CharacterClassV2.Human:
                    PlayerHP = 72; MaxHP = 72; Sanity = 100; Honor = 0; break;
                case CharacterClassV2.ShadowBind:
                    PlayerHP = 80; MaxHP = 80; Sanity = 100; break;
                case CharacterClassV2.PactBinder:
                    PlayerHP = 65; MaxHP = 65; Sanity = 100; break;
                case CharacterClassV2.TheOrbMaster:
                    PlayerHP = 50; MaxHP = 50; Sanity = 100; break;
                case CharacterClassV2.TheMixer:
                    PlayerHP = 50; MaxHP = 50; Sanity = 50; break;
                case CharacterClassV2.DreamSlayer:
                    PlayerHP = 90; MaxHP = 90; Sanity = 100; Honor = 0; break;
            }
        }

        public void DrawCard()
        {
            if (Deck.Count <= 0) return;

            CardManager card = Deck[0];
            Deck.RemoveAt(0);

            for (int i = 0; i < Hand.Length; i++)
            {
                if (Hand[i] == null)
                {
                    Hand[i] = card;
                    break;
                }
            }
        }

        public void UseCard(int handIndex)
        {
            if (IsValidHandIndex(handIndex))
            {
                // Send the played card to the Discard Pile
                DiscardPile.Add(Hand[handIndex]);
                Hand[handIndex] = null;
            }
        }

        public void ReturnCardToDeck(int handIndex)
        {
            if (IsValidHandIndex(handIndex))
            {
                Deck.Add(Hand[handIndex]);
                Hand[handIndex] = null;
            }
        }

        public void Shuffle()
        {
            int n = Deck.Count;
            while (n > 1)
            {
                n--;
                int k = _rand.Next(n + 1);
                CardManager value = Deck[k];
                Deck[k] = Deck[n];
                Deck[n] = value;
            }
        }

        public void PlayerTurn()
        {
            Energy = Sanity >= 80 ? 5 : (Sanity >= 50 ? 3 : 2);

            for (int i = 0; i < Hand.Length; i++)
            {
                if (Hand[i] == null) DrawCard();
            }
        }

        public void EndTurn()
        {
            // Clear remaining hand into discard pile
            for (int i = 0; i < Hand.Length; i++)
            {
                if (Hand[i] != null)
                {
                    DiscardPile.Add(Hand[i]);
                    Hand[i] = null;
                }
            }

            // FIXED: Moved outside the loop so it only happens once per turn end, not 5 times!
            Sanity += 30;

            // Optional cap so Sanity doesn't go over max (assuming 100 max for HumanDemo)
            if (Sanity > 100) Sanity = 100;

            // If deck runs out, recycle discard pile back into the deck
            if (Deck.Count == 0)
            {
                Deck.AddRange(DiscardPile);
                DiscardPile.Clear();
                Shuffle();  
            }
        }

        private bool IsValidHandIndex(int index) => index >= 0 && index < Hand.Length && Hand[index] != null;
    }
}