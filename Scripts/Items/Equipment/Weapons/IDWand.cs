using System;
using Server.Multis;

namespace Server.Items
{
    public class IDWand : BaseWand
    {
        [Constructable]
        public IDWand()
            : base(WandEffect.Identification, 50, 50)
        {
        }
		
		public IDWand( int saveskill ) : base(WandEffect.Identification, 50, 50)
		{
			m_SaveSkill = saveskill;
		}

        public IDWand(Serial serial)
            : base(serial)
        {
        }

		private int m_SaveSkill;
        [CommandProperty(AccessLevel.GameMaster)]
        public int SaveSkill
        {
            get { return m_SaveSkill; }
            set { m_SaveSkill = value; InvalidateProperties(); }
        }		
		
        public override TimeSpan GetUseDelay
        {
            get
            {
                return TimeSpan.Zero;
            }
        }
        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)1); // version
			//스킬 저장
            writer.Write(m_SaveSkill);			
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
            switch ( version )
            {
                case 1:
                    {
                        this.m_SaveSkill = (int)reader.ReadInt();
                        break;
                    }
            }
        }

        public override void GetProperties(ObjectPropertyList list)
        {
            base.GetProperties(list);
			string Idgrade = Misc.Util.ItemRankName(m_SaveSkill);
			list.Add(1060659, "감정 가능한 등급\t{0}", Idgrade);
		}

		/*
		private bool OptionCheck(int power)
		{
			if( m_SaveSkill < power )
				return false;
			return true;
		}

		private bool DiceCheck(int power)
		{
			if( power == 100 )
				return true;
			if( Utility.Random(100) >= ( power - 3 ) * 18 )
				return true;
			return false;
		}
		*/
        public override bool OnWandTarget(Mobile from, object o)
        {
			if( o is Item )
			{
				Item item = o as Item;
				BaseHouse house = BaseHouse.FindHouseAt(from);
				if( item.RootParent == from || ( house != null && house.IsOwner(from)) )
				{
					int skillvalue = ( m_SaveSkill - 3 ) * 400;
					Misc.Util.ItemIdentified( from, skillvalue, item, true );
				}
				return true;
			}				
			return false;
		}
	}

}