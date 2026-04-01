using System;
using System.Collections.Generic;
using Server;
using Server.Items;

namespace Server.Mobiles
{
    public class AggroControl
    {
        private readonly BaseCreature _owner;
        private readonly Dictionary<Mobile, double> _table = new();

        public Dictionary<Mobile, double> Table => _table;

        public AggroControl(BaseCreature owner) => _owner = owner;

        public void Update(Mobile m, int damage, int aggroModifier, bool isHealing = false)
        {
            if (m == null || m.Deleted || !m.Alive || m == _owner) return;

            // 1. 기본 배율 설정
            double multiplier = 1.0;

            // 2. 힐링이 아닌 일반 피해 상황일 때 방패 보너스 체크
            if (!isHealing)
            {
                // 방패 착용자(탱커)는 2.0배, 일반은 1.0배
                multiplier = (m.FindItemOnLayer(Layer.TwoHanded) is BaseShield) ? 2.0 : 1.0;
            }

            double strBonus = 1.0;
            if (m is PlayerMobile)
            {
                // 힘이 1000이면 multiplier가 1.0이 되어 최종적으로 1.0배가 됨
                // 힘이 9999이면 multiplier가 9.9이 되어 약 9.9배가 됨
                strBonus = m.Str * 0.001;
            }
            // ---------------------------------------------------------

            // 3. 전달받은 aggro 인자 및 힘 보너스 적용
            double finalAggro = damage * multiplier * strBonus * (aggroModifier / 100.0);

            // 4. 테이블 갱신 [수정: out 키워드 제거 및 GetValueOrDefault 사용]
            double current = _table.GetValueOrDefault(m);
            _table[m] = current + finalAggro;
        }

        public Mobile GetTopAggro()
        {
            Mobile top = null;
            double max = -1;

            foreach (var (m, score) in _table)
            {
                if (m.Deleted || !m.Alive || m.Map != _owner.Map || !_owner.InRange(m, 16)) continue;
                if (score > max) { max = score; top = m; }
            }
            return top;
        }

        public void Clear()
        {
            _table.Clear();
        }       

        public static int HealCheck(Mobile from, Mobile to, int heal)
        {
            // 1. 치유량 보정 및 합산 [수정: ItemOptionCreator 사용]
            // ※ 주의: 실제 서버에 설정된 옵션 번호로 숫자 0을 변경해주세요.
            int percent = Server.Misc.ItemOptionCreator.GetAttributeValue(from, 0 /* EnhancePotions ID */);
            int plus = Server.Misc.ItemOptionCreator.GetAttributeValue(from, 0 /* HealBonus ID */);

            heal = (heal * (100 + percent)) / 100 + plus;
            
            // 2. 오버힐 방지 (실제 회복된 HP 계산)
            if (to.Hits + heal > to.HitsMax)
                heal = to.HitsMax - to.Hits;

            if (heal <= 0) return 0;

            // 3. 주변 몬스터 어그로 업데이트
            IPooledEnumerable eable = to.Map.GetMobilesInRange(to.Location, 20);

            foreach (Mobile m in eable)
            {
                if (m is BaseCreature bc && !bc.Controlled && bc.SummonMaster == null)
                {
                    // 몬스터가 힐 대상을 적대하고 있는 상태인지 확인
                    if (bc.Combatant == to || bc.Aggro.Table.ContainsKey(to))
                    {
                        // isHealing: true를 전달하여 방패 보너스(2배) 제외
                        bc.Aggro.Update(from, heal, 50, true);
                    }
                }
            }
            eable.Free();
            
            return heal;
        }   
    }
}