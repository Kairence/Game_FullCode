# Mobiles (PlayerMobile Core Hub)

## 1. 개요
기존의 단순 유저 객체를 넘어, 모든 커스텀 시스템(전투, 경제, 사회)의 누적 데이터가 10,000줄 넘는 코드로 집중되는 거대 데이터베이스(DB) 및 통합 연산 허브입니다.

## 2. 데이터 직렬화 (Serialization V65)
잦은 동적 할당과 가비지 컬렉터(GC) 부하를 막기 위해 고정 길이의 1차원 정수 배열로 극단적인 최적화를 취했습니다.
*   `m_QuestCheck[50000]`, `m_MonsterPoint[50000]`
*   `m_CraftPoint[6000]`, `m_HarvestPoint[1000]`
*   `m_GradeData[6]`, `m_SlayerData[8]`, `m_ItemSetSaveValue[500]`

## 3. UpdateEquipOptions() 연산 엔진
모든 패시브, 버프, 장비 옵션을 10,000배율 단위로 합산하여 유저의 최종 스탯을 결정합니다.
1.  **배열 합산**: 크기가 `ItemOptionCreator.MaxOptionCount`인 `_totalEquipOptions`에 고정, 마법, 재련, 기본 장비 옵션을 모두 합산합니다.
2.  **마스터리 투입**: `CombatMastery.ApplyGradePassiveOptions` 등 외부 성장 포인트를 투입합니다.
3.  **가문 스킬 연동 (Family Skill)**: 
    *   `Account.Point` 배열의 401~600번 인덱스를 순회합니다.
    *   `FamilySkillManager.Skills` (시민 사회 상호작용으로 쌓은 명성 트리)의 패시브 옵션을 캐릭터 물리 스탯에 직접 합산시킵니다.
4.  **네트워크 최적화 (`ApplyEquipMods`)**: C# 12의 컬렉션 표현식(`[]`)을 통해 관리되는 `_equipStatMods` 리스트를 바탕으로, 수치 변동이 있을 때만 업데이트를 수행하여 패킷을 절약합니다.

## 4. 의존성 (Dependencies)
*   모든 Custom System의 데이터가 이 파일로 모입니다.
*   수정 시 배열 인덱스 초과 에러 방지(Bound Check) 및 Span 구조 사용을 권장합니다.
