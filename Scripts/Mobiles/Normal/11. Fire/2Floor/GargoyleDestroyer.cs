using System;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a gargoyle corpse")]
    public class GargoyleDestroyer : BaseCreature
    {
        [Constructable]
        public GargoyleDestroyer()
            : base(AIType.AI_Mage, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            this.Name = "Gargoyle Destroyer";
            this.Body = 0x2F3;
            this.BaseSoundID = 0x174;

            /* Gargoyle Destroyer - Fame 16,500 / Karma -16,500 */
			/* [HP Calculation]
			   - Target HP: ~75,000
			   - Fame Bonus (16,500): ~42,150
			   - SetHits Required: 32,850 (Target - Bonus)
			*/
			this.SetStr(800, 1000);      
			this.SetDex(220, 320);       // 매우 빠른 공격 속도
			this.SetInt(200, 300);       

			// [Hits] 최종 약 72,000 ~ 78,000 타겟
			this.SetHits(29850, 35850); 
			this.SetStam(220, 320);      

			SetAttackSpeed(10.0);
			SetDamage(20, 30);      

			this.SetDamageType(ResistanceType.Physical, 60);
			this.SetDamageType(ResistanceType.Fire, 40);

			this.SetResistance(ResistanceType.Physical, 65, 75); // Max 75%
			this.SetResistance(ResistanceType.Fire, 75, 75);     
			this.SetResistance(ResistanceType.Cold, 15, 30);     
			this.SetResistance(ResistanceType.Poison, 50, 65);

			this.SetSkill(SkillName.Swords, 120.0, 140.0);      // 검술의 달인
			this.SetSkill(SkillName.Tactics, 120.0, 140.0);
			this.SetSkill(SkillName.Anatomy, 120.0, 140.0);

			this.VirtualArmor = 12;      
			this.Tamable = false;

			this.Fame = 16500;           
			this.Karma = -16500;
        }

        public GargoyleDestroyer(Serial serial)
            : base(serial)
        {
        }

        public override bool BardImmune
        {
            get
            {
                return !Core.AOS;
            }
        }
        public override int Meat
        {
            get
            {
                return 1;
            }
        }
        public override bool CanFly
        {
            get
            {
                return true;
            }
        }
        public override void GenerateLoot()
        {
            this.AddLoot(LootPack.FilthyRich);
            this.AddLoot(LootPack.Rich);
            this.AddLoot(LootPack.MedScrolls);
            this.AddLoot(LootPack.Gems, 2);
        }

        public override void OnDamagedBySpell(Mobile from)
        {
            if (from != null && from.Alive && 0.4 > Utility.RandomDouble())
            {
                this.ThrowHatchet(from);
            }
        }

        public override void OnGotMeleeAttack(Mobile attacker)
        {
            base.OnGotMeleeAttack(attacker);

            if (attacker != null && attacker.Alive && attacker.Weapon is BaseRanged && 0.4 > Utility.RandomDouble())
            {
                this.ThrowHatchet(attacker);
            }
        }

        public void ThrowHatchet(Mobile to)
        {
            int damage = 50;
            this.MovingEffect(to, 0xF43, 10, 0, false, false);
            this.DoHarmful(to);
            AOS.Damage(to, this, damage, 100, 0, 0, 0, 0);
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