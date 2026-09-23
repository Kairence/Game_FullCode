# Spells

## 1. 개요
마법 시전(메저리, 네크로맨서, 기사도 등)과 관련된 마나 소모, 범위 연산, 캐스팅 이펙트를 제어합니다.

## 2. Kairence Server 연동 및 정밀 스캔 결과
*   **광역 힐 어그로 훅 발견 (`SpellHelper.cs`)**: 130만 줄 검색 결과, 가장 핵심적인 어그로 훅이 `SpellHelper.cs`의 1615번 라인(`AggroControl.HealCheck(from, target, amount);`)에 완벽하게 꽂혀 있음을 확인했습니다.
*   **아키텍처 의미**: 힐링, 메저리, 기사도, 바드(HealingChorusSpell) 등 마법의 종류에 상관없이, 대상의 체력을 회복시키는 모든 스펠이 `SpellHelper.Heal`을 거치게 되므로, 오버힐(초과 치유량)을 제외한 유효 힐량의 50%가 주변 20타일 내의 모든 몬스터에게 분산되는 로직이 단 한 줄의 훅(Hook)으로 전역 통제되고 있습니다.
