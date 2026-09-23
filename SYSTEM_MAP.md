# Kairence Server Root System Map

본 문서는 Kairence Server (버전 1.0)의 글로벌 아키텍처 규칙과 각 하위 폴더의 분산 문서 위치를 안내하는 **'최상위 인덱스 및 코어 룰북'**입니다. 
구체적인 시스템별 역할, 수학적 공식, 그리고 클래스 의존성은 각 하위 폴더에 위치한 `_SystemDocs.md`를 참조하십시오.

## 1. 분산 문서화 인덱스 (Sub-System Documentation)
기획 확인 및 코드 수정 시, 연관된 폴더의 로컬 문서를 먼저 로드하십시오.

### 1.1. Core Data Hub
*   **[Mobiles (Core Hub)](file:///c:/Kairence_UO/4.0/Scripts/Mobiles/_SystemDocs.md)**: `PlayerMobile.cs`의 데이터 직렬화(V65), `UpdateEquipOptions` 매커니즘, 가문 스킬 연동 기록.

### 1.2. Major Systems
*   **[City Economy System](file:///c:/Kairence_UO/4.0/Scripts/Custom/City%20Economy%20System/_SystemDocs.md)**: 거시 경제 물가 산출식, 가상 시민 유저 벤더 직접 구매, 영토 분쟁/원한 시스템.
*   **[Aggro System](file:///c:/Kairence_UO/4.0/Scripts/Custom/Aggro%20System/_SystemDocs.md)**: 방패/근력(STR) 비례 어그로 산출식, 힐러 광역 분산 공식, 타겟 강제 고정 알고리즘.
*   **[Point System](file:///c:/Kairence_UO/4.0/Scripts/Custom/Point%20System/_SystemDocs.md)**: 전투 마스터리 경험치 공식, 던전 열기 비례 실버 포인트 등 4대 성장 엔진.
*   **[New Combat System](file:///c:/Kairence_UO/4.0/Scripts/Custom/New%20Combat%20System/_SystemDocs.md)**: 1만 배율 연산 기반 타격 연산, 커스텀 내구도 차감 및 세트 아이템 보너스 부여.
*   **[New Respawn System](file:///c:/Kairence_UO/4.0/Scripts/Custom/New%20Respawn%20System/_SystemDocs.md)**: 생태계 자원 물리적 노드 통제 및 유저-NPC 자원 경쟁 매커니즘.
*   **[Animal Setting System](file:///c:/Kairence_UO/4.0/Scripts/Custom/Animal%20Setting%20System/_SystemDocs.md)**: 소환수/동물 패시브 스킬 부여 및 등급 오버라이드 밸런싱.

### 1.3. Sub & Utility Systems
*   **[New House Extension System](file:///c:/Kairence_UO/4.0/Scripts/Custom/New%20House%20Extension%20System/_SystemDocs.md)**: 가상 시민의 물리적 주택 건설 및 강제 철거(DestroyEstate) 연동.
*   **[Dungeon Check System](file:///c:/Kairence_UO/4.0/Scripts/Custom/Dungeon%20Check%20System/_SystemDocs.md)**: 던전 진입 조건, 열기(Heat) 검증 시스템.
*   **[New Stat System](file:///c:/Kairence_UO/4.0/Scripts/Custom/New%20Stat%20System/_SystemDocs.md)**: 캡(Cap) 돌파 한계치 제어 로직.
*   **[New Trap System](file:///c:/Kairence_UO/4.0/Scripts/Custom/New%20Trap%20System/_SystemDocs.md)**: 던전 열기 연동형 기믹 트랩.
*   **[FirstSkillUser](file:///c:/Kairence_UO/4.0/Scripts/Custom/FirstSkillUser/_SystemDocs.md)**: 신규 진입 시 초기 스킬/가문 특성 할당기.
*   **[Virtues](file:///c:/Kairence_UO/4.0/Scripts/Custom/Virtues/_SystemDocs.md)**: 가문 스킬 트리 패시브 연동 모듈.
*   **[Season](file:///c:/Kairence_UO/4.0/Scripts/Custom/Season/_SystemDocs.md)**: 사계절 생태계 변화 통제.
*   **[Town Ship System](file:///c:/Kairence_UO/4.0/Scripts/Custom/Town%20Ship%20System/_SystemDocs.md)**: 해상 무역 및 가상 시민 해상 반경 확장기.
*   **[Casino](file:///c:/Kairence_UO/4.0/Scripts/Custom/Casino/_SystemDocs.md)**: 슬롯머신 기반 골드 인플레이션 억제기.
*   **[Monster List](file:///c:/Kairence_UO/4.0/Scripts/Custom/Monster%20List/_SystemDocs.md)**: 종족/분류별 몬스터 사전(Dictionary).
*   **[WorldEvent](file:///c:/Kairence_UO/4.0/Scripts/Custom/WorldEvent/_SystemDocs.md)**: 서버 전역 레이드/축제 스케줄러.
*   **[World Omniporter v2.5](file:///c:/Kairence_UO/4.0/Scripts/Custom/World%20Omniporter%20v2.5/_SystemDocs.md)**: 글로벌 게이트웨이 시스템.
*   **[Admin Toolbar](file:///c:/Kairence_UO/4.0/Scripts/Custom/Admin%20Toolbar/_SystemDocs.md)** & **[Staff Commands](file:///c:/Kairence_UO/4.0/Scripts/Custom/Staff%20Commands/_SystemDocs.md)**: 관리자용 시각적 모니터링 툴 및 명령어 집합.
*   **[Player Commands](file:///c:/Kairence_UO/4.0/Scripts/Custom/Player%20Commands/_SystemDocs.md)**: 유저용 인터페이스 브릿지.

### 1.4. Base Engine & Standard Folders (기본 엔진 체계)
*   **[Abilities](file:///c:/Kairence_UO/4.0/Scripts/Abilities/_SystemDocs.md)**: 무기 특수기 및 타격 시 어그로/위협 가산 연계부.
*   **[Accounting](file:///c:/Kairence_UO/4.0/Scripts/Accounting/_SystemDocs.md)**: 계정 권한 및 가문 미덕(Account.Point) 데이터 연동 훅.
*   **[Commands](file:///c:/Kairence_UO/4.0/Scripts/Commands/_SystemDocs.md)**: 명령어 체계 기반(CommandEventArgs) 로직.
*   **[Context Menus](file:///c:/Kairence_UO/4.0/Scripts/Context%20Menus/_SystemDocs.md)**: 시민 상호작용 및 UI 메뉴 엔트리.
*   **[Gumps](file:///c:/Kairence_UO/4.0/Scripts/Gumps/_SystemDocs.md)**: 커스텀 UI 다이얼로그의 상속 뼈대 엔진.
*   **[Items](file:///c:/Kairence_UO/4.0/Scripts/Items/_SystemDocs.md)**: BaseWeapon 등 장비 객체의 1만 배율 적용 기반 스크립트.
*   **[Misc](file:///c:/Kairence_UO/4.0/Scripts/Misc/_SystemDocs.md)**: 마스터 틱 엔진 및 난수 발생기(CSPRandom) 초기화 훅.
*   **[Multis](file:///c:/Kairence_UO/4.0/Scripts/Multis/_SystemDocs.md)**: 가상 시민의 물리적 주택(House) 처리 기반 코드.
*   **[Quests](file:///c:/Kairence_UO/4.0/Scripts/Quests/_SystemDocs.md)** & **[Regions](file:///c:/Kairence_UO/4.0/Scripts/Regions/_SystemDocs.md)**: 커스텀 RegionCode 스폰 체계 및 퀘스트 배열 통제 베이스.
*   **[Services](file:///c:/Kairence_UO/4.0/Scripts/Services/_SystemDocs.md)** & **[Skills](file:///c:/Kairence_UO/4.0/Scripts/Skills/_SystemDocs.md)**: 생산 마스터리와 성공률 통제 및 스킬 캐스팅 모션 제어부.
*   **[Spells](file:///c:/Kairence_UO/4.0/Scripts/Spells/_SystemDocs.md)**: 힐러의 유효 치유량 판정 및 광역 어그로 분산 연산 처리부.
*   **[Targets](file:///c:/Kairence_UO/4.0/Scripts/Targets/_SystemDocs.md)** & **[VendorInfo](file:///c:/Kairence_UO/4.0/Scripts/VendorInfo/_SystemDocs.md)**: 구형 타게팅 엔진 및 시민 경제 전환용 상인 데이터.
*   **[XSpawner Rev1.4](file:///c:/Kairence_UO/4.0/Scripts/XSpawner%20Rev1.4/_SystemDocs.md)**: 생태계 물리 노드 스폰을 지원하는 서드파티 스케줄러.
*   **[Staff Runebook](file:///c:/Kairence_UO/4.0/Scripts/Staff%20Runebook/_SystemDocs.md)**: 관리자용 툴 모듈.

---

## 2. Core Engine Upgrade (코어 환경)
*   **런타임 환경**: .NET 8.0 (Assembly 8.0.418), Roslyn 컴파일러.
*   **MasterTickEngine.cs**: 30초 단위 틱. 가상 시민(40분할), 모험가(10분할), 생태계(9분할) 등 부하를 쪼개어 GC 프리징 및 스레드 락을 차단.
*   **난수 생성기**: 병렬 처리 충돌을 막기 위해 모든 난수 연산에 `System.Random` 대신 `CSPRandom` 모듈 사용 강제.

## 3. 절대 개발 원칙 (Strict Guidelines)
새로운 스크립트를 작성하거나 기존 코드를 수정할 때 무조건 준수해야 하는 규칙입니다.
1.  **메모리 및 GC 통제**: 틱 엔진에서 루프를 돌 때 동적 배열(`new List`, `new int[]`) 생성을 극도로 제한합니다. 고정 배열 인덱싱, `ref`, `ReadOnlySpan`을 적극 사용하십시오.
2.  **소수점과 10,000배율 스케일링**: 스탯 계수를 건드릴 때 `double` 캐스팅 남용을 피하고, 엔진 내부 연산은 `ItemOptionCreator` 룰에 따라 10,000배율의 정수(`int`)로 묶어서 계산하십시오.
3.  **좌표(X, Y) 기반 스폰 통제 금지**: 스폰이나 영역 로직 작성 시 Raw 좌표(`Rectangle2D`)를 하드코딩하지 마십시오. 반드시 `RegionCode` 및 `Zone` 객체를 통해 논리적으로 맵핑해야 합니다.
4.  **멀티스레드 안전성**: 글로벌 경제 변수나 어그로 테이블을 갱신할 때는 반드시 명확한 스레드 락(Lock) 체계를 준수하십시오.
