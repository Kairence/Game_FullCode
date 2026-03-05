using System;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a rotting corpse")]
    public class RottingCorpse : BaseCreature
    {
        [Constructable]
        public RottingCorpse()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Name = "a rotting corpse";
            Body = 155;
            BaseSoundID = 471;

            this.Fame = 14000;
			this.Karma = -14000;

			// [역산] 최종 Str 2,500 / Dex 1,200 목표
			this.SetStr(390, 450);    
			this.SetDex(350, 400);    
			this.SetInt(200, 300);

			// [역산] 최종 Hits 약 50,000 목표 (기초 7,120 + 보너스 42,880)
			this.SetHits(7120, 7500); 

			// [역산] 최종 Stam/Mana 약 1,200 목표 (기초 145 + 보너스 1,055)
			this.SetStam(140, 150);
			this.SetMana(140, 150);

			this.SetAttackSpeed(1.8);  // 부패한 몸치고는 상당히 빠른 연사력
			SetDamage(55, 75);        // 한 방이 뼈를 부수는 파괴력

			// [Skill] 최종 180.0(±10.0) 목표 (기초 131.9 + 보너스 48.1)
			this.SetSkill(SkillName.Wrestling, 122.0, 142.0); 
			this.SetSkill(SkillName.Tactics, 122.0, 142.0);
			this.SetSkill(SkillName.Poisoning, 150.0); // 1층 최고의 맹독

			this.SetDamageType(ResistanceType.Physical, 30);
			this.SetDamageType(ResistanceType.Poison, 70);

			this.SetResistance(ResistanceType.Physical, 35, 50);
			this.SetResistance(ResistanceType.Poison, 100); 
			this.SetResistance(ResistanceType.Fire, -20, -10); // 여전히 불에 취약
			this.VirtualArmor = 20;
        }

        public RottingCorpse(Serial serial)
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
                return Poison.Lethal;
            }
        }
        public override Poison HitPoison
        {
            get
            {
                return Poison.Lethal;
            }
        }
        public override int TreasureMapLevel
        {
            get
            {
                return 5;
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
            AddLoot(LootPack.FilthyRich, 2);
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
