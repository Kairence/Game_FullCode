using System;
using Server.Items;

namespace Server.Mobiles 
{ 
    [CorpseName("a golem controller corpse")] 
    public class GolemController : BaseCreature 
    { 
        [Constructable] 
        public GolemController()
            : base(AIType.AI_Mage, FightMode.Closest, 10, 1, 0.2, 0.4)
        { 
            Name = NameList.RandomName("golem controller");
            Title = "the controller";

            Body = 400;
            Hue = 0x455;

            AddArcane(new Robe());
            AddArcane(new ThighBoots());
            AddArcane(new LeatherGloves());
            AddArcane(new Cloak());

            /* Golem Controller - Fame 8,000 / Karma -8,000 */
			/* [HP Calculation]
			   - Target HP: ~15,000
			   - Fame Bonus (8,000): ~19,540
			   - SetHits Required: 100~500 (Bonus already covers target)
			*/
			this.SetStr(200, 300);       
			this.SetDex(150, 200);       
			this.SetInt(600, 800);       

			this.SetHits(100, 500); 
			this.SetStam(150, 200);      
			this.SetMana(600, 800);      

			SetAttackSpeed(10.0);
			SetDamage(12, 18);     

			this.SetResistance(ResistanceType.Physical, 35, 45);
			this.SetResistance(ResistanceType.Fire, 35, 45);
			this.SetResistance(ResistanceType.Energy, 50, 65);

			this.SetSkill(SkillName.Magery, 100.0, 115.0);
			this.SetSkill(SkillName.EvalInt, 100.0, 115.0);
			this.SetSkill(SkillName.MagicResist, 100.0, 115.0);

			this.VirtualArmor = 8;       
			this.Tamable = false;

			this.Fame = 8000;           
			this.Karma = -8000;
        }

        public GolemController(Serial serial)
            : base(serial)
        { 
        }

        public override bool ClickTitle
        {
            get
            {
                return false;
            }
        }
        public override bool ShowFameTitle
        {
            get
            {
                return false;
            }
        }
        public override bool AlwaysMurderer
        {
            get
            {
                return true;
            }
        }
        public override void GenerateLoot()
        {
            AddLoot(LootPack.Rich);
        }

        public void AddArcane(Item item)
        {
            if (item is IArcaneEquip)
            {
                IArcaneEquip eq = (IArcaneEquip)item;
                eq.CurArcaneCharges = eq.MaxArcaneCharges = 20;
            }

            item.Hue = ArcaneGem.DefaultArcaneHue;
            item.LootType = LootType.Newbied;

            AddItem(item);
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
