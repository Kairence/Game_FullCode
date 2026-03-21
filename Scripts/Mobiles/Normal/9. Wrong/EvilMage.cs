using System;
using Server.Items;

namespace Server.Mobiles 
{ 
    [CorpseName("an evil mage corpse")] 
    public class EvilMage : BaseCreature 
    { 
        [Constructable] 
        public EvilMage()
            : base(AIType.AI_Mage, FightMode.Closest, 10, 1, 0.2, 0.4)
        { 
            Name = NameList.RandomName("evil mage");
            Title = "the evil mage";

            this.AddItem(new Robe(1172));
            this.AddItem(new Sandals(1172));

            if (Female = Utility.RandomBool())
            {
                Body = 0x191;
                Name = NameList.RandomName("female");
                AddItem(new Skirt(Utility.RandomNeutralHue()));
            }
            else
            {
                Body = 0x190;
                Name = NameList.RandomName("male");
                AddItem(new ShortPants(Utility.RandomNeutralHue()));
            }

			Hue = 1172;
			
            /* Evil Mage - Fame 6,000 / Karma -6,000 */
			/* [HP Calculation]
			   - Target HP: ~12,000
			   - Fame Bonus (6,000): ~13,850
			   - SetHits Required: 100~200 (Bonus already covers target)
			*/
			this.SetStr(100, 150);       
			this.SetDex(100, 150);       
			this.SetInt(500, 650);       

			this.SetHits(100, 200); 
			this.SetStam(100, 150);      
			this.SetMana(500, 650);      

			SetAttackSpeed(10.0);
			SetDamage(12, 18);      

			this.SetDamageType(ResistanceType.Energy, 100);

			this.SetResistance(ResistanceType.Physical, 25, 35);
			this.SetResistance(ResistanceType.Fire, 30, 45);
			this.SetResistance(ResistanceType.Cold, 30, 45);
			this.SetResistance(ResistanceType.Poison, 30, 45);
			this.SetResistance(ResistanceType.Energy, 50, 65);

			this.SetSkill(SkillName.Magery, 95.0, 105.0);
			this.SetSkill(SkillName.EvalInt, 95.0, 105.0);
			this.SetSkill(SkillName.MagicResist, 90.0, 105.0);

			this.VirtualArmor = 4;       // 천 옷을 입은 마법사 (낮은 방어력)
			this.Tamable = false;

			this.Fame = 6000;           
			this.Karma = -6000;
            PackReg(6);
        }

        public override int GetDeathSound()
        {
            return 0x423;
        }

        public override int GetHurtSound()
        {
            return 0x436;
        }

        public EvilMage(Serial serial)
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
        public override bool AlwaysMurderer
        {
            get
            {
                return true;
            }
        }
        public override int Meat
        {
            get
            {
                return 1;
            }
        }
        public override int TreasureMapLevel
        {
            get
            {
                return Core.AOS ? 1 : 0;
            }
        }
        public override void GenerateLoot()
        {
            AddLoot(LootPack.Average);
            AddLoot(LootPack.MedScrolls);
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
