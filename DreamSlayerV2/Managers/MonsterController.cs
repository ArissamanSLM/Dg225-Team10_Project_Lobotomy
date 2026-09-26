using System.Collections.Generic;
using System;

namespace DreamSlayerV2
{
    enum EnermyList
    {
        Unrealwolf,
        CursedFlower,
    }
    public class MonsterController
    {
        
        public int MonsterHP { get; set; }
        public int MaxHP { get; set; }
        public int MonsterBlock { get; set; }
        public int IntentDamage { get; set; }
        public string IntentType { get; set; }

        private readonly Random _rand = new Random();

        public MonsterController(Enum EnermyList, int hp)
        {
            
            MonsterHP = hp;
            MaxHP = hp;
            MonsterBlock = 0;
            DetermineNextIntent();
        }

        public void TakeDamage(int damage)
        {
            int netDamage = damage - MonsterBlock;
            if (netDamage > 0)
            {
                MonsterBlock = 0;
                MonsterHP -= netDamage;
                if (MonsterHP < 0) MonsterHP = 0;
            }
            else
            {
                MonsterBlock -= damage;
            }
        }

        public void DetermineNextIntent()
        {
            int roll = _rand.Next(1, 4);
            switch (roll)
            {
                case 1:
                    IntentType = "Attack";
                    IntentDamage = _rand.Next(3, 9);
                    break;
                case 2:
                    IntentType = "Defend";
                    IntentDamage = 0;
                    MonsterBlock += 8;
                    break;
                case 3:
                    IntentType = "Debuff";
                    IntentDamage = _rand.Next(2, 5);
                    break;
            }
        }

        public void PerformTurn(PlayerController player)
{
    if (IntentType == "Attack")
    {
        // คำนวณดาเมจที่ทะลุเกราะมา
        int netDamage = IntentDamage - player.Defense;

        if (netDamage > 0)
        {
            // ถ้าดาเมจทะลุเกราะ: ให้เกราะเหลือ 0 และเอาดาเมจส่วนที่เหลือไปลด HP
            player.Defense = 0;
            player.PlayerHP -= netDamage;
        }
        else
        {
            // ถ้าเกราะรับดาเมจได้หมด: ให้เกราะลดลงตามดาเมจที่โจมตีมา (HP ไม่ลด)
            player.Defense -= IntentDamage;
        }

        // ป้องกันไม่ให้เลือดติดลบ
        if (player.PlayerHP < 0) player.PlayerHP = 0;
    }
    else if (IntentType == "Debuff")
    {
        player.Sanity -= IntentDamage;
        if (player.Sanity < 0) player.Sanity = 0;
    }
    
    DetermineNextIntent();
}
    }
}