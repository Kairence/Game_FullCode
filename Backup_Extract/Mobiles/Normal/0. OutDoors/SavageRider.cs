using System;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a savage corpse")]
    public class SavageRider : BaseCreature
    {
        [Constructable]
        public SavageRider()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.15, 0.4)
        {
            this.Name = NameList.RandomName("savage rider");

            this.Body = 185;

            this.SetStr(144, 194); // 최종 Str 800~850
			this.SetDex(166, 216); // 최종 Dex ~450 (매우 빠름)

			this.SetHits(2102, 3102); // 최종 Hits 6,000~7,000
			this.SetStam(66, 116);
			this.SetMana(100, 200);

			SetAttackSpeed(2.2); 
			SetDamage(20, 35); 

			this.SetDamageType(ResistanceType.Physical, 100);

			this.SetResistance(ResistanceType.Physical, 35, 45);
			this.SetResistance(ResistanceType.Cold, 20, 30);

			this.SetSkill(SkillName.Wrestling, 124.6, 144.6); // 최종 130~150
			this.SetSkill(SkillName.Tactics, 40.0, 50.0);

			SetAttackSpeed(2.8);
			SetDamage(25, 38); // 기동성을 이용한 강력한 찌르기
			this.VirtualArmor = 8;

            this.PackItem(new Bandage(Utility.RandomMinMax(1, 15)));

            if (0.1 > Utility.RandomDouble())
                this.PackItem(new BolaBall());

            this.AddItem(new TribalSpear());
            this.AddItem(new BoneArms());
            this.AddItem(new BoneLegs());
            this.AddItem(new BearMask());

            new SavageRidgeback().Rider = this;
			
			this.SpecialType2 = 6;
			this.SpecialChance2 = 0.1;
        }

        public SavageRider(Serial serial)
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
        public override bool AlwaysMurderer
        {
            get
            {
                return true;
            }
        }
        public override bool ShowFameTitle
        {
            get
            {
                return false;
            }
        }

        public override TribeType Tribe { get { return TribeType.Savage; } }

        public override OppositionGroup OppositionGroup
        {
            get
            {
                return OppositionGroup.SavagesAndOrcs;
            }
        }
        public override void GenerateLoot()
        {
            this.AddLoot(LootPack.Average);
        }

        public override bool OnBeforeDeath()
        {
            IMount mount = this.Mount;

            if (mount != null)
                mount.Rider = null;

            if (mount is Mobile)
                ((Mobile)mount).Delete();

            return base.OnBeforeDeath();
        }

        public override bool IsEnemy(Mobile m)
        {
            if (m.BodyMod == 183 || m.BodyMod == 184)
                return false;

            return base.IsEnemy(m);
        }

        public override void AggressiveAction(Mobile aggressor, bool criminal)
        {
            base.AggressiveAction(aggressor, criminal);

            if (aggressor.BodyMod == 183 || aggressor.BodyMod == 184)
            {
                AOS.Damage(aggressor, 50, 0, 100, 0, 0, 0);
                aggressor.BodyMod = 0;
                aggressor.HueMod = -1;
                aggressor.FixedParticles(0x36BD, 20, 10, 5044, EffectLayer.Head);
                aggressor.PlaySound(0x307);
                aggressor.SendLocalizedMessage(1040008); // Your skin is scorched as the tribal paint burns away!

                if (aggressor is PlayerMobile)
                    ((PlayerMobile)aggressor).SavagePaintExpiration = TimeSpan.Zero;
            }
        }

        public override void AlterMeleeDamageTo(Mobile to, ref int damage)
        {
            if (to is Dragon || to is WhiteWyrm || to is SwampDragon || to is Drake || to is Nightmare || to is Hiryu || to is LesserHiryu || to is Daemon)
                damage *= 3;
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
