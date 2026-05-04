using System.Collections.Generic;
using Server.Misc;

public class FamilyTitleData
{
    public int TitleID { get; set; }
    public string TitleName { get; set; }
    public string Description { get; set; } // 달성 조건 (관리자 참고용)
    public int[] OptIDs { get; set; }
    public int[] OptValues { get; set; }

    public FamilyTitleData(int id, string name, string desc, int[] ids, int[] values)
    {
        TitleID = id;
        TitleName = name;
        Description = desc;
        OptIDs = ids;
        OptValues = values;
    }
}

public static class TitleManager
{
    // 스탯 계산 기준: 10000 = 1% 또는 스탯 1
    // 스킬 계산 기준: 10000 = 0.2 상승 (100% = 20 상승, 즉 500000 = +10 스킬)
    
    public static readonly List<FamilyTitleData> Titles = new List<FamilyTitleData>
    {
        // =========================================================================
        // [Category 1] 흑역사와 죽음 (001 ~ 020) - 예능형 페널티 & 생존 특화
        // =========================================================================
        
        /* 001 */ new FamilyTitleData(1, "몽뱃에게 굴복한", "조건: 몽뱃에게 사망", 
            new[] { CustomOption.Hits, CustomOption.AllDamage }, new[] { 3000000, -50000 }), // 체력 +300 / 데미지 -5%
            
        /* 002 */ new FamilyTitleData(2, "10살에 곰을 때려잡은", "조건: 뉴비 시절 곰 맨손 처치", 
            new[] { CustomOption.Str, CustomOption.HitChance }, new[] { 2000000, -50000 }), // 힘 +200 / 명중 -5%
            
        /* 003 */ new FamilyTitleData(3, "한 방에 주님 곁으로", "조건: 오버킬 즉사", 
            new[] { CustomOption.HealPlus, CustomOption.PhysResist }, new[] { 200000, -100000 }), // 치유량 +20% / 물리저항 -10%
            
        /* 004 */ new FamilyTitleData(4, "실 가닥에 매달린", "조건: 체력 1로 3분 생존", 
            new[] { CustomOption.HitsRegen, CustomOption.Hits }, new[] { 300000, -1000000 }), // 체재생 +30% / 최대체력 -100
            
        /* 005 */ new FamilyTitleData(5, "붕대 감다 죽을 뻔한", "조건: 붕대 딜레이 중 사망 100회", 
            new[] { CustomOption.Healing, CustomOption.Dex }, new[] { 500000, -500000 }), // 힐링 +10 / 민첩 -50
            
        /* 006 */ new FamilyTitleData(6, "독에 절여진", "조건: 맹독 상태 1시간 생존", 
            new[] { CustomOption.PoisonResist, CustomOption.HitsRegen }, new[] { 300000, -100000 }), // 독저항 +30% / 체재생 -10%
            
        /* 007 */ new FamilyTitleData(7, "뒤통수가 남아나지 않는", "조건: 백어택 500회 허용", 
            new[] { CustomOption.SwingSpeed, CustomOption.DefChance }, new[] { 150000, -100000 }), // 공속 +15% / 방어율 -10%
            
        /* 008 */ new FamilyTitleData(8, "챔피언의 발닦개", "조건: 보스방 입장 10초 내 사망", 
            new[] { CustomOption.Hits, CustomOption.AllDamage }, new[] { 2000000, -50000 }), // 체력 +200 / 데미지 -5%
            
        /* 009 */ new FamilyTitleData(9, "드래곤의 이쑤시개", "조건: 고룡 브레스 극딜 생존", 
            new[] { CustomOption.FireResist, CustomOption.ColdResist }, new[] { 200000, -100000 }), // 불저항 +20% / 냉기저항 -10%
            
        /* 010 */ new FamilyTitleData(10, "부활의 제단 VIP", "조건: 하루 부활 50회", 
            new[] { CustomOption.AllRegen, CustomOption.Hits }, new[] { 150000, -500000 }), // 모든재생 +15% / 최대체력 -50
            
        /* 011 */ new FamilyTitleData(11, "가드킬 단골손님", "조건: 가드에게 50번 즉사", 
            new[] { CustomOption.AllSpeed, CustomOption.AllArmor }, new[] { 100000, -100000 }), // 모든속도 +10% / 모든방어 -10%
            
        /* 012 */ new FamilyTitleData(12, "낙마주의", "조건: 강제 낙마 100회", 
            new[] { CustomOption.Dex, CustomOption.DefChance }, new[] { 1000000, -50000 }), // 민첩 +100 / 방어율 -5%
            
        /* 013 */ new FamilyTitleData(13, "함정 감별사", "조건: 트랩 연속 50회 밟음", 
            new[] { CustomOption.MagicResist, CustomOption.PhysResist }, new[] { 500000, -100000 }), // 마법저항(스킬) +10 / 물리저항 -10%
            
        /* 014 */ new FamilyTitleData(14, "화살받이", "조건: 한 전투 원거리 500피격", 
            new[] { CustomOption.PhysResist, CustomOption.DefChance }, new[] { 200000, -100000 }), // 물리저항 +20% / 방어율 -10%
            
        /* 015 */ new FamilyTitleData(15, "심해의 공포", "조건: 크라켄에게 100회 피격", 
            new[] { CustomOption.ColdResist, CustomOption.FireResist }, new[] { 200000, -100000 }), // 냉기저항 +20% / 불저항 -10%
            
        /* 016 */ new FamilyTitleData(16, "불타는 발바닥", "조건: 파이어월 위 1000회 피격", 
            new[] { CustomOption.FireResist, CustomOption.Stam }, new[] { 300000, -1000000 }), // 불저항 +30% / 기력 -100
            
        /* 017 */ new FamilyTitleData(17, "무면허 라이더", "조건: 남의 펫 타다 100번 거부", 
            new[] { CustomOption.AnimalTaming, CustomOption.HitChance }, new[] { 500000, -50000 }), // 테이밍 +10 / 명중 -5%
            
        /* 018 */ new FamilyTitleData(18, "길 잃은 어린양", "조건: 굶주림 상태 아웃", 
            new[] { CustomOption.Luck, CustomOption.Int }, new[] { 5000000, -500000 }), // 운 +500 / 지능 -50
            
        /* 019 */ new FamilyTitleData(19, "유령선 선장", "조건: 유령으로 바다 1시간 표류", 
            new[] { CustomOption.Fishing, CustomOption.FireResist }, new[] { 500000, -100000 }), // 낚시 +10 / 불저항 -10%
            
        /* 020 */ new FamilyTitleData(20, "마을 귀환 지정석", "조건: 솔로 사망 1000회", 
            new[] { CustomOption.AllArmor, CustomOption.WeaponDamage }, new[] { 150000, -100000 }), // 모든방어 +15% / 데미지 -10%


        // =========================================================================
        // [Category 2] 하드코어 전투 및 기행 (021 ~ 040) - 데미지 및 슬레이어 특화
        // =========================================================================

        /* 021 */ new FamilyTitleData(21, "맨주먹 곰 사냥꾼", "조건: 무기 없이 곰 100마리 처치", 
            new[] { CustomOption.DefChance, CustomOption.Str, CustomOption.WeaponDamage }, new[] { 150000, 1000000, -100000 }), // 방어 +15%, 힘 +100 / 데미지 -10%
            
        /* 022 */ new FamilyTitleData(22, "마력 결핍증", "조건: 마나 0으로 평타 1000킬", 
            new[] { CustomOption.WeaponDamage, CustomOption.Mana }, new[] { 200000, -1000000 }), // 물리피해 +20% / 마나 -100
            
        /* 023 */ new FamilyTitleData(23, "패링의 미학", "조건: 방패방어 30연속 성공", 
            new[] { CustomOption.BlockChance, CustomOption.SwingSpeed }, new[] { 150000, -50000 }), // 블럭 +15% / 공속 -5%
            
        /* 024 */ new FamilyTitleData(24, "죽음을 속인", "조건: 독뎀 체력 1에서 해독 100회", 
            new[] { CustomOption.AllRegen, CustomOption.Hits }, new[] { 200000, -1000000 }), // 모든재생 +20% / 체력 -100
            
        /* 025 */ new FamilyTitleData(25, "던전의 망령", "조건: 24시간 한 던전 사냥", 
            new[] { CustomOption.AllDamage, CustomOption.Luck }, new[] { 150000, -2000000 }), // 모든피해 +15% / 운 -200
            
        /* 026 */ new FamilyTitleData(26, "나 혼자 산다", "조건: 파티 없이 둠 보스 킬", 
            new[] { CustomOption.AllStat, CustomOption.DefChance }, new[] { 500000, -50000 }), // 올스탯 +50 / 방어율 -5%
            
        /* 027 */ new FamilyTitleData(27, "붉은 이름을 지우는 자", "조건: 머더러 10회 처치", 
            new[] { CustomOption.HumanoidSlayer, CustomOption.AllResist }, new[] { 200000, 50000 }), // 인간형피해 +20% / 저항 +5%
            
        /* 028 */ new FamilyTitleData(28, "학살의 춤사위", "조건: 10초 내 광역 20킬", 
            new[] { CustomOption.SpellDamage, CustomOption.CastFocus }, new[] { 150000, -100000 }), // 주문피해 +15% / 시전집중 -10%
            
        /* 029 */ new FamilyTitleData(29, "거미줄 청소부", "조건: 거미 10,000킬", 
            new[] { CustomOption.InsectSlayer, CustomOption.PoisonResist }, new[] { 200000, 100000 }), // 곤충피해 +20% / 독저항 +10%
            
        /* 030 */ new FamilyTitleData(30, "오크들의 재앙", "조건: 오크 50,000킬", 
            new[] { CustomOption.HumanoidSlayer, CustomOption.PhysResist }, new[] { 200000, 100000 }), // 인간형피해 +20% / 물리저항 +10%
            
        /* 031 */ new FamilyTitleData(31, "도축업자", "조건: 시체 해체 10,000회", 
            new[] { CustomOption.Skinning, CustomOption.Dex }, new[] { 500000, 500000 }), // 무두술 +10 / 민첩 +50
            
        /* 032 */ new FamilyTitleData(32, "일격필살", "조건: 단일 10,000 뎀지", 
            new[] { CustomOption.WeaponCriChance, CustomOption.SwingSpeed }, new[] { 150000, -100000 }), // 치명확률 +15% / 공속 -10%
            
        /* 033 */ new FamilyTitleData(33, "강철의 방패", "조건: 100만 데미지 방어", 
            new[] { CustomOption.AllArmor, CustomOption.AllSpeed }, new[] { 200000, -100000 }), // 모든방어 +20% / 속도 -10%
            
        /* 034 */ new FamilyTitleData(34, "명사수", "조건: 화살 10만발 소모", 
            new[] { CustomOption.Archery, CustomOption.DefChance }, new[] { 500000, -50000 }), // 궁술 +10 / 방어율 -5%
            
        /* 035 */ new FamilyTitleData(35, "피의 굶주림을 이겨낸", "조건: 흡혈 없이 뱀파폼 사냥", 
            new[] { CustomOption.UndeadSlayer, CustomOption.HitsLeech }, new[] { 200000, 100000 }), // 언데드피해 +20% / 피흡 +10%
            
        /* 036 */ new FamilyTitleData(36, "맨몸의 투사", "조건: 노아머 드래곤 처치", 
            new[] { CustomOption.DefChance, CustomOption.AllArmor }, new[] { 200000, -200000 }), // 방어율 +20% / 모든방어 -20%
            
        /* 037 */ new FamilyTitleData(37, "몬스터들의 아이돌", "조건: 어그로 30마리 이상", 
            new[] { CustomOption.AggroPercent, CustomOption.AllDamage }, new[] { 300000, 100000 }), // 어그로 +30% / 뎀지 +10%
            
        /* 038 */ new FamilyTitleData(38, "챔피언 스틸러", "조건: 1% 남았을때 막타", 
            new[] { CustomOption.Luck, CustomOption.HitChance }, new[] { 10000000, -100000 }), // 운 +1000 / 명중 -10%
            
        /* 039 */ new FamilyTitleData(39, "시간여행자", "조건: 튕김 후 몹밭 생존", 
            new[] { CustomOption.AllResist, CustomOption.AllSpeed }, new[] { 100000, -50000 }), // 저항 +10% / 속도 -5%
            
        /* 040 */ new FamilyTitleData(40, "파라곤 브레이커", "조건: 황금몹 500킬", 
            new[] { CustomOption.AllDamage, CustomOption.AllArmor }, new[] { 150000, -50000 }), // 데미지 +15% / 방어 -5%


        // =========================================================================
        // [Category 3] 생산과 노가다 (041 ~ 060) - 생활 스킬 및 유틸 특화
        // =========================================================================

        /* 041 */ new FamilyTitleData(41, "숯덩이 연성술사", "조건: 요리 태우기 1000회", 
            new[] { CustomOption.Cooking, CustomOption.FireResist }, new[] { 500000, 100000 }), // 요리 +10 / 불저항 +10%
            
        /* 042 */ new FamilyTitleData(42, "폭발물 처리반", "조건: 포션 폭발 100회", 
            new[] { CustomOption.Alchemy, CustomOption.HitChance }, new[] { 500000, -50000 }), // 연금술 +10 / 명중 -5%
            
        /* 043 */ new FamilyTitleData(43, "대장간의 파괴자", "조건: 장비 수리 완전파괴 50회", 
            new[] { CustomOption.Blacksmith, CustomOption.Dex }, new[] { 500000, -500000 }), // 대장 +10 / 민첩 -50
            
        /* 044 */ new FamilyTitleData(44, "헐벗은 양들의 친구", "조건: 양털 깎기 10000회", 
            new[] { CustomOption.Tailoring, CustomOption.Dex }, new[] { 500000, 500000 }), // 재봉 +10 / 민첩 +50
            
        /* 045 */ new FamilyTitleData(45, "나무꾼의 무딘 도끼", "조건: 도끼 부서질때까지 벌목", 
            new[] { CustomOption.Lumberjacking, CustomOption.WeaponDamage }, new[] { 500000, -50000 }), // 벌목 +10 / 뎀지 -5%
            
        /* 046 */ new FamilyTitleData(46, "광맥을 스쳐가는 손", "조건: 채광 실패 연속 50회", 
            new[] { CustomOption.Mining, CustomOption.Luck }, new[] { 500000, 5000000 }), // 채광 +10 / 운 +500
            
        /* 047 */ new FamilyTitleData(47, "바다의 쓰레기통", "조건: 신발 1000개 낚기", 
            new[] { CustomOption.Fishing, CustomOption.ColdResist }, new[] { 500000, 100000 }), // 낚시 +10 / 냉기저항 +10%
            
        /* 048 */ new FamilyTitleData(48, "재봉사의 바늘 찔림", "조건: 옷감 날림 1000번", 
            new[] { CustomOption.Tailoring, CustomOption.Hits }, new[] { 500000, -500000 }), // 재봉 +10 / 체력 -50
            
        /* 049 */ new FamilyTitleData(49, "곡괭이 브레이커", "조건: 곡괭이 500개 파손", 
            new[] { CustomOption.Mining, CustomOption.Str }, new[] { 500000, 500000 }), // 채광 +10 / 힘 +50
            
        /* 050 */ new FamilyTitleData(50, "붕대 깎는 노인", "조건: 붕대 직접 제작 10만개", 
            new[] { CustomOption.Healing, CustomOption.Dex }, new[] { 500000, 500000 }), // 힐링 +10 / 민첩 +50
            
        /* 051 */ new FamilyTitleData(51, "마법의 잉크 중독", "조건: 스크롤 제작 실패 500회", 
            new[] { CustomOption.Inscription, CustomOption.Mana }, new[] { 500000, -500000 }), // 기록술 +10 / 마나 -50
            
        /* 052 */ new FamilyTitleData(52, "동물들의 샌드백", "조건: 테이밍 중 물림 1000회", 
            new[] { CustomOption.AnimalTaming, CustomOption.HitChance }, new[] { 500000, -50000 }), // 테이밍 +10 / 명중 -5%
            
        /* 053 */ new FamilyTitleData(53, "길바닥 수집가", "조건: 버린 잡템 10000줍기", 
            new[] { CustomOption.Luck, CustomOption.Stam }, new[] { 5000000, -500000 }), // 운 +500 / 기력 -50
            
        /* 054 */ new FamilyTitleData(54, "지상 최대의 짠돌이", "조건: 구매 없이 판매로 100만", 
            new[] { CustomOption.Gold, CustomOption.AllDamage }, new[] { 200000, -50000 }), // 골드 +20% / 뎀지 -5%
            
        /* 055 */ new FamilyTitleData(55, "만물상", "조건: 벤더 판매 10000번", 
            new[] { CustomOption.Gold, CustomOption.Luck }, new[] { 100000, 3000000 }), // 골드 +10% / 운 +300
            
        /* 056 */ new FamilyTitleData(56, "잡동사니 백과사전", "조건: 잡템 100종류 동시 소지", 
            new[] { CustomOption.Luck, CustomOption.Dex }, new[] { 5000000, -500000 }), // 운 +500 / 민첩 -50
            
        /* 057 */ new FamilyTitleData(57, "부동산 갑부", "조건: 성 이상 하우징 설치", 
            new[] { CustomOption.AllStat, CustomOption.Gold }, new[] { 500000, 100000 }), // 올스탯 +50 / 골드 +10%
            
        /* 058 */ new FamilyTitleData(58, "집 없는 달팽이", "조건: 문서 100일 미설치", 
            new[] { CustomOption.AllResist, CustomOption.AllSpeed }, new[] { 100000, -50000 }), // 저항 +10% / 속도 -5%
            
        /* 059 */ new FamilyTitleData(59, "트레저 헌터", "조건: 보물상자 100회", 
            new[] { CustomOption.Cartography, CustomOption.Luck }, new[] { 500000, 5000000 }), // 지도제작 +10 / 운 +500
            
        /* 060 */ new FamilyTitleData(60, "가방이 찢어질 듯한", "조건: 무게초과 10000보", 
            new[] { CustomOption.Str, CustomOption.Stam }, new[] { 3000000, -1000000 }), // 힘 +300 / 기력 -100


        // =========================================================================
        // [Category 4] 소셜 및 상호작용 (061 ~ 080) - 코스트 감소 및 재생 특화
        // =========================================================================

        /* 061 */ new FamilyTitleData(61, "수다쟁이", "조건: 채팅 10000줄", 
            new[] { CustomOption.Int, CustomOption.CastFocus }, new[] { 1000000, -50000 }), // 지능 +100 / 시전집중 -5%
            
        /* 062 */ new FamilyTitleData(62, "고독한 미식가", "조건: 모든 주점 요리 구매", 
            new[] { CustomOption.HitsRegen, CustomOption.Str }, new[] { 200000, 500000 }), // 체재생 +20% / 힘 +50
            
        /* 063 */ new FamilyTitleData(63, "은행 앞의 망부석", "조건: 은행 앞 잠수 100시간", 
            new[] { CustomOption.AllRegen, CustomOption.AllSpeed }, new[] { 150000, -100000 }), // 모든재생 +15% / 모든속도 -10%
            
        /* 064 */ new FamilyTitleData(64, "문게이트 멀미", "조건: 게이트 500번 연속통과", 
            new[] { CustomOption.Magery, CustomOption.Hits }, new[] { 500000, -500000 }), // 마법학 +10 / 체력 -50
            
        /* 065 */ new FamilyTitleData(65, "지옥문을 연 자", "조건: 마을에서 데몬 소환 100회", 
            new[] { CustomOption.DemonSlayer, CustomOption.FirePlus }, new[] { 200000, 100000 }), // 악마피해 +20% / 최종화염피해 +10%
            
        /* 066 */ new FamilyTitleData(66, "팁을 바라는 자", "조건: NPC 적선 1000회", 
            new[] { CustomOption.Gold, CustomOption.Luck }, new[] { 150000, 2000000 }), // 골드 +15% / 운 +200
            
        /* 067 */ new FamilyTitleData(67, "숨바꼭질의 달인", "조건: 은신 1시간", 
            new[] { CustomOption.Hiding, CustomOption.DefChance }, new[] { 500000, -50000 }), // 은신 +10 / 방어율 -5%
            
        /* 068 */ new FamilyTitleData(68, "범죄의 재구성", "조건: 범죄자로 가드피함 10분", 
            new[] { CustomOption.AllSpeed, CustomOption.AllArmor }, new[] { 100000, -100000 }), // 속도 +10% / 방어 -10%
            
        /* 069 */ new FamilyTitleData(69, "거렁뱅이", "조건: 자산 0골드로 생존", 
            new[] { CustomOption.Gold, CustomOption.AllDamage }, new[] { 300000, -100000 }), // 골드 +30% / 뎀지 -10%
            
        /* 070 */ new FamilyTitleData(70, "친절한 이웃", "조건: 1000번 부활시켜줌", 
            new[] { CustomOption.HealPlus, CustomOption.AllResist }, new[] { 150000, 50000 }), // 치유량 +15% / 모든저항 +5%
            
        /* 071 */ new FamilyTitleData(71, "애완동물 작명가", "조건: 펫이름 100번 변경", 
            new[] { CustomOption.AnimalLore, CustomOption.Int }, new[] { 500000, 500000 }), // 동물지식 +10 / 지능 +50
            
        /* 072 */ new FamilyTitleData(72, "음악에 취한", "조건: 바드 연주 10000번 들음", 
            new[] { CustomOption.Musicianship, CustomOption.Peacemaking }, new[] { 500000, 500000 }), // 음악연주 +10 / 평화연주 +10
            
        /* 073 */ new FamilyTitleData(73, "평화주의자", "조건: 데미지 없이 10시간 플탐", 
            new[] { CustomOption.DefChance, CustomOption.WeaponDamage }, new[] { 200000, -200000 }), // 방어율 +20% / 무기피해 -20%
            
        /* 074 */ new FamilyTitleData(74, "스토커", "조건: 1시간 졸졸 따라다님", 
            new[] { CustomOption.Tracking, CustomOption.Dex }, new[] { 500000, 500000 }), // 추적 +10 / 민첩 +50
            
        /* 075 */ new FamilyTitleData(75, "이 구역의 미친 말", "조건: 마을 1000바퀴 돔", 
            new[] { CustomOption.StamRegen, CustomOption.Dex }, new[] { 200000, 500000 }), // 기력재생 +20% / 민첩 +50
            
        /* 076 */ new FamilyTitleData(76, "소사리아 지리학자", "조건: 대륙 끝좌표 밟기", 
            new[] { CustomOption.AllRegen, CustomOption.Luck }, new[] { 100000, 3000000 }), // 재생 +10% / 운 +300
            
        /* 077 */ new FamilyTitleData(77, "용암 수영 선수", "조건: 용암 버티기 10분", 
            new[] { CustomOption.FireResist, CustomOption.ColdResist }, new[] { 300000, -100000 }), // 불저항 +30% / 냉기저항 -10%
            
        /* 078 */ new FamilyTitleData(78, "독거미의 친구", "조건: 거미밭 투명 산책 10분", 
            new[] { CustomOption.PoisonResist, CustomOption.EnergyResist }, new[] { 300000, -100000 }), // 독저항 +30% / 에너지저항 -10%
            
        /* 079 */ new FamilyTitleData(79, "룬북 택시기사", "조건: 게이트 1000번 열어줌", 
            new[] { CustomOption.LowerManaCost, CustomOption.Magery }, new[] { 100000, 250000 }), // 마나소모감소 +10% / 마법 +5
            
        /* 080 */ new FamilyTitleData(80, "독서광", "조건: 책 100권 읽음", 
            new[] { CustomOption.EvalInt, CustomOption.Int }, new[] { 500000, 500000 }), // 지능평가 +10 / 지능 +50


        // =========================================================================
        // [Category 5] 기념일 및 이벤트 보상 (081 ~ 096) - 페널티 없는 순수 버프
        // =========================================================================

        /* 081 */ new FamilyTitleData(81, "새해를 여는 자", "조건: 신정 이벤트", 
            new[] { CustomOption.AllStat, CustomOption.Luck }, new[] { 500000, 5000000 }), // 스탯 +50 / 운 +500
            
        /* 082 */ new FamilyTitleData(82, "까치의 선물", "조건: 설날 이벤트", 
            new[] { CustomOption.Gold, CustomOption.Luck }, new[] { 200000, 5000000 }), // 골드 +20% / 운 +500
            
        /* 083 */ new FamilyTitleData(83, "독립의 함성", "조건: 삼일절 이벤트", 
            new[] { CustomOption.AllSpeed, CustomOption.AllDamage }, new[] { 100000, 50000 }), // 공속 +10% / 뎀지 +5%
            
        /* 084 */ new FamilyTitleData(84, "세계수의 가지", "조건: 식목일 이벤트", 
            new[] { CustomOption.AllRegen, CustomOption.AllResist }, new[] { 100000, 50000 }), // 재생 +10% / 저항 +5%
            
        /* 085 */ new FamilyTitleData(85, "피어나는 동심", "조건: 어린이날 이벤트", 
            new[] { CustomOption.StamRegen, CustomOption.Dex }, new[] { 200000, 1000000 }), // 기재생 +20% / 민첩 +100
            
        /* 086 */ new FamilyTitleData(86, "은혜를 아는 자", "조건: 어버이날 이벤트", 
            new[] { CustomOption.HealPlus, CustomOption.Hits }, new[] { 100000, 1000000 }), // 치유량 +10% / 체력 +100
            
        /* 087 */ new FamilyTitleData(87, "지혜의 샘", "조건: 스승의날 이벤트", 
            new[] { CustomOption.Int, CustomOption.LowerAllCost }, new[] { 1000000, 50000 }), // 지능 +100 / 소모감소 +5%
            
        /* 088 */ new FamilyTitleData(88, "자비의 연꽃", "조건: 석가탄신일 이벤트", 
            new[] { CustomOption.AllArmor, CustomOption.DefChance }, new[] { 100000, 50000 }), // 방어 +10% / 방어율 +5%
            
        /* 089 */ new FamilyTitleData(89, "잊지 않을 이름", "조건: 현충일 이벤트", 
            new[] { CustomOption.PhysResist, CustomOption.HitsRegen }, new[] { 100000, 100000 }), // 물리저항 +10% / 체재생 +10%
            
        /* 090 */ new FamilyTitleData(90, "흔들림 없는 저울", "조건: 제헌절 이벤트", 
            new[] { CustomOption.BlockChance, CustomOption.AllResist }, new[] { 50000, 50000 }), // 블럭율 +5% / 저항 +5%
            
        /* 091 */ new FamilyTitleData(91, "찬란한 빛을 찾은", "조건: 광복절 이벤트", 
            new[] { CustomOption.AllDamage, CustomOption.Magic }, new[] { 100000, 100000 }), // 뎀지 +10% / 매직확률 +10%
            
        /* 092 */ new FamilyTitleData(92, "풍요의 보름달", "조건: 추석 이벤트", 
            new[] { CustomOption.Gold, CustomOption.AllGain }, new[] { 200000, 100000 }), // 골드 +20% / 모든획득 +10
            
        /* 093 */ new FamilyTitleData(93, "하늘이 열린 날", "조건: 개천절 이벤트", 
            new[] { CustomOption.AllStat, CustomOption.AllDamage }, new[] { 500000, 50000 }), // 스탯 +50 / 뎀지 +5%
            
        /* 094 */ new FamilyTitleData(94, "한글 사랑꾼", "조건: 한글날 이벤트", 
            new[] { CustomOption.Int, CustomOption.SpellDamage }, new[] { 1000000, 50000 }), // 지능 +100 / 마법피해 +5%
            
        /* 095 */ new FamilyTitleData(95, "장난꾸러기 유령", "조건: 할로윈 이벤트", 
            new[] { CustomOption.Stealth, CustomOption.AllSpeed }, new[] { 250000, 50000 }), // 은신이동 +5 / 속도 +5%
            
        /* 096 */ new FamilyTitleData(96, "성야의 기적", "조건: 크리스마스 이벤트", 
            new[] { CustomOption.ColdResist, CustomOption.Luck }, new[] { 100000, 10000000 }), // 냉기저항 +10% / 운 +1000


        // =========================================================================
        // [Category 6] 극악 스탯 달성 및 히든 (097 ~ 100) - 오버클럭형 (극단적 스펙)
        // =========================================================================

        /* 097 */ new FamilyTitleData(97, "근육 조선", "조건: 힘 9999 달성", 
            new[] { CustomOption.Str, CustomOption.PhysPlus, CustomOption.Int }, new[] { 5000000, 200000, -2000000 }), // 힘 +500 / 최물피 +20% / 지능 -200
            
        /* 098 */ new FamilyTitleData(98, "빛의 속도", "조건: 민첩 9999 달성", 
            new[] { CustomOption.Dex, CustomOption.AllSpeed, CustomOption.Str }, new[] { 5000000, 200000, -2000000 }), // 민첩 +500 / 모든속도 +20% / 힘 -200
            
        /* 099 */ new FamilyTitleData(99, "걸어다니는 도서관", "조건: 지능 9999 달성", 
            new[] { CustomOption.Int, CustomOption.SpellDamage, CustomOption.Dex }, new[] { 5000000, 200000, -2000000 }), // 지능 +500 / 마법피해 +20% / 민첩 -200
            
        /* 100 */ new FamilyTitleData(100, "타이틀의 지배자", "조건: 위 타이틀 50개 이상 수집", 
            new[] { CustomOption.AllStat, CustomOption.Luck, CustomOption.AllDamage }, new[] { 3000000, 20000000, 150000 }) // 올스탯 +300 / 운 +2000 / 모든피해 +15%
    };
}