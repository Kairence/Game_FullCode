using System;
using Server.Items;
using Server.Spells;

namespace Server.Mobiles
{
    [CorpseName("a changeling corpse")]
    public class Changeling : BaseCreature
    {
        private static readonly int[] m_FireNorth = new int[]
        {
            -1, -1,
            1, -1,
            -1, 2,
            1, 2
        };
        private static readonly int[] m_FireEast = new int[]
        {
            -1, 0,
            2, 0
        };

        private Mobile m_MorphedInto;
        private DateTime m_LastMorph;

        [Constructable]
        public Changeling()
            : base(AIType.AI_Spellweaving, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Name = DefaultName;
            Body = 264;
            Hue = DefaultHue;

			/* [Changeling - Normal - Fame 10,000 / Weight 1.22]
			   - 정글 던전의 환각 복제사 / 일반 몬스터 공식
			   - 배수: 1x (Normal)
			   - VirtualArmor: 10 (명성/1000 보정 0)
			   -------------------------------------------------- */

			// [Attributes] 역산된 Set 값 정밀 적용
			this.SetStr(210, 230); 
			this.SetHits(4800, 4950); 
			this.SetDex(40, 50);
			this.SetInt(40, 50);

			// [Combat Options] 물리 100% (복제 대상에 따라 가변적이나 기본 물리)
			this.SetDamage(25, 45);
			this.SetAttackSpeed(2.0); // 변신술사다운 빠른 반응 속도
			this.SetDamageType(ResistanceType.Physical, 100);

			// [Resistances] 최고 저항 75 이하 준수 / 에너지 약점 설정
			this.SetResistance(ResistanceType.Physical, 40, 50); 
			this.SetResistance(ResistanceType.Fire, 40, 50);      
			this.SetResistance(ResistanceType.Cold, 40, 50);    
			this.SetResistance(ResistanceType.Poison, 40, 50); 
			this.SetResistance(ResistanceType.Energy, 20, 30);   // ★ 변신 해제 유도 약점

			// [Skills] 기본 100~110에 역산 보너스(7.3) 가산
			this.SetSkill(SkillName.Wrestling, 107.0, 117.0); 
			this.SetSkill(SkillName.Tactics, 107.0, 117.0);
			this.SetSkill(SkillName.Magery, 100.0, 115.0);       
			this.SetSkill(SkillName.EvalInt, 100.0, 115.0);
			this.SetSkill(SkillName.MagicResist, 100.0, 115.0);

			this.Tamable = false;
			this.VirtualArmor = 10;
			this.Fame = 10000;
			this.Karma = -10000;

            PackItem(new Arrow(35));
            PackItem(new Bolt(25));
            PackGem(2);

            for (int i = 0; i < Utility.RandomMinMax(0, 1); i++)
            {
                PackItem(Loot.RandomScroll(0, Loot.ArcanistScrollTypes.Length, SpellbookType.Arcanist));
            }
        }

        public Changeling(Serial serial)
            : base(serial)
        {
        }

        public override bool IsEnemy(Mobile m)
        {
            if (m is BaseCreature && ((BaseCreature)m).IsMonster && m.Karma > 0)
            {
                return true;
            }

            return base.IsEnemy(m);
        }

        public virtual string DefaultName
        {
            get
            {
                return "a changeling";
            }
        }
        public virtual int DefaultHue
        {
            get
            {
                return 0;
            }
        }

        public override bool UseSmartAI { get { return true; } }

        public override bool ShowFameTitle
        {
            get
            {
                return false;
            }
        }
        public override bool InitialInnocent
        {
            get
            {
                return (m_MorphedInto != null);
            }
        }
        [CommandProperty(AccessLevel.GameMaster)]
        public Mobile MorphedInto
        {
            get
            {
                return m_MorphedInto;
            }
            set
            {
                if (value == this)
                    value = null;

                if (m_MorphedInto != value)
                {
                    Revert();

                    if (value != null)
                    {
                        Morph(value);
                        m_LastMorph = DateTime.UtcNow;
                    }

                    m_MorphedInto = value;
                    Delta(MobileDelta.Noto);
                }
            }
        }
        public override void GenerateLoot()
        {
            AddLoot(LootPack.AosRich, 3);
            AddLoot(LootPack.LowScrolls);
            AddLoot(LootPack.MedScrolls);
        }

        public override int GetAngerSound()
        {
            return 0x46E;
        }

        public override int GetIdleSound()
        {
            return 0x470;
        }

        public override int GetAttackSound()
        {
            return 0x46D;
        }

        public override int GetHurtSound()
        {
            return 0x471;
        }

        public override int GetDeathSound()
        {
            return 0x46F;
        }

        public override void OnThink()
        {
            base.OnThink();

            if (Combatant is PlayerMobile && m_MorphedInto != Combatant && Utility.RandomDouble() < 0.05)
            {
                MorphedInto = Combatant as Mobile;
            }
        }

        public override bool CheckIdle()
        {
            bool idle = base.CheckIdle();

            if (idle && m_MorphedInto != null && DateTime.UtcNow - m_LastMorph > TimeSpan.FromSeconds(30))
                MorphedInto = null;

            return idle;
        }

        public void DeleteClonedItems()
        {
            for (int i = Items.Count - 1; i >= 0; --i)
            {
                Item item = Items[i];

                if (item is ClonedItem)
                    item.Delete();
            }

            if (Backpack != null)
            {
                for (int i = Backpack.Items.Count - 1; i >= 0; --i)
                {
                    Item item = Backpack.Items[i];

                    if (item is ClonedItem)
                        item.Delete();
                }
            }
        }

        public override void OnAfterDelete()
        {
            DeleteClonedItems();

            base.OnAfterDelete();
        }

        public override void ClearHands()
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0); // version
            writer.Write((m_MorphedInto != null));
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            if (reader.ReadBool())
                ValidationQueue<Changeling>.Add(this);
        }

        public void Validate()
        {
            Revert();
        }

        protected virtual void Morph(Mobile m)
        {
            Body = m.Body;
            Hue = m.Hue;
            Female = m.Female;
            Name = m.Name;
            NameHue = m.NameHue;
            Title = m.Title;
            Kills = m.Kills;
            HairItemID = m.HairItemID;
            HairHue = m.HairHue;
            FacialHairItemID = m.FacialHairItemID;
            FacialHairHue = m.FacialHairHue;

            // TODO: Skills?

            foreach (Item item in m.Items)
            {
                if (item.Layer != Layer.Backpack && item.Layer != Layer.Mount && item.Layer != Layer.Bank)
                    AddItem(new ClonedItem(item)); // TODO: Clone weapon/armor attributes
            }

            PlaySound(0x511);
            FixedParticles(0x376A, 1, 14, 5045, EffectLayer.Waist);
        }

        protected virtual void Revert()
        {
            Body = 264;
            Hue = (IsParagon && DefaultHue == 0) ? Paragon.Hue : DefaultHue;
            Female = false;
            Name = DefaultName;
            NameHue = -1;
            Title = null;
            Kills = 0;
            HairItemID = 0;
            HairHue = 0;
            FacialHairItemID = 0;
            FacialHairHue = 0;

            DeleteClonedItems();

            PlaySound(0x511);
            FixedParticles(0x376A, 1, 14, 5045, EffectLayer.Waist);
        }

        private void FireEffects(int itemID, int[] offsets)
        {
            for (int i = 0; i < offsets.Length; i += 2)
            {
                Point3D p = Location;

                p.X += offsets[i];
                p.Y += offsets[i + 1];

                if (SpellHelper.AdjustField(ref p, Map, 12, false))
                    Effects.SendLocationEffect(p, Map, itemID, 50);
            }
        }

        private class ClonedItem : Item
        {
            public ClonedItem(Item item)
                : base(item.ItemID)
            {
                Name = item.Name;
                Weight = item.Weight;
                Hue = item.Hue;
                Layer = item.Layer;
                Movable = false;
            }

            public ClonedItem(Serial serial)
                : base(serial)
            {
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
            }
        }
    }
}