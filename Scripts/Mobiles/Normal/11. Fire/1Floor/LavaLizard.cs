using Server.Items;
using Server.Services;

namespace Server.Mobiles
{
    [CorpseName("a lava lizard corpse")]
    [TypeAlias("Server.Mobiles.Lavalizard")]
    public class LavaLizard : BaseCreature
    {
        [Constructable]
        public LavaLizard()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Name = "a lava lizard";
            Body = 0xCE;
			
			if( 0.000001 > Utility.RandomDouble() )
				Hue = 1174;
			else
				Hue = Utility.RandomList(0x647, 0x650, 0x659, 0x662, 0x66B, 0x674);
            BaseSoundID = 0x5A;

            /* Lava Lizard - Fame 3,500 / Karma -3,500 */
			/* [HP Calculation]
			   - Target HP: ~8,500
			   - Fame Bonus (3,500): ~6,560
			   - SetHits Required: 1,940 (Target - Bonus)
			*/
			this.SetStr(200, 300);       
			this.SetDex(100, 150);       

			// [Hits] 최종 약 8,000 ~ 9,000 타겟
			this.SetHits(1440, 2440); 
			this.SetStam(100, 150);      

			SetAttackSpeed(2.2);
			SetDamage(12, 22);     

			this.SetDamageType(ResistanceType.Physical, 50);
			this.SetDamageType(ResistanceType.Fire, 50);

			this.SetResistance(ResistanceType.Physical, 40, 50);
			this.SetResistance(ResistanceType.Fire, 75, 75);     // 화염 면역 (Max 75%)
			this.SetResistance(ResistanceType.Cold, -10, 5);     // 냉기 약점
			this.SetResistance(ResistanceType.Poison, 30, 45);

			this.SetSkill(SkillName.Wrestling, 80.0, 95.0);
			this.SetSkill(SkillName.Tactics, 80.0, 95.0);

			this.VirtualArmor = 8;       

			// [Taming Settings]
			this.Tamable = true;         
			this.ControlSlots = 1;       
			this.MinTameSkill = 95.0;    // 200 상한 대비 초중반용 숙련도

			this.Fame = 3500;           
			this.Karma = -3500;

        }

        public LavaLizard(Serial serial)
            : base(serial)
        {
        }

        public override int Hides
        {
            get { return 12; }
        }

        public override HideType HideType
        {
            get { return HideType.Spined; }
        }

        public override void GenerateLoot()
        {
            AddLoot(LootPack.Meager);
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write(0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            var version = reader.ReadInt();
        }
    }
}
