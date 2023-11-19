using System;
using Server.Mobiles;
using Server.Targeting;
using System.Collections.Generic;
using Server.Network;
using Server.SkillHandlers;
using Server.Multis;

namespace Server.Items
{
    public class ItemIdentification
    {
        public static void Initialize()
        {
            SkillInfo.Table[(int)SkillName.ItemID].Callback = new SkillUseCallback(OnUse);
        }

        public static TimeSpan OnUse(Mobile from)
        {
			/*
			if( from.Hunger < 100 )
				from.SendMessage("감정을 하기 위해서는 최소 만복도가 1% 이상이어야 합니다.");
			else
			{
				from.Hunger -= 100;
				from.SendLocalizedMessage(500343); // What do you wish to appraise and identify?
				from.Target = new InternalTarget();
			}
			if( from is PlayerMobile )
			{
				PlayerMobile pm = from as PlayerMobile;
				pm.Tired += 10;
			}
			*/
			from.SendLocalizedMessage(500343); // What do you wish to appraise and identify?
			from.Target = new InternalTarget();
            return TimeSpan.FromSeconds(1.0);
        }

        [PlayerVendorTarget]
        private class InternalTarget : Target
        {
            public InternalTarget()
                : base(8, false, TargetFlags.None)
            {
                this.AllowNonlocal = true;
            }

            protected override void OnTarget(Mobile from, object o)
            {
				if( o is Item )
				{
					Item item = o as Item;
					BaseHouse house = BaseHouse.FindHouseAt(from);
					if( item is IEquipOption )
					{
						IEquipOption equip = item as IEquipOption;

						if( !equip.Identified || item is Container )
						{
							if( item.RootParent == from || ( house != null && house.IsOwner(from)) )
							{
								int skillvalue = (int)from.Skills[SkillName.ItemID].Value * 10;
								Misc.Util.ItemIdentified( from, skillvalue, item );
							}
						}
					}
				}				
				/*
				bool fail = false;
				int power = 0;
				double skill = from.Skills[SkillName.ItemID].Value;
				from.SendMessage("아이템을 감정합니다.");
			
                if (item == null && m == null)
                {
                    from.SendLocalizedMessage(500353); // You are not certain...
                    return;
                }
				if( item is IDWand )
				{
					IDWand checkitem = item as IDWand;
					if( checkitem.SaveSkill == 0 && from.Skills.ItemID.Value >= 40 )
					{
						if( from.Skills.ItemID.Value >= 120 )
							checkitem.SaveSkill = 8;
						else if( from.Skills.ItemID.Value >= 105 )
							checkitem.SaveSkill = 7;
						else if( from.Skills.ItemID.Value >= 100 )
							checkitem.SaveSkill = 6;
						else if( from.Skills.ItemID.Value >= 80 )
							checkitem.SaveSkill = 5;
						else if( from.Skills.ItemID.Value >= 40 )
							checkitem.SaveSkill = 4;
						checkitem.Identified = true;
					}
					else
					{
						from.SendMessage("이미 감정이 완료되었거나 감정 스킬이 낮습니다.");
						return;
					}
				}
				else if (item is BaseWeapon ) //|| item is BaseArmor || item is BaseJewel || item is BaseHat)
				{
					BaseWeapon checkitem = item as BaseWeapon;
					if( checkitem.Identified )
					{
						from.SendMessage("이미 감정이 완료된 아이템입니다!");
						return;
					}
					power = (int)checkitem.ItemPower;
					if( OptionCheck(power, skill) )
					{
						if( DiceCheck(power) )
						{
							checkitem.Identified = true;

							int fire, phys, cold, nrgy, pois, chaos, direct;

							fire = checkitem.AosElementDamages.Fire;
							cold = checkitem.AosElementDamages.Cold;
							pois = checkitem.AosElementDamages.Poison;
							nrgy = checkitem.AosElementDamages.Energy;
							chaos = checkitem.AosElementDamages.Chaos;
							direct = checkitem.AosElementDamages.Direct;

							phys = 100 - fire - cold - pois - nrgy - chaos - direct;
							checkitem.GetDamageTypes(null, out phys, out fire, out cold, out pois, out nrgy, out chaos, out direct);
							if( phys != 100 )
								checkitem.Hue = checkitem.GetElementalDamageHue();

							Effects.PlaySound( from.Location, from.Map, 0x243 );
							Effects.SendMovingParticles( new Entity( Serial.Zero, new Point3D( from.X - 6, from.Y - 6, from.Z + 15 ), from.Map ), from, 0x36D4, 7, 0, false, true, 0x497, 0, 9502, 1, 0, (EffectLayer)255, 0x100 );
							Effects.SendMovingParticles( new Entity( Serial.Zero, new Point3D( from.X - 4, from.Y - 6, from.Z + 15 ), from.Map ), from, 0x36D4, 7, 0, false, true, 0x497, 0, 9502, 1, 0, (EffectLayer)255, 0x100 );
							Effects.SendMovingParticles( new Entity( Serial.Zero, new Point3D( from.X - 6, from.Y - 4, from.Z + 15 ), from.Map ), from, 0x36D4, 7, 0, false, true, 0x497, 0, 9502, 1, 0, (EffectLayer)255, 0x100 );
							from.SendMessage("아이템 감정에 성공합니다!");
						}
						else
						{
							from.SendMessage("아이템 감정에 실패합니다...");
							if( checkitem.MaxHitPoints <= 1 )
								checkitem.Delete();
							else
							{
								checkitem.MaxHitPoints--;
								if( checkitem.HitPoints > checkitem.MaxHitPoints )
									checkitem.HitPoints = checkitem.MaxHitPoints;
							}
						}
					}
					else
					{
						from.SendMessage("당신은 이 무기를 감정하기에는 스킬이 부족합니다.");
						fail = true;
						if( checkitem.MaxHitPoints <= 1 )
							checkitem.Delete();
						else
						{
							checkitem.MaxHitPoints--;
							if( checkitem.HitPoints > checkitem.MaxHitPoints )
								checkitem.HitPoints = checkitem.MaxHitPoints;
						}
					}
				}
				else if (item is BaseArmor ) //|| item is BaseArmor || item is BaseJewel || item is BaseHat)
				{
					BaseArmor checkitem = item as BaseArmor;
					if( checkitem.Identified )
					{
						from.SendMessage("이미 감정이 완료된 아이템입니다!");
						return;
					}
					power = (int)checkitem.ItemPower;
					if( OptionCheck(power, skill) )
					{
						if( DiceCheck(power) )
						{
							checkitem.Identified = true;

							Effects.PlaySound( from.Location, from.Map, 0x243 );
							Effects.SendMovingParticles( new Entity( Serial.Zero, new Point3D( from.X - 6, from.Y - 6, from.Z + 15 ), from.Map ), from, 0x36D4, 7, 0, false, true, 0x497, 0, 9502, 1, 0, (EffectLayer)255, 0x100 );
							Effects.SendMovingParticles( new Entity( Serial.Zero, new Point3D( from.X - 4, from.Y - 6, from.Z + 15 ), from.Map ), from, 0x36D4, 7, 0, false, true, 0x497, 0, 9502, 1, 0, (EffectLayer)255, 0x100 );
							Effects.SendMovingParticles( new Entity( Serial.Zero, new Point3D( from.X - 6, from.Y - 4, from.Z + 15 ), from.Map ), from, 0x36D4, 7, 0, false, true, 0x497, 0, 9502, 1, 0, (EffectLayer)255, 0x100 );
							from.SendMessage("아이템 감정에 성공합니다!");
						}
						else
						{
							from.SendMessage("아이템 감정에 실패합니다...");
							if( checkitem.MaxHitPoints <= 1 )
								checkitem.Delete();
							else
							{
								checkitem.MaxHitPoints--;
								if( checkitem.HitPoints > checkitem.MaxHitPoints )
									checkitem.HitPoints = checkitem.MaxHitPoints;

							}
						}
					}
					else
					{
						from.SendMessage("당신은 이 무기를 감정하기에는 스킬이 부족합니다.");
						fail = true;
						if( checkitem.MaxHitPoints <= 1 )
							checkitem.Delete();
						else
						{
							checkitem.MaxHitPoints--;
							if( checkitem.HitPoints > checkitem.MaxHitPoints )
								checkitem.HitPoints = checkitem.MaxHitPoints;
						}
					}
				}
				else if (item is BaseJewel ) //|| item is BaseArmor || item is BaseJewel || item is BaseHat)
				{
					BaseJewel checkitem = item as BaseJewel;
					if( checkitem.Identified )
					{
						from.SendMessage("이미 감정이 완료된 아이템입니다!");
						return;
					}
					power = (int)checkitem.ItemPower;
					if( OptionCheck(power, skill) )
					{
						if( DiceCheck(power) )
						{
							checkitem.Identified = true;

							Effects.PlaySound( from.Location, from.Map, 0x243 );
							Effects.SendMovingParticles( new Entity( Serial.Zero, new Point3D( from.X - 6, from.Y - 6, from.Z + 15 ), from.Map ), from, 0x36D4, 7, 0, false, true, 0x497, 0, 9502, 1, 0, (EffectLayer)255, 0x100 );
							Effects.SendMovingParticles( new Entity( Serial.Zero, new Point3D( from.X - 4, from.Y - 6, from.Z + 15 ), from.Map ), from, 0x36D4, 7, 0, false, true, 0x497, 0, 9502, 1, 0, (EffectLayer)255, 0x100 );
							Effects.SendMovingParticles( new Entity( Serial.Zero, new Point3D( from.X - 6, from.Y - 4, from.Z + 15 ), from.Map ), from, 0x36D4, 7, 0, false, true, 0x497, 0, 9502, 1, 0, (EffectLayer)255, 0x100 );
							from.SendMessage("아이템 감정에 성공합니다!");
						}
						else
						{
							from.SendMessage("아이템 감정에 실패합니다...");
							if( checkitem.MaxHitPoints <= 1 )
								checkitem.Delete();
							else
							{
								checkitem.MaxHitPoints--;
								if( checkitem.HitPoints > checkitem.MaxHitPoints )
									checkitem.HitPoints = checkitem.MaxHitPoints;

							}
						}
					}
					else
					{
						from.SendMessage("당신은 이 무기를 감정하기에는 스킬이 부족합니다.");
						fail = true;
						if( checkitem.MaxHitPoints <= 1 )
							checkitem.Delete();
						else
						{
							checkitem.MaxHitPoints--;
							if( checkitem.HitPoints > checkitem.MaxHitPoints )
								checkitem.HitPoints = checkitem.MaxHitPoints;
						}
					}
				}
				else if (item is BaseClothing ) //|| item is BaseArmor || item is BaseJewel || item is BaseHat)
				{
					BaseClothing checkitem = item as BaseClothing;

					if( checkitem.Identified )
					{
						from.SendMessage("이미 감정이 완료된 아이템입니다!");
						return;
					}
					power = (int)checkitem.ItemPower;
					if( OptionCheck(power, skill) )
					{
						if( DiceCheck(power) )
						{
							checkitem.Identified = true;
							
							Effects.PlaySound( from.Location, from.Map, 0x243 );
							Effects.SendMovingParticles( new Entity( Serial.Zero, new Point3D( from.X - 6, from.Y - 6, from.Z + 15 ), from.Map ), from, 0x36D4, 7, 0, false, true, 0x497, 0, 9502, 1, 0, (EffectLayer)255, 0x100 );
							Effects.SendMovingParticles( new Entity( Serial.Zero, new Point3D( from.X - 4, from.Y - 6, from.Z + 15 ), from.Map ), from, 0x36D4, 7, 0, false, true, 0x497, 0, 9502, 1, 0, (EffectLayer)255, 0x100 );
							Effects.SendMovingParticles( new Entity( Serial.Zero, new Point3D( from.X - 6, from.Y - 4, from.Z + 15 ), from.Map ), from, 0x36D4, 7, 0, false, true, 0x497, 0, 9502, 1, 0, (EffectLayer)255, 0x100 );
							from.SendMessage("아이템 감정에 성공합니다!");
						}
						else
						{
							from.SendMessage("아이템 감정에 실패합니다...");
							if( checkitem.MaxHitPoints <= 1 )
								checkitem.Delete();
							else
							{
								checkitem.MaxHitPoints--;
								if( checkitem.HitPoints > checkitem.MaxHitPoints )
									checkitem.HitPoints = checkitem.MaxHitPoints;

							}
						}
					}
					else
					{
						from.SendMessage("당신은 이 무기를 감정하기에는 스킬이 부족합니다.");
						fail = true;
						if( checkitem.MaxHitPoints <= 1 )
							checkitem.Delete();
						else
						{
							checkitem.MaxHitPoints--;
							if( checkitem.HitPoints > checkitem.MaxHitPoints )
								checkitem.HitPoints = checkitem.MaxHitPoints;
						}
					}
				}
				else if (item is Spellbook ) //|| item is BaseArmor || item is BaseJewel || item is BaseHat)
				{
					Spellbook checkitem = item as Spellbook;
					if( checkitem.Identified )
					{
						from.SendMessage("이미 감정이 완료된 아이템입니다!");
						return;
					}
					power = (int)checkitem.ItemPower;
					if( OptionCheck(power, skill) )
					{
						if( DiceCheck(power) )
						{
							checkitem.Identified = true;

							Effects.PlaySound( from.Location, from.Map, 0x243 );
							Effects.SendMovingParticles( new Entity( Serial.Zero, new Point3D( from.X - 6, from.Y - 6, from.Z + 15 ), from.Map ), from, 0x36D4, 7, 0, false, true, 0x497, 0, 9502, 1, 0, (EffectLayer)255, 0x100 );
							Effects.SendMovingParticles( new Entity( Serial.Zero, new Point3D( from.X - 4, from.Y - 6, from.Z + 15 ), from.Map ), from, 0x36D4, 7, 0, false, true, 0x497, 0, 9502, 1, 0, (EffectLayer)255, 0x100 );
							Effects.SendMovingParticles( new Entity( Serial.Zero, new Point3D( from.X - 6, from.Y - 4, from.Z + 15 ), from.Map ), from, 0x36D4, 7, 0, false, true, 0x497, 0, 9502, 1, 0, (EffectLayer)255, 0x100 );
							from.SendMessage("아이템 감정에 성공합니다!");
						}
						else
						{
							from.SendMessage("아이템 감정에 실패합니다...");
							if( checkitem.MaxHitPoints <= 1 )
								checkitem.Delete();
							else
							{
								checkitem.MaxHitPoints--;
								if( checkitem.HitPoints > checkitem.MaxHitPoints )
									checkitem.HitPoints = checkitem.MaxHitPoints;

							}
						}
					}
					else
					{
						from.SendMessage("당신은 이 무기를 감정하기에는 스킬이 부족합니다.");
						fail = true;
						if( checkitem.MaxHitPoints <= 1 )
							checkitem.Delete();
						else
						{
							checkitem.MaxHitPoints--;
							if( checkitem.HitPoints > checkitem.MaxHitPoints )
								checkitem.HitPoints = checkitem.MaxHitPoints;
						}
					}
				}
                else
                {
					power = -1;
                    from.SendLocalizedMessage(500353); // You are not certain...
                }

				int point = 100 + power * 20;
				
				if( power > -1 )
					from.CheckSkill( SkillName.ItemID, 100 + point * 20 );	

				*/
                Server.Engines.XmlSpawner2.XmlAttach.RevealAttachments(from, o);
            }

			/*
            public static int GetPriceFor(Item item)
            {
                Type type = item.GetType();

                if (GenericBuyInfo.BuyPrices.ContainsKey(type))
                {
                    return GenericBuyInfo.BuyPrices[item.GetType()] * item.Amount;
                }

                if (TypeCostCache == null)
                    TypeCostCache = new Dictionary<Type, int>();

                if (!TypeCostCache.ContainsKey(type))
                    TypeCostCache[type] = Utility.RandomMinMax(2, 7);

                return TypeCostCache[type];
            }

            public static Dictionary<Type, int> TypeCostCache { get; set; }
			*/
        }
    }
}
