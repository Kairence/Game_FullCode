using System;
using System.Collections.Generic;
using Server;

namespace Server.Items
{
    public class TotalAttributes
    {
        // 핵심: 한 번의 해시 연산으로 고유/마법 값을 동시 관리 (C# 12 튜플 딕셔너리)
        private readonly Dictionary<int, (int Fixed, int Magic)> m_Data = [];
        
        // 세트 ID (인덱스 50번) 전용 필드 (SetItem.cs 최적화용)
        private int m_SetID;

        public TotalAttributes() { }

        // 데이터 주입
        public void SetValue(int index, int val, bool isFixed)
        {
            if (index == 50) { m_SetID = val; return; }

            var current = m_Data.GetValueOrDefault(index);
            
            if (isFixed) current.Fixed = val;
            else current.Magic = val;

            // 두 값이 모두 0이면 메모리 해제, 아니면 갱신
            if (current.Fixed == 0 && current.Magic == 0) m_Data.Remove(index);
            else m_Data[index] = current;
        }

        // 데이터 획득 (튜플 반환)
        public (int Fixed, int Magic, int Total) GetValues(int index)
        {
            if (index == 50) return (m_SetID, 0, m_SetID);

            if (m_Data.TryGetValue(index, out var val))
                return (val.Fixed, val.Magic, val.Fixed + val.Magic);

            return (0, 0, 0);
        }

        // 저장 및 불러오기 (Now 기준)
        public void Serialize(GenericWriter writer)
        {
            writer.Write(0); // version
            writer.Write(m_SetID);
            writer.Write(m_Data.Count);
            foreach (var kvp in m_Data)
            {
                writer.Write(kvp.Key);
                writer.Write(kvp.Value.Fixed);
                writer.Write(kvp.Value.Magic);
            }
        }

        public void Deserialize(GenericReader reader)
        {
            reader.ReadInt(); // version
            m_SetID = reader.ReadInt();
            int count = reader.ReadInt();
            for (int i = 0; i < count; i++)
            {
                int key = reader.ReadInt();
                m_Data[key] = (reader.ReadInt(), reader.ReadInt());
            }
        }
    }
}