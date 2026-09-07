using System;
using System.Collections.Generic;
using System.Linq;

namespace DreamSlayerV2
{
    public enum Rarity { Common, Uncommon, Rare, Epic, Legendary }
    public enum RelicSlot { Passive, PassiveOrActive }

    public class RelicDefinition
    {
        public string Name { get; }
        public Rarity Rarity { get; }
        public RelicSlot Slot { get; }
        public int[] TierValues { get; }
        public string Description { get; }

        public RelicDefinition(string name, Rarity rarity, RelicSlot slot, int[] tierValues, string description = "")
        {
            Name = name;
            Rarity = rarity;
            Slot = slot;
            TierValues = tierValues ?? Array.Empty<int>();
            Description = description;
        }
    }

    public class RelicInstance
    {
        public RelicDefinition Definition { get; }
        public int Tier { get; }

        public RelicInstance(RelicDefinition def, int tier)
        {
            Definition = def;
            Tier = Math.Max(1, Math.Min(tier, def.TierValues.Length));
        }
    }

    // Simple manager for relics / items — stores available definitions and player's picked relics for a run.
    public class ItemManager
    {
        private readonly List<RelicDefinition> _definitions = new List<RelicDefinition>();
        private readonly List<RelicInstance> _playerRelics = new List<RelicInstance>();

        public ItemManager()
        {
            InitializeDefaultRelics();
        }

        public IReadOnlyList<RelicDefinition> Definitions => _definitions;
        public IReadOnlyList<RelicInstance> PlayerRelics => _playerRelics;

        public void AddRelicToPlayer(string relicName, int tier = 1)
        {
            var def = _definitions.FirstOrDefault(d => string.Equals(d.Name, relicName, StringComparison.OrdinalIgnoreCase));
            if (def == null) throw new ArgumentException($"Relic not found: {relicName}");
            var inst = new RelicInstance(def, tier);
            _playerRelics.Add(inst);
        }

        public bool RemovePlayerRelic(string relicName)
        {
            var inst = _playerRelics.FirstOrDefault(r => string.Equals(r.Definition.Name, relicName, StringComparison.OrdinalIgnoreCase));
            if (inst == null) return false;
            _playerRelics.Remove(inst);
            return true;
        }

        public bool HasPlayerRelic(string relicName) => _playerRelics.Any(r => string.Equals(r.Definition.Name, relicName, StringComparison.OrdinalIgnoreCase));

        private void InitializeDefaultRelics()
        {
            // Common (all usable in Passive slot)
            _definitions.Add(new RelicDefinition("Relic Of Max HP", Rarity.Common, RelicSlot.Passive, new int[] { 5, 8, 10, 12, 15 }, "Increase Max HP by tier value"));
            _definitions.Add(new RelicDefinition("Relic Of Energy", Rarity.Common, RelicSlot.Passive, new int[] { 1, 1, 2, 2, 3 }, "Increase starting energy or energy capacity"));
            _definitions.Add(new RelicDefinition("Relic Of Heal", Rarity.Common, RelicSlot.Passive, new int[] { 2, 2, 3, 4, 5 }, "Heal X% after boss defeated"));
            _definitions.Add(new RelicDefinition("Relic Of Berserk", Rarity.Common, RelicSlot.Passive, new int[] { 1, 1, 2, 2, 3 }, "Gain Strength at start of encounter, decays by 1 per turn"));
            _definitions.Add(new RelicDefinition("Relic Of Protection", Rarity.Common, RelicSlot.Passive, new int[] { 4, 6, 8, 10, 12 }, "Gain Block at start of encounter"));

            // Uncommon
            _definitions.Add(new RelicDefinition("Relic Of Hold Card", Rarity.Uncommon, RelicSlot.PassiveOrActive, new int[] { 1, 1, 2, 2, 3 }, "Hold extra cards between turns"));
            _definitions.Add(new RelicDefinition("Relic Of Double", Rarity.Uncommon, RelicSlot.PassiveOrActive, new int[] { 30, 28, 25, 22, 20 }, "Every X cards allow the player to act again"));
            _definitions.Add(new RelicDefinition("Relic Of Anti Block", Rarity.Uncommon, RelicSlot.PassiveOrActive, new int[] { 3, 6, 9, 12, 15 }, "If you didn't play defense this turn, gain X defense"));
            _definitions.Add(new RelicDefinition("Relic Of Anti Fight", Rarity.Uncommon, RelicSlot.PassiveOrActive, new int[] { 1, 1, 2, 2, 3 }, "Custom anti-fight behaviour (tweak in code)") );

            // Rare
            _definitions.Add(new RelicDefinition("Sanity Control", Rarity.Rare, RelicSlot.Passive, new int[] { 1 }, "Prevent sanity loss from monster hits"));
            _definitions.Add(new RelicDefinition("Ice Cream Energy", Rarity.Rare, RelicSlot.Passive, new int[] { 1, 2, 3 }, "Keep energy between turns; +2 free energy when you have none (max 3 levels)"));

            // Epic
            _definitions.Add(new RelicDefinition("Constant Attack", Rarity.Epic, RelicSlot.Passive, new int[] { 1 }, "Every 3 attacks gain Strength"));
            _definitions.Add(new RelicDefinition("Constant Defend", Rarity.Epic, RelicSlot.Passive, new int[] { 1 }, "Every 3 defends gain a Forcefield (halves block loss)"));

            // Legendary / Ultra
            _definitions.Add(new RelicDefinition("Ultra Card", Rarity.Legendary, RelicSlot.Passive, new int[] { 1 }, "Every 5 cards, the next card casts twice at cost 1"));
            _definitions.Add(new RelicDefinition("Poison Burst", Rarity.Legendary, RelicSlot.Passive, new int[] { 3, 6, 9, 12, 12 }, "Apply poison to all enemies (values per tier; last is 2-turn duration)"));
            _definitions.Add(new RelicDefinition("Stun", Rarity.Legendary, RelicSlot.Passive, new int[] { 2, 3, 999 }, "At first encounter, stun enemies (tier controls count/coverage and duration)") );
        }
    }
}
