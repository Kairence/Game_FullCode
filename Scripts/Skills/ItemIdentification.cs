using System; // TimeSpan을 위해 필수!
using Server.Items;
using Server.Mobiles;
using Server.Targeting;

namespace Server.Items;

public class ItemIdentification
{
    private static readonly string[] GradeNames = ["일반", "희귀", "영웅", "서사", "전설", "신화"];
    private static readonly int[] GradeSkillTable = [0, 0, 50, 100, 150, 200];

    public static void Initialize() 
        => SkillInfo.Table[(int)SkillName.ItemID].Callback = OnUse;

    public static TimeSpan OnUse(Mobile from)
    {
        if (!from.BeginAction(typeof(ItemIdentification)))
        {
            from.SendMessage(0x22, "이미 아이템을 분석 중입니다.");
            return TimeSpan.FromSeconds(1.0);
        }

        from.SendLocalizedMessage(500343);
        from.Target = new InternalTarget();
        return TimeSpan.FromSeconds(1.0);
    }

    private class InternalTarget : Target
    {
        public InternalTarget() : base(8, false, TargetFlags.None) => AllowNonlocal = true;

        protected override void OnTargetCancel(Mobile from, TargetCancelType cancelType) 
            => from.EndAction(typeof(ItemIdentification));

		protected override void OnTarget(Mobile from, object o)
        {
            // 1. [완드 제작 로직] 우선순위 1번
            // 타겟팅한 대상이 BaseWand이고, 우리가 만든 IDWand가 아닐 때만 "제작" 실행
			
			if (o is IDWand)
            {
                from.SendMessage(0x22, "아이템 감정 완드는 분석하거나 강화할 수 없습니다.");
                from.EndAction(typeof(ItemIdentification));
                return;
            }			
			
            if (o is BaseWand targetWand && o is not IDWand)
            {
                int createGrade = from.Skills.ItemID.Value switch
                {
                    >= 200 => 5,
                    >= 150 => 4,
                    >= 100 => 3,
                    >= 50 => 2,
                    >= 30 => 1,
                    _ => 0
                };

                from.PlaySound(0x1F7);
                from.FixedParticles(0x375A, 1, 15, 5012, EffectLayer.Waist);
                
                from.AddToBackpack(new IDWand(createGrade));
                targetWand.Delete();

                from.SendMessage(0x35, $"{GradeNames[createGrade]} 등급의 아이템 감정 완드를 제작하였습니다!");
                from.EndAction(typeof(ItemIdentification));
                return; // 제작 완료 후 종료
            }

            // 2. [아이템 감정/강화 로직] 우선순위 2번
            // 타겟팅한 대상이 장비 아이템(IEquipOption)일 때만 실행
            if (o is Item item && item.RootParent == from && item is IEquipOption equip)
            {
				// [추가] 저주받은 장비는 강화 시도 불가
				if (item.LootType == LootType.Cursed)
				{
					from.SendMessage(0x22, "저주받은 장비는 사용할 수 없습니다.");
					from.EndAction(typeof(ItemIdentification));
					return;
				}			
			
                int grade = equip.SuffixOption[1];

                if (from.Skills.ItemID.Value < GradeSkillTable[grade])
                {
                    from.SendMessage(0x22, $"{GradeNames[grade]} 등급 장비를 감정하려면 스킬이 {GradeSkillTable[grade]} 이상 필요합니다.");
                    from.EndAction(typeof(ItemIdentification));
                    return;
                }

                int iterations = GetIterations(equip.SuffixOption[10]);
                from.SendMessage(0x35, "아이템의 잠재력을 분석하기 시작합니다...");
                from.NextSkillTime = Core.TickCount + (int)TimeSpan.FromSeconds(iterations * 2.1).TotalMilliseconds;
                
                new EnhanceTimer(from, item, iterations).Start();
            }
            else
            {
                // 완드도 아니고 감정 가능한 아이템도 아닐 때
                if (o is Item) 
                    from.SendMessage("이 아이템은 감정하거나 강화할 수 없습니다.");
                
                from.EndAction(typeof(ItemIdentification));
            }

            Server.Engines.XmlSpawner2.XmlAttach.RevealAttachments(from, o);
        }
    }

    public static int GetIterations(int step) => step switch
    {
        <= 2 => 1, <= 4 => 2, <= 6 => 3, _ => step - 3
    };

    public class EnhanceTimer(Mobile from, Item item, int count) 
        : Timer(TimeSpan.FromSeconds(2.0), TimeSpan.FromSeconds(2.0))
    {
        private readonly Point3D _location = from?.Location ?? Point3D.Zero;

        protected override void OnTick()
        {
            if (from is not { Alive: true } || item is not { Deleted: false } || from.Location != _location || item.RootParent != from)
            {
                from?.SendMessage(0x22, "집중력이 흐트러져 감정이 중단되었습니다.");
                from?.EndAction(typeof(ItemIdentification));
                Stop();
                return;
            }

            if (item is not IEquipOption { SuffixOption: not null } equip)
            {
                from.EndAction(typeof(ItemIdentification));
                Stop();
                return;
            }

            if (count-- > 0)
            {
                PlayEnhanceEffects(equip.SuffixOption[10]);
            }
            else
            {
                from.EndAction(typeof(ItemIdentification));
                CompleteEnhance(from, item, equip);
                Stop();
            }
        }

        private void PlayEnhanceEffects(int step)
        {
            if (step >= 9)
            {
                from.Animate(31, 7, 1, true, false, 0);
                from.PlaySound(0x51D);
                from.FixedParticles(0x3709, 10, 30, 5052, EffectLayer.LeftFoot);
                from.SendMessage(0x21, "한계에 도전하고 있습니다!");
            }
            else
            {
                from.Animate(17, 5, 1, true, false, 0);
                from.PlaySound(step >= 7 ? 0x243 : 0x1F7);
                from.SendMessage(0x35, step >= 7 ? "위험한 강화를 시도하고 있습니다..." : "아이템을 분석 중입니다...");
            }
            from.FixedParticles(0x376A, 9, 32, 5005, EffectLayer.Waist);
        }
    }

    private static void CompleteEnhance(Mobile from, Item item, IEquipOption equip)
    {
        if (from == null || item == null || equip == null) return;

        int result = Misc.EnhancedChance.TryEnhance(from, item);
        int currentStep = equip.SuffixOption[10];
        int grade = equip.SuffixOption[1];
        string itemName = item.Name ?? (item.LabelNumber > 0 ? $"#{item.LabelNumber}" : item.GetType().Name);

        if (result == 1)
        {
            if (currentStep >= 7)
            {
                string args = $"{from.Name}\t{itemName}\t{currentStep}";
                Misc.Util.BroadcastLocalized(1083501, args, 1165);
                from.FixedParticles(0x373A, 10, 30, 5012, EffectLayer.Waist);
                from.FixedParticles(0x375A, 10, 20, 5027, EffectLayer.Head);
                from.PlaySound(0x209);
            }
            else
            {
                from.SendLocalizedMessage(1083503, $"{itemName}\t{currentStep}");
                from.PlaySound(0x3E3);
            }
        }
        else
        {
            if (currentStep >= 7)
            {
                string args = $"{from.Name}\t{itemName}\t{currentStep}";
                Misc.Util.BroadcastLocalized(1083502, args, 1166);
                from.FixedParticles(0x36BD, 20, 10, 5044, EffectLayer.Head);
				// 저주 부여
				item.LootType = LootType.Cursed;
            }
            else
            {
                from.SendLocalizedMessage(1083504, $"{itemName}\t{currentStep}");
                from.PlaySound(0x54);
            }

            if (equip.MaxHitPoints <= 0)
            {
                from.SendMessage(0x22, "강화 실패로 아이템이 완전히 파괴되었습니다.");
                from.PlaySound(0x207);
                item.Delete();
                return;
            }
        }
        equip.Identified = true;
        item.InvalidateProperties();
    }
}