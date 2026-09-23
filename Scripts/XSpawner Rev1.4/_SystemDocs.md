# XSpawner Rev1.4

## 1. 개요
광범위한 리스폰, 트리거 이벤트 처리를 지원하는 서드파티 스포너 플러그인 폴더입니다.

## 2. Kairence Server 연동 및 정밀 스캔 결과
*   **완벽한 디커플링 (Decoupling) 입증**: 검색 결과, XSpawner 자체 소스 코드 내부에는 커스텀 생태계나 Kairence 전용 변수명이 전혀 하드코딩되지 않았음을 확인했습니다.
*   **아키텍처 의미**: 이는 서드파티 플러그인(XSpawner)의 원본 코드를 오염시키지 않고, `New Respawn System` 측에서 단방향으로 XSpawner의 이벤트만 읽어와(Listen) 물리 노드를 생성하는 깔끔한 모듈 분리(Decoupled) 구조임을 입증합니다.
