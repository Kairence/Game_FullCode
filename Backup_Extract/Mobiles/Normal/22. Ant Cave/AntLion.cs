using System;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("an ant lion corpse")]
    public class AntLion : BaseCreature
    {
        private DateTime _NextTunnel;
        private Map _StartTunnelMap;
        private Point3D _StartTunnelLoc;
        private bool _Tunneling;

        [Constructable]
        public AntLion()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Name = "an ant lion";
            Body = 787;
            BaseSoundID = 1006;

			/* [Ant Lion - Fame 4,500 / General / Weight 1.18]
			   - 스킬 200 마스터 서버용 '중급 암살자' 밸런스 적용
			   - 가상 방어력(VirtualArmor): (4,500/1000) + 3 = 7 (매몰형 갑각 보정 +3)
			   - 저항 밸런스: 최대 75 상한 엄격 준수
			   -------------------------------------------------- */

			// [Attributes] 명성 4,500 보너스 + 가중치 1.18 반영
			this.SetStr(60, 80); 
			this.SetHits(1400, 1650); 
			this.SetDex(10, 20);
			this.SetInt(10, 20);

			SetAttackSpeed(3.0);
			SetDamage(30, 45);

			// [Damage Types] 100% 물리 공격 (묵직한 턱 힘)
			this.SetDamageType(ResistanceType.Physical, 100);

			// [Resistances] 총합 약 215 (Max 75 준수)
			this.SetResistance(ResistanceType.Physical, 55, 65);
			this.SetResistance(ResistanceType.Fire, 25, 35);
			this.SetResistance(ResistanceType.Cold, 30, 40);
			this.SetResistance(ResistanceType.Poison, 55, 65);
			this.SetResistance(ResistanceType.Energy, 20, 30);

			// [Skills] ★ 스킬 200 서버 기준 - 중급자용 핵심 타겟 (재설계)
			// 유저 스킬 70 ~ 90 구간 수련 및 전투에 적합
			this.SetSkill(SkillName.Wrestling, 60.0, 75.0); 
			this.SetSkill(SkillName.Tactics, 60.0, 75.0);
			this.SetSkill(SkillName.Anatomy, 55.0, 70.0);
			this.SetSkill(SkillName.MagicResist, 50.0, 65.0);

			// [Misc] 가상 방어력(Virtual Armor): (4,500/1000) + 3 = 7
			this.VirtualArmor = 7;

			this.Fame = 4500;
			this.Karma = -4500;

            PackItem(new Bone(3));
            PackItem(new FertileDirt(Utility.RandomMinMax(1, 5)));

            if (Core.ML && Utility.RandomDouble() < .33)
                PackItem(Engines.Plants.Seed.RandomPeculiarSeed(3));

            Item orepile = null; /* no trust, no love :( */

            switch (Utility.Random(4))
            {
                case 0:
                    orepile = new DullCopperOre();
                    break;
                case 1:
                    orepile = new ShadowIronOre();
                    break;
                case 2:
                    orepile = new CopperOre();
                    break;
                default:
                    orepile = new BronzeOre();
                    break;
            }

            orepile.Amount = Utility.RandomMinMax(1, 10);
            orepile.ItemID = 0x19B9;
            PackItem(orepile);

            PackBones();
			
			if ( 0.07 >= Utility.RandomDouble() )
			{
				switch ( Utility.Random( 3 ) )
				{
					case 0: PackItem( new UnknownBardSkeleton() ); break;
					case 1: PackItem( new UnknownMageSkeleton() ); break;
					case 2: PackItem( new UnknownRogueSkeleton() ); break;
				}
			}

            SetSpecialAbility(SpecialAbility.DragonBreath);
        }

        public override void OnThink()
        {
            base.OnThink();

            if (!(Combatant is Mobile))
                return;
            
            Mobile combatant = Combatant as Mobile;

            if (_NextTunnel < DateTime.UtcNow && combatant.InRange(Location, 10))
            {
                _NextTunnel = DateTime.UtcNow + TimeSpan.FromSeconds(Utility.RandomMinMax(30, 40));
                DoTunnel(combatant);
            }
        }

        private void DoTunnel(Mobile combatant)
        {
            PublicOverheadMessage(Server.Network.MessageType.Regular, 0x3B3, false, "* The ant lion begins tunneling into the ground *");
            Effects.SendTargetParticles(this, 0x36B0, 20, 10, 1734, 0, 5044, EffectLayer.Head, 0);

            Frozen = true;
            _Tunneling = true;
            _StartTunnelLoc = Location;
            _StartTunnelMap = Map;

            Timer.DelayCall(TimeSpan.FromSeconds(3), () =>
                {
                    if (_Tunneling)
                    {
                        Hidden = true;
                        Blessed = true;

                        Item item = new InternalItem(3892);
                        item.MoveToWorld(Location, Map);

                        item = new InternalItem(4967);
                        item.MoveToWorld(Location, Map);

                        Timer.DelayCall(TimeSpan.FromSeconds(3), () =>
                            {
                                Hidden = false;
                                Blessed = false;

                                if (!combatant.Alive || !combatant.InRange(_StartTunnelLoc, 20) || combatant.Map != _StartTunnelMap)
                                {
                                    MoveToWorld(_StartTunnelLoc, _StartTunnelMap);
                                }
                                else
                                {
                                    MoveToWorld(combatant.Location, combatant.Map);
                                    AOS.Damage(combatant, this, 25, 70, 0, 0, 30, 0);

                                    Item item2 = new InternalItem(3892);
                                    item2.MoveToWorld(Location, Map);

                                    item2 = new InternalItem(4967);
                                    item2.MoveToWorld(Location, Map);
                                }

                                _StartTunnelLoc = Point3D.Zero;
                                _StartTunnelMap = null;
                                _Tunneling = false;
                                Frozen = false;
                            });
                    }
                });
        }

        public override int Damage(int amount, Mobile from, bool informMount, bool checkDisrupt)
        {
            if (_Tunneling && !Hidden && 0.25 > Utility.RandomDouble())
            {
                PublicOverheadMessage(Server.Network.MessageType.Regular, 0x3B3, false, "* You interrupt the ant lion's digging! *");

                Frozen = false;
                Hidden = false;
                Blessed = false;
                _Tunneling = false;
                _StartTunnelLoc = Point3D.Zero;
                _StartTunnelMap = null;
            }

            return base.Damage(amount, from, informMount, checkDisrupt);
        }

        public AntLion(Serial serial)
            : base(serial)
        {
        }
		
		public override void OnGotMeleeAttack(Mobile attacker)
        {
            if (attacker.Weapon is BaseRanged)
                BeginAcidBreath();

            base.OnGotMeleeAttack(attacker);
        }

        public override void OnDamagedBySpell(Mobile attacker)
        {
            base.OnDamagedBySpell(attacker);

            BeginAcidBreath();
        }

        #region Acid Breath
        private DateTime m_NextAcidBreath;

        public void BeginAcidBreath()
        {
            PlayerMobile m = Combatant as PlayerMobile;
            // Mobile m = Combatant;

            if (m == null || m.Deleted || !m.Alive || !Alive || m_NextAcidBreath > DateTime.Now || !CanBeHarmful(m))
                return;

            PlaySound(0x118);
            MovingEffect(m, 0x36D4, 1, 0, false, false, 0x3F, 0);

            TimeSpan delay = TimeSpan.FromSeconds(GetDistanceToSqrt(m) / 5.0);
            Timer.DelayCall<Mobile>(delay, new TimerStateCallback<Mobile>(EndAcidBreath), m);

            m_NextAcidBreath = DateTime.Now + TimeSpan.FromSeconds(5);
        }

        public void EndAcidBreath(Mobile m)
        {
            if (m == null || m.Deleted || !m.Alive || !Alive)
                return;

            if (0.2 >= Utility.RandomDouble())
                m.ApplyPoison(this, Poison.Greater);

            AOS.Damage(m, Utility.RandomMinMax(100, 120), 0, 0, 0, 100, 0);
        }
        #endregion

        public override int GetAngerSound() { return 0x5A; }
        public override int GetIdleSound() { return 0x5A; }
        public override int GetAttackSound() { return 0x164; }
        public override int GetHurtSound() { return 0x187; }
        public override int GetDeathSound() { return 0x1BA; }
        
        public override void GenerateLoot()
        {
            AddLoot(LootPack.Average, 2);
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

            Hidden = false;
            Blessed = false;
        }

        private class InternalItem : Item
        {
            public override int LabelNumber { get { return 1027025; } }

            public InternalItem(int id)
                : base(id)
            {
                Timer.DelayCall(TimeSpan.FromSeconds(10), Delete);
                Hue = 1;
            }

            public InternalItem(Serial serial)
                : base(serial)
            {
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

                Delete();
            }
        }
    }
}
