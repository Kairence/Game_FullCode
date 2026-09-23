using System;
using Server.Guilds;
using Server.Engines.Craft;

namespace Server.Items
{
    [Alterable(typeof(DefBlacksmithy), typeof(GargishChaosShield))]
    public class ChaosShield : BaseShield
    {
        [Constructable]
        public ChaosShield()
            : base(0x1BC3)
        {
            Weight = 40.0;
        }

        public ChaosShield(Serial serial)
            : base(serial)
        {
        }

        public override int InitMinHits
        {
            get
            {
                return 100;
            }
        }
        public override int InitMaxHits
        {
            get
            {
                return 100;
            }
        }
        public override int AosStrReq
        {
            get
            {
                return 3000;
            }
        }
        public override int AosDexReq
        {
            get
            {
                return 3000;
            }
        }
        public override int AosIntReq
        {
            get
            {
                return 3000;
            }
        }
		public override double ArmorRating
		{
			get
			{
				return 16.0; // 원하는 감소 수치를 입력하세요.
			}
		}
        public override int ArmorBase
        {
            get
            {
                return 11;
            }
        }		
        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0);//version
        }

        public override bool OnEquip(Mobile from)
        {
            return this.Validate(from) && base.OnEquip(from);
        }

        // [커스텀] 혼돈 방패(카오스 방패) 장착 조건: 강령술(Necromancy) 150 이상
        public override bool CanEquip(Mobile from)
        {
            if (from.Skills[SkillName.Necromancy].Value < 150.0)
            {
                from.SendMessage("네크로맨시 스킬이 150 이상이어야 혼돈 방패를 장착할 수 있습니다.");
                return false;
            }

            return base.CanEquip(from);
        }

        public override void OnSingleClick(Mobile from)
        {
            if (this.Validate(this.Parent as Mobile))
                base.OnSingleClick(from);
        }

        public virtual bool Validate(Mobile m)
        {
            if (m == null || !m.Player || m.IsStaff() || Core.AOS)
                return true;

            Guild g = m.Guild as Guild;

            if (g == null || g.Type != GuildType.Chaos)
            {
                m.FixedEffect(0x3728, 10, 13);
                this.Delete();

                return false;
            }

            return true;
        }
    }
}
