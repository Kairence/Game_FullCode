using System;
using Server;

namespace Server.Misc
{
    public class BioStats
    {
        public const int Unit = 10000; // 1% = 10,000
        public const int MaxValue = 100 * Unit;
        public const int MinValue = -100 * Unit;
        public const int DecayAmount = 1000; // 0.1% 하락

        public int Weight { get; set; }
        public int Metabolism { get; set; }
        public int Focus { get; set; }
        public int Perception { get; set; }
        public int Adaptability { get; set; }

        public BioStats() { }

        // 만복도 50% 미만 시 호출될 감쇄 로직
        public void ApplyDecay()
        {
            Weight = Math.Max(MinValue, Weight - DecayAmount);
            Metabolism = Math.Max(MinValue, Metabolism - DecayAmount);
            Focus = Math.Max(MinValue, Focus - DecayAmount);
            Perception = Math.Max(MinValue, Perception - DecayAmount);
            Adaptability = Math.Max(MinValue, Adaptability - DecayAmount);
        }

        // 만복도 0% 지속 시 기아 상태(체중만 대폭 감소)
        public void ApplyStarvation()
        {
            Weight = Math.Max(MinValue, Weight - (DecayAmount * 5)); 
        }

		// [신규 추가] 던전 및 전투 상태 시 정신/생존 스탯 급속 소모
        public void ApplyEnvironmentalStress(int stressAmount)
        {
            // 집중, 감각, 적응력만 타격을 받음 (체중과 대사는 물리적 영역이므로 제외)
            Focus = Math.Max(MinValue, Focus - stressAmount);
            Perception = Math.Max(MinValue, Perception - stressAmount);
            Adaptability = Math.Max(MinValue, Adaptability - stressAmount);
        }
		
        // 음식 섭취 시 스탯 증감 (튜플 사용)
        public void AddStats((int w, int m, int f, int p, int a) stats)
        {
            Weight = Math.Clamp(Weight + stats.w, MinValue, MaxValue);
            Metabolism = Math.Clamp(Metabolism + stats.m, MinValue, MaxValue);
            Focus = Math.Clamp(Focus + stats.f, MinValue, MaxValue);
            Perception = Math.Clamp(Perception + stats.p, MinValue, MaxValue);
            Adaptability = Math.Clamp(Adaptability + stats.a, MinValue, MaxValue);
        }

        // 과식 시 체중 보정 로직
        public void ApplyOvereat(int overeatHungerAmount)
        {
            // 예: 초과한 Hunger 1당 Weight 1 증가 (10,000 초과 시 1% 증가)
            Weight = Math.Clamp(Weight + overeatHungerAmount, MinValue, MaxValue);
        }

        public void Serialize(GenericWriter writer)
        {
            writer.Write(0); // version
            writer.Write(Weight);
            writer.Write(Metabolism);
            writer.Write(Focus);
            writer.Write(Perception);
            writer.Write(Adaptability);
        }

        public BioStats(GenericReader reader)
        {
            int version = reader.ReadInt();
            Weight = reader.ReadInt();
            Metabolism = reader.ReadInt();
            Focus = reader.ReadInt();
            Perception = reader.ReadInt();
            Adaptability = reader.ReadInt();
        }
    }
}