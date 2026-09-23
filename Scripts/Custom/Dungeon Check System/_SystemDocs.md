# Dungeon Check System

## 1. 개요
던전 진입 플래그, 열기(Heat) 상태 점검 및 안전도와 연동되는 검증 시스템입니다.

## 2. 주요 로직 및 파일
*   **DungeonCheck.cs / DungeonCheckGump.cs**: 유저가 던전에 진입하거나 탐험할 때의 조건, 입장료(포인트), 위협 수준을 인터페이스로 체크합니다.
*   방치된 던전의 열기(Heat)가 상승하면 거시 경제(`TownEconomy`) 및 마을 범죄율과 직결되는 플래그를 관리합니다.

## 3. 의존성 (Dependencies)
*   `Scripts\Custom\Point System\DungeonPointSystem.cs` (열기에 따른 획득 포인트 배율 통제)
