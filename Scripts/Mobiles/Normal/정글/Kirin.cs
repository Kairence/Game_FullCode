using System;
using Server.Network;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a ki-rin corpse")]
    public class Kirin : BaseMount
    {
        [Constructable]
        public Kirin()
            : this("a ki-rin")
        {
        }

        [Constructable]
        public Kirin(string name)
            : base(name, 132, 0x3EAD, AIType.AI_Mage, FightMode.Evil, 10, 1, 0.2, 0.4)
        {
            this.BaseSoundID = 0x3C5;

			/* [Kirin - Normal - Fame 22,000 / Karma +22,000 / Weight 1.25]
			   - 정글 던전의 성스러운 기린 / 최상급 탈것
			   - 배수: 1x (Normal)
			   - VirtualArmor: 25 (기본 22 + 보정 3)
			   - 테이밍 가능: 2슬롯 (전략적 고효율 펫)
			   -------------------------------------------------- */

			// [Attributes] 역산된 Set 값 정밀 적용
			this.SetStr(700, 730); 
			this.SetHits(15700, 16000); 
			this.SetDex(140, 160); // 매우 뛰어난 기동성
			this.SetInt(140, 160);

			// [Combat Options] 물리 30% / 에너지 70% (천둥의 일격)
			this.SetDamage(50, 80);
			this.SetAttackSpeed(1.8); // 정령급의 빠른 공격
			this.SetDamageType(ResistanceType.Physical, 30);
			this.SetDamageType(ResistanceType.Energy, 70);

			// [Resistances] 최고 저항 75 이하 준수 / 독 약점 설정
			this.SetResistance(ResistanceType.Physical, 55, 65); 
			this.SetResistance(ResistanceType.Fire, 55, 65);      
			this.SetResistance(ResistanceType.Cold, 55, 65);    
			this.SetResistance(ResistanceType.Poison, 30, 45);   // ★ 확실한 약점 (오염에 취약)
			this.SetResistance(ResistanceType.Energy, 70, 75);  // 번개의 화신 (Max 75)

			// [Skills] 기본 120~130에 역산 보너스(23.8) 가산
			this.SetSkill(SkillName.Wrestling, 143.0, 153.0); 
			this.SetSkill(SkillName.Tactics, 143.0, 153.0);
			this.SetSkill(SkillName.Anatomy, 143.0, 153.0);
			this.SetSkill(SkillName.MagicResist, 130.0, 145.0);
			this.SetSkill(SkillName.Magery, 140.0, 155.0);       // 상급 번개 마법 구사
			this.SetSkill(SkillName.EvalInt, 140.0, 155.0);

			// [Misc]
			this.Tamable = true; 
			this.ControlSlots = 2; // 숙련도 시대의 고효율 2슬롯 펫
			this.MinTameSkill = 158.2; // 200 시대의 상징적 난이도
			this.VirtualArmor = 25;
			this.Fame = 22000;
			this.Karma = 22000; // 영물 (선 성향)
        }

        public Kirin(Serial serial)
            : base(serial)
        {
        }

        public override bool AllowFemaleRider
        {
            get
            {
                return false;
            }
        }
        public override bool AllowFemaleTamer
        {
            get
            {
                return false;
            }
        }
        public override bool InitialInnocent
        {
            get
            {
                return true;
            }
        }
        public override TimeSpan MountAbilityDelay
        {
            get
            {
                return TimeSpan.FromHours(1.0);
            }
        }

        public override TribeType Tribe { get { return TribeType.Fey; } }

        public override OppositionGroup OppositionGroup
        {
            get
            {
                return OppositionGroup.FeyAndUndead;
            }
        }
        public override int Meat
        {
            get
            {
                return 3;
            }
        }
        public override int Hides
        {
            get
            {
                return 10;
            }
        }
        public override HideType HideType
        {
            get
            {
                return HideType.Horned;
            }
        }
        public override FoodType FavoriteFood
        {
            get
            {
                return FoodType.FruitsAndVegies | FoodType.GrainsAndHay;
            }
        }
        public override void OnDisallowedRider(Mobile m)
        {
            m.SendLocalizedMessage(1042319); // The Ki-Rin refuses your attempts to mount it.
        }

        public override bool DoMountAbility(int damage, Mobile attacker)
        {
            if (this.Rider == null || attacker == null)	//sanity
                return false;

            if ((this.Rider.Hits - damage) < 30 && this.Rider.Map == attacker.Map && this.Rider.InRange(attacker, 18))	//Range and map checked here instead of other base fuction because of abiliites that don't need to check this
            {
                attacker.BoltEffect(0);
                // 35~100 damage, unresistable, by the Ki-rin.
                attacker.Damage(Utility.RandomMinMax(35, 100), this, false);	//Don't inform mount about this damage, Still unsure wether or not it's flagged as the mount doing damage or the player.  If changed to player, without the extra bool it'd be an infinite loop

                this.Rider.LocalOverheadMessage(MessageType.Regular, 0x3B2, 1042534);	// Your mount calls down the forces of nature on your opponent.
                this.Rider.FixedParticles(0, 0, 0, 0x13A7, EffectLayer.Waist);
                this.Rider.PlaySound(0xA9);	// Ki-rin's whinny.
                return true;
            }

            return false;
        }

        public override void GenerateLoot()
        {
            this.AddLoot(LootPack.Rich);
            this.AddLoot(LootPack.LowScrolls);
            this.AddLoot(LootPack.Potions);
        }

		public override void OnDeath(Container c)
        {
            base.OnDeath(c);

            if (!Controlled && Utility.RandomDouble() < 0.3)
                c.DropItem(new KirinBrains());
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)1); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            if (version == 0)
                this.AI = AIType.AI_Mage;
        }
    }
}
