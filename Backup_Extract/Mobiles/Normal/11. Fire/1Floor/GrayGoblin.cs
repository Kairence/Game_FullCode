using System;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a goblin corpse")]
    public class GrayGoblin : BaseCreature
    {
        [Constructable]
        public GrayGoblin()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Name = "a gray goblin";

            Body = 723;
            Hue = 1900;
            BaseSoundID = 0x600;

            /* Gray Goblin - Fame 1,800 / Karma -1,800 */
			/* [HP Calculation]
			   - Target HP: ~4,500
			   - Fame Bonus (1,800): ~3,150
			   - SetHits Required: 1,350 (Target - Bonus)
			*/
			this.SetStr(120, 160);       
			this.SetDex(150, 200);       

			// [Hits] 최종 약 4,000 ~ 5,000 타겟
			this.SetHits(850, 1850); 
			this.SetStam(150, 200);      

			SetAttackSpeed(2.2);
			SetDamage(10, 15);    

			this.SetResistance(ResistanceType.Physical, 25, 35);
			this.SetResistance(ResistanceType.Fire, 50, 60);     
			this.SetResistance(ResistanceType.Cold, 0, 10);      

			this.SetSkill(SkillName.Fencing, 75.0, 90.0);
			this.SetSkill(SkillName.Tactics, 75.0, 90.0);

			this.VirtualArmor = 4;       
			this.Tamable = false;

			this.Fame = 1800;           
			this.Karma = -1800;
        }

        public GrayGoblin(Serial serial)
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
        //public override TribeType Tribe { get { return TribeType.GrayGoblin; } }

        public override void GenerateLoot()
        {
            AddLoot(LootPack.Meager);
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write((int)1);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();

            if (version == 0)
            {
                Body = 723;
                Hue = 1900;
            }
        }
    }
}
