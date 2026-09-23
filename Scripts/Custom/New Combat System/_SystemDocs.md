# New Combat System

## 1. 개요
기존 울온의 단순 타격 방식을 넘어선 커스텀 타격 연산, 장비 스케일링, 내구도 시스템을 포괄하는 신규 전투 엔진입니다.

## 2. 주요 로직 및 파일
*   **ItemOptionCreator.cs**: 서버 내 모든 스탯 연산을 10,000배율 단위로 스케일링하여 소수점 오류를 극복하고 정밀한 옵션을 장비에 부여합니다.
*   **CombatEngine.cs**: 타격 시의 기본 연산 및 등급(Grade)에 따른 피해량 증폭 공식을 가로채어 계산합니다.
*   **NewDurabilityManager.cs**: 채집 마스터리 등과 연동되어 내구도 차감 및 파괴 확률을 재정의합니다.
*   **HitLocationManager.cs / Setitem.cs**: 부위별 타격 확률 재정의 및 세트 아이템 누적 보너스 부여.

## 3. 의존성 (Dependencies)
*   `Scripts\Mobiles\PlayerMobile.cs` (UpdateEquipOptions를 통한 1만 배율 연산 데이터 최종 반영)
