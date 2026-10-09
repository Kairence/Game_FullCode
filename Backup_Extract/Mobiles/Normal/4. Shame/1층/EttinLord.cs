using System;

namespace Server.Mobiles
{
    [CorpseName("an ettins corpse")]
    public class EttinLord : BaseCreature
    {
        [Constructable]
        public EttinLord()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            this.Name = "an ettin lord";
            this.Body = 261;
            this.BaseSoundID = 367;

			Boss = true;

            /* Ettin Lord - Fame 8,000 / Karma -8,000 */
			/* [HP Calculation]
			   - Target HP: ~85,000
			   - Fame Bonus (8,000): ~19,500
			   - SetHits Required: 65,500 (Target - Bonus)
			*/
			this.SetStr(800, 1000);      
			this.SetDex(150, 250);       
			this.SetInt(100, 200);       

			// [Hits] 최종 약 80,000 ~ 90,000 타겟
			this.SetHits(60500, 70500); 
			this.SetStam(150, 250);      
			this.SetMana(100, 200);      

			SetAttackSpeed(5.0);
			SetDamage(65, 95);     

			this.SetDamageType(ResistanceType.Physical, 100);

			this.SetResistance(ResistanceType.Physical, 55, 65);
			this.SetResistance(ResistanceType.Fire, 20, 30);
			this.SetResistance(ResistanceType.Cold, 30, 40);
			this.SetResistance(ResistanceType.Poison, 30, 40);
			this.SetResistance(ResistanceType.Energy, 30, 40);

			this.SetSkill(SkillName.Wrestling, 110.0, 120.0);
			this.SetSkill(SkillName.Tactics, 110.0, 120.0);
			this.SetSkill(SkillName.MagicResist, 100.0, 115.0);

			this.VirtualArmor = 30;      // 물리 보스다운 견고함
			this.Tamable = false;

			this.Fame = 8000;           
			this.Karma = -8000;
        }

        public EttinLord(Serial serial)
            : base(serial)
        {
        }

        public override bool CanRummageCorpses
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
        public override int Meat
        {
            get
            {
                return 4;
            }
        }
        public override void GenerateLoot()
        {

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
