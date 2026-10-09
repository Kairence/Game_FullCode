using System;
using Server.Items;

namespace Server.Mobiles
{
    public class KhaldunZealot : BaseCreature
    {
        [Constructable]
        public KhaldunZealot()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            this.Body = 0x190;
            this.Name = "Zealot of Khaldun";
            this.Title = "the Knight";
            this.Hue = 0;

			/* [Khaldun Zealot - Fame 9,500 / Khaldun / Weight 1.23]
			   - 스킬 200 마스터 서버용 '중상급 가디언' 밸런스 적용
			   - 카르마 보정: 명성(9,500) + 1,500 보정 = -11,000
			   - 가상 방어력(VirtualArmor): (9,500/1000) + 1.5 = 11 (광신도의 판금)
			   -------------------------------------------------- */

			// [Attributes] 명성 9,500 보너스 + 가중치 1.23 반영
			this.SetStr(180, 230); 
			this.SetHits(4500, 5000); 
			this.SetDex(35, 50);
			this.SetInt(35, 50);

			SetAttackSpeed(2.0);
			SetDamage(40, 55);

			// [Damage Types] 80% 물리 + 20% 에너지 (광기의 타격)
			this.SetDamageType(ResistanceType.Physical, 80);
			this.SetDamageType(ResistanceType.Energy, 20);

			// [Resistances] 전사형 저항 (물리/불 특화)
			this.SetResistance(ResistanceType.Physical, 60, 70); // 판금 갑옷급 방어력
			this.SetResistance(ResistanceType.Fire, 50, 60);
			this.SetResistance(ResistanceType.Cold, 20, 30);     // 냉기에 취약
			this.SetResistance(ResistanceType.Poison, 40, 50);
			this.SetResistance(ResistanceType.Energy, 30, 40);

			// [Skills] ★ 스킬 200 서버 기준 - 숙련된 전사의 라이벌 (재설계)
			// 유저 스킬 110 ~ 130 구간에서 상대하기 적합
			this.SetSkill(SkillName.Wrestling, 105.0, 125.0); 
			this.SetSkill(SkillName.Tactics, 105.0, 125.0);
			this.SetSkill(SkillName.Anatomy, 110.0, 130.0);   // 급소를 노리는 광기
			this.SetSkill(SkillName.MagicResist, 90.0, 110.0);
			this.SetSkill(SkillName.Healing, 80.0, 100.0);    // 자가 치유를 통한 끈질김

			// [Misc]
			this.VirtualArmor = 11;

			this.Fame = 9500;
			this.Karma = -11000; // 칼둔 보정 적용 (-9,500 - 1,500)

            VikingSword weapon = new VikingSword();
            weapon.Hue = 0x835;
            weapon.Movable = false;
            this.AddItem(weapon);

            MetalShield shield = new MetalShield();
            shield.Hue = 0x835;
            shield.Movable = false;
            this.AddItem(shield);

            BoneHelm helm = new BoneHelm();
            helm.Hue = 0x835;
            this.AddItem(helm);

            BoneArms arms = new BoneArms();
            arms.Hue = 0x835;
            this.AddItem(arms);

            BoneGloves gloves = new BoneGloves();
            gloves.Hue = 0x835;
            this.AddItem(gloves);

            BoneChest tunic = new BoneChest();
            tunic.Hue = 0x835;
            this.AddItem(tunic);

            BoneLegs legs = new BoneLegs();
            legs.Hue = 0x835;
            this.AddItem(legs);

            this.AddItem(new Boots());
        }

        public KhaldunZealot(Serial serial)
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
        public override bool Unprovokable
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
                return Poison.Deadly;
            }
        }
        public override int GetIdleSound()
        {
            return 0x184;
        }

        public override int GetAngerSound()
        {
            return 0x286;
        }

        public override int GetDeathSound()
        {
            return 0x288;
        }

        public override int GetHurtSound()
        {
            return 0x19F;
        }

        public override bool OnBeforeDeath()
        {
            BoneKnight rm = new BoneKnight();
            rm.Team = this.Team;
            rm.Combatant = this.Combatant;
            rm.NoKillAwards = true;

            if (rm.Backpack == null)
            {
                Backpack pack = new Backpack();
                pack.Movable = false;
                rm.AddItem(pack);
            }

            for (int i = 0; i < 2; i++)
            {
                LootPack.FilthyRich.Generate(this, rm.Backpack, true, LootPack.GetLuckChanceForKiller(this));
                LootPack.FilthyRich.Generate(this, rm.Backpack, false, LootPack.GetLuckChanceForKiller(this));
            }

            Effects.PlaySound(this, this.Map, this.GetDeathSound());
            Effects.SendLocationEffect(this.Location, this.Map, 0x3709, 30, 10, 0x835, 0);
            rm.MoveToWorld(this.Location, this.Map);

            this.Delete();
            return false;
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
