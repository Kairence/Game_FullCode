using System;
using Server.Items;
using Server.Network;

namespace Server.Mobiles
{
    [CorpseName("a golem corpse")]
    public class Golem : BaseCreature, IRepairableMobile
    {
        [CommandProperty(AccessLevel.GameMaster)]
        public virtual Type RepairResource
        {
            get
            {
                return typeof(IronIngot);
            }
        }

        [Constructable]
        public Golem()
            : this(false, 1)
        {
        }

        [Constructable]
        public Golem(bool summoned, double scalar)
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.4, 0.8)
        {               
            Name = "a golem";
            Body = 752;


            if (summoned)
            {
                Hue = 2101;               

                /* Summoned Golem - Fame 6,000 / Karma 0 (Neutral) */
				/* [HP Calculation]
				   - Target HP: ~15,000
				   - Fame Bonus (6,000): ~13,850
				   - SetHits Required: 1,150 (Target - Bonus)
				*/
				this.SetStr(400, 500);       
				this.SetDex(100, 150);       
				this.SetInt(10, 50);         

				// [Hits] 최종 약 14,000 ~ 16,000 타겟
				this.SetHits(150, 2150); 
				this.SetStam(100, 150);      
				this.SetMana(10, 50);        

				SetAttackSpeed(3.5);
				SetDamage(35, 50);     

				this.SetResistance(ResistanceType.Physical, 60, 75); // Max 75%
				this.SetResistance(ResistanceType.Fire, 20, 30);
				this.SetResistance(ResistanceType.Cold, 40, 50);
				this.SetResistance(ResistanceType.Poison, 75, 75);   // 독 면역 (Max 75%)
				this.SetResistance(ResistanceType.Energy, 30, 40);

				this.SetSkill(SkillName.Wrestling, 90.0, 105.0);
				this.SetSkill(SkillName.Tactics, 90.0, 105.0);
				this.SetSkill(SkillName.MagicResist, 100.0, 100.0);

				this.VirtualArmor = 12;      
				this.ControlSlots = 3;       // 소환수 슬롯 설정

				this.Fame = 100;           
				this.Karma = 100;
            }
            else
            {
				/* Golem - Fame 10,000 / Karma -10,000 */
				/* [HP Calculation]
				   - Target HP: ~35,000
				   - Fame Bonus (10,000): ~24,150
				   - SetHits Required: 10,850 (Target - Bonus)
				*/
				this.SetStr(500, 700);       
				this.SetDex(100, 150);       
				this.SetInt(10, 50);         

				// [Hits] 최종 약 33,000 ~ 37,000 타겟
				this.SetHits(8850, 12850); 
				this.SetStam(100, 150);      
				this.SetMana(10, 50);        

				this.SetAttackSpeed(3.0);    
				this.SetDamage(25, 45);      

				this.SetDamageType(ResistanceType.Physical, 100);

				this.SetResistance(ResistanceType.Physical, 65, 75); // Max 75%
				this.SetResistance(ResistanceType.Fire, 25, 35);
				this.SetResistance(ResistanceType.Cold, 45, 55);
				this.SetResistance(ResistanceType.Poison, 75, 75);   // 독 면역 (Max 75%)
				this.SetResistance(ResistanceType.Energy, 30, 45);

				this.SetSkill(SkillName.Wrestling, 100.0, 115.0);
				this.SetSkill(SkillName.Tactics, 100.0, 115.0);
				this.SetSkill(SkillName.MagicResist, 120.0, 120.0); // 마법 저항 최상

				this.VirtualArmor = 15;      // 금속 몸체이나 타격감을 위해 15 제한
				this.Tamable = false;

				this.Fame = 10000;           
				this.Karma = -10000;
            }

			SetAttackSpeed( 20.0 );

            SetDamageType(ResistanceType.Physical, 100);

            SetResistance(ResistanceType.Physical, 40, 60);
            SetResistance(ResistanceType.Cold, 20, 30);
            SetResistance(ResistanceType.Energy, 30, 45);

            ControlSlots = 3;

            SetSpecialAbility(SpecialAbility.ColossalBlow);
        }

        public Golem(Serial serial)
            : base(serial)
        {
        }

        public override bool IsScaredOfScaryThings { get { return false; } }
        public override bool IsScaryToPets { get { return !Controlled; } }
        public override bool IsBondable { get { return false; } }
        public override FoodType FavoriteFood { get { return FoodType.None; } }
        public override bool CanBeDistracted { get { return false; } }
        public override bool DeleteOnRelease { get { return true; } }
        public override bool AutoDispel { get { return !Controlled; } }
        public override bool BleedImmune { get { return true; } }
        public override bool BardImmune { get { return !Core.AOS || !Controlled; } }
        public override Poison PoisonImmune { get { return Poison.Lethal; } }

        public override void OnDeath(Container c)
        {
            base.OnDeath(c);

            if (0.05 > Utility.RandomDouble() && !Controlled)
            {
                if (!IsParagon)
                {
                    if (0.75 > Utility.RandomDouble())
                        c.DropItem(DawnsMusicGear.RandomCommon);
                    else
                        c.DropItem(DawnsMusicGear.RandomUncommon);
                }
                else
                    c.DropItem(DawnsMusicGear.RandomRare);
            }
        }

        public override int GetAngerSound()
        {
            return 541;
        }

        public override int GetIdleSound()
        {
            if (!Controlled)
                return 542;

            return base.GetIdleSound();
        }

        public override int GetDeathSound()
        {
            if (!Controlled)
                return 545;

            return base.GetDeathSound();
        }

        public override int GetAttackSound()
        {
            return 562;
        }

        public override int GetHurtSound()
        {
            if (Controlled)
                return 320;

            return base.GetHurtSound();
        }

        public override void OnDamage(int amount, Mobile from, bool willKill)
        {
            if (Controlled || Summoned)
            {
                Mobile master = (ControlMaster);

                if (master == null)
                    master = SummonMaster;

                if (master != null && master.Player && master.Map == Map && master.InRange(Location, 20))
                {
                    if (master.Mana >= amount)
                    {
                        master.Mana -= amount;
                    }
                    else
                    {
                        amount -= master.Mana;
                        master.Mana = 0;
                        master.Damage(amount);
                    }
                }
            }

            base.OnDamage(amount, from, willKill);
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write((int)0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
        }
    }
}
