# New House Extension System

## 1. 개요
플레이어 주택 및 가상 시민(Virtual Citizen)의 건축 관련 물리적 제어 및 스마트 철거/확장을 담당하는 시스템입니다.

## 2. 주요 로직 및 파일
*   **ExAddonSystem.cs**: 주택 애드온(Addon)의 스마트 철거, 재배치, 확장 규격을 제어합니다.
*   TownSocietyEngine의 `StartNewConstruction()` 로직과 결합되어 가상 시민이 물리적으로 집을 짓거나 타인의 집을 `DestroyEstate()` 할 때 물리적 맵 데이터를 조작합니다.

## 3. 의존성 (Dependencies)
*   `Scripts\Custom\City Economy System\TownSocietyEngine.cs` (가상 시민의 주택 건축 및 원한 로직 연동)
