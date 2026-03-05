using System;
using System.Collections.Generic;
using Server.ContextMenus;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a giant beetle corpse")]
    public class Beetle : BaseMount
    {
        public virtual double BoostedSpeed
        {
            get
            {
                return 0.1;
            }
        }

        [Constructable]
        public Beetle()
            : this("a giant beetle")
        {
        }

        public override bool SubdueBeforeTame
        {
            get
            {
                return true;
            }
        }// Must be beaten into submission
        public override bool ReduceSpeedWithDamage
        {
            get
            {
                return false;
            }
        }

        [Constructable]
        public Beetle(string name)
            : base(name, 0x317, 0x3EBC, AIType.AI_Melee, FightMode.Closest, 10, 1, 0.25, 0.5) //791, 16060
        {
			/* [Beetle - Fame 4,000 / General / Weight 1.14]
			   - 스킬 200 마스터 서버용 '중하급' 밸런스 적용
			   - 가상 방어력(VirtualArmor): (4,000/1000) + 2 = 6 (컨셉 보정 +2)
			   - 저항 밸런스: 최대 75 상한 엄격 준수
			   -------------------------------------------------- */

			// [Attributes] 명성 4,000 보너스 + 가중치 1.14 반영
			this.SetStr(40, 55); 
			this.SetHits(1000, 1150); 
			this.SetDex(8, 12);
			this.SetInt(8, 12);

			// [Combat Options]
			this.SetDamage(12, 18);
			this.SetAttackSpeed(2.0);

			// [Damage Types] 100% 물리 공격
			this.SetDamageType(ResistanceType.Physical, 100);

			// [Resistances] 총합 약 190 (Max 75 준수)
			this.SetResistance(ResistanceType.Physical, 55, 65);
			this.SetResistance(ResistanceType.Fire, 20, 30);
			this.SetResistance(ResistanceType.Cold, 30, 40);
			this.SetResistance(ResistanceType.Poison, 40, 50);
			this.SetResistance(ResistanceType.Energy, 25, 35);

			// [Skills] ★ 스킬 200 서버 기준 - 중급자 진입용 (재설계)
			// 유저 스킬 50 ~ 70 구간 수련 및 테이밍 타겟
			this.SetSkill(SkillName.Wrestling, 45.0, 55.0); 
			this.SetSkill(SkillName.Tactics, 45.0, 55.0);
			this.SetSkill(SkillName.Anatomy, 40.0, 50.0);
			this.SetSkill(SkillName.MagicResist, 35.0, 45.0);

			// [Taming Code] 스킬 200 서버용 테이밍 난이도
			this.Tamable = true;
			this.ControlSlots = 1;
			this.MinTameSkill = 85.0; // 서버 특성상 테이밍은 숙련 필요

			// [Misc] 가상 방어력(Virtual Armor): (4,000/1000) + 2 = 6
			this.VirtualArmor = 6;

			this.Fame = 4000;
			this.Karma = -4000;
            Container pack = Backpack;

            if (pack != null)
                pack.Delete();

            pack = new StrongBackpack();
            pack.Movable = false;

            AddItem(pack);
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

        public override FoodType FavoriteFood
        {
            get
            {
                return FoodType.Meat;
            }
        }

        public override bool CanAutoStable { get { return (Backpack == null || Backpack.Items.Count == 0) && base.CanAutoStable; } }

        public Beetle(Serial serial)
            : base(serial)
        {
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

        #region Pack Animal Methods
        public override bool OnBeforeDeath()
        {
            if (!base.OnBeforeDeath())
                return false;

            PackAnimal.CombineBackpacks(this);

            return true;
        }

        public override DeathMoveResult GetInventoryMoveResultFor(Item item)
        {
            return DeathMoveResult.MoveToCorpse;
        }

        public override bool IsSnoop(Mobile from)
        {
            if (PackAnimal.CheckAccess(this, from))
                return false;

            return base.IsSnoop(from);
        }

        public override bool OnDragDrop(Mobile from, Item item)
        {
            if (CheckFeed(from, item))
                return true;

            if (PackAnimal.CheckAccess(this, from))
            {
                AddToBackpack(item);
                return true;
            }

            return base.OnDragDrop(from, item);
        }

        public override bool CheckNonlocalDrop(Mobile from, Item item, Item target)
        {
            return PackAnimal.CheckAccess(this, from);
        }

        public override bool CheckNonlocalLift(Mobile from, Item item)
        {
            return PackAnimal.CheckAccess(this, from);
        }

        public override void GetContextMenuEntries(Mobile from, List<ContextMenuEntry> list)
        {
            base.GetContextMenuEntries(from, list);

            PackAnimal.GetContextMenuEntries(this, from, list);
        }

        #endregion

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

            writer.Write((int)1); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

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
    }
}
