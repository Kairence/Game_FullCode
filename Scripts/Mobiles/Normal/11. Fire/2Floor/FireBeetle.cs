using System;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a fire beetle corpse")]
    [Server.Engines.Craft.Forge]
    public class FireBeetle : BaseMount
    {
        [Constructable]
        public FireBeetle()
            : base("a fire beetle", 0xA9, 0x3E95, AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            /* Fire Beetle - Fame 7,000 / Karma -7,000 */
			/* [HP Calculation]
			   - Target HP: ~18,000
			   - Fame Bonus (7,000): ~16,400
			   - SetHits Required: 1,600 (Target - Bonus)
			*/
			this.SetStr(350, 500);       
			this.SetDex(120, 180);       

			// [Hits] 최종 약 17,000 ~ 19,000 타겟
			this.SetHits(600, 2600); 
			this.SetStam(120, 180);      

			SetAttackSpeed(2.5);
			SetDamage(35, 50);     

			this.SetDamageType(ResistanceType.Fire, 100);

			this.SetResistance(ResistanceType.Physical, 45, 60);
			this.SetResistance(ResistanceType.Fire, 75, 75);     
			this.SetResistance(ResistanceType.Cold, 0, 15);      
			this.SetResistance(ResistanceType.Poison, 40, 55);

			this.SetSkill(SkillName.Wrestling, 95.0, 110.0);
			this.SetSkill(SkillName.Tactics, 95.0, 110.0);
			this.SetSkill(SkillName.MagicResist, 90.0, 105.0);

			this.VirtualArmor = 15;      // 단단한 껍질 표현

			// [Taming Settings]
			this.Tamable = true;         
			this.ControlSlots = 3;       
			this.MinTameSkill = 115.0;   // 상한 200 서버 기준 중급 숙련도 (제련 기능 고려)

			this.Fame = 7000;           
			this.Karma = -7000;

            PackItem(new SulfurousAsh(Utility.RandomMinMax(16, 25)));
            PackItem(new IronIngot(2));

            Hue = 0x489;
        }

        public FireBeetle(Serial serial)
            : base(serial)
        {
        }

        public override bool SubdueBeforeTame
        {
            get
            {
                return true;
            }
        }// Must be beaten into submission
        public override bool StatLossAfterTame
        {
            get
            {
                return true;
            }
        }
        public virtual double BoostedSpeed
        {
            get
            {
                return 0.1;
            }
        }
        public override bool ReduceSpeedWithDamage
        {
            get
            {
                return false;
            }
        }
        public override int Meat
        {
            get
            {
                return 16;
            }
        }
        public override FoodType FavoriteFood
        {
            get
            {
                return FoodType.Meat;
            }
        }
        public override void OnHarmfulSpell(Mobile from)
        {
            if (!Controlled && ControlMaster == null)
                CurrentSpeed = BoostedSpeed;
        }

        public override void OnCombatantChange()
        {
            if (Combatant == null && !Controlled && ControlMaster == null)
                CurrentSpeed = PassiveSpeed;
        }

        public override bool OverrideBondingReqs()
        {
            return true;
        }

        public override int GetAngerSound()
        {
            return 0x21D;
        }

        public override int GetIdleSound()
        {
            return 0x21D;
        }

        public override int GetAttackSound()
        {
            return 0x162;
        }

        public override int GetHurtSound()
        {
            return 0x163;
        }

        public override int GetDeathSound()
        {
            return 0x21D;
        }

        public override double GetControlChance(Mobile m, bool useBaseSkill)
        {
            if (PetTrainingHelper.Enabled)
            {
                var profile = PetTrainingHelper.GetAbilityProfile(this);

                if (profile != null && profile.HasCustomized())
                {
                    return base.GetControlChance(m, useBaseSkill);
                }
            }

            return 1.0;
        }

        public override void OnAfterTame(Mobile tamer)
        {
            base.OnAfterTame(tamer);

            if (Owners.Count == 0 && PetTrainingHelper.Enabled)
            {
                SetInt(500);
            }
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)3); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            if (version < 2 && Controlled && RawStr >= 300 && ControlSlots == ControlSlotsMin)
            {
                //Server.SkillHandlers.AnimalTaming.ScaleStats(this, 0.5);
            }

            if (PetTrainingHelper.Enabled && version == 2)
            {
                if (version < 1 && PetTrainingHelper.Enabled && ControlSlots <= 3)
                {
                    var profile = PetTrainingHelper.GetAbilityProfile(this);

                    if (profile == null || !profile.HasCustomized())
                    {
                        MinTameSkill = 98.7;
                        ControlSlotsMin = 1;
                        ControlSlots = 1;
                    }

                    if ((ControlMaster != null || IsStabled) && Int < 500)
                    {
                        SetInt(500);
                    }
                }
            }

            if (version == 0)
                Hue = 0x489;
        }
    }
}
