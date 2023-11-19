using System;
using Server.Mobiles;
using Server.Multis;
using Server.Spells;
using Server.Spells.Spellweaving;

namespace Server.Items
{
	public class LockpickChest : AddonComponent
	{
        private Timer m_Timer;
        public LockpickChest(int itemID)
            : base(itemID)
        {
        }

        public LockpickChest(Serial serial)
            : base(serial)
        {
        }
		
		public void EndSpell(Mobile from)
		{
            if (this.m_Timer != null)
                this.m_Timer.Stop();

            this.m_Timer = null;

			if( from is PlayerMobile )
			{
				PlayerMobile pm = from as PlayerMobile;
				if( pm.Loop && pm.TimerList[71] == 0 )
				{
					pm.LoopCheck = false;
					OnDoubleClick( from );
				}
			}
		}
        public override int LabelNumber
        {
            get
            {
                return 1044297;
            }
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
					this.SendLocalizedMessageTo(from, 502075);				
					return;
				}

				from.Direction = from.GetDirectionTo(this.GetWorldLocation());

				Effects.PlaySound(this.GetWorldLocation(), this.Map, 0x241);
				this.SendLocalizedMessageTo(from, 502076); // You carelessly bump the dip and start it swinging.

				/*
				if( from is PlayerMobile )
				{
					PlayerMobile pm = from as PlayerMobile;
					
					if( pm.Loop )
					{
						pm.LoopCheck = false;
						Timer.DelayCall( TimeSpan.FromSeconds( 3.0 ), new TimerStateCallback<Mobile>( OnDoubleClick ), from );
					}
				}
				*/
				from.CheckSkill(SkillName.Lockpicking, skillbonus );
			}
        }		
        private class InternalTimer : Timer
        {
            private readonly LockpickChest m_Dip;
			private Mobile m_From;
            public InternalTimer(LockpickChest dip, Mobile from)
                : base(TimeSpan.FromSeconds(0.25), TimeSpan.FromSeconds(2.75))
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
				else if( pm.TimerList[71] == 0 )
				{
					pm.LastTarget = this;
					pm.TimerList[71] = 30;
					this.Use(from);
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
	
	
    public class LockpickChestAddon : BaseAddon
    {
        [Constructable]
        public LockpickChestAddon()
        {
            this.AddComponent(new LockpickChest(0x4D0C), 0, 0, 0);
        }

        public LockpickChestAddon(Serial serial)
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
                ValidationQueue<LockpickChestAddon>.Add(this);
		}
    }

    public class LockpickChestDeed : BaseAddonDeed
    {
        [Constructable]
        public LockpickChestDeed()
        {
        }

        public LockpickChestDeed(Serial serial)
            : base(serial)
        {
        }

        public override BaseAddon Addon
        {
            get
            {
                return new LockpickChestAddon();
            }
        }
        public override int LabelNumber
        {
            get
            {
                return 1044297;
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