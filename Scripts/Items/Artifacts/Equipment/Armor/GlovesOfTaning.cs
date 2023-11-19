using System;
using Server.Engines.Craft;

namespace Server.Items
{
    [FlipableAttribute(0x13c6, 0x13ce)]
    public class LeatherGlovesOfTaning : GlovesOfTaning
	{
		public override bool IsArtifact { get { return true; } }
        public override int Lifespan { get { return 86400; } }

        [Constructable]
        public LeatherGlovesOfTaning(int bonus)
            : base(bonus, 0x13C6)
        {
            Weight = 1;
        }

        public LeatherGlovesOfTaning(Serial serial)
            : base(serial)
        {
        }

        public override int InitMinHits
        {
            get
            {
                return 30;
            }
        }
        public override int InitMaxHits
        {
            get
            {
                return 40;
            }
        }
        public override int AosStrReq
        {
            get
            {
                return 20;
            }
        }
        public override int OldStrReq
        {
            get
            {
                return 10;
            }
        }

        public override ArmorMaterialType MaterialType
        {
            get
            {
                return ArmorMaterialType.Leather;
            }
        }
        public override CraftResource DefaultResource
        {
            get
            {
                return CraftResource.RegularLeather;
            }
        }
        public override ArmorMeditationAllowance DefMedAllowance
        {
            get
            {
                return ArmorMeditationAllowance.All;
            }
        }
        public override int LabelNumber
        {
            get
            {
                return 1063642;
            }
        }// leather blacksmith gloves of mining
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

    [FlipableAttribute(0x13d5, 0x13dd)]
    public class StuddedGlovesOfTaning : GlovesOfTaning
    {
        public override int Lifespan { get { return 259200; } }
        [Constructable]
        public StuddedGlovesOfTaning(int bonus)
            : base(bonus, 0x13D5)
        {
            Weight = 2;
        }

        public StuddedGlovesOfTaning(Serial serial)
            : base(serial)
        {
        }

        public override int InitMinHits
        {
            get
            {
                return 35;
            }
        }
        public override int InitMaxHits
        {
            get
            {
                return 45;
            }
        }
        public override int AosStrReq
        {
            get
            {
                return 25;
            }
        }
        public override int OldStrReq
        {
            get
            {
                return 25;
            }
        }

        public override ArmorMaterialType MaterialType
        {
            get
            {
                return ArmorMaterialType.Studded;
            }
        }
        public override CraftResource DefaultResource
        {
            get
            {
                return CraftResource.RegularLeather;
            }
        }
        public override int LabelNumber
        {
            get
            {
                return 1063643;
            }
        }// studded leather blacksmith gloves of mining
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

    [FlipableAttribute(0x13eb, 0x13f2)]
    public class RingmailGlovesOfTaning : GlovesOfTaning
    {
        public override int Lifespan { get { return 604800; } }
        [Constructable]
        public RingmailGlovesOfTaning(int bonus)
            : base(bonus, 0x13EB)
        {
            Weight = 1;
        }

        public RingmailGlovesOfTaning(Serial serial)
            : base(serial)
        {
        }

        public override int InitMinHits
        {
            get
            {
                return 40;
            }
        }
        public override int InitMaxHits
        {
            get
            {
                return 50;
            }
        }
        public override int AosStrReq
        {
            get
            {
                return 40;
            }
        }
        public override int OldStrReq
        {
            get
            {
                return 20;
            }
        }
        public override int OldDexBonus
        {
            get
            {
                return -1;
            }
        }

        public override ArmorMaterialType MaterialType
        {
            get
            {
                return ArmorMaterialType.Ringmail;
            }
        }
        public override int LabelNumber
        {
            get
            {
                return 1063644;
            }
        }// ringmail blacksmith gloves of mining
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

    public abstract class GlovesOfTaning : BaseArmor
    {
        private int m_Bonus;
        private SkillMod m_SkillMod;
        private int m_Lifespan;
        private Timer m_Timer;

		public virtual int Lifespan { get { return 0; } }
        public virtual bool UseSeconds { get { return true; } }
        [CommandProperty(AccessLevel.GameMaster)]
        public int TimeLeft 
		{ 
			get { return m_Lifespan; }
            set
            {
                m_Lifespan = value;

                InvalidateProperties();
            }
        }

        public GlovesOfTaning(int bonus, int itemID)
            : base(itemID)
        {
            this.m_Bonus = bonus;

            this.Hue = Misc.Util.RandomColor_Yellow(true);

            if (Lifespan > 0)
            {
                m_Lifespan = this.Lifespan;
                StartTimer();
            }
        }

        public GlovesOfTaning(Serial serial)
            : base(serial)
        {
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int Bonus
        {
            get
            {
                return this.m_Bonus;
            }
            set
            {
                this.m_Bonus = value;
                this.InvalidateProperties();

                if (this.m_Bonus == 0)
                {
                    if (this.m_SkillMod != null)
                        this.m_SkillMod.Remove();

                    this.m_SkillMod = null;
                }
                else if (this.m_SkillMod == null && this.Parent is Mobile)
                {
                    this.m_SkillMod = new DefaultSkillMod(SkillName.TasteID, true, this.m_Bonus);
                    ((Mobile)this.Parent).AddSkillMod(this.m_SkillMod);
                }
                else if (this.m_SkillMod != null)
                {
                    this.m_SkillMod.Value = this.m_Bonus;
                }
            }
        }
        public override void OnAdded(object parent)
        {
            base.OnAdded(parent);

            if (this.m_Bonus != 0 && parent is Mobile)
            {
                if (this.m_SkillMod != null)
                    this.m_SkillMod.Remove();

                this.m_SkillMod = new DefaultSkillMod(SkillName.TasteID, true, this.m_Bonus);
                ((Mobile)parent).AddSkillMod(this.m_SkillMod);
            }
        }

        public override void OnRemoved(object parent)
        {
            base.OnRemoved(parent);

            if (this.m_SkillMod != null)
                this.m_SkillMod.Remove();

            this.m_SkillMod = null;
        }

        public override void GetProperties(ObjectPropertyList list)
        {
            base.GetProperties(list);

            if (this.m_Bonus != 0)
                list.Add(1063679, this.m_Bonus.ToString()); // mining bonus +~1_val~

            if (Lifespan > 0)
            {
                if (UseSeconds)
                    list.Add(1072517, m_Lifespan.ToString()); // Lifespan: ~1_val~ seconds
                else
                {
                    TimeSpan t = TimeSpan.FromSeconds(TimeLeft);

                    int weeks = (int)t.Days / 7;
                    int days = t.Days;
                    int hours = t.Hours;
                    int minutes = t.Minutes;

                    if (weeks > 1)
                        list.Add(1153092, (t.Days / 7).ToString()); // Lifespan: ~1_val~ weeks
                    else if (days > 1)
                        list.Add(1153091, t.Days.ToString()); // Lifespan: ~1_val~ days
                    else if (hours > 1)
                        list.Add(1153090, t.Hours.ToString()); // Lifespan: ~1_val~ hours
                    else if (minutes > 1)
                        list.Add(1153089, t.Minutes.ToString()); // Lifespan: ~1_val~ minutes
                    else
                        list.Add(1072517, t.Seconds.ToString()); // Lifespan: ~1_val~ seconds
                }
            }
        }
		
        public virtual void StartTimer()
        {
            if (m_Timer != null || Lifespan == 0)
                return;
	
            m_Timer = Timer.DelayCall(TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(10), new TimerCallback(Slice));
            m_Timer.Priority = TimerPriority.OneSecond;
        }

        public virtual void StopTimer()
        {
            if (m_Timer != null)
                m_Timer.Stop();

            m_Timer = null;
        }

        public virtual void Slice()
        {
            m_Lifespan -= 10;
			
            InvalidateProperties();
			
            if (m_Lifespan <= 0)
                Decay();
        }

        public virtual void Decay()
        {
            if (RootParent is Mobile)
            {
                Mobile parent = (Mobile)RootParent;
				
                if (Name == null)
                    parent.SendLocalizedMessage(1072515, "#" + LabelNumber); // The ~1_name~ expired...
                else
                    parent.SendLocalizedMessage(1072515, Name); // The ~1_name~ expired...
					
                Effects.SendLocationParticles(EffectItem.Create(parent.Location, parent.Map, EffectItem.DefaultDuration), 0x3728, 8, 20, 5042);
                Effects.PlaySound(parent.Location, parent.Map, 0x201);
            }
            else
            {
                Effects.SendLocationParticles(EffectItem.Create(this.Location, this.Map, EffectItem.DefaultDuration), 0x3728, 8, 20, 5042);
                Effects.PlaySound(this.Location, this.Map, 0x201);
            }
			
            StopTimer();
            Delete();
        }
		
        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0); // version

            writer.Write((int)m_Lifespan);
            writer.Write((int)this.m_Bonus);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            switch ( version )
            {
                case 0:
                    {
                        m_Lifespan = reader.ReadInt();
                        this.m_Bonus = reader.ReadInt();
                        break;
                    }
            }

            if(Lifespan > 0)
                StartTimer();

            if (this.m_Bonus != 0 && this.Parent is Mobile)
            {
                if (this.m_SkillMod != null)
                    this.m_SkillMod.Remove();

                this.m_SkillMod = new DefaultSkillMod(SkillName.TasteID, true, this.m_Bonus);
                ((Mobile)this.Parent).AddSkillMod(this.m_SkillMod);
            }
        }
    }
}