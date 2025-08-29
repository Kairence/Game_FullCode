using System;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a patchwork skeletal corpse")]
    public class PatchworkSkeleton : BaseCreature
    {
        [Constructable]
        public PatchworkSkeleton()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Name = "a patchwork skeleton";
            Body = 309;
            BaseSoundID = 0x48D;

            SetStr(2500, 4255);
            SetDex(3000, 4310);
            SetInt(2500, 4255);

            SetHits(8850, 9860);
			SetStam(5520, 6630);
			SetMana(200, 300);
			
			SetAttackSpeed( 20.0 );

            SetDamage(330, 590);

            SetDamageType(ResistanceType.Physical, 85);
            SetDamageType(ResistanceType.Cold, 15);

            SetResistance(ResistanceType.Physical, 75, 85);
            SetResistance(ResistanceType.Fire, 50, 60);
            SetResistance(ResistanceType.Cold, 70, 80);
            SetResistance(ResistanceType.Poison, 100);
            SetResistance(ResistanceType.Energy, 40, 50);

            SetSkill(SkillName.MagicResist, 155.1, 160.0);
            SetSkill(SkillName.Tactics, 155.1, 160.0);
            SetSkill(SkillName.Wrestling, 185.1, 190.0);

            Fame = 15000;
            Karma = -15000;

            VirtualArmor = 75;

            //SetWeaponAbility(WeaponAbility.Dismount);
        }

        public PatchworkSkeleton(Serial serial)
            : base(serial)
        {
        }

        public override bool BleedImmune
        {
            get
            {
                return true;
            }
        }
        public override Poison PoisonImmune
        {
            get
            {
                return Poison.Lethal;
            }
        }
        public override int TreasureMapLevel
        {
            get
            {
                return 1;
            }
        }

        public override void GenerateLoot()
        {
            AddLoot(LootPack.Meager);
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