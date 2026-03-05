using System;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("an goblin corpse")]
    public class EnslavedGreenGoblin : BaseCreature
    {
        [Constructable]
        public EnslavedGreenGoblin()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Name = "Enslaved Green Goblin";
            Body = 334;
			Hue = 874;
            BaseSoundID = 0x600;

            /* Enslaved Green Goblin - Fame 800 / Karma -800 */
			this.SetStr(80, 120);       
			this.SetDex(100, 150);       

			this.SetHits(550, 1250); 
			this.SetStam(100, 150);      

			this.SetAttackSpeed(2.4);    
			this.SetDamage(5, 9);        

			this.SetDamageType(ResistanceType.Poison, 20);

			this.SetResistance(ResistanceType.Physical, 15, 25);
			this.SetResistance(ResistanceType.Fire, 40, 50);     
			this.SetResistance(ResistanceType.Poison, 60, 75);   // 독 저항 상한

			this.SetSkill(SkillName.Wrestling, 60.0, 75.0);
			this.SetSkill(SkillName.Poisoning, 50.0, 70.0);    // 낮은 수준의 독

			this.VirtualArmor = 2;       
			this.Tamable = false;

			this.Fame = 800;           
			this.Karma = -800;
        }

        public EnslavedGreenGoblin(Serial serial)
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
        //public override OppositionGroup OppositionGroup { get { return OppositionGroup.SavagesAndOrcs; } }

        public override void GenerateLoot()
        {
            this.AddLoot(LootPack.Meager);
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