# New Respawn System

## 1. 개요
단순한 몬스터/아이템 스폰을 넘어, 생태계 자원 노드(Node)를 월드에 물리적으로 배치하고 고갈 상태를 추적하는 동적 스폰 제어기입니다.

## 2. 주요 로직 및 환경
*   단순한 (X,Y) 좌표 범위를 넘어서 `RegionCode`와 `Zone` 기반의 논리적 영역 할당 방식을 엄격하게 사용합니다.
*   **물리 노드 생성**: `AnimalNode`, `HerbNode` 등의 식생 자원을 맵 상에 물리적으로 뿌려둡니다.
*   **생태계 연동**: 가상 시민(사냥꾼, 약초꾼)이 리플렉션으로 이 스폰된 자원 노드를 찾아내어 먼저 채취해 감으로써 유저와 스폰 자원을 두고 경쟁하게 만듭니다.

## 3. 의존성 (Dependencies)
*   `Scripts\Custom\City Economy System\EcosystemHarvester.cs` (자원 고갈 로직 연동)
