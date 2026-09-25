using System;
using System.Linq;
using Server;
using Server.Gumps;
using Server.Network;
using Server.Multis;
using Server.Mobiles;
using Server.Misc;

namespace Server.Misc
{
    public class PlayerJobBoardGump : Gump
    {
        private Mobile m_From;
        private BaseHouse m_House;

        public PlayerJobBoardGump(Mobile from, BaseHouse house) : base(50, 50)
        {
            m_From = from;
            m_House = house;

            Closable = true;
            Disposable = true;
            Dragable = true;
            Resizable = false;

            AddPage(0);
            AddBackground(0, 0, 400, 300, 9270);
            AddHtml(0, 15, 400, 20, "<CENTER><BASEFONT COLOR=#FFFFFF>?�업 ?�설 (구인 공고) 관�?/BASEFONT></CENTER>", false, false);

            int maxSlots = house.MaxSecures;
            int usedSlots = PartTimeManager.ActiveRequests.Count(r => r.Origin == JobOrigin.PlayerPrivate && r.TargetHouseName == house.Sign.Name) 
                          + RetailVendor.RetailVendors.Count(v => v.Owner == from && v.Map == house.Map && house.Region.Contains(v.Location));

            AddHtml(20, 50, 360, 20, $"<BASEFONT COLOR=#FFFFFF>?�재 ?�용 중인 ?�업 ?�롯: {usedSlots} / {maxSlots}</BASEFONT>", false, false);

            AddHtml(20, 80, 200, 20, "<BASEFONT COLOR=#FFFFFF>?�급 (최소 600 Gold):</BASEFONT>", false, false);
            AddBackground(200, 75, 100, 25, 9350);
            AddTextEntry(205, 78, 90, 20, 0, 1, "600"); // TextID 1 is Daily Wage

            AddButton(20, 120, 247, 248, 1, GumpButtonType.Reply, 0);
            AddHtml(50, 122, 200, 20, "<BASEFONT COLOR=#FFFFFF>?�매 ?�점(Retail) ?�바 구인</BASEFONT>", false, false);

            AddButton(20, 160, 247, 248, 2, GumpButtonType.Reply, 0);
            AddHtml(50, 162, 200, 20, "<BASEFONT COLOR=#FFFFFF>공고 취소 / ?�바???�고</BASEFONT>", false, false);
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            if (info.ButtonID == 0) return;

            if (info.ButtonID == 1) // Post Job
            {
                int maxSlots = m_House.MaxSecures;
                int usedSlots = PartTimeManager.ActiveRequests.Count(r => r.Origin == JobOrigin.PlayerPrivate && r.TargetHouseName == m_House.Sign.Name) 
                              + RetailVendor.RetailVendors.Count(v => v.Owner == m_From && v.Map == m_House.Map && m_House.Region.Contains(v.Location));

                if (usedSlots >= maxSlots)
                {
                    m_From.SendMessage("?�업 ?�롯(?�큐???�도)??부족하?????�상 구인 공고�??????�습?�다.");
                    return;
                }

                TextRelay wageEntry = info.GetTextEntry(1);
                int wage = 600;
                if (wageEntry != null && int.TryParse(wageEntry.Text, out int parsed))
                {
                    wage = Math.Max(600, parsed); // Minimum 600
                }

                TownJobRequest req = new TownJobRequest
                {
                    Origin = JobOrigin.PlayerPrivate,
                    TargetHouseName = m_House.Sign.Name,
                    Category = JobCategory.Merchant,
                    Tier = JobTier.Beginner,
                    Title = $"[{m_House.Sign.Name}] ?�매 ?�점 관리자 구인 (?�급 {wage}G)",
                    RegionName = m_House.Region.Name,
                    RewardGold = wage, // We store the daily wage in RewardGold
                    IssuerHouse = null
                };

                // Automatically find nearest town
                var nearestTown = TownEconomyManager.Towns.Values.OrderBy(t => Utility.GetDistanceToSqrt(t.Center, m_House.Location)).FirstOrDefault(t => t.Facet == m_House.Map);
                if (nearestTown != null)
                {
                    req.TownName = nearestTown.TownName;
                    PartTimeManager.ActiveRequests.Add(req);
                    m_From.SendMessage($"{nearestTown.TownName} 마을 게시?�에 구인 공고가 ?�록?�었?�니?? (?�급: {wage}G)");
                }
                else
                {
                    m_From.SendMessage("근처??공고�??�록??마을??찾을 ???�습?�다.");
                }
            }
            else if (info.ButtonID == 2) // Cancel/Fire
            {
                // Find pending requests
                var pending = PartTimeManager.ActiveRequests.FirstOrDefault(r => r.Origin == JobOrigin.PlayerPrivate && r.TargetHouseName == m_House.Sign.Name && !r.IsAIAssigned);
                if (pending != null)
                {
                    PartTimeManager.ActiveRequests.Remove(pending);
                    m_From.SendMessage("?�록??구인 공고�?취소?�습?�다.");
                }
                else
                {
                    // Find employed vendor
                    var vendor = RetailVendor.RetailVendors.FirstOrDefault(v => v.Owner == m_From && v.Map == m_House.Map && m_House.Region.Contains(v.Location));
                    if (vendor != null)
                    {
                        if (vendor.EmployeeID != Guid.Empty)
                        {
                            // Logic to fire employee would go here (e.g. telling VirtualCitizenSystem to clear their job)
                            vendor.EmployeeID = Guid.Empty;
                            m_From.SendMessage("고용???�바?�을 ?�고?�습?�다. ?�점???�시 ?�업 ?�태가 ?�니??");
                        }
                        else
                        {
                            m_From.SendMessage("취소??공고???�고???�바?�이 ?�습?�다.");
                        }
                    }
                }
            }
        }
    }
}

