using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a trapdoor spider corpse")]
    public class TrapdoorSpider : BaseCreature
    {
        public override bool CanStealth { get { return true; } } 

        [Constructable]
        public TrapdoorSpider()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Name = "a trapdoor spider";
            Body = 737;
            Hidden = true; 

            SetStr(200, 400);
            SetDex(10, 50);
            SetInt(10, 50);

            SetHits(400, 410);
			SetStam(100, 200);
			SetMana(50, 90);

            SetDamage(10, 70);
			SetAttackSpeed( 30.0 );

            SetDamageType(ResistanceType.Poison, 100);

            SetResistance(ResistanceType.Physical, 0);
            SetResistance(ResistanceType.Fire, 10, 15);
            SetResistance(ResistanceType.Cold, 10, 15);
            SetResistance(ResistanceType.Poison, 20, 25);
            SetResistance(ResistanceType.Energy, 25, 30);
			
            SetSkill(SkillName.Hiding, 50.3, 59.9);
            SetSkill(SkillName.Stealth, 50.5, 59.6);

            Fame = 5000;
            Karma = -5000;
            VirtualArmor = 10;
		}

        public TrapdoorSpider(Serial serial)
            : base(serial)
        {
        }

        public override void GenerateLoot()
        {
            AddLoot(LootPack.Rich);
        }

        public override int GetIdleSound()
        {
            return 1605;
        }

        public override int GetAngerSound()
        {
            return 1602;
        }

        public override int GetHurtSound()
        {
            return 1604;
        }

        public override int GetDeathSound()
        {
            return 1603;
        }

        public override Poison HitPoison
        {
            get
            {
                return Poison.Regular;
            }
        }
		
        public override void OnThink()
        {
            if (!this.Alive || this.Deleted)
            {
                return;
            }

            if (!this.Hidden)
            {
                HideSelf();
                base.OnThink();
            }
        }

        private void HideSelf()
        {
            if (Core.TickCount >= this.NextSkillTime)
            {
                Effects.SendLocationParticles(
                    EffectItem.Create(this.Location, this.Map, EffectItem.DefaultDuration), 0x3728, 10, 10, 2023);

                this.PlaySound(0x22F);
                this.Hidden = true;

                this.UseSkill(SkillName.Stealth);
            }
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            var version = reader.ReadInt();
        }
    }
}