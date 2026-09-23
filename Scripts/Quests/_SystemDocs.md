# Quests

## 1. 개요
기존 울온의 Mondain's Legacy 퀘스트 기반 시스템 및 관련 엔티티 폴더입니다.

## 2. Kairence Server 연동 및 정밀 스캔 결과
*   **PlayerMobile 하드코딩 종속성**: `CollectorQuest`, `DarkTidesQuest`, `Eodon` 퀘스트 등 내부의 검증(CheckComplete, GiveRewardTo 등) 로직들이 최상위 객체인 `Mobile` 대신 `PlayerMobile`을 하드코딩된 파라미터 타입으로 강제 요구하도록 전면 수정되어 있습니다.
*   **아키텍처 의미**: 퀘스트 완료 보상이나 진행 과정이 Kairence Server의 커스텀 배열(`m_QuestCheck`)이나 마스터리 포인트 지급 등과 직결되기 위해 타입 캐스팅 제한을 걸어둔 것입니다.
