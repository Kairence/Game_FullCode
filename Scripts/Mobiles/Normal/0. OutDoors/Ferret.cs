using System;
using Server.Engines.Quests;

namespace Server.Mobiles
{
    [CorpseName("a ferret corpse")]
    public class Ferret : BaseCreature
    {
        private static readonly string[] m_Vocabulary = new string[]
        {
            "dook",
            "dook dook",
            "dook dook dook!"
        };
        private bool m_CanTalk;
        [Constructable]
        public Ferret()
            : base(AIType.AI_Animal, FightMode.Aggressor, 10, 1, 0.2, 0.4)
        {
            this.Name = "a ferret";
            this.Body = 0x117;


			this.SetStr(5, 10);
			this.SetDex(100, 150); 

			this.SetHits(66, 100); // 최종 Hits 600~634
			this.SetStam(97, 127); // 최종 Stam 150~180 (지치지 않음)
			this.SetMana(0);

			this.SetAttackSpeed(3.0);  // [조정] 2.0초 -> 3.0초.
									   // 유저 평균 공속과 일치시켜 대응 리듬을 제공합니다.
									   // "매우 빠르다"는 느낌은 높은 Dex를 통한 회피율로 보여줍니다.

			this.SetDamage(10, 14);    // [방어구 효능 반영] 개(Dog)와 동일한 수준.

			this.SetDamageType(ResistanceType.Physical, 100);

			this.SetResistance(ResistanceType.Physical, 5, 10);
			this.SetResistance(ResistanceType.Energy, 10, 20);

			this.Fame = 200;
			this.VirtualArmor = 0;

			this.Tamable = true;
			this.MinTameSkill = -18.9;
            this.m_CanTalk = true;
        }

        public Ferret(Serial serial)
            : base(serial)
        {
        }

        public override int Meat
        {
            get
            {
                return 1;
            }
        }
        public override FoodType FavoriteFood
        {
            get
            {
                return FoodType.Fish;
            }
        }
        public override void OnMovement(Mobile m, Point3D oldLocation) 
        {
            if (m is Ferret && m.InRange(this, 3) && m.Alive)
                this.Talk((Ferret)m);
        }

        public void Talk()
        {
            this.Talk(null);
        }

        public void Talk(Ferret to)
        {
            if (this.m_CanTalk)
            {
                if (to != null)
                    QuestSystem.FocusTo(this, to);

                this.Say(m_Vocabulary[Utility.Random(m_Vocabulary.Length)]);
			
                if (to != null && Utility.RandomBool())
                    Timer.DelayCall(TimeSpan.FromSeconds(Utility.RandomMinMax(5, 8)), new TimerCallback(delegate() { to.Talk(); }));

                this.m_CanTalk = false;

                Timer.DelayCall(TimeSpan.FromSeconds(Utility.RandomMinMax(20, 30)), new TimerCallback(delegate() { this.m_CanTalk = true; }));
            }
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

            this.m_CanTalk = true;
        }
    }
}