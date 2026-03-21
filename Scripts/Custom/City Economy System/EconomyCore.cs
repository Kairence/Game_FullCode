using System;
using Server;
using Server.Items;

namespace Server.Misc
{
    public enum NpcJobClass { Peasant, Crafter, Warrior, Mage, Noble }
    public enum NpcRank { Novice, Journeyman, Expert, Master }
    public enum ItemCategory { None, Essential, Tool, Luxury }

    public abstract class VirtualAgent
    {
        public NpcJobClass JobClass { get; set; }
        public NpcRank Rank { get; set; }
        public int Gold { get; set; }
        public int Hunger { get; set; } // 공통 Hunger 추가

        public VirtualAgent(NpcJobClass job, NpcRank rank) 
        { 
            JobClass = job; 
            Rank = rank; 
            Gold = CalculateStartingGold(job, rank);
            Hunger = Utility.RandomMinMax(0, 30);
        }

        public VirtualAgent(GenericReader reader) 
        { 
            int v = reader.ReadInt(); 
            JobClass = (NpcJobClass)reader.ReadInt(); 
            Rank = (NpcRank)reader.ReadInt(); 
            Gold = reader.ReadInt(); 
            Hunger = reader.ReadInt();
        }

        public virtual void Serialize(GenericWriter writer) 
        { 
            writer.Write(0); 
            writer.Write((int)JobClass); 
            writer.Write((int)Rank); 
            writer.Write(Gold); 
            writer.Write(Hunger);
        }

        public static int CalculateStartingGold(NpcJobClass job, NpcRank rank)
        {
            int baseGold = job switch { NpcJobClass.Peasant => 100, NpcJobClass.Crafter => 300, NpcJobClass.Warrior => 500, NpcJobClass.Mage => 800, NpcJobClass.Noble => 2000, _ => 100 };
            int rankMultiplier = rank switch { NpcRank.Novice => 1, NpcRank.Journeyman => 2, NpcRank.Expert => 5, NpcRank.Master => 10, _ => 1 };
            return baseGold * rankMultiplier;
        }

        public ItemCategory ClassifyItem(Item item)
        {
            if (item is Food || item is BaseBeverage || item is Backpack || item is Pouch) return ItemCategory.Essential;
            if (item is BaseJewel) return ItemCategory.Luxury;
            return ItemCategory.None;
        }
    }
}