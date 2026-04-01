using System;
using Server.Items;

namespace Server.Mobiles
{
    // 광맥 정령임을 식별하기 위한 마커 인터페이스
    public interface IOreElemental { }
}

namespace Server.Mobiles
{
    [CorpseName("an iron elemental corpse")]
    public class IronElemental : BaseCreature, IOreElemental
    {
        [Constructable]
        public IronElemental()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            this.Name = "an iron elemental";
            this.Body = 14;
            this.BaseSoundID = 268;

            /* Earth Elemental - Fame 4,500 / Common Spirit */
			this.SetStr(250, 350);       
			this.SetDex(60, 100);       
			this.SetInt(60, 100);       

			// [Hits] 명성 보석(9,800) 포함 최종 1.1만 내외
			this.SetHits(1000, 1400); 
			this.SetStam(60, 100);      
			this.SetMana(60, 100);      

			SetAttackSpeed(4.5);
			SetDamage(45, 65);    

			this.SetDamageType(ResistanceType.Physical, 100);

			this.SetResistance(ResistanceType.Physical, 30, 40);
			this.SetResistance(ResistanceType.Fire, 5, 10);
			this.SetResistance(ResistanceType.Cold, 5, 10);
			this.SetResistance(ResistanceType.Poison, 15, 25);
			this.SetResistance(ResistanceType.Energy, 10, 20);

			this.SetSkill(SkillName.Wrestling, 70.0, 80.0);
			this.SetSkill(SkillName.Tactics, 70.0, 80.0);
			this.SetSkill(SkillName.MagicResist, 40.0, 60.0);

			this.VirtualArmor = 15;      
			this.Tamable = false;

			this.Fame = 0;           
			this.Karma = 0;
		}

        public IronElemental(Serial serial)
            : base(serial)
        {
        }

        public override double DispelDifficulty
        {
            get
            {
                return 117.5;
            }
        }
        public override double DispelFocus
        {
            get
            {
                return 45.0;
            }
        }
        public override bool BleedImmune
        {
            get
            {
                return true;
            }
        }
        public override int TreasureMapLevel
        {
            get
            {
                return 1;
            }
        }
        public override void GenerateLoot()
        {
            this.AddLoot(LootPack.Average);
            this.AddLoot(LootPack.Meager);
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
