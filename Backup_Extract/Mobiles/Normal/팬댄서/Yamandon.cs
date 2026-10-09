using System;
using System.Collections;
using Server.Items;

namespace Server.Mobiles
{
    [TypeAlias("Server.Mobiles.Yamadon")]
    [CorpseName("a yamandon corpse")]
    public class Yamandon : BaseCreature
    {
        [Constructable]
        public Yamandon()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Name = "a yamandon";
            Body = 249;

			/* [Yamandon - World Boss - Fame 29,000 / Weight 1.30]
			   - 성지 던전의 전설적 맹독수 / 보스급 배수 적용
			   - Attributes: x5 / Skills: x2 (Keep Formula)
			   - VirtualArmor: 30 (공식 34이나 최대치 30 준수)
			   -------------------------------------------------- */

			Boss = true;

			// 최종 Str 약 27,803 (보너스 포함)
			this.SetStr(23300, 23700); 

			// 최종 Hits 약 616,655 (민맥 편차 2,000 고정 룰)
			this.SetHits(520700, 522700); 

			// 최종 Dex/Int 약 5,560
			this.SetDex(4700, 4800);
			this.SetInt(4700, 4800);

			// [Combat Options] 물리 40% / 독 60% (치명적인 맹독 타격)
			this.SetDamage(100, 150);
			this.SetAttackSpeed(2.5);
			this.SetDamageType(ResistanceType.Physical, 40);
			this.SetDamageType(ResistanceType.Poison, 60);

			// [Resistances] 최고 저항 75 이하 엄격 준수 (형님 지침)
			this.SetResistance(ResistanceType.Physical, 65, 75); 
			this.SetResistance(ResistanceType.Fire, 50, 60);      
			this.SetResistance(ResistanceType.Cold, 35, 45);    // ★ 명확한 약점 (변온 동물 컨셉)
			this.SetResistance(ResistanceType.Poison, 75, 75);   // 독의 지배자 (Max 75)
			this.SetResistance(ResistanceType.Energy, 55, 65);   

			// [Skills] 최종 숙련도 약 370 (서버 캡 200.0에 맞춰 설정)
			this.SetSkill(SkillName.Wrestling, 195.0, 200.0); 
			this.SetSkill(SkillName.Tactics, 195.0, 200.0);
			this.SetSkill(SkillName.Anatomy, 195.0, 200.0);
			this.SetSkill(SkillName.Poisoning, 195.0, 200.0);     // 전설적인 독술
			this.SetSkill(SkillName.MagicResist, 185.0, 200.0);

			this.Tamable = false;
			this.VirtualArmor = 30;
			this.Fame = 29000;
			this.Karma = -29000;

            if (Utility.RandomDouble() < .50)
                PackItem(Engines.Plants.Seed.RandomBonsaiSeed());

            PackItem(new Eggs(2));

            SetWeaponAbility(WeaponAbility.DoubleStrike);
        }

        public Yamandon(Serial serial)
            : base(serial)
        {
        }

        public override bool ReacquireOnMovement
        {
            get
            {
                return true;
            }
        }
        public override Poison PoisonImmune
        {
            get
            {
                return Poison.Lethal;
            }
        }
        public override Poison HitPoison
        {
            get
            {
                return Utility.RandomBool() ? Poison.Deadly : Poison.Lethal;
            }
        }
        public override int TreasureMapLevel
        {
            get
            {
                return 5;
            }
        }
        public override int Hides
        {
            get
            {
                return 20;
            }
        }

        public override void GenerateLoot()
        {
            AddLoot(LootPack.UltraRich);
            AddLoot(LootPack.FilthyRich, 2);
            AddLoot(LootPack.Gems, 6);
        }

        public override void OnDamagedBySpell(Mobile attacker)
        {
            base.OnDamagedBySpell(attacker);

            DoCounter(attacker);
        }

        public override void OnGotMeleeAttack(Mobile attacker)
        {
            base.OnGotMeleeAttack(attacker);

            DoCounter(attacker);
        }

        public override int GetAttackSound()
        {
            return 1260;
        }

        public override int GetAngerSound()
        {
            return 1262;
        }

        public override int GetDeathSound()
        {
            return 1259; //Other Death sound is 1258... One for Yamadon, one for Serado?
        }

        public override int GetHurtSound()
        {
            return 1263;
        }

        public override int GetIdleSound()
        {
            return 1261;
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

        private void DoCounter(Mobile attacker)
        {
            if (Map == null)
                return;

            if (attacker is BaseCreature && ((BaseCreature)attacker).BardProvoked)
                return;

            if (0.2 > Utility.RandomDouble())
            {
                /* Counterattack with Hit Poison Area
                * 20-25 damage, unresistable
                * Lethal poison, 100% of the time
                * Particle effect: Type: "2" From: "0x4061A107" To: "0x0" ItemId: "0x36BD" ItemIdName: "explosion" FromLocation: "(296 615, 17)" ToLocation: "(296 615, 17)" Speed: "1" Duration: "10" FixedDirection: "True" Explode: "False" Hue: "0xA6" RenderMode: "0x0" Effect: "0x1F78" ExplodeEffect: "0x1" ExplodeSound: "0x0" Serial: "0x4061A107" Layer: "255" Unknown: "0x0"
                * Doesn't work on provoked monsters
                */
                Mobile target = null;

                if (attacker is BaseCreature)
                {
                    Mobile m = ((BaseCreature)attacker).GetMaster();
					
                    if (m != null)
                        target = m;
                }

                if (target == null || !target.InRange(this, 18))
                    target = attacker;

                Animate(10, 4, 1, true, false, 0);

                ArrayList targets = new ArrayList();
                IPooledEnumerable eable = target.GetMobilesInRange(8);

                foreach (Mobile m in eable)
                {
                    if (m == this || !CanBeHarmful(m))
                        continue;

                    if (m is BaseCreature && (((BaseCreature)m).Controlled || ((BaseCreature)m).Summoned || ((BaseCreature)m).Team != Team))
                        targets.Add(m);
                    else if (m.Player && m.Alive)
                        targets.Add(m);
                }
                eable.Free();

                for (int i = 0; i < targets.Count; ++i)
                {
                    Mobile m = (Mobile)targets[i];

                    DoHarmful(m);

                    AOS.Damage(m, this, Utility.RandomMinMax(20, 25), true, 0, 0, 0, 100, 0);

                    m.FixedParticles(0x36BD, 1, 10, 0x1F78, 0xA6, 0, (EffectLayer)255);
                    m.ApplyPoison(this, Poison.Lethal);
                }
            }
        }
    }
}
