using System.Collections.Generic;
using System;
using System.Linq;
using DreamSlayerV2.Core;
using DreamSlayerV2.Scenes;
using Microsoft.Xna.Framework.Graphics;

namespace DreamSlayerV2
{
    public enum CharacterClassV2
    {
        Human,
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
        public int Energy { get; set; }
        public int Defense { get; set;} = 0;
        public CharacterClassV2 SelectedClass { get; set; }
        public List<CardManager> Deck { get; set; } = new List<CardManager>();
        public CardManager[] Hand { get; set; } = new CardManager[5];
        public List<CardManager> DiscardPile { get; set; } = new List<CardManager>(); // <-- ADD THIS
        public int Level { get; set; } = 1;
        public int SoulCoins { get; set; } = 0;
        public int IncreaseInHandDamageValue { get; set; } = 0;
        public int TurnCount { get; set; }
        public int[] PassiveRelics { get; set; } = new int[5];
        private readonly Random _rand = new Random();
        public bool IsNightmareMode { get; set; }
        public bool HasStartBonus { get; set; }

        public PlayerController(CharacterClassV2 characterClass, bool isNightmareMode = false)
        {
            SelectedClass = characterClass;
            IsNightmareMode = isNightmareMode;
            InitializeCharacterStats();
            int hpBonus = UpgradeOrbPermanent.GetMaxHPBonus() + UpgradeOrbPermanent.GetAscendedStartingHpBonus();
            MaxHP += hpBonus;
            PlayerHP += hpBonus;
            SoulCoins = UpgradeOrbPermanent.GetStartingCoinsBonus() + UpgradeOrbPermanent.GetAscendedStartingCoinsBonus();
            Energy = (IsNightmareMode ? 3 : 5) + UpgradeOrbPermanent.GetStartingEnergyBonus();
            Deck = CardStarterDeck.GetStarterDeck(SelectedClass);
            if (UpgradeOrbPermanent.ClockPlusUnlocked)
            {
                TryAddCard(new CardManager(10, "Clock+", 1, CardManager.CardType.Utility), out _);
            }
            Shuffle();
        }

        private void InitializeCharacterStats()
        {
            switch (SelectedClass)
            {
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

        public bool OwnsUniqueCard(CardManager card)
        {
            ArgumentNullException.ThrowIfNull(card);
            string family = card.UniqueFamily;
            return family != null && EnumerateOwnedCards().Any(ownedCard => ownedCard.UniqueFamily == family);
        }

        public int GetDuplicateCardSalePrice(CardManager card)
        {
            ArgumentNullException.ThrowIfNull(card);
            return OwnsUniqueCard(card) ? Shop.CalculateSellPrice(card) * 2 : 0;
        }

        public bool TryAddCard(CardManager card, out int duplicateSaleValue)
        {
            return TryAddCard(card, null, out duplicateSaleValue);
        }

        public bool TryAddCard(CardManager card, int? duplicateSalePrice, out int duplicateSaleValue)
        {
            ArgumentNullException.ThrowIfNull(card);
            duplicateSaleValue = 0;

            if (OwnsUniqueCard(card))
            {
                duplicateSaleValue = duplicateSalePrice ?? Shop.CalculateSellPrice(card) * 2;
                SoulCoins += duplicateSaleValue;
                return false;
            }

            Deck.Add(card);
            return true;
        }

        public void ResolveUniqueCardDuplicates()
        {
            var ownedFamilies = new HashSet<string>();

            for (int i = Deck.Count - 1; i >= 0; i--)
            {
                if (RemoveDuplicateCard(Deck[i], ownedFamilies))
                {
                    SoulCoins += Shop.CalculateSellPrice(Deck[i]) * 2;
                    Deck.RemoveAt(i);
                }
            }

            for (int i = DiscardPile.Count - 1; i >= 0; i--)
            {
                if (RemoveDuplicateCard(DiscardPile[i], ownedFamilies))
                {
                    SoulCoins += Shop.CalculateSellPrice(DiscardPile[i]) * 2;
                    DiscardPile.RemoveAt(i);
                }
            }

            for (int i = 0; i < Hand.Length; i++)
            {
                CardManager card = Hand[i];
                if (card != null && RemoveDuplicateCard(card, ownedFamilies))
                {
                    SoulCoins += Shop.CalculateSellPrice(card) * 2;
                    Hand[i] = null;
                }
            }
        }

        private IEnumerable<CardManager> EnumerateOwnedCards()
        {
            foreach (CardManager card in Deck) yield return card;
            foreach (CardManager card in DiscardPile) yield return card;
            foreach (CardManager card in Hand)
            {
                if (card != null) yield return card;
            }
        }

        private static bool RemoveDuplicateCard(CardManager card, HashSet<string> ownedFamilies)
        {
            string family = card.UniqueFamily;
            return family != null && !ownedFamilies.Add(family);
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
            TurnCount++;
            Energy = Sanity >= 80 ? 5 : (Sanity >= 50 ? 3 : 2);

            for (int i = 0; i < Hand.Length; i++)
            {
                if (Hand[i] == null) DrawCard();
            }
        }

        public void StartEncounterTurn()
        {
            foreach (CardManager card in Hand)
            {
                if (card != null) Deck.Add(card);
            }

            Array.Clear(Hand, 0, Hand.Length);
            Deck.AddRange(DiscardPile);
            DiscardPile.Clear();
            Shuffle();
            PlayerTurn();
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

            Sanity += TurnCount % 2 == 1 ? 30 : 40;

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
                public void DrawCards(int amount)
        {
            for (int i = 0; i < amount; i++)
            {
                DrawCard();
            }
        }
        public void increaseInHandDamage(int amount)
        {
            IncreaseInHandDamageValue += amount;

            // วนลูปเพิ่มดาเมจให้กับการ์ดประเภท Attack ที่อยู่ในมือปัจจุบันทั้งหมดทันที (ถ้าต้องการ)
            foreach (var card in Hand)
            {
                if (card != null && card.Type == CardManager.CardType.Attack)
                {
                    card.InHandDamage += amount;
                }
            }
        }
        private bool IsValidHandIndex(int index) => index >= 0 && index < Hand.Length && Hand[index] != null;
    }
    
}