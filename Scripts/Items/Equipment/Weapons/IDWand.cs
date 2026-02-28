using System;
using Server.Targeting;

namespace Server.Items;

public class IDWand : BaseWand
{
    private int m_Grade;
    private static readonly string[] GradeNames = ["일반", "희귀", "영웅", "서사", "전설", "신화"];

    [CommandProperty(AccessLevel.GameMaster)]
    public int Grade 
    { 
        get => m_Grade; 
        set { m_Grade = value; InvalidateProperties(); } 
    }

    [Constructable]
    public IDWand() : this(0) 
    { 
    }

    [Constructable]
    public IDWand(int grade) : base(WandEffect.Identification, 100, 100)
    {
        m_Grade = grade;
        Name = "아이템 감정 완드";
        Hue = GetGradeHue(grade); // 등급별 색상 추가 (선택사항)
    }

    // [중요] 서버 재시작 시 아이템을 불러오는 핵심 생성자
    public IDWand(Serial serial) : base(serial) 
    { 
    }

    public override void GetProperties(ObjectPropertyList list)
    {
        base.GetProperties(list);
        string name = (m_Grade >= 0 && m_Grade < GradeNames.Length) ? GradeNames[m_Grade] : "알 수 없음";
        list.Add(1060659, $"감정 가능 한계\t{name} 등급");
    }

    // 등급에 따른 색상 (서버 껐다 켜도 유지됨)
    private static int GetGradeHue(int grade) => grade switch
    {
        1 => 2129, // 희귀 (하늘색 계열)
        2 => 2117, // 영웅 (보라색 계열)
        3 => 2127, // 서사 (분홍색 계열)
        4 => 2500, // 전설 (주황색 계열)
        5 => 1161, // 신화 (하얀색/금색 계열)
        _ => 0     // 일반
    };

    public override bool OnWandTarget(Mobile from, object o)
    {
        if (o is not Item item || item is not IEquipOption equip)
        {
            from.SendMessage("이 아이템에는 완드를 사용할 수 없습니다.");
            return false;
        }

        if (!from.BeginAction(typeof(ItemIdentification)))
        {
            from.SendMessage(0x22, "이미 아이템을 분석 중입니다.");
            return false;
        }

        if (item.RootParent != from)
        {
            from.SendMessage("가방에 있는 아이템만 사용할 수 있습니다.");
            return End(from);
        }

        if (m_Grade < equip.SuffixOption[1])
        {
            from.SendMessage(0x22, "완드의 등급이 낮아 감정할 수 없습니다.");
            return End(from);
        }

        if (Charges <= 0)
        {
            from.SendMessage("차지가 부족합니다.");
            return End(from);
        }

        int iterations = ItemIdentification.GetIterations(equip.SuffixOption[10]);
        from.SendMessage(0x35, "완드의 마력을 집중합니다...");
        from.NextSkillTime = Core.TickCount + (int)TimeSpan.FromSeconds((iterations + 1) * 2.1).TotalMilliseconds;
        
        new ItemIdentification.EnhanceTimer(from, item, iterations).Start();
        
        // 사용 시 차감 로직 (BaseWand 기능을 활용하거나 직접 구현)
        Charges--; 
        return true;

        bool End(Mobile m) { m.EndAction(typeof(ItemIdentification)); return false; }
    }

    public override void Serialize(GenericWriter writer)
    {
        base.Serialize(writer);
        writer.Write(1); // Version: 1 (등급 저장 추가)
        writer.Write(m_Grade);
    }

    public override void Deserialize(GenericReader reader)
    {
        base.Deserialize(reader);
        int version = reader.ReadInt();
        m_Grade = reader.ReadInt();
    }
}