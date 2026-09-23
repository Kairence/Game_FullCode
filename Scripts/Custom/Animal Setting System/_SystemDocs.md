# Animal Setting System

## 1. 개요
동물 및 소환수의 밸런스를 조절하고, 커스텀 패시브 스킬을 부여하는 시스템입니다.

## 2. 주요 로직 및 파일
*   **AnimalPassiveSkillHandler.cs**: 길들인 동물 및 소환수에게 고유 패시브 효과를 부여하고 제어합니다.
*   **CreatureBalancer.cs**: 맵 기반 혹은 특정 조건에 따라 동물의 등급과 능력치를 강제로 오버라이드 및 밸런싱합니다.
*   **SummonPoolManager.cs**: 소환수의 풀(Pool)과 스폰 정보를 관리하여 부하를 제어합니다.

## 3. 의존성 (Dependencies)
*   `PlayerMobile.cs` (펫 슬롯 및 팔로워 통제 로직)
*(추후 기획 작업 시 이 문서에 세부 밸런스 공식이 기록됩니다)*
