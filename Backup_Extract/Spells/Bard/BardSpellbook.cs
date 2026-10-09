using System;
using Server;
using Server.Items;
using Server.Gumps;
using Server.Network;
using Server.Spells;
using Server.Spells.Bard;
using Server.Custom.Bard;

namespace Server.Items
{
    public class BardSpellbook : Item
    {
        [Constructable]
        public BardSpellbook() : base(0xEFA) 
        {
            Weight = 1.0;
            Name = "바드 악보집 (Bardic Primer)";
            Hue = 0x482;
            LootType = LootType.Blessed;
        }

        public BardSpellbook(Serial serial) : base(serial) { }

        public override void OnDoubleClick(Mobile from)
        {
            if (Parent == from || (from.Backpack != null && IsChildOf(from.Backpack)))
            {
                from.CloseGump(typeof(BardSpellbookGump));
                from.SendGump(new BardSpellbookGump(from));
            }
            else
            {
                from.SendLocalizedMessage(1042001); // That must be in your pack for you to use it.
            }
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
        }
    }
}

namespace Server.Custom.Bard
{
    public class BardSpellbookGump : Gump
    {
        private readonly Mobile _owner;

        public BardSpellbookGump(Mobile owner) : base(100, 100)
        {
            _owner = owner;
            
            AddPage(0);
            AddBackground(0, 0, 480, 300, 5054);
            AddHtml(0, 15, 480, 20, "<center>바드 악보집 (Bardic Primer)</center>", false, false);

            AddHtml(55, 50, 140, 20, "<b>Music & Peace</b>", false, false);
            AddHtml(265, 50, 140, 20, "<b>Discord & Provoke</b>", false, false);

            // Music & Peace
            DrawSpell(1, 40, 80, SkillName.Musicianship, 50.0, "인내의 아리아", 0x5B02);
            DrawSpell(2, 40, 120, SkillName.Musicianship, 100.0, "영웅의 행진곡", 0x5B01);
            DrawSpell(5, 40, 170, SkillName.Peacemaking, 50.0, "자장가", 0x5D04);
            DrawSpell(6, 40, 210, SkillName.Peacemaking, 100.0, "치유의 화음", 0x5D09);

            // Discord & Provoke
            DrawSpell(9, 250, 80, SkillName.Discordance, 50.0, "음파 붕괴", 0x5D0F);
            DrawSpell(10, 250, 120, SkillName.Discordance, 100.0, "파멸의 공명", 0x5D0B);
            DrawSpell(13, 250, 170, SkillName.Provocation, 50.0, "정신 착란", 0x5D0C);
            DrawSpell(15, 250, 210, SkillName.Provocation, 150.0, "세이렌의 부름", 0x5B03);
        }

        private void DrawSpell(int id, int x, int y, SkillName sk, double val, string name, int icon)
        {
            bool canCast = _owner.Skills[sk].Value >= val;
            
            AddImage(x, y, icon, canCast ? 0 : 995);

            if (canCast)
            {
                AddButton(x + 40, y + 10, 0x120E, 0x120F, id, GumpButtonType.Reply, 0);
                AddLabel(x + 60, y + 10, 0x480, name);
            }
            else
            {
                AddLabel(x + 60, y + 10, 995, name);
            }
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            if (info.ButtonID <= 0) return;
            Spell s = SpellRegistry.NewSpell(BardSpellRegistry.Offset + info.ButtonID, sender.Mobile, null);
            
            if (s != null) s.Cast();
            else sender.Mobile.SendMessage("존재하지 않는 곡입니다.");
        }
    }
}