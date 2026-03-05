using System;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a ghostly corpse")]
    public class Shade : BaseCreature
    {
        [Constructable]
        public Shade()
            : base(AIType.AI_Mage, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            this.Name = "a shade";
            this.Body = 740;
            this.Hue = 0x4001;
            this.BaseSoundID = 0x482;

            /* Shade - Fame 9,500 */
			this.Fame = 9500;
			this.Karma = -9500;

			this.SetInt(500, 600);    // 최종 Int 약 2,000
			this.SetHits(2000, 3000);  // 최종 Hits 약 32,000
			this.SetMana(1000, 1500);

			this.SetAttackSpeed(2.0);
			SetDamage(20, 30);

			this.SetSkill(SkillName.Magery, 130.0, 145.0); 
			this.SetSkill(SkillName.EvalInt, 130.0, 145.0);
			this.SetSkill(SkillName.Meditation, 150.0);

			this.SetDamageType(ResistanceType.Cold, 50);
			this.SetDamageType(ResistanceType.Energy, 50);

			this.SetResistance(ResistanceType.Physical, 70, 75); // 영체 보정: 물리 저항 매우 높음
			this.SetResistance(ResistanceType.Energy, 50, 65);
			this.VirtualArmor = 0; // 살점이 없어 가상 방어력 의미 없음

            //this.PackReg(10);
        }

        public Shade(Serial serial)
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

        public override TribeType Tribe { get { return TribeType.Undead; } }

        public override OppositionGroup OppositionGroup
        {
            get
            {
                return OppositionGroup.FeyAndUndead;
            }
        }
        public override Poison PoisonImmune
        {
            get
            {
                return Poison.Lethal;
            }
        }
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
