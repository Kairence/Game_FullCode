using System;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a skeletal corpse")]
    public class BoneKnight : BaseCreature
    {
        [Constructable]
        public BoneKnight()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            this.Name = "a bone knight";
            this.Body = 57;
            this.BaseSoundID = 451;

            /* Bone Knight - Fame 4,500 */
			this.Fame = 4500;
			this.Karma = -4500;

			// [역산] 보너스: Str +622 / Hits +12,230 / Skill +16.3
			this.SetStr(180, 220);    // 최종 Str 약 800
			this.SetHits(200, 300);    // 최종 Hits 약 12,500
			this.SetDex(80, 100);      
			this.SetStam(80, 100);

			this.SetAttackSpeed(2.5);  
			SetDamage(15, 25);        

			this.SetSkill(SkillName.Wrestling, 100.0, 110.0); // 최종 약 121
			this.SetSkill(SkillName.Tactics, 100.0, 110.0);

			this.SetDamageType(ResistanceType.Physical, 100); // 순수 물리 공격

			this.SetResistance(ResistanceType.Physical, 35, 45);
			this.SetResistance(ResistanceType.Fire, -30, -20); // 불에 취약
			this.VirtualArmor = 15;
        }

        public BoneKnight(Serial serial)
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
        public override void GenerateLoot()
        {
            this.AddLoot(LootPack.Average);
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
