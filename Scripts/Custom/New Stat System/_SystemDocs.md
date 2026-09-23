# New Stat System

## 1. 개요
기존 울온의 최대 스탯(Stat) 제한을 넘어서 캐릭터의 캡(Cap)과 패시브 스탯을 재정의하는 확장 스탯 엔진입니다.

## 2. 주요 로직
*   `ItemOptionCreator`의 1만 배율 연산 및 `PlayerMobile`의 `ApplyEquipMods`에 의해 변동되는 스탯의 한계치를 통제하고, 커스텀 공식에 따라 Hits/Stam/Mana의 최대량을 결정합니다.
*(추후 기획 작업 시 이 문서에 세부 한계 돌파 공식이 기록됩니다)*
