using System;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a fire elemental corpse")]
    public class FireElemental : BaseCreature
    {
        [Constructable]
        public FireElemental()
            : base(AIType.AI_Mage, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            this.Name = "a fire elemental";
            this.Body = 15;
            this.BaseSoundID = 838;

            /* Fire Elemental - Fame 7,000 / Fire Spirit */
			this.SetStr(300, 400);       
			this.SetDex(200, 300);       
			this.SetInt(500, 650);       // 지능/마력 가중치

			// [Hits] 최종 약 17,000 ~ 19,000 타겟
			this.SetHits(400, 2400); 
			this.SetStam(200, 300);      
			this.SetMana(500, 650);      

			this.SetAttackSpeed(2.5);    
			this.SetDamage(15, 25);      

			this.SetDamageType(ResistanceType.Fire, 100);

			this.SetResistance(ResistanceType.Physical, 35, 45);
			this.SetResistance(ResistanceType.Fire, 70, 75); // 화염 면역 수준
			this.SetResistance(ResistanceType.Cold, -10, 5);  // 냉기 약점
			this.SetResistance(ResistanceType.Poison, 30, 40);
			this.SetResistance(ResistanceType.Energy, 30, 40);

			this.SetSkill(SkillName.Wrestling, 90.0, 105.0);
			this.SetSkill(SkillName.Tactics, 90.0, 105.0);
			this.SetSkill(SkillName.Magery, 95.0, 105.0);
			this.SetSkill(SkillName.EvalInt, 90.0, 100.0);

			this.VirtualArmor = 20;      
			this.Tamable = false;

			this.Fame = 7000;           
			this.Karma = -7000;
            this.ControlSlots = 4;

            this.PackItem(new SulfurousAsh(4));

            //this.AddItem(new LightSource());
        }

        public FireElemental(Serial serial)
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
                return 2;
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