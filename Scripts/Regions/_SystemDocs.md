# Regions

## 1. 개요
맵 상의 논리적 구역(마을, 던전, 보호 구역 등)을 나누고 입장/퇴장 시 이벤트를 처리합니다.

## 2. Kairence Server 연동 및 정밀 스캔 결과
*   **Region 내부의 독립성 유지**: 스캔 결과 `TownRegion.cs`나 `DamagingRegion.cs` 자체에는 커스텀 열기(TargetHeat) 변수가 하드코딩되어 있지 않습니다.
*   **아키텍처 의미**: 이는 지역(Region) 객체가 경제나 열기를 스스로 판단하는 것이 아니라, `DungeonPointSystem` 시스템이 전역 맵의 Region 목록을 역으로 순회하며 포인트 배율을 곱해주는 **단방향 제어 아키텍처**임을 증명합니다. 지역 코드는 순수하게 맵의 물리적 영역(RegionCode)만을 보존합니다.
