# VendorInfo

## 1. 개요
기존 울온의 시스템 상인(NPC)들이 판매하는 물품 및 기본 시세 리스트 정보입니다.

## 2. Kairence Server 연동 및 정밀 스캔 결과
*   **경제 통합 훅 발견 (`BaseVendor.cs`)**: 스캔 결과, 기존 울온의 단순 시스템 상인 통제용 베이스 파일에 `SyncToTownEconomy()`라는 커스텀 함수가 직접 꽂혀 있음을 확인했습니다.
*   **아키텍처 의미**: 이 훅은 상인의 물품 매매가 일어날 때마다 `TownEconomyManager.Towns[TownID]`를 호출하여, 구형 시스템 상인에서 발생한 골드 유입/유출을 신규 시스템인 가상 거시 경제(`TownEconomy`) 및 인플레이션 지표에 즉각 반영(`VirtualTradeSystem` 연동)하도록 만들어주는 완벽한 브릿지(Bridge) 역할을 수행합니다.
