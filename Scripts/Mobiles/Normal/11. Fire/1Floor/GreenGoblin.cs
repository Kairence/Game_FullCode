using System;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a goblin corpse")]
    public class GreenGoblin : BaseCreature
    {
        [Constructable]
        public GreenGoblin()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Name = "a green goblin";
            Body = 723;
            BaseSoundID = 0x600;

            /* Green Goblin - Fame 1,800 / Karma -1,800 */
			this.SetStr(120, 160);       
			this.SetDex(150, 200);       

			this.SetHits(850, 1850); 
			this.SetStam(150, 200);      

			SetAttackSpeed(2.4);
			SetDamage(8, 14);   

			this.SetDamageType(ResistanceType.Physical, 70);
			this.SetDamageType(ResistanceType.Poison, 30);

			this.SetResistance(ResistanceType.Physical, 25, 35);
			this.SetResistance(ResistanceType.Fire, 50, 60);     
			this.SetResistance(ResistanceType.Poison, 75, 75);   // 독 면역 (Max 75%)

			this.SetSkill(SkillName.Wrestling, 75.0, 90.0);
			this.SetSkill(SkillName.Poisoning, 85.0, 105.0); // 수준 높은 독 공격

			this.VirtualArmor = 4;       
			this.Tamable = false;

			this.Fame = 1800;           
			this.Karma = -1800;
        }

        public GreenGoblin(Serial serial)
            : base(serial)
        {
        }

        public override int GetAngerSound() { return 0x600; }
        public override int GetIdleSound() { return 0x600; }
        public override int GetAttackSound() { return 0x5FD; }
        public override int GetHurtSound() { return 0x5FF; }
        public override int GetDeathSound() { return 0x5FE; }

        public override bool CanRummageCorpses { get { return true; } }
        public override int TreasureMapLevel { get { return 1; } }
        public override int Meat { get { return 1; } }
        //public override TribeType Tribe { get { return TribeType.GreenGoblin; } }

        public override void GenerateLoot()
        {
            AddLoot(LootPack.Meager);
        }

        public override void OnDeath(Container c)
        {
            base.OnDeath(c);

            if (Utility.RandomDouble() < 0.01)
                c.DropItem(new LuckyCoin());
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write((int)0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
        }
    }
}
