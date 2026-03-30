using System;
using Server;

namespace Server.Items
{
    interface IEquipOption : IDurability
    {
        // --- [오직 이 두 배열이 모든 걸 통제합니다] ---
        int[] PrefixOption { get; set; }
        int[] SuffixOption { get; set; }

        // --- [기본 속성들] ---
        int Hue { get; set; }
        int MaxHitPoints { get; set; }
        int HitPoints { get; set; }
        Mobile Crafter { get; set; }
        bool PlayerConstructed { get; set; }
        CraftResource Resource { get; set; }
        ItemPower ItemPower { get; set; }
        bool Identified { get; set; }
        Map Map { get; set; }
        Point3D Location { get; set; }	
    }
}