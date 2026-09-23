# Commands

## 1. 개요
인게임 명령어 시스템의 기본 뼈대를 관리합니다.

## 2. Kairence Server 연동 및 정밀 스캔 결과
*   **가시성 통제 (VisibilityList)**: `VisibilityList.cs` 및 `Properties.cs` 등에서 명령어를 호출한 주체가 `PlayerMobile`일 경우에 한해, 커스텀 시야 목록(은신 감지 등)과 World Creation 툴에 권한 훅을 연동합니다.
