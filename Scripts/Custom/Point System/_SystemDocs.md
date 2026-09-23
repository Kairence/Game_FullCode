# Point System & Mastery (4대 성장 마스터리)

## 1. 개요
유저의 모든 행동(전투, 채집, 제작, 던전 공략)을 경험치화하여 10,000배율 스케일링 기반의 초정밀 스탯으로 변환하는 통합 성장 엔진입니다.

## 2. 주요 시스템별 상세 구조

### 2.1. CombatMastery.cs (전투 마스터리)
*   **경험치 공식**: `(Lv + 1)^2 * 25` 를 기준으로 등급(Grade) 별 요구 경험치가 산출됩니다.
*   몬스터 처치, 슬레이어 등급, 엘리트/보스 등급 경험치를 축적합니다.
*   **스탯 연동**: 도달한 레벨에 비례하여 방어력 무시 및 피해량 폭증 수치를 생성하고, `ApplyGradePassiveOptions`를 통해 `PlayerMobile.UpdateEquipOptions()`에 직접 이식합니다.

### 2.2. HarvestMastery.cs (채집 마스터리)
*   250여 종 이상의 자원별 개별 레벨링을 추적합니다.
*   특정 레벨 도달 시 '도구 내구도 100% 보호', '5% 확률로 즉시 채집 모션 생략 및 완료' 등의 하드코딩된 패시브 이득을 부여합니다.

### 2.3. ProductionMastery.cs (제작 마스터리)
*   12개의 대분류 스킬과 2217종의 개별 아이템 제작 숙련도를 추적하며, 성공률 및 결과물 등급(Exceptional) 보정에 관여합니다.

### 2.4. DungeonPointSystem.cs (던전 강화 시스템)
*   각 던전별 몬스터 포화도 및 열기(`TargetHeat`)에 따라 SilverPoint 획득 배율(1~10배)이 가변적으로 적용됩니다.
*   획득한 포인트로 유저는 총 48종의 장비 옵션 중 하나를 1,000레벨까지 강화할 수 있으며, 이 데이터는 `ApplyGoldPointOptions`를 통해 유저 스탯에 직결됩니다.

## 3. 의존성 (Dependencies)
*   `Scripts\Mobiles\PlayerMobile.cs` (데이터 캐싱 `m_GradeData`, `m_SlayerData`, 연산 주입부 `UpdateEquipOptions`)
*   `Scripts\Custom\New Combat System\ItemOptionCreator.cs` (10,000배율 변환기)
