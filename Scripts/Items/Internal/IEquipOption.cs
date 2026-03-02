using System;

namespace Server.Items
{
	interface IEquipOption : IDurability
	{
		int[] PrefixOption { get; set; }
		int[] SuffixOption { get; set; }
		int Hue { get; set; }
		int MaxHitPoints { get; set; }
		int HitPoints { get; set; }
		Mobile Crafter { get; set; }
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
		Map Map { get; set; }
		Point3D Location { get; set; }
	}
}
