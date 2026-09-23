# Abilities

## 1. 개요
무기 특수기(Special Moves) 및 직업별 기본 전투 어빌리티를 관리하는 폴더입니다.

## 2. Kairence Server 연동 및 정밀 스캔 결과
*   **독립성 유지 (Hook Interception)**: `grep_search`를 통해 130만 줄의 커스텀 훅(`AggroControl`, `CombatMastery` 등) 교차 참조를 스캔한 결과, `Abilities` 폴더 내부의 특수기 로직 자체에는 하드코딩된 커스텀 엔진 훅이 직접적으로 개입하지 않습니다.
*   이는 Kairence Server의 1만 배율 연산 및 레이드 어그로 시스템이 개별 무기 스킬 내부를 오염시키지 않고, 상위 엔진인 `Scripts\Custom\New Combat System\CombatEngine.cs`에서 최종 데미지 발생 시점에 **일괄 가로채기(Intercept)** 방식으로 연산되도록 설계되었음을 의미합니다.
*   **PlayerMobile 참조**: `WeaponAbility.cs`, `Disarm.cs`, `FrenziedWhirlwind.cs` 등에서 대상이 유저(`PlayerMobile`)일 경우에만 PvP 캡(Cap)이나 특정 디버프 지속 시간을 제한하는 패턴 매칭(`if (defender is PlayerMobile pm)`) 최신 문법(C# 12)이 다수 적용되어 있습니다.
