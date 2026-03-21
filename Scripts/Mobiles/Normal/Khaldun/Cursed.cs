using System;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("an inhuman corpse")]
    public class Cursed : BaseCreature
    {
        [Constructable]
        public Cursed()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            this.Title = "the Cursed";

            this.Hue = Utility.RandomMinMax(0x8596, 0x8599);
            this.Body = 0x190;
            this.Name = NameList.RandomName("male");
            this.BaseSoundID = 471;

            this.AddItem(new ShortPants(Utility.RandomNeutralHue()));
            this.AddItem(new Shirt(Utility.RandomNeutralHue()));

			/* [Khaldun Cursed - Fame 8,500 / Khaldun / Weight 1.21]
			   - 스킬 200 마스터 서버용 '중상급 정예' 밸런스 적용
			   - 카르마 보정: 명성(8,500) + 1,500 보정 = -10,000
			   - 가상 방어력(VirtualArmor): (8,500/1000) - 1.5 = 7
			   -------------------------------------------------- */

			// [Attributes] 명성 8,500 보너스 + 가중치 1.21 반영
			this.SetStr(150, 180); 
			this.SetHits(3500, 4000); 
			this.SetDex(30, 45);
			this.SetInt(30, 45);

			SetAttackSpeed(2.2);
			SetDamage(35, 50);

			// [Damage Types] 영체의 저주 (냉기/에너지 중심)
			this.SetDamageType(ResistanceType.Physical, 20);
			this.SetDamageType(ResistanceType.Cold, 40);
			this.SetDamageType(ResistanceType.Energy, 40);

			// [Resistances] 칼둔의 어둠 (냉기/에너지 특화)
			this.SetResistance(ResistanceType.Physical, 30, 40);
			this.SetResistance(ResistanceType.Fire, 10, 20);      // 신성한 불에 매우 취약
			this.SetResistance(ResistanceType.Cold, 65, 75);     // 극한의 냉기 내성
			this.SetResistance(ResistanceType.Poison, 50, 60);
			this.SetResistance(ResistanceType.Energy, 60, 75);    // 에너지 저항 우수

			// [Skills] ★ 스킬 200 서버 기준 - 마스터(200)로 가는 중반부 관문
			this.SetSkill(SkillName.Wrestling, 90.0, 105.0); 
			this.SetSkill(SkillName.Tactics, 90.0, 105.0);
			this.SetSkill(SkillName.Anatomy, 90.0, 105.0);
			this.SetSkill(SkillName.MagicResist, 110.0, 130.0); // 칼둔 몬스터는 마법 저항이 더욱 강력

			// [Misc] 
			this.VirtualArmor = 7;

			this.Fame = 8500;
			this.Karma = -10000; // 칼둔 보정 적용 (-8,500 - 1,500)

            BaseWeapon weapon = Loot.RandomWeapon();
            weapon.Movable = false;
            this.AddItem(weapon);
        }

        public Cursed(Serial serial)
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
        public override int GetAttackSound()
        {
            return -1;
        }

        public override void GenerateLoot()
        {
            this.AddLoot(LootPack.Meager);
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