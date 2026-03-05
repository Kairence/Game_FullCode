using System;
using Server.Items;
using Server.Network;
using System.Collections.Generic;

namespace Server.Mobiles
{
    public interface IBloodCreature
    {
    }

    [CorpseName("a bloodworm corpse")]
    public class BloodWorm : BaseCreature, IBloodCreature
    {
        [Constructable]
        public BloodWorm()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Name = "a bloodworm";
            Body = 287;
			
			Boss = true;

            /* [Despise Level 2 Boss - Blood Worm - Fame 12,000 / Weight 1.24]
			   - 컨셉: 흡혈 거대 벌레 (생명력 강화형)
			   - VirtualArmor: (12,000/1000) - 4 = 8 (물렁한 외피, 보정 -4)
			   - 편차 수정: 체력 5만 이상 룰 적용 (편차 2,000 이내)
			   -------------------------------------------------- */

			// 최종 Str 약 7,800
			this.SetStr(6400, 6700); 

			// 최종 Hits 약 154,000 (민맥 편차 2,000 고정)
			this.SetHits(128700, 130700); 

			// 최종 Dex/Int 약 1,550
			this.SetDex(1250, 1350);
			this.SetInt(1250, 1350);

			// 최종 Stam/Mana 약 1,450
			this.SetStam(1150, 1250);
			this.SetMana(1150, 1250);

			// [Combat Options]
			this.SetDamage(45, 65);
			this.SetAttackSpeed(1.6);

			// [Resistances] 최고 저항 75 이하 엄격 준수
			this.SetResistance(ResistanceType.Physical, 35, 45); // 물리 방어는 낮음
			this.SetResistance(ResistanceType.Fire, 60, 70);     // 뜨거운 피를 가져 불에 강함
			this.SetResistance(ResistanceType.Cold, 20, 30);     // 약점: 냉기 (피가 굳음)
			this.SetResistance(ResistanceType.Poison, 70, 75);   // 독 면역 수준
			this.SetResistance(ResistanceType.Energy, 40, 50);

			// [Skills] 최종 104.1 부근
			this.SetSkill(SkillName.Wrestling, 58.0, 63.0);
			this.SetSkill(SkillName.Tactics, 58.0, 63.0);
			this.SetSkill(SkillName.Anatomy, 58.0, 63.0);
			this.SetSkill(SkillName.MagicResist, 70.0, 80.0);

			// 가방 방어력: (12,000/1000) - 4 = 8
			this.VirtualArmor = 8;

			this.Fame = 12000;
			this.Karma = -12000;
			
            SetSpecialAbility(SpecialAbility.Anemia);
        }

        public BloodWorm(Serial serial)
            : base(serial)
        {
        }

        public override void GenerateLoot()
        {
            AddLoot(LootPack.Rich);
            AddLoot(LootPack.Average);
        }

        public override void OnDeath(Container c)
        {
            base.OnDeath(c);
        }

        public override int GetIdleSound()
        {
            return 1503;
        }

        public override int GetAngerSound()
        {
            return 1500;
        }

        public override int GetHurtSound()
        {
            return 1502;
        }

        public override int GetDeathSound()
        {
            return 1501;
        }

        public override void OnAfterMove(Point3D oldLocation)
        {
            base.OnAfterMove(oldLocation);

            if (Hits < HitsMax && 0.25 > Utility.RandomDouble())
            {
                Corpse toAbsorb = null;

                foreach (Item item in Map.GetItemsInRange(Location, 1))
                {
                    if (item is Corpse)
                    {
                        Corpse c = (Corpse)item;

                        if (c.ItemID == 0x2006)
                        {
                            toAbsorb = c;
                            break;
                        }
                    }
                }

                if (toAbsorb != null)
                {
                    toAbsorb.ProcessDelta();
                    toAbsorb.SendRemovePacket();
                    toAbsorb.ItemID = Utility.Random(0xECA, 9); // bone graphic
                    toAbsorb.Hue = 0;
                    toAbsorb.Direction = Direction.North;
                    toAbsorb.ProcessDelta();

                    Hits = HitsMax;

                    // * The creature drains blood from a nearby corpse to heal itself. *
                    PublicOverheadMessage(MessageType.Regular, 0x3B2, 1111699);
                }
            }
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
