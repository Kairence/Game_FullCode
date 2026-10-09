using System;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a frozen ogre corpse")]
    [TypeAlias("Server.Mobiles.ArticOgreLord")]
    public class ArcticOgreLord : BaseCreature
    {
        [Constructable]
        public ArcticOgreLord()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Name = "an arctic ogre";
            Body = 135;
            BaseSoundID = 427;

            /* Arctic Ogre Lord - Fame 15,000 / Karma -15,000 */
			/* [HP Calculation]
			   - Target HP: ~65,000
			   - Fame Bonus (15,000): ~37,200
			   - SetHits Required: 27,800 (Target - Bonus)
			*/
			this.SetStr(800, 1000);      
			this.SetDex(150, 250);       
			this.SetInt(100, 150);       

			// [Hits] 최종 약 60,000 ~ 70,000 타겟
			this.SetHits(22800, 32800); 
			this.SetStam(150, 250);      
			this.SetMana(100, 150);      

			SetAttackSpeed(4.5);
			SetDamage(70, 100);      

			this.SetDamageType(ResistanceType.Physical, 70);
			this.SetDamageType(ResistanceType.Cold, 30);

			// [Resistance] 냉기 면역 수준, 화염에 매우 취약
			this.SetResistance(ResistanceType.Physical, 55, 65);
			this.SetResistance(ResistanceType.Fire, -10, 5);    // 화염 약점 (불에 잘 녹음)
			this.SetResistance(ResistanceType.Cold, 75, 75);    // 냉기 면역 (Max 75%)
			this.SetResistance(ResistanceType.Poison, 40, 55);
			this.SetResistance(ResistanceType.Energy, 40, 55);

			this.SetSkill(SkillName.Wrestling, 115.0, 130.0);
			this.SetSkill(SkillName.Tactics, 115.0, 130.0);
			this.SetSkill(SkillName.MagicResist, 100.0, 115.0);

			this.VirtualArmor = 15;      // 타격감 확보를 위해 15 제한
			this.Tamable = false;

			this.Fame = 15000;           
			this.Karma = -15000;

            this.VirtualArmor = Utility.RandomMinMax(55, 120);

            PackItem(new Club());
        }

        public ArcticOgreLord(Serial serial)
            : base(serial)
        {
        }

        public override Poison PoisonImmune
        {
            get
            {
                return Poison.Regular;
            }
        }
        
		public override int TreasureMapLevel { get { return 3; } }
		
        public override void GenerateLoot()
        {
            AddLoot(LootPack.FilthyRich);
            AddLoot(LootPack.Rich);
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
