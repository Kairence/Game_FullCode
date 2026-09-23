# Items

## 1. 개요
서버 내 존재하는 모든 무기, 방어구, 소모품, 장식품 등의 클래스 정의가 포함된 방대한 폴더입니다.

## 2. Kairence Server 연동 및 정밀 스캔 결과
*   **PlayerMobile 형변환 (Type Cast)**: `Items` 폴더 내부의 `Corpse.cs`, `BaseSpecialScrollBook.cs`, `SeedBox.cs` 등 수많은 객체에서 상호작용(OnDoubleClick 등) 시 대상을 `PlayerMobile`로 명시적 형변환하여 커스텀 UI(Gump)와 필터 속성(`UseOwnFilter` 등)을 제어하고 있습니다.
*   **ItemOptionCreator 분리**: 1만 배율 연산 로직 자체는 `Items` 폴더 내부의 개별 장비 파일에 하드코딩되지 않고, 코어 단(`BaseWeapon` 및 `BaseArmor`)을 통해 `ItemOptionCreator`로 위임 처리되는 깔끔한 아키텍처를 유지 중입니다.
