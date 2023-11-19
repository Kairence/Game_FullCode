using System;
using Server.Items;
using Server.Targeting;
using Server.Multis;

namespace Server.Spells.Third
{
    public class TelekinesisSpell : MagerySpell
    {
        private static readonly SpellInfo m_Info = new SpellInfo(
            "Telekinesis", "Ort Por Ylem",
            203,
            9031,
            Reagent.Bloodmoss,
            Reagent.MandrakeRoot);
        public TelekinesisSpell(Mobile caster, Item scroll)
            : base(caster, scroll, m_Info)
        {
        }

        public override SpellCircle Circle
        {
            get
            {
                return SpellCircle.Third;
            }
        }
        public override void OnCast()
        {
            this.Caster.Target = new InternalFirstTarget(this);
        }

        public void Target(ITelekinesisable obj)
        {
            if (this.CheckSequence())
            {
                SpellHelper.Turn(this.Caster, obj);

                obj.OnTelekinesis(this.Caster);
            }

            this.FinishSequence();
        }
		
        public class InternalFirstTarget : Target
        {
            private readonly TelekinesisSpell m_Owner;
            public InternalFirstTarget(TelekinesisSpell owner)
                : base(Core.ML ? 10 : 12, false, TargetFlags.None)
            {
                this.m_Owner = owner;
            }

            protected override void OnTarget(Mobile from, object o)
            {
				BaseHouse house = BaseHouse.FindHouseAt(from);
				if( house != null && house.IsOwner(from) )
				{
					if( o is HouseSign )
					{
						from.SendMessage("집 간판은 이동시킬 수 없습니다.");
					}
					else if( o is Item )
					{
						from.SendMessage("이 아이템을 어디로 옮기시겠습니까?");
						from.Target = new InternalSecondTarget(m_Owner, o);
					}
					else if( o is Mobile )
					{
						from.SendMessage("이 생명체를 어디로 옮기시겠습니까?");
						from.Target = new InternalSecondTarget(m_Owner, o);
					}
					else
					{
						from.SendMessage("이것은 옮길 수 없습니다.");
					}
				}
            }
        }
		
        public class InternalSecondTarget : Target
        {
			private readonly object m_Object;
            private readonly TelekinesisSpell m_Owner;
			public InternalSecondTarget(TelekinesisSpell owner, object o)
				: base(-1, true, TargetFlags.None)
			{
				this.m_Owner = owner;
				this.m_Object = o;
			}
            protected override void OnTargetFinish(Mobile from)
            {
                this.m_Owner.FinishSequence();
            }

			protected override void OnTarget(Mobile from, object o)
			{
				IPoint3D p = o as IPoint3D;

				//SpellHelper.GetSurfaceTop(ref p);
				//Point3D to = new Point3D(p);

				if ( p != null )
				{
					if( p is Item )
						p = ((Item)p).GetWorldTop();

					Point3D to = new Point3D(p);
					if ( m_Object is Item )
					{
						Item item = (Item)this.m_Object;

						BaseHouse house = BaseHouse.FindHouseAt(to, from.Map, item.ItemData.Height);
						if( house == null )
						{
							from.SendLocalizedMessage(500447); // That is not accessible.
							return;
						}
						else if (!item.Deleted)
						{
							from.SendMessage("아이템을 이동합니다.");
							item.MoveToWorld(new Point3D(p), from.Map);
						}
						else
						{
							from.SendLocalizedMessage(1154965); // Invalid item.
						}
					}
					else if ( m_Object is Mobile )
					{
						Mobile m = (Mobile)this.m_Object;

						BaseHouse house = BaseHouse.FindHouseAt(to, from.Map, 16);
						if( house == null )
						{
							from.SendLocalizedMessage(500447); // That is not accessible.
							return;
						}
						else if (!m.Deleted)
						{
							from.SendMessage("생명체를 이동합니다.");
							m.MoveToWorld(new Point3D(p), from.Map);
						}
						else
						{
							from.SendMessage("그건 이동할 수 없습니다.");
						}
					}					
				}
				else
					from.SendMessage("타겟이 없습니다.");
				m_Owner.FinishSequence();
			}
		}		
    }
}

namespace Server
{
    public interface ITelekinesisable : IPoint3D
    {
        void OnTelekinesis(Mobile from);
    }
}
