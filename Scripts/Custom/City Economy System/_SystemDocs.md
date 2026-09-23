# City Economy System (가상 시민 및 거시 경제)

## 1. 개요
기존 서버 상인(NPC)을 대체하고, 생명 주기를 가진 가상 시민(Virtual Citizen)과 유저 간의 경제 상호작용 및 영토 분쟁을 시뮬레이션하는 코어 시스템입니다.

## 2. 주요 클래스 및 작동 원리
*   **VirtualCitizen & BioStats**: 단순 상인이 아닌, `Hits, Stam, Mana, Food, Water, Sleep`을 가지며 노동하고 임금을 받습니다. `BioFoodRegistry.cs`의 영양소를 소모합니다.
*   **TownEconomy.cs (거시 경제 변동)**:
    *   **물가 공식**: `MacroModifier = Math.Clamp((double)Wealth / Math.Max(1, BaseWealth) - 1.0, -0.5, 1.0)`
    *   마을의 부(`Wealth`)와 대상인(Merchant)의 유입, 혹은 던전 열기에 의해 인플레이션/디플레이션이 통제됩니다.
*   **TownSocietyEngine.cs (건축과 원한)**:
    *   **건축 욕구**: 가문의 `HousingAmbition >= 100` 도달 시 물리적 주택 건설(`ConstructionStarter.StartFromMulti()`)을 시도합니다.
    *   **원한(Grudges)**: 영토가 부족해 하층민의 집을 강제 철거/매입(`DestroyEstate()`)할 경우 발생합니다. `Grudges > 50`이 되면 해당 가문들 간의 혼담이 파기됩니다.
*   **VirtualTradeSystem.cs (실물 경제 편입)**:
    *   NPC가 시스템 상점이 아닌, 유저 벤더를 먼저 방문합니다(`SearchPlayerVendors`).
    *   `ItemOptionCreator.GetAttributeValue`를 호출하여 가성비를 연산하고, 합격 시 직접 유저 벤더에 골드를 지불하여 실물 경제를 순환시킵니다.
*   **EcosystemHarvester.cs (자원 연동)**:
    *   시민이 채집 시 `pool.ConsumeResource(itemType)`를 호출하여 서버 내 물리 맵 자원을 실제로 감소시킵니다.

## 3. 의존성 (Dependencies)
*   `Scripts\Mobiles\PlayerMobile.cs` (상점 데이터 및 거래 연동)
*   `Scripts\Custom\New Combat System\ItemOptionCreator.cs` (가성비 판단)
