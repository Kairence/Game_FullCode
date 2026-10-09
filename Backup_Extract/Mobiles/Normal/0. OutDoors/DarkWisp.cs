using System;
using Server.Items;
using Server.Misc;

namespace Server.Mobiles
{
    [CorpseName("a wisp corpse")]
    public class DarkWisp : BaseCreature
    {
        [Constructable]
        public DarkWisp()
            : base(AIType.AI_NecroMage, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Name = "a wisp";
            Body = 165;
            BaseSoundID = 466;

            // [역산] 명성 3,500 보너스(Str+793, Hits+6702, Stam+113, Skill+11.3) 반영
			this.SetStr(7, 15); 
			this.SetDex(87, 100); // 최종 Dex ~300 (매우 빠름)
			this.SetInt(300, 500); 

			this.SetHits(3298, 3500); // 최종 Hits 10,000~10,202
			this.SetStam(37, 50);
			this.SetMana(500, 800);

			this.SetAttackSpeed(2.0);  // 쿠거와 동급의 최상위권 공속. 영체 특유의 날렵함 강조.
			this.SetDamage(20, 30);    // [마전사 밸런싱] 평타 자체는 명성 대비 낮게 측정.
									   // 하지만 마법 데미지가 추가로 들어오는 점을 고려하면 
									   // 실질적인 체감 데미지는 황소(Bull)급 혹은 그 이상입니다.

			this.SetSkill(SkillName.Magery, 8.7, 13.7); // 최종 20.0~25.0
			this.SetSkill(SkillName.MagicResist, 18.7, 23.7); // 마법 저항 특화

			this.Fame = 3500;
			this.VirtualArmor = 3;

            //AddItem(new LightSource());
        }

        public DarkWisp(Serial serial)
            : base(serial)
        {
        }

        public override InhumanSpeech SpeechType
        {
            get
            {
                return InhumanSpeech.Wisp;
            }
        }
        /*public override Ethics.Ethic EthicAllegiance
        {
            get
            {
                return Ethics.Ethic.Evil;
            }
        }*/
        public override TimeSpan ReacquireDelay
        {
            get
            {
                return TimeSpan.FromSeconds(1.0);
            }
        }
        public override OppositionGroup OppositionGroup
        {
            get
            {
                return OppositionGroup.FeyAndUndead;
            }
        }
        public override void GenerateLoot()
        {
            AddLoot(LootPack.Rich);
            AddLoot(LootPack.Average);
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
