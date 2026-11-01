using DreamSlayerV2.Core;
using System.Collections.Generic;

namespace DreamSlayerV2.Core
{
    public enum GrimoireType
    {
        BookOfRage,
        BookOfNonExist,
        BookOfUltimate
    }

    public class GrimoireManager
    {
        public string Name { get; set; }
        public int EnergyCost { get; set; }
        public string Description { get; set; }
        public GrimoireType Type { get; set; }

        public GrimoireManager(GrimoireType type)
        {
            Type = type;
            switch (type)
            {
                case GrimoireType.BookOfRage:
                    Name = "Book of Rage";
                    EnergyCost = 1;
                    Description = "+10 Damage to all Attack cards this turn.";
                    break;
                case GrimoireType.BookOfNonExist:
                    Name = "Book of Non-Exist";
                    EnergyCost = 1;
                    Description = "All cards cost 0 Sanity until end of turn.";
                    break;
                case GrimoireType.BookOfUltimate:
                    Name = "Book of Ultimate";
                    EnergyCost = 2;
                    Description = "Transforms all Basic cards in hand to Ultra Rare cards.";
                    break;
            }
        }

        public bool TryCast(PlayerController player)
        {
            if (player.Energy < EnergyCost) return false;

            player.Energy -= EnergyCost;

            switch (Type)
            {
                case GrimoireType.BookOfRage:
                    foreach (var card in player.Hand)
                    {
                        if (card != null && card.Type == CardManager.CardType.Attack)
                        {
                            card.InHandDamage += 10;
                        }
                    }
                    break;

                case GrimoireType.BookOfNonExist:
                    foreach (var card in player.Hand)
                    {
                        if (card != null)
                        {
                            card.Cost = 0; // ปรับค่าใช้จ่าย Sanity เป็น 0 ในเทิร์นนี้
                        }
                    }
                    break;

                case GrimoireType.BookOfUltimate:
                    for (int i = 0; i < player.Hand.Length; i++)
                    {
                        if (player.Hand[i] != null && (player.Hand[i].Name == "Strike" || player.Hand[i].Name == "Defend"))
                        {
                            // แปลงการ์ดพื้นฐานเป็น Ultra Rare Card
                            player.Hand[i] = new CardManager(999, "Omega Apocalypse", 3, CardManager.CardType.Attack)
                            {
                                InHandDamage = 45,
                                Description = "Eldritch obliteration transformed by the Ultimate Book."
                            };
                        }
                    }
                    break;
            }
            return true;
        }
    }
}