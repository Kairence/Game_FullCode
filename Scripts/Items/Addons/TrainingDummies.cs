using System;
using Server.Mobiles;
using Server.Multis;

namespace Server.Items
{
    [Flipable(0x1070, 0x1074)]
    public class TrainingDummy : AddonComponent
    {
        private double m_MinSkill;
        private double m_MaxSkill;
        private Timer m_Timer;
        [Constructable]
        public TrainingDummy()
            : this(0x1074)
        {
        }

        [Constructable]
        public TrainingDummy(int itemID)
            : base(itemID)
        {
            this.m_MinSkill = -25.0;
            this.m_MaxSkill = +25.0;
        }

        public TrainingDummy(Serial serial)
            : base(serial)
        {
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public double MinSkill
        {
            get
            {
                return this.m_MinSkill;
            }
            set
            {
                this.m_MinSkill = value;
            }
        }
        [CommandProperty(AccessLevel.GameMaster)]
        public double MaxSkill
        {
            get
            {
                return this.m_MaxSkill;
            }
            set
            {
                this.m_MaxSkill = value;
            }
        }
        [CommandProperty(AccessLevel.GameMaster)]
        public bool Swinging
        {
            get
            {
                return (this.m_Timer != null);
            }
        }
        public virtual void UpdateItemID()
        {
            int baseItemID = (this.ItemID / 2) * 2;

            this.ItemID = baseItemID + (this.Swinging ? 1 : 0);
        }

        public virtual void BeginSwing(Mobile from)
        {
            if (this.m_Timer != null)
                this.m_Timer.Stop();

            this.m_Timer = new InternalTimer(this, from);
            this.m_Timer.Start();
        }

        public virtual void EndSwing(Mobile from)
        {
            if (this.m_Timer != null)
                this.m_Timer.Stop();

            this.m_Timer = null;

            this.UpdateItemID();
        }

        public void OnHit()
        {
            this.UpdateItemID();
            Effects.PlaySound(this.GetWorldLocation(), this.Map, Utility.RandomList(0x3A4, 0x3A6, 0x3A9, 0x3AE, 0x3B4, 0x3B6));
        }

		Mobile m_From = null;
		
        public void Use(Mobile from, BaseWeapon weapon)
        {
			Skill atkSkill = from.Skills[weapon.Skill];
			
			int skillbonus = 20;
			//집, 마을 체크
			BaseHouse house = BaseHouse.FindHouseAt(from);
			if( house != null && house.IsOwner(from) )
				skillbonus = 24;
			else if( atkSkill.Value >= 100 )
			{
				this.SendLocalizedMessageTo(from, 501828);				
				return;
			}

			this.BeginSwing(from);

			from.Direction = from.GetDirectionTo(this.GetWorldLocation());
			weapon.PlaySwingAnimation(from);

			from.CheckSkill(weapon.Skill, skillbonus );
        }

        public override void OnDoubleClick(Mobile from)
        {
			if ( from is PlayerMobile )
			{
				PlayerMobile pm = from as PlayerMobile;
				BaseWeapon weapon = from.Weapon as BaseWeapon;

				if (weapon is BaseRanged)
					this.SendLocalizedMessageTo(from, 501822); // You can't practice ranged weapons on this.
				else if (weapon == null || !from.InRange(this.GetWorldLocation(), weapon.MaxRange))
					this.SendLocalizedMessageTo(from, 501816); // You are too far away to do that.
				else if (this.Swinging)
					this.SendLocalizedMessageTo(from, 501815); // You have to wait until it stops swinging.
				//else if (from.Skills[weapon.Skill].Base >= this.m_MaxSkill)
				//    this.SendLocalizedMessageTo(from, 501828); // Your skill cannot improve any further by simply practicing with a dummy.
				else if (from.Mounted)
					this.SendLocalizedMessageTo(from, 501829); // You can't practice on this while on a mount.
				else
				{
					if( pm.TimerList[71] == 0 )
					{
						pm.TimerList[71] = 30;
						pm.LastTarget = this;
						this.Use(from, weapon);
					}
					else if( pm.Loop )
					{
						OnDoubleClick( from );
					}
				}
			}
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0);

            writer.Write(this.m_MinSkill);
            writer.Write(this.m_MaxSkill);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            switch ( version )
            {
                case 0:
                    {
                        this.m_MinSkill = reader.ReadDouble();
                        this.m_MaxSkill = reader.ReadDouble();

                        if (this.m_MinSkill == 0.0 && this.m_MaxSkill == 30.0)
                        {
                            this.m_MinSkill = -25.0;
                            this.m_MaxSkill = 25.0;
                        }

                        break;
                    }
            }

            this.UpdateItemID();
        }

        private class InternalTimer : Timer
        {
            private readonly TrainingDummy m_Dummy;
			private Mobile m_From;
            private bool m_Delay = true;
            public InternalTimer(TrainingDummy dummy, Mobile from)
                : base(TimeSpan.FromSeconds(0.25), TimeSpan.FromSeconds(2.75))
            {
				this.m_From = from;
                this.m_Dummy = dummy;
                this.Priority = TimerPriority.FiftyMS;
            }

            protected override void OnTick()
            {
                if (this.m_Delay)
                    this.m_Dummy.OnHit();
                else
                    this.m_Dummy.EndSwing(m_From);

                this.m_Delay = !this.m_Delay;
            }
        }
    }

    public class TrainingDummyEastAddon : BaseAddon
    {
        [Constructable]
        public TrainingDummyEastAddon()
        {
            this.AddComponent(new TrainingDummy(0x1074), 0, 0, 0);
        }

        public TrainingDummyEastAddon(Serial serial)
            : base(serial)
        {
        }

        public override BaseAddonDeed Deed
        {
            get
            {
                return new TrainingDummyEastDeed();
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
        }
    }

    public class TrainingDummyEastDeed : BaseAddonDeed
    {
        [Constructable]
        public TrainingDummyEastDeed()
        {
        }

        public TrainingDummyEastDeed(Serial serial)
            : base(serial)
        {
        }

        public override BaseAddon Addon
        {
            get
            {
                return new TrainingDummyEastAddon();
            }
        }
        public override int LabelNumber
        {
            get
            {
                return 1044335;
            }
        }// training dummy (east)
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

    public class TrainingDummySouthAddon : BaseAddon
    {
        [Constructable]
        public TrainingDummySouthAddon()
        {
            this.AddComponent(new TrainingDummy(0x1070), 0, 0, 0);
        }

        public TrainingDummySouthAddon(Serial serial)
            : base(serial)
        {
        }

        public override BaseAddonDeed Deed
        {
            get
            {
                return new TrainingDummySouthDeed();
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
        }
    }

    public class TrainingDummySouthDeed : BaseAddonDeed
    {
        [Constructable]
        public TrainingDummySouthDeed()
        {
        }

        public TrainingDummySouthDeed(Serial serial)
            : base(serial)
        {
        }

        public override BaseAddon Addon
        {
            get
            {
                return new TrainingDummySouthAddon();
            }
        }
        public override int LabelNumber
        {
            get
            {
                return 1044336;
            }
        }// training dummy (south)
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