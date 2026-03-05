using System;
using Server.Items;

namespace Server.Mobiles 
{ 
    [CorpseName("an elf corpse")]
    public class ElfBrigand : BaseCreature
    {
        [Constructable]
        public ElfBrigand()
            : base(AIType.AI_Archer, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Race = Race.Elf;

            if (Female = Utility.RandomBool())
            {
                Body = 606;
                Name = NameList.RandomName("Elf female");
            }
            else
            {
                Body = 605;
                Name = NameList.RandomName("Elf male");
            }

            Title = "the brigand";
            Hue = Race.RandomSkinHue();

            this.SetStr(13, 33);    // 최종 Str 590~610
			this.SetDex(83, 113);   // 최종 Dex ~400
			this.SetInt(23, 43);    // 최종 Int 90~110

			this.SetHits(134, 234); // 최종 Hits 2,000~2,100
			this.SetStam(83, 113);
			this.SetMana(90, 110);

			SetAttackSpeed(3.0);    // 활 공격 속도
			SetDamage(10, 15);

			// 공격 속성: 엘프 화살 (물리 70% / 에너지 30%)
			this.SetDamageType(ResistanceType.Physical, 70);
			this.SetDamageType(ResistanceType.Energy, 30);

			// 저항: 엘프 가죽 옷 (낮은 명성에 맞춰 하향)
			this.SetResistance(ResistanceType.Physical, 5, 12);
			this.SetResistance(ResistanceType.Energy, 10, 20);
			this.SetResistance(ResistanceType.Fire, 5, 10);

			// 최종 Skill 45.0 내외 (궁술 특화)
			// 45.0 - 2.5 = 42.5
			this.SetSkill(SkillName.Archery, 42.5, 52.5);
			this.SetSkill(SkillName.Tactics, 37.5, 47.5);
			this.SetSkill(SkillName.MagicResist, 37.5, 47.5);

			this.VirtualArmor = 2;

			this.Fame = 1000;
			this.Karma = -1000;

            // outfit
            AddItem(new Shirt(Utility.RandomNeutralHue()));

            this.AddItem(new Bow());
            //this.PackItem(new Arrow(Utility.RandomMinMax(50, 70)));


            if (Female)
            {
                if (Utility.RandomBool())
                    AddItem(new Skirt(Utility.RandomNeutralHue()));
                else
                    AddItem(new Kilt(Utility.RandomNeutralHue()));
            }
            else
                AddItem(new ShortPants(Utility.RandomNeutralHue()));

            // hair, facial hair			
            HairItemID = Race.RandomHair(Female);
            HairHue = Race.RandomHairHue();

            // weapon, shield
            //Item weapon = Loot.RandomWeapon();

            //AddItem(weapon);

            //if (weapon.Layer == Layer.OneHanded && Utility.RandomBool())
             //   AddItem(Loot.RandomShield());

            PackGold(50, 150);
        }

        public ElfBrigand(Serial serial)
            : base(serial)
        {
        }

        public override bool AlwaysMurderer
        {
            get
            {
                return true;
            }
        }
        public override bool ShowFameTitle
        {
            get
            {
                return false;
            }
        }
        public override void OnDeath(Container c)
        {
            base.OnDeath(c);

            if (Utility.RandomDouble() < 0.75)
                c.DropItem(new SeveredElfEars());
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }
    }
}
