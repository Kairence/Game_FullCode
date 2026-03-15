using System;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a mummy corpse")]
    public class Mummy : BaseCreature
    {
        [Constructable]
        public Mummy()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.4, 0.8)
        {
            Name = "a mummy";
            Body = 154;
            BaseSoundID = 471;

            /* Mummy - Fame 5,500 */
			this.Fame = 5500;
			this.Karma = -5500;

			// [역산] 보너스: Str +783 / Hits +15,315 / Skill +20.3
			this.SetStr(200, 250);    
			this.SetHits(1500, 2000);  // 최종 Hits 약 17,000
			this.SetDex(50, 70);       // 매우 느림

			SetAttackSpeed(4.0);
			SetDamage(40, 55);        

			this.SetSkill(SkillName.Wrestling, 110.0, 120.0); 
			this.SetSkill(SkillName.Tactics, 110.0, 120.0);

			this.SetResistance(ResistanceType.Physical, 40, 50);
			this.SetResistance(ResistanceType.Fire, -50, -40); // 붕대는 불에 잘 탑니다
			this.VirtualArmor = 10;
        }

        public Mummy(Serial serial)
            : base(serial)
        {
        }

		public override int TreasureMapLevel
        {
            get
            {
                return 1;
            }
        }
        public override bool BleedImmune
        {
            get
            {
                return true;
            }
        }
        public override Poison PoisonImmune
        {
            get
            {
                return Poison.Lesser;
            }
        }

        public override TribeType Tribe { get { return TribeType.Undead; } }

        public override OppositionGroup OppositionGroup
        {
            get
            {
                return OppositionGroup.FeyAndUndead;
            }
        }
        public override void GenerateLoot()
        {
            this.AddLoot(LootPack.Rich);
            this.AddLoot(LootPack.Gems);
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
