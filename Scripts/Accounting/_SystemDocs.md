# Accounting

## 1. 개요
계정 권한(AccessLevel), 패스워드 검증, 서버 로그인 처리 등을 담당하는 폴더입니다.

## 2. Kairence Server 연동 및 정밀 스캔 결과
*   **계정 귀속 커스텀 배열 (`Account.Point[1000]`)**: `Account.cs` 파일을 정밀 스캔한 결과, 기존 UO에 없는 1,000칸짜리 하드코딩 정수 배열 `m_Point`와 `DonationPoint`가 직접 주입되어 있음을 확인했습니다.
*   **가문 스킬 시스템과의 직결성**: 앞서 `PlayerMobile.cs` 분석에서 보았던 401번~600번 인덱스의 '가문 미덕 스킬(Family Skill)' 데이터는 개별 캐릭터가 아닌 **'계정(Account)' 단위로 직렬화(Serialization)** 됨이 이번 스캔으로 완벽히 증명되었습니다. 즉, 한 계정 내의 모든 캐릭터는 동일한 가문 명성과 패시브 혜택을 공유합니다.
*   **C# 최신 문법 활용**: `m_Mobiles.OfType<PlayerMobile>().Aggregate(...)` 등 LINQ 체이닝이 적용되어 계정 내 모든 캐릭터의 총 플레이 타임이나 영(Young) 플레이어 상태를 연산합니다.
