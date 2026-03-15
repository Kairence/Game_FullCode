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

            /* Trapdoor Spider - Fame 5,000 */
			this.SetStr(100, 150);  
			this.SetDex(140, 160);   
			this.SetInt(40, 60);     

			this.SetHits(200, 400);  // 최종 Hits 약 10,500
			this.SetStam(140, 160);

			SetAttackSpeed(1.6);
			SetDamage(32, 48);

			this.SetSkill(SkillName.Wrestling, 105.2);
			this.SetSkill(SkillName.Tactics, 105.2);
			this.SetSkill(SkillName.Hiding, 100.0);
			this.SetSkill(SkillName.Stealth, 100.0); // 스텔스 추가

			this.SetDamageType(ResistanceType.Physical, 70);
			this.SetDamageType(ResistanceType.Cold, 30); 

			this.SetResistance(ResistanceType.Physical, -40, -30);
			this.SetResistance(ResistanceType.Fire, -60, -50);
			this.VirtualArmor = 2;

			this.Tamable = true;
			this.ControlSlots = 1;
			this.MinTameSkill = 85.1;

			this.Fame = 5000;
			this.Karma = -5000;
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