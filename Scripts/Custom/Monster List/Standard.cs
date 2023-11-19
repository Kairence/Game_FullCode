using Server.Items;
using System;
using Server.Mobiles;

/*
1 오우거 Ogre
2 오우거 로드 OgreLord
3 좀비 Zombie
4 가고일 Gargoyle
5 독수리 Eagle
6 새 Bird
7 오크 캡틴 OrcCaptain
8 위핑 바인 WhippingVine
9 악마 Demon
10 하급 악마 LesserDemon
11 드레드 거미 DreadSpider
12 드래곤 Dragon
13 공기정령 AirElemental
14 대지정령 EarthElemental
15 불정령 FireElemental
16 물정령 WaterElemental
17 오크 Orc
18 에틴 Ettin
19 드래드 거미 DreadSpider (11번과 겹침)
20 얼음 거미 FrostSpider
21 큰 뱀 GiantSerpent
22 게이저 Gazer
23 회색 늑대 GreyWolf
24 리치 Lich
25 밝은 회색 늑대
26 유령 Wraith
27 어두운 회색 늑대 WolfDarkGrey
28 큰거미 GiantSpider
29 고릴라 Gorilla
30 하피 Harpy
31 헤드레스원 HeadlessOne
32 없음
33 리자드맨 Lizardman
34 회색늑대 WilfGrey
35 리자드맨스피어 LizardmanSpear
36 리자드맨메이스 LizardmanMace
37 북극늑대 ArticWolf
38 악마 로드 Demon Lord
39 몽벳 Mongbat
40 없음
41 



*/

namespace Server.Lists
{
	public class Monsters
	{
        public static readonly Type[] Standard_A = new[]
        {
            typeof(AcidElemental), typeof(AirElemental), typeof(AntLion), typeof(Archmage), 
			typeof(Wraith), typeof(SkeletalDragon), typeof(LichLord), typeof(FleshGolem), typeof(Lich), typeof(SkeletalKnight),
            typeof(BoneKnight), typeof(Mummy), typeof(SkeletalMage), typeof(BoneMagi), typeof(PatchworkSkeleton)
        };
	}
}
