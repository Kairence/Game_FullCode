using System;
using Server.Network;
using Server.Mobiles;

namespace Server.Items
{
    public class BaseShield : BaseArmor
    {
        public BaseShield(int itemID)
            : base(itemID)
        {
        }

        public BaseShield(Serial serial)
            : base(serial)
        {
        }

        public override ArmorMaterialType MaterialType
        {
            get
            {
                return ArmorMaterialType.Plate;
            }
        }

        public override double ArmorRating
        {
            get
            {
                Mobile m = Parent as Mobile;
                double ar = base.ArmorRating;

                if (m != null)
                    return ((m.Skills[SkillName.Parry].Value * ar) / 200.0) + 1.0;
                else
                    return ar;
            }
        }

        public int LastParryChance { get; set; }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)1);//version
			if( this.BaseArmorRating != 0 )
				BaseArmorRating = 0;
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            if (version < 1)
            {
                if (this is Aegis)
                    return;

                // The 15 bonus points to resistances are not applied to shields on OSI.
                PhysicalBonus = 0;
                FireBonus = 0;
                ColdBonus = 0;
                PoisonBonus = 0;
                EnergyBonus = 0;
            }
        }

        public override void AddNameProperties(ObjectPropertyList list)
        {
            base.AddNameProperties(list);

            if (Core.EJ && LastParryChance > 0)
            {
                list.Add(1158861, LastParryChance.ToString()); // Last Parry Chance: ~1_val~%
            }
        }

        public override void OnRemoved(object parent)
        {
            LastParryChance = 0;

            base.OnRemoved(parent);
        }

        public override int OnHit(BaseWeapon weapon, int damage)
        {
			HiddenRank += damage;
			bool destroy = false;
			int breaken = 1;
			if( HiddenRank >= 1000 )
			{
				destroy = true;
				breaken = HiddenRank / 1000;
				HiddenRank -= 1000 * breaken;
			}
            if ( destroy ) // 25% chance to lower durability
			{
				if (MaxHitPoints > 0 + breaken)
				{
					if (HitPoints >= 1 + breaken)
						HitPoints -= 1 + breaken;
					else if ( MaxHitPoints > 0 + breaken)
					{
						MaxHitPoints -= 1 + breaken;
						
						if (Parent is Mobile)
							((Mobile)Parent).LocalOverheadMessage(MessageType.Regular, 0x3B2, 1061121); // Your equipment is severely damaged.
						if (MaxHitPoints <= 0 + breaken )
							Delete();
					}
				}
				if( Parent is PlayerMobile )
				{
					PlayerMobile pm = Parent as PlayerMobile;
					//Misc.Util.EquipPoint( pm, this );
				}
			}
            return damage;
        }

        public override int GetLuckBonus()
        {
            if (CraftResources.GetType(Resource) != CraftResourceType.Wood)
            {
                return base.GetLuckBonus();
            }
            else
            {
                CraftAttributeInfo attrInfo = GetResourceAttrs(Resource);

                if (attrInfo == null)
                    return 0;

                return attrInfo.ShieldLuck;
            }
        }

        public override void DistributeExceptionalBonuses(Mobile from, int amount)
        {
        }

        public override void DistributeMaterialBonus(CraftAttributeInfo attrInfo)
        {
            if (CraftResources.GetType(Resource) != CraftResourceType.Wood)
            {
                base.DistributeMaterialBonus(attrInfo);
            }
            else
            {
                if (Resource != CraftResource.Heartwood)
                {
                    Attributes.SpellChanneling += attrInfo.ShieldSpellChanneling;
                    ArmorAttributes.LowerStatReq += attrInfo.ShieldLowerRequirements;
                    Attributes.RegenHits += attrInfo.ShieldRegenHits;
                }
                else
                {
                    switch (Utility.Random(7))
                    {
                        case 0: Attributes.BonusDex += attrInfo.ShieldBonusDex; break;
                        case 1: Attributes.BonusStr += attrInfo.ShieldBonusStr; break;
                        case 2: PhysicalBonus += attrInfo.ShieldPhysicalRandom; break;
                        case 3: Attributes.ReflectPhysical += attrInfo.ShieldReflectPhys; break;
                        case 4: ArmorAttributes.SelfRepair += attrInfo.ShieldSelfRepair; break;
                        case 5: ColdBonus += attrInfo.ShieldColdRandom; break;
                        case 6: Attributes.SpellChanneling += attrInfo.ShieldSpellChanneling; break;
                    }
                }
            }
        }

        protected override void ApplyResourceResistances(CraftResource oldResource)
        {
			return;
            if (CraftResources.GetType(Resource) != CraftResourceType.Wood)
            {
                base.ApplyResourceResistances(oldResource);
            }
            else
            {
                CraftAttributeInfo info;

                if (oldResource > CraftResource.None)
                {
                    info = GetResourceAttrs(oldResource);
                    // Remove old bonus

                    PhysicalBonus = Math.Max(0, PhysicalBonus - info.ShieldPhysicalResist);
                    FireBonus = Math.Max(0, FireBonus - info.ShieldFireResist);
                    ColdBonus = Math.Max(0, ColdBonus - info.ShieldColdResist);
                    PoisonBonus = Math.Max(0, PoisonBonus - info.ShieldPoisonResist);
                    EnergyBonus = Math.Max(0, EnergyBonus - info.ShieldEnergyResist);

                    PhysNonImbuing = Math.Max(0, PhysNonImbuing - info.ShieldPhysicalResist);
                    FireNonImbuing = Math.Max(0, FireNonImbuing - info.ShieldFireResist);
                    ColdNonImbuing = Math.Max(0, ColdNonImbuing - info.ShieldColdResist);
                    PoisonNonImbuing = Math.Max(0, PoisonNonImbuing - info.ShieldPoisonResist);
                    EnergyNonImbuing = Math.Max(0, EnergyNonImbuing - info.ShieldEnergyResist);
                }

                info = GetResourceAttrs(Resource);

                // add new bonus
                PhysicalBonus += info.ShieldPhysicalResist;
                FireBonus += info.ShieldFireResist;
                ColdBonus += info.ShieldColdResist;
                PoisonBonus += info.ShieldPoisonResist;
                EnergyBonus += info.ShieldEnergyResist;

                PhysNonImbuing += info.ShieldPhysicalResist;
                FireNonImbuing += info.ShieldFireResist;
                ColdNonImbuing += info.ShieldColdResist;
                PoisonNonImbuing += info.ShieldPoisonResist;
                EnergyNonImbuing += info.ShieldEnergyResist;
            }
        }
    }
}
