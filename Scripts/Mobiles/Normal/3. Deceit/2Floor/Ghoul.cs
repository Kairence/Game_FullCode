using System;

namespace Server.Mobiles
{
    [CorpseName("a ghostly corpse")]
    public class Ghoul : BaseCreature
    {
        [Constructable]
        public Ghoul()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            this.Name = "a ghoul";
            this.Body = 740;
            this.BaseSoundID = 0x482;

            /* Ghoul - Fame 7,000 */
			this.Fame = 7000;
			this.Karma = -7000;

			// [역산] 보너스: Str +957 / Hits +18,720 / Skill +24.1
			this.SetStr(200, 300);    // 최종 Str 약 1,200
			this.SetHits(1200, 1500);  // 최종 Hits 약 20,000
			this.SetDex(150, 200);     

			this.SetAttackSpeed(2.0);
			SetDamage(25, 35);        

			this.SetSkill(SkillName.Wrestling, 120.0, 130.0); // 최종 약 150
			this.SetSkill(SkillName.Tactics, 120.0, 130.0);

			this.SetDamageType(ResistanceType.Physical, 60);
			this.SetDamageType(ResistanceType.Poison, 40);

			this.SetResistance(ResistanceType.Physical, 30, 40);
			this.SetResistance(ResistanceType.Fire, -20, -10);
			this.VirtualArmor = 15;
        }

        public Ghoul(Serial serial)
            : base(serial)
        {
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
                return Poison.Regular;
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
            this.AddLoot(LootPack.Meager);
            this.PackItem(Loot.RandomWeapon());
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
