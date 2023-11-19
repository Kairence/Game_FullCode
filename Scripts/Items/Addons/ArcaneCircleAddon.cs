using System;
using Server.Mobiles;
using Server.Multis;
using Server.Spells;
using Server.Spells.Spellweaving;

namespace Server.Items
{
	public class ArcaneCircle : AddonComponent
	{
        private Timer m_Timer;
        public ArcaneCircle(int itemID)
            : base(itemID)
        {
        }

        public ArcaneCircle(Serial serial)
            : base(serial)
        {
        }
		
		public void EndSpell(Mobile from)
		{
            if (this.m_Timer != null)
                this.m_Timer.Stop();

            this.m_Timer = null;
		}
		
       public void Use(Mobile from)
        {
			if( from.Spell == null )
			{
				this.m_Timer = new InternalTimer(this, from);
				this.m_Timer.Start();
				int skillbonus = 20;
				//집, 마을 체크
				BaseHouse house = BaseHouse.FindHouseAt(from);
				if( house != null && house.IsOwner(from) )
					skillbonus = 24;
				else if( from.Skills.Magery.Value >= 100 )
				{
					this.SendLocalizedMessageTo(from, 1046225);				
					return;
				}

				from.Direction = from.GetDirectionTo(this.GetWorldLocation());

				Effects.PlaySound(this.GetWorldLocation(), this.Map, 0xF9);
				this.SendLocalizedMessageTo(from, 1045131); // You carelessly bump the dip and start it swinging.
				
				Spell spell = new ArcaneCircleSpell(from, null);
				spell.Cast();
				EndSpell(from);

				from.CheckSkill(SkillName.Magery, skillbonus );
			}
        }		
        private class InternalTimer : Timer
        {
            private readonly ArcaneCircle m_Dip;
			private Mobile m_From;
            public InternalTimer(ArcaneCircle dip, Mobile from)
                : base(TimeSpan.FromSeconds(3.0))
            {
				this.m_From = from;
                this.m_Dip = dip;
                this.Priority = TimerPriority.FiftyMS;
            }

            protected override void OnTick()
            {
                this.m_Dip.EndSpell(m_From);
            }
        }
	
        public override void OnDoubleClick(Mobile from)
        {
			if ( from is PlayerMobile )
			{
				PlayerMobile pm = from as PlayerMobile;
				if (!from.InRange(this.GetWorldLocation(), 1))
					this.SendLocalizedMessageTo(from, 501816); // You are too far away to do that.
				else if (from.Mounted)
					this.SendLocalizedMessageTo(from, 501829); // You can't practice on this while on a mount.
				else
				{
					if( pm.TimerList[71] == 0 )
					{
						pm.LastTarget = this;
						pm.TimerList[71] = 30;
						this.Use(from);
					}						
				}
			}
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
	
	
    public class ArcaneCircleAddon : BaseAddon
    {
        [Constructable]
        public ArcaneCircleAddon()
        {
            this.AddComponent(new ArcaneCircle(0x3083), -1, -1, 0);
            this.AddComponent(new ArcaneCircle(0x3080), -1, 0, 0);
            this.AddComponent(new ArcaneCircle(0x3082), 0, -1, 0);
            this.AddComponent(new ArcaneCircle(0x3081), 1, -1, 0);
            this.AddComponent(new ArcaneCircle(0x307D), -1, 1, 0);
            this.AddComponent(new ArcaneCircle(0x307F), 0, 0, 0);
            this.AddComponent(new ArcaneCircle(0x307E), 1, 0, 0);
            this.AddComponent(new ArcaneCircle(0x307C), 0, 1, 0);
            this.AddComponent(new ArcaneCircle(0x307B), 1, 1, 0);
        }

        public ArcaneCircleAddon(Serial serial)
            : base(serial)
        {
        }

 		
        public override BaseAddonDeed Deed
        {
            get
            {
                return new ArcaneCircleDeed();
            }
        }
        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.WriteEncodedInt(1); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadEncodedInt();

            if (version == 0)
                ValidationQueue<ArcaneCircleAddon>.Add(this);
        }

        public void Validate()
        {
            foreach (AddonComponent c in this.Components)
            {
                if (c.ItemID == 0x3083)
                {
                    c.Offset = new Point3D(-1, -1, 0);
                    c.MoveToWorld(new Point3D(this.X + c.Offset.X, this.Y + c.Offset.Y, this.Z + c.Offset.Z), this.Map);
                }
            }
        }
    }

    public class ArcaneCircleDeed : BaseAddonDeed
    {
        [Constructable]
        public ArcaneCircleDeed()
        {
        }

        public ArcaneCircleDeed(Serial serial)
            : base(serial)
        {
        }

        public override BaseAddon Addon
        {
            get
            {
                return new ArcaneCircleAddon();
            }
        }
        public override int LabelNumber
        {
            get
            {
                return 1072703;
            }
        }// arcane circle
        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.WriteEncodedInt(0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadEncodedInt();
        }
    }
}