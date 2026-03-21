using System;
using Server.Items;
using Server.Network;

namespace Server.Mobiles
{
    [CorpseName("a boura corpse")]
    public class LowlandBoura : BaseCreature, ICarvable
    {
        private bool GatheredFur { get; set; }

        [Constructable]
        public LowlandBoura() : base(AIType.AI_Animal, FightMode.Aggressor, 10, 1, 0.2, 0.4)
        {
            Name = "a lowland boura";
            Body = 715;

			/* [Lowland Boura - Normal - Fame 12,000 / Weight 1.25]
			   - 정글 던전의 거대 초식수 / 일반 몬스터 공식
			   - 배수: 1x (Normal)
			   - VirtualArmor: 15 (명성/1000 + 3 보정)
			   -------------------------------------------------- */

			// [Attributes] 역산된 Set 값 정밀 적용
			this.SetStr(310, 325); 
			this.SetHits(6900, 7100); 
			this.SetDex(60, 75);
			this.SetInt(60, 75);

			// [Combat Options] 물리 100% (거대한 뿔과 몸통 박치기)
			this.SetDamage(40, 65);
			this.SetAttackSpeed(3.0); // 묵직하고 느린 공격
			this.SetDamageType(ResistanceType.Physical, 100);

			// [Resistances] 최고 저항 75 이하 준수 / 에너지 및 냉기 약점 설정
			this.SetResistance(ResistanceType.Physical, 60, 75); // 매우 단단한 가죽
			this.SetResistance(ResistanceType.Fire, 40, 50);      
			this.SetResistance(ResistanceType.Cold, 25, 35);    // ★ 약점 1
			this.SetResistance(ResistanceType.Poison, 40, 55); 
			this.SetResistance(ResistanceType.Energy, 20, 30);  // ★ 약점 2 (전격에 취약)

			// [Skills] 기본 110~120에 역산 보너스(10.5) 가산
			this.SetSkill(SkillName.Wrestling, 120.5, 130.5); 
			this.SetSkill(SkillName.Tactics, 120.5, 130.5);
			this.SetSkill(SkillName.Anatomy, 120.5, 130.5);
			this.SetSkill(SkillName.MagicResist, 95.0, 110.0);

			// [Misc]
			this.Tamable = true; 
			this.ControlSlots = 2; // 200 숙련도 시대의 든든한 2슬롯 탱커 펫
			this.MinTameSkill = 115.0; 
			this.VirtualArmor = 15;
			this.Fame = 12000;
			this.Karma = -12000;
        }

        public LowlandBoura(Serial serial) : base(serial)
        {
        }

        public override int Meat { get { return 10; } }
        public override int Hides { get { return 20; } }
        public override int DragonBlood { get { return 8; } }
        public override HideType HideType { get { return HideType.Horned; } }
        public override FoodType FavoriteFood { get { return FoodType.FruitsAndVegies; } }
        public override int Fur { get { return GatheredFur ? 0 : 30; } }
        public override FurType FurType { get { return FurType.Green; } }

        public bool Carve(Mobile from, Item item)
        {
            if (!GatheredFur)
            {
                var fur = new Fur(FurType, Fur);

                if (from.Backpack == null || !from.Backpack.TryDropItem(from, fur, false))
                {
                    from.SendLocalizedMessage(1112352); // You would not be able to place the gathered boura fur in your backpack!
                    fur.Delete();
                }
                else
                {
                    from.SendLocalizedMessage(1112353); // You place the gathered boura fur into your backpack.
                    GatheredFur = true;

                    return true;
                }
            }
            else
            {
                PrivateOverheadMessage(MessageType.Regular, 0x3B2, 1112354, from.NetState); // The boura glares at you and will not let you shear its fur.
            }

            return false;
        }

        public override int GetIdleSound()
        {
            return 1507;
        }

        public override int GetAngerSound()
        {
            return 1504;
        }

        public override int GetHurtSound()
        {
            return 1506;
        }

        public override int GetDeathSound()
        {
            return 1505;
        }

        public override void OnDeath(Container c)
        {
            base.OnDeath(c);
            
            if (Controlled)
                return;

            if (!Controlled)
            c.DropItem(new BouraSkin());
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
            {
                GatheredFur = reader.ReadBool();
            }
        }
    }
}
