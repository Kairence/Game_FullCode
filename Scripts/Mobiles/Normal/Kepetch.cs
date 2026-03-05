using System;
using Server.Items;
using Server.Network;

namespace Server.Mobiles
{
    [CorpseName("a kepetch corpse")]
    public class Kepetch : BaseCreature, ICarvable
    {
        public bool GatheredFur { get; set; }

        [Constructable]
        public Kepetch()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Name = "a kepetch";
            Body = 726;

			/* [Kepetch - Fame 3,500 / General / Weight 1.16]
			   - 스킬 200 마스터 서버용 '중하급' 밸런스 적용
			   - 가상 방어력(VirtualArmor): (3,500/1000) + 0.5 = 4
			   - 카파(30~40)와 비틀(45~55) 사이의 징검다리
			   -------------------------------------------------- */

			// [Attributes] 명성 3,500 보너스 + 가중치 1.16 반영
			this.SetStr(35, 50); 
			this.SetHits(850, 1000); 
			this.SetDex(8, 12);
			this.SetInt(8, 12);

			// [Combat Options]
			this.SetDamage(15, 25);
			this.SetAttackSpeed(1.8); // 야수 특유의 빠른 연타

			// [Damage Types] 100% 물리 공격 (날카로운 발톱)
			this.SetDamageType(ResistanceType.Physical, 100);

			// [Resistances] 총합 약 145 (초급 상위 저항)
			this.SetResistance(ResistanceType.Physical, 30, 40);
			this.SetResistance(ResistanceType.Fire, 20, 30);
			this.SetResistance(ResistanceType.Cold, 30, 40);      // 설산 적응력
			this.SetResistance(ResistanceType.Poison, 10, 20);
			this.SetResistance(ResistanceType.Energy, 15, 25);

			// [Skills] ★ 스킬 200 서버 기준 - 본격적인 전투 훈련용 (재설계)
			// 유저 스킬 45 ~ 65 구간 사냥에 최적화
			this.SetSkill(SkillName.Wrestling, 40.0, 55.0); 
			this.SetSkill(SkillName.Tactics, 40.0, 55.0);
			this.SetSkill(SkillName.Anatomy, 40.0, 55.0);
			this.SetSkill(SkillName.MagicResist, 35.0, 50.0);

			// [Misc] 가상 방어력(Virtual Armor): (3,500/1000) + 0.5 = 4
			this.VirtualArmor = 4;

			this.Fame = 3500;
			this.Karma = -3500;

            SetSpecialAbility(SpecialAbility.ViciousBite);
        }

        public Kepetch(Serial serial)
            : base(serial)
        {
        }

        public override int Meat { get { return 5; } }
        public override int Hides { get { return 14; } }
        public override HideType HideType { get { return HideType.Spined; } }
        public override FoodType FavoriteFood { get { return FoodType.FruitsAndVegies | FoodType.GrainsAndHay; } }
        public override int DragonBlood { get { return 8; } }
        public override int Fur { get { return GatheredFur ? 0 : 15; } }
        public override FurType FurType { get { return FurType.Brown; } }

        public bool Carve(Mobile from, Item item)
        {
            if (!GatheredFur)
            {
                var fur = new Fur(FurType, Fur);

                if (from.Backpack == null || !from.Backpack.TryDropItem(from, fur, false))
                {
                    from.SendLocalizedMessage(1112359); // You would not be able to place the gathered kepetch fur in your backpack!
                    fur.Delete();
                }
                else
                {
                    from.SendLocalizedMessage(1112360); // You place the gathered kepetch fur into your backpack.
                    GatheredFur = true;
                    return true;
                }
            }
            else
            {
                PrivateOverheadMessage(MessageType.Regular, 0x3B2, 1112358, from.NetState); // The Kepetch nimbly escapes your attempts to shear its mane.
            }

            return false;
        }

        public override void GenerateLoot()
        {
            AddLoot(LootPack.Average, 2);
        }

        public override int GetIdleSound()
        {
            return 1545;
        }

        public override int GetAngerSound()
        {
            return 1542;
        }

        public override int GetHurtSound()
        {
            return 1544;
        }

        public override int GetDeathSound()
        {
            return 1543;
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(2);

            writer.Write(GatheredFur);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            var version = reader.ReadInt();

            if (version == 1)
                reader.ReadDeltaTime();
            else
                GatheredFur = reader.ReadBool();
        }
    }
}
