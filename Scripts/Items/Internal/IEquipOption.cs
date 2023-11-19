using System;

namespace Server.Items
{
    interface IEquipOption : IDurability
    {
        int[] PrefixOption { get; set; }
        int[] SuffixOption { get; set; }
        AosAttributes Attributes { get; }
        AosArmorAttributes ArmorAttributes { get; }
        AosSkillBonuses SkillBonuses { get; }
        AosWeaponAttributes WeaponAttributes { get; }
        SAAbsorptionAttributes AbsorptionAttributes { get; }
        ExtendedWeaponAttributes ExtendedWeaponAttributes { get; }
		bool PlayerConstructed { get; set; }
		CraftResource Resource { get; set; }
		ItemPower ItemPower { get; set; }
		bool Identified { get; set; }
    }
}