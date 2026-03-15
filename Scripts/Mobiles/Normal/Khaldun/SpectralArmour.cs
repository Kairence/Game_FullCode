using System;
using Server.Items;

namespace Server.Mobiles 
{ 
    public class SpectralArmour : BaseCreature 
    { 
        [Constructable] 
        public SpectralArmour()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        { 
            this.Body = 637; 
            this.Hue = 0x8026; 
            this.Name = "spectral armour"; 

            Buckler buckler = new Buckler();
            ChainCoif coif = new ChainCoif();
            PlateGloves gloves = new PlateGloves();

            buckler.Hue = 0x835;
            buckler.Movable = false;
            coif.Hue = 0x835;
            gloves.Hue = 0x835;

            this.AddItem(buckler);
            this.AddItem(coif);
            this.AddItem(gloves);

			/* [Khaldun Spectral Armour - Revised]
			   - 명성 11,000 / 가중치 1.24 / 카르마 -13,000
			   - 너무 높은 면역을 하향하여 '공략 가능한 상급 몬스터'로 재조정
			   -------------------------------------------------- */

			this.SetStr(250, 290); 
			this.SetHits(5500, 6500); 
			this.SetDex(45, 60);
			this.SetInt(45, 60);

			SetAttackSpeed(2.6);
			SetDamage(50, 75);

			// 속성 분배 (유저의 저항 세팅 유도)
			this.SetDamageType(ResistanceType.Physical, 60);
			this.SetDamageType(ResistanceType.Cold, 40);

			// [Resistances] ★ 면역 하향 조정
			this.SetResistance(ResistanceType.Physical, 55, 65); // 판금의 단단함 유지
			this.SetResistance(ResistanceType.Fire, 25, 35);    // 약점 명확화
			this.SetResistance(ResistanceType.Cold, 50, 60);
			this.SetResistance(ResistanceType.Poison, 60, 75);  // 면역 제거, 높은 저항으로 대체
			this.SetResistance(ResistanceType.Energy, 35, 45);

			// [Skills] 유저 스킬 110 ~ 130 구간용
			this.SetSkill(SkillName.Wrestling, 110.0, 125.0); 
			this.SetSkill(SkillName.Tactics, 110.0, 125.0);
			this.SetSkill(SkillName.MagicResist, 100.0, 115.0); // 마법 저항 하향

			this.VirtualArmor = 18; // 가상 방어력 하향 (30이하 가이드 준수)

			this.Fame = 11000;
			this.Karma = -13000;        
        }

        public SpectralArmour(Serial serial)
            : base(serial)
        { 
        }

        public override bool DeleteCorpseOnDeath
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
                return Poison.Regular;
            }
        }
        public override int GetIdleSound()
        {
            return 0x200;
        }

        public override int GetAngerSound()
        {
            return 0x56;
        }

        public override bool OnBeforeDeath()
        {
            if (!base.OnBeforeDeath())
                return false;

            Gold gold = new Gold(Utility.RandomMinMax(240, 375));
            gold.MoveToWorld(this.Location, this.Map);

            Effects.SendLocationEffect(this.Location, this.Map, 0x376A, 10, 1);
            return true;
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