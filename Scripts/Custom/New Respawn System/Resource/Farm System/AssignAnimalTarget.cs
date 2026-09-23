using System;
using Server.Targeting;
using Server.Mobiles;
using Server.Items;

namespace Server.Misc
{
    public class AssignAnimalTarget : Target
    {
        private readonly PrivateFarmAddon m_Addon;
        private readonly int m_Type;

        public AssignAnimalTarget(PrivateFarmAddon addon, int type) : base(10, false, TargetFlags.None)
        {
            m_Addon = addon;
            m_Type = type;
        }

        protected override void OnTarget(Mobile from, object targeted)
        {
            if (targeted is BaseCreature bc)
            {
                // 1. 소유권 및 상태 체크
                if (!bc.Controlled || bc.ControlMaster != from)
                {
                    from.SendMessage(33, "당신이 길들인 동물만 등록할 수 있습니다.");
                    return;
                }

                if (bc.IsDeadPet)
                {
                    from.SendMessage(33, "죽은 동물은 등록할 수 없습니다.");
                    return;
                }

                // 2. 가축 제한 체크 (기존 FarmingSystem.CanPlant 로직 등과 연계 가능)
                // 현재 에드온의 GetLivestockCount()를 활용해 제한을 걸 수 있습니다.
                int limit = 5 + (int)(from.Skills[SkillName.Herding].Value / 20); 
                if (m_Addon.Animals.Count >= limit)
                {
                    from.SendMessage(33, $"이 농장에는 최대 {limit}마리까지만 등록할 수 있습니다.");
                    return;
                }

                // 3. 🌟 가축별 농사(Herding) 스킬 체크 및 실패 페널티
                double minSkill = 0;
                double maxSkill = 50.0;

                if (bc is Chicken) { minSkill = 30.0; maxSkill = 80.0; }
                else if (bc is Cow) { minSkill = 50.0; maxSkill = 100.0; }
                else if (bc is Pig) { minSkill = 80.0; maxSkill = 130.0; }
                else if (bc is Sheep) { minSkill = 115.0; maxSkill = 165.0; }
                else if (bc is PackHorse || bc is PackLlama) { minSkill = 140.0; maxSkill = 190.0; }
                else
                {
                    from.SendMessage(33, "이 동물은 농장에 등록할 수 없는 가축입니다.");
                    return;
                }

                if (!from.CheckSkill(SkillName.Herding, minSkill, maxSkill))
                {
                    from.SendMessage(33, "가축을 축사에 적응시키는 데 실패하여 동물이 도망(증발)갔습니다.");
                    bc.Delete();
                    return;
                }

                // 4. [핵심] 유저님이 작성하신 메서드 호출
                m_Addon.AssignAnimal(from, bc);
            }
            else
            {
                from.SendMessage(33, "동물을 선택해야 합니다.");
            }
        }
    }
}