#region References
using Server.Items;
#endregion

namespace Server.Mobiles
{
    [CorpseName("a Fairy dragon corpse")]
    public class FairyDragon : BaseCreature
    {

        public override bool AutoDispel { get { return !Controlled; } }
        //public override int TreasureMapLevel { get { return 3; } }
        public override int Meat { get { return 9; } }
        public override Poison HitPoison { get { return Poison.Greater; } }
        public override double HitPoisonChance { get { return 0.75; } }
        public override FoodType FavoriteFood { get { return FoodType.Meat; } }

        [Constructable]
        public FairyDragon()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Name = "Fairy Dragon";
            Body = 718;
            BaseSoundID = 362;

            // [역산] 명성 5000 보너스(Str+937, Hits+10104, Skill+14.5) 반영
			this.SetStr(63, 113); 
			this.SetDex(158, 258); // 최종 Dex ~500 (매우 날렵함)

			// 저항이 강력하므로 추가 Hits는 억제
			this.SetHits(896, 1396); // 최종 Hits 11,000~11,500
			this.SetStam(58, 108); 
			this.SetMana(1500, 2500);

			SetAttackSpeed(2.0); 
			SetDamage(15, 25); 

			// 공격 속성: 신비로운 마력 타격
			this.SetDamageType(ResistanceType.Physical, 20);
			this.SetDamageType(ResistanceType.Cold, 40);
			this.SetDamageType(ResistanceType.Energy, 40);

			// 저항 설정 (전 속성 고루 높음)
			this.SetResistance(ResistanceType.Physical, 35, 45);
			this.SetResistance(ResistanceType.Fire, 40, 50);
			this.SetResistance(ResistanceType.Cold, 40, 50);
			this.SetResistance(ResistanceType.Poison, 40, 50);
			this.SetResistance(ResistanceType.Energy, 40, 50);

			this.Fame = 5000;
			this.Karma = 0; // 중립적인 환상 생물
			this.VirtualArmor = 5;

			this.Tamable = true;
			this.ControlSlots = 2;
			this.MinTameSkill = 155.1; // 스킬 200 서버 기준 상급 난이도
        }

        public FairyDragon(Serial serial)
            : base(serial)
        { }

        public override void GenerateLoot()
        {
            AddLoot(LootPack.Rich);
            AddLoot(LootPack.MedScrolls, 2);
        }

        public override void OnDeath(Container c)
        {

            base.OnDeath(c);

            if (Utility.RandomDouble() <= 0.25)
            {
                c.DropItem(new FairyDragonWing());
            }

            if (Utility.RandomDouble() < 0.10)
            {
                c.DropItem(new DraconicOrb());

            }
        }

        public override int GetAttackSound()
        {
            return 1513;
        }

        public override int GetAngerSound()
        {
            return 1558;
        }

        public override int GetDeathSound()
        {
            return 1514;
        }

        public override int GetHurtSound()
        {
            return 1515;
        }

        public override int GetIdleSound()
        {
            return 1516;
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write(0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            reader.ReadInt();
        }
    }
}