# Context Menus

## 1. 개요
NPC나 플레이어를 단일 클릭(혹은 쉬프트 클릭)했을 때 나타나는 컨텍스트 메뉴 UI를 관리합니다.

## 2. Kairence Server 연동 및 정밀 스캔 결과
*   **UI 브릿지**: `OpenBankEntry.cs` 등 특정 메뉴 클릭 시 `PlayerMobile` 형변환을 거쳐 커스텀 Gump(예: `BankerGump`)로 연결시키는 진입점 브릿지로 동작합니다.
