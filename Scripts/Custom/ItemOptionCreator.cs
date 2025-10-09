using System;
using System.Text;
using Server;
using Server.Mobiles;
using Server.Items;
using Server.Engines.Craft;
using System.Collections.Generic;
using System.Linq;

namespace Server.Misc
{
	public class ItemOptionCreator
	{

		#region 장비 신코드 유물 코드
		public static Item Artifact_Select(Item item, int rank )
		{
			//유물 체크
			List <Type> artifactitemSelect = new List <Type>();
			Item artifactitem = null;
			switch(rank)
			{
				/*
				case 5:
				{
					for( int i = 0; i < Artifact_5Tier.Length; ++i )
					{
						if( Artifact_Search( item.GetType().BaseType, Artifact_5Tier[i].GetType().BaseType.BaseType )
						{
							artifactitemSelect.Add( Artifact_5Tier[i] );
						}
					}
					if( artifactitemSelect.Count > 0 )
					{
						artifactitem = (Item)Activator.CreateInstance(artifactitemSelect[Utility.Random(artifactitemSelect)]);
						break;
					}
					else
					{
						goto case 4:
					}
				}
				case 4:
				{
					for( int i = 0; i < Artifact_4Tier.Length; ++i )
					{
						if( Artifact_Search( item.GetType().BaseType, Artifact_4Tier[i].GetType().BaseType.BaseType )
						{
							artifactitemSelect.Add( Artifact_4Tier[i] );
						}
					}
					if( artifactitemSelect.Count > 0 )
					{
						artifactitem = (Item)Activator.CreateInstance(artifactitemSelect[Utility.Random(artifactitemSelect)]);
						break;
					}
					else
					{
						goto case 3:
					}
				}
				case 3:
				{
					for( int i = 0; i < Artifact_3Tier.Length; ++i )
					{
						if( Artifact_Search( item.GetType().BaseType, Artifact_3Tier[i].GetType().BaseType.BaseType )
						{
							artifactitemSelect.Add( Artifact_3Tier[i] );
						}
					}
					if( artifactitemSelect.Count > 0 )
					{
						artifactitem = (Item)Activator.CreateInstance(artifactitemSelect[Utility.Random(artifactitemSelect)]);
						break;
					}
					else
					{
						goto case 2:
					}
				}
				case 2:
				{
					for( int i = 0; i < Artifact_2Tier.Length; ++i )
					{
						if( Artifact_Search( item.GetType().BaseType, Artifact_2Tier[i].GetType().BaseType.BaseType )
						{
							artifactitemSelect.Add( Artifact_2Tier[i] );
						}
					}
					if( artifactitemSelect.Count > 0 )
					{
						artifactitem = (Item)Activator.CreateInstance(artifactitemSelect[Utility.Random(artifactitemSelect)]);
						break;
					}
					else
					{
						goto case 1:
					}
				}
				*/

				case 5:
					goto case 4;
				case 4:
					goto case 3;
				case 3:
					goto case 2;
				case 2:
					goto case 1;
				case 1:
				{
					for( int i = 0; i < Artifact_1Tier.Length; ++i )
					{
						if( item.GetType() == Artifact_1Tier[i].BaseType )
						{
							artifactitemSelect.Add( Artifact_1Tier[i] );
						}
					}
					if( artifactitemSelect.Count > 0 )
					{
						artifactitem = (Item)Activator.CreateInstance(artifactitemSelect[Utility.Random(artifactitemSelect.Count)]);
					}
					break;
				}
				
			}
			return artifactitem;
		}
		
		public static Type[] Artifact_1Tier = 
		{
			typeof( AdventurersMachete ), typeof( SilverEtchedMace ), typeof( Luckblade ), typeof( RubyMace ), typeof( TrueSpellblade ), typeof( EmeraldMace ), typeof( ArcanistsWildStaff ), typeof( AncientWildStaff ), typeof( IcySpellblade ), 
			typeof( FierySpellblade ), typeof( SpellbladeOfDefense ), typeof( TrueAssassinSpike ), typeof( ChargedAssassinSpike ), typeof( MagekillerAssassinSpike ), typeof( MagekillerLeafblade ), typeof( TrueLeafblade ), typeof( WoundingAssassinSpike ), typeof( LeafbladeOfEase ), typeof( ButchersWarCleaver ), 
			typeof( KnightsWarCleaver ), typeof( OrcishMachete ), typeof( SerratedWarCleaver ), typeof( TrueWarCleaver ), typeof( DiseasedMachete ), typeof( MacheteOfDefense ), typeof( MagesRuneBlade ), typeof( RuneBladeOfKnowledge ), typeof( Runesabre ), typeof( OrcishBow ), 
			typeof( DemonForks ), typeof( DragonNunchaku ), typeof( PeasantsBokuto ), typeof( PilferedDancerFans ), typeof( TomeOfEnlightenment ), typeof( TheDestroyer ), typeof( HanzosBow ), typeof( Exiler ), typeof( HailstormHuman ), typeof( AssassinsShortbow ), 
			typeof( AxeOfAbandon ), typeof( AxesOfFury ), typeof( BarbedLongbow ), typeof( BladeOfBattle ), typeof( CorruptedRuneBlade ), typeof( DarkglowScimitar ), typeof( EternalGuardianStaff ), typeof( HolySword ), typeof( IcyScimitar ), typeof( JadeWarAxe ), 
			typeof( LongbowOfMight ), typeof( MysticalShortbow ), typeof( PhantomStaff ), typeof( RangersShortbow ), typeof( SlayerLongbow ), typeof( ResonantStaffofEnlightenment ), typeof( RunedDriftwoodBow ), typeof( SingingAxe ), typeof( WindOfCorruption )
		};
				
		
		#endregion

		#region 장비 전역 변수
		
		//2.1 버전 랜덤 옵션
        public static readonly int[,] EquipRandomOption = new int[,]
		{
			//	이름,	Score, 		Max
            { 1080578,   100,    20000},    //0 힘 증가
            { 1080579,   100,    20000},    //1 민첩 증가
            { 1080580,   100,    20000},    //2 지능 증가
            { 1080581, 10000,  2000000},    //3 운 증가
            { 1080582, 10000,  4000000},    //4 체력 증가
            { 1080583, 10000,  4000000},    //5 기력 증가
            { 1080584, 10000,  4000000},    //6 마나 증가
            { 1080585,    10,   250000},    //7 무기 피해%
            { 1080586,    10,   250000},    //8 주문 피해%
            { 1080587,     0,        0},    //9 관통 피해 증가%
            { 1080588,     0,        0},    //10 충격 피해 증가%
            { 1080589,     0,        0},    //11 출혈 피해 증가%
            { 1080590, 10000,   200000},    //12 물리 저항
            { 1080591, 10000,   200000},    //13 화염 저항
            { 1080592, 10000,   200000},    //14 냉기 저항
            { 1080593, 10000,   200000},    //15 독 저항
            { 1080594, 10000,   200000},    //16 에너지 저항력
            { 1080595,    10,   250000},    //17 명중률 증가
            { 1080596,    10,   250000},    //18 방어율 증가
            { 1080597,   100,   100000},    //19 체력 회복
            { 1080598,   100,   100000},    //20 기력 회복
            { 1080599,   100,   100000},    //21 마나 회복
            { 1080600,     0,        0},    //22 물리 피해 증가%
            { 1080601,     0,        0},    //23 불 피해 증가%
            { 1080602,     0,        0},    //24 냉기 피해 증가%
            { 1080603,     0,        0},    //25 독 피해 증가%
            { 1080604,     0,        0},    //26 에너지 피해 증가%
            { 1080605,     0,        0},    //27 광역 물리 데미지 증가%
            { 1080606,     0,        0},    //28 광역 화염 데미지 증가%
            { 1080607,     0,        0},    //29 광역 냉기 데미지 증가%
            { 1080608,     0,        0},    //30 광역 독 데미지 증가%
            { 1080609,     0,        0},    //31 광역 에너지 데미지 증가%
            { 1080610, 10000,  2500000},    //32 최종 물리 피해 증가
            { 1080611, 10000,  2500000},    //33 최종 불 피해 증가
            { 1080612, 10000,  2500000},    //34 최종 냉기 피해 증가
            { 1080613, 10000,  2500000},    //35 최종 독 피해 증가
            { 1080614, 10000,  2500000},    //36 최종 에너지 피해 증가
            { 1080615,    10,   100000},    //37 체력 흡수%
            { 1080616,    10,   100000},    //38 기력 흡수%
            { 1080617,    10,   100000},    //39 마나 흡수%
            { 1080618,    10,   500000},    //40 공격 속도 증가%
            { 1080619,    10,   500000},    //41 시전 속도 증가%
            { 1080620,    10,   100000},    //42 물리 치명타 확률 증가%
            { 1080621,    10,   100000},    //43 마법 치명타 확률 증가%
            { 1080622,    10,   200000},    //44 물리 치명타 피해 증가%
            { 1080623,    10,   200000},    //45 마법 치명타 피해 증가%
            { 1080624,    10,  1000000},    //46 치유량 증가%
            { 1080625, 10000,  5000000},    //47 치유량 증가
            { 1080626,     0,        0},    //48 관통 피해 증가
            { 1080627,     0,        0},    //49 충격 피해 증가
            { 1080628,     0,        0},    //50 출혈 피해 증가
            { 1080629,    10,   100000},    //51 금화 획득 증가%
            { 1080630,    10,   750000},    //52 마법 화살 공격%
            { 1080631,    10,   500000},    //53 체력 손상 공격%
            { 1080632,    10,   300000},    //54 화염구 공격%
            { 1080633,    10,   200000},    //55 번개 공격%
            { 1080634,    10,   250000},    //56 영장류 피해 증가%
            { 1080635,    10,   250000},    //57 언데드 피해 증가%
            { 1080636,    10,   250000},    //58 정령 피해량 증가%
            { 1080637,    10,   250000},    //59 곤충 피해 증가%
            { 1080638,    10,   250000},    //60 파충류 피해 증가%
            { 1080639,    10,   250000},    //61 악마 피해량 증가%
            { 1080640,    10,   250000},    //62 요정 피해량 증가%
            {       1,  1000,   200000},    //63 해부학 스킬 증가%
            {       2,  1000,   200000},    //64 동물지식 스킬 증가%
            {       5,  1000,   200000},    //65 방패술 스킬 증가%
            {       9,  1000,   200000},    //66 평화연주 스킬 증가%
            {      14,  1000,   200000},    //67 은신감지 스킬 증가%
            {      15,  1000,   200000},    //68 불협화음 스킬 증가%
            {      16,  1000,   200000},    //69 지능평가 스킬 증가%
            {      17,  1000,   200000},    //70 회복술 스킬 증가%
            {      19,  1000,   200000},    //71 법의학 스킬 증가%
            {      20,  1000,   200000},    //72 목동술 스킬 증가%
            {      21,  1000,   200000},    //73 은신 스킬 증가%
            {      22,  1000,   200000},    //74 도발연주 스킬 증가%
            {      25,  1000,   200000},    //75 마법학 스킬 증가%
            {      26,  1000,   200000},    //76 마법저항 스킬 증가%
            {      27,  1000,   200000},    //77 전술 스킬 증가%
            {      28,  1000,   200000},    //78 훔쳐보기 스킬 증가%
            {      29,  1000,   200000},    //79 음악연주 스킬 증가%
            {      30,  1000,   200000},    //80 포이즈닝 스킬 증가%
            {      31,  1000,   200000},    //81 궁술 스킬 증가%
            {      32,  1000,   200000},    //82 영혼대화 스킬 증가%
            {      33,  1000,   200000},    //83 훔치기 스킬 증가%
            {      35,  1000,   200000},    //84 길들이기 스킬 증가%
            {      38,  1000,   200000},    //85 추적하기 스킬 증가%
            {      39,  1000,   200000},    //86 수의학 스킬 증가%
            {      40,  1000,   200000},    //87 검술 스킬 증가%
            {      41,  1000,   200000},    //88 둔기술 스킬 증가%
            {      42,  1000,   200000},    //89 펜싱 스킬 증가%
            {      46,  1000,   200000},    //90 명상 스킬 증가%
            {      47,  1000,   200000},    //91 은신이동 스킬 증가%
            {      49,  1000,   200000},    //92 강령술 스킬 증가%
            {      50,  1000,   200000},    //93 집중 스킬 증가%
            {      51,  1000,   200000},    //94 기사도 스킬 증가%
            {      52,  1000,   200000},    //95 무사도 스킬 증가%
            {      53,  1000,   200000},    //96 닌자술 스킬 증가%
            {      54,  1000,   200000},    //97 주문조합 스킬 증가%
            {      55,  1000,   200000},    //98 신비술 스킬 증가%
            {      57,  1000,   200000},    //99 던지기 스킬 증가%
            { 1080651,    10,   500000},    //100 무기 공격 반사%
            { 1080652,     0,        0},    //101 전투 경험치%
            { 1080653,     0,        0},    //102 혼돈 피해%
            { 1080654,     0,        0},    //103 신성 피해%
            { 1080655,     0,        0},    //104 방어력
            { 1080656,     0,        0},    //105 마법 방어력
            { 1080657,     0,        0},    //106 기절 시간 감소
            { 1080658,     0,        0},    //107 혼돈 피해
            { 1080659,     0,        0},    //108 신성 피해
            { 1080660,     0,        0},    //109 방패 방어 확률
            { 1080661,     0,        0},    //110 전체 피격 감소
            { 1080662,     0,        0},    //111 어그로%
            { 1080663,     0,        0},    //112 어그로
            { 1080664,     0,        0},    //113 원소 저항력%
            { 1080665,     0,        0},    //114 모든 저항력%
            { 1080666,  1000,    50000},    //115 기력 소모 감소%
            { 1080667,     0,        0},    //116 시전 실패 감소%
            { 1080668,     0,        0},    //117 모든 피해%
            { 1080669,     0,        0},    //118 모든 속도%
            { 1080670,  1000,    50000},    //119 마나 소모 감소%
            { 1080671,     0,        0},    //120 장비 요구치 감소%
            { 1080672,     0,        0},    //121 무기 피해
            { 1080673,     0,        0},    //122 마법 피해
            { 1080674,     0,        0},    //123 모든 피해
            { 1080675,     0,        0},    //124 피격 시 물리 치명 확률 감소
            { 1080676,     0,        0},    //125 피격 시 물리 치명 피해 감소
            { 1080677,     0,        0},    //126 피격 시 마법 치명 확률 감소
            { 1080678,     0,        0},    //127 피격 시 마법 치명 피해 감소
            { 1080679,     0,        0},    //128 붕대 사용 시 독 회복 확률%
            { 1080680,     0,        0},    //129 독 저항성%
            { 1080681,     0,        0},    //130 독 저항성
            { 1080682,     0,        0},    //131 함정 회피%
            { 1080683,     0,        0},    //132 모든 특수기 증가
            { 1080684,     0,        0},    //133 첫 번째 특수기 증가
            { 1080685,     0,        0},    //134 두 번째 특수기 증가
            { 1080686,     0,        0},    //135 검 특수기 증가
            { 1080687,     0,        0},    //136 둔기 특수기 증가
            { 1080688,     0,        0},    //137 펜싱 특수기 증가
            { 1080689,     0,        0},    //138 활&석궁 특수기 증가
            { 1080690,     0,        0},    //139 맨손 특수기 증가
            { 1080691,     0,        0},    //140 모든 마법 스킬 증가
            { 1080692,     0,        0},    //141 1써클 마법 스킬 증가
            { 1080693,     0,        0},    //142 2써클 마법 스킬 증가
            { 1080694,     0,        0},    //143 3써클 마법 스킬 증가
            { 1080695,     0,        0},    //144 4써클 마법 스킬 증가
            { 1080696,     0,        0},    //145 5써클 마법 스킬 증가
            { 1080697,     0,        0},    //146 6써클 마법 스킬 증가
            { 1080698,     0,        0},    //147 7써클 마법 스킬 증가
            { 1080699,     0,        0},    //148 8써클 마법 스킬 증가
            { 1080700,     0,        0},    //149 강령술 마법 스킬 증가
            { 1080701,     0,        0},    //150 원소술 마법 스킬 증가
            { 1080702,     0,        0},    //151 신비술 마법 스킬 증가
            { 1080703,     0,        0},    //152 기사도 마법 스킬 증가
            { 1080704,     0,        0},    //153 화염 속성
            { 1080705,     0,        0},    //154 냉기 속성
            { 1080706,     0,        0},    //155 독 속성
            { 1080707,     0,        0},    //156 에너지 속성
            { 1080708,     0,        0},    //157 혼돈 속성
            { 1080709,     0,        0}     //158 신성 속성
		};
				
		//2.1 버전 고정 옵션
		public static readonly int[,] EquipStaticOption = new int[,]
		{
			{ 0, 10000, 20000, 30000, 40000, 50000}, 				//무기
			{ 0, 400000, 800000, 1200000, 1600000, 2000000},		//방어구
			{ 0, 600000, 1200000, 1800000, 2400000, 3000000},		//전투 장신구
			{ 0, 600000, 1200000, 1800000, 2400000, 3000000}		//마법 장신구
		};

		//장비별 옵션 선택
		private static readonly int[][] EquipOptionType = new int[][]
		{
			new int[] {0, 1, 2, 3, 4, 5, 6, 7, 8, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 51, 52, 53, 55, 56, 57, 58, 59, 60, 61, 62, 63, 65, 66, 67, 68, 69, 70, 71, 73, 74, 75, 76, 77, 78, 79, 80, 82, 83, 85, 87, 90, 91, 92, 93, 94, 96, 97, 98, 100, 101, 115, 119, 132, 133, 134, 135, 140, 141, 142, 143, 144, 145, 146, 147, 148, 149, 150, 151, 152 }, //한손검
			new int[] {0, 1, 2, 3, 4, 5, 6, 7, 8, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 51, 53, 54, 56, 57, 58, 59, 60, 61, 62, 63, 67, 69, 70, 71, 75, 76, 77, 82, 87, 90, 92, 93, 94, 95, 97, 98, 100, 101, 115, 119, 132, 133, 134, 135, 140, 141, 142, 143, 144, 145, 146, 147, 148, 149, 150, 151, 152 }, //양손검
			new int[] {0, 1, 2, 3, 4, 5, 6, 7, 12, 13, 14, 15, 16, 17, 18, 19, 20, 32, 33, 34, 35, 36, 37, 38, 40, 42, 44, 51, 53, 55, 56, 57, 58, 59, 60, 61, 62, 63, 70, 73, 76, 77, 78, 80, 82, 83, 87, 90, 91, 92, 93, 95, 100, 101, 115, 132, 133, 134, 135, 149 }, //도끼
			new int[] {0, 1, 2, 3, 4, 5, 6, 7, 8, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 51, 52, 53, 54, 55, 56, 57, 58, 59, 60, 61, 62, 63, 65, 66, 67, 68, 69, 70, 71, 74, 75, 76, 77, 79, 82, 85, 88, 90, 92, 93, 94, 96, 97, 98, 100, 101, 115, 119, 132, 133, 134, 136, 140, 141, 142, 143, 144, 145, 146, 147, 148, 149, 150, 151, 152 }, //한손 둔기
			new int[] {0, 1, 2, 3, 4, 5, 6, 7, 8, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 51, 53, 54, 56, 57, 58, 59, 60, 61, 62, 63, 64, 66, 67, 68, 69, 70, 71, 74, 75, 76, 77, 79, 82, 84, 86, 88, 90, 92, 93, 94, 95, 97, 98, 100, 101, 115, 119, 132, 133, 134, 136, 140, 141, 142, 143, 144, 145, 146, 147, 148, 149, 150, 151, 152 }, //양손 둔기
			new int[] {0, 1, 2, 3, 4, 5, 6, 7, 12, 13, 14, 15, 16, 17, 18, 19, 20, 32, 33, 34, 35, 36, 37, 38, 40, 42, 44, 51, 52, 53, 55, 56, 57, 58, 59, 60, 61, 62, 63, 65, 67, 70, 73, 76, 77, 78, 80, 82, 83, 85, 89, 90, 91, 92, 93, 94, 96, 100, 101, 115, 132, 133, 134, 137, 149, 152 }, //한손 펜싱
			new int[] {0, 1, 2, 3, 4, 5, 6, 7, 12, 13, 14, 15, 16, 17, 18, 19, 20, 32, 33, 34, 35, 36, 37, 38, 40, 42, 44, 51, 53, 55, 56, 57, 58, 59, 60, 61, 62, 63, 69, 70, 73, 75, 76, 77, 78, 80, 82, 83, 89, 90, 91, 92, 93, 95, 97, 98, 100, 101, 115, 119, 132, 133, 134, 137, 140, 141, 142, 143, 144, 145, 146, 147, 148, 149, 150, 151, 152 }, //양손 펜싱
			new int[] {0, 1, 2, 3, 4, 5, 6, 7, 8, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 51, 52, 53, 54, 55, 56, 57, 58, 59, 60, 61, 62, 63, 64, 67, 69, 73, 75, 76, 77, 78, 80, 81, 83, 84, 86, 90, 91, 93, 95, 97, 98, 101, 115, 119, 132, 133, 134, 138, 140, 141, 142, 143, 144, 145, 146, 147, 148, 149, 150, 151, 152 }, //활
			new int[] {0, 1, 2, 3, 4, 5, 6, 7, 12, 13, 14, 15, 16, 17, 18, 19, 20, 32, 33, 34, 35, 36, 37, 38, 40, 42, 44, 46, 47, 51, 52, 53, 54, 55, 56, 57, 58, 59, 60, 61, 62, 63, 64, 67, 76, 77, 81, 84, 86, 93, 95, 101, 115, 132, 133, 134, 138 }, //석궁
			new int[] {0, 1, 2, 3, 4, 5, 6, 8, 13, 14, 15, 16, 18, 21, 32, 33, 34, 35, 36, 39, 41, 43, 45, 46, 47, 51, 52, 53, 54, 55, 56, 57, 58, 59, 60, 61, 62, 64, 65, 66, 68, 69, 74, 75, 76, 79, 84, 86, 90, 96, 97, 98, 101, 119, 132, 133, 134, 140, 141, 142, 143, 144, 145, 146, 147, 148, 149, 150, 151, 152 }, //마법책(9)
			new int[] {0, 1, 2, 3, 4, 5, 6, 12, 13, 14, 15, 16, 18, 19, 20, 21, 46, 47, 51, 63, 64, 65, 66, 68, 69, 70, 71, 74, 75, 76, 77, 79, 82, 83, 84, 86, 87, 88, 89, 90, 92, 93, 94, 97, 98, 100, 101, 115, 119, 139, 141, 142, 143, 144, 149, 150, 151, 152 }, //방패
			new int[] {0, 1, 2, 3, 4, 5, 6, 8, 12, 13, 14, 15, 16, 18, 19, 20, 21, 33, 34, 35, 36, 39, 41, 43, 45, 46, 47, 51, 56, 57, 58, 59, 60, 61, 62, 63, 64, 66, 67, 68, 69, 70, 71, 73, 74, 75, 76, 77, 78, 79, 80, 81, 82, 83, 84, 85, 86, 87, 88, 89, 90, 91, 92, 93, 94, 96, 97, 98, 99, 100, 101, 115, 119, 135, 136, 137, 138, 139, 141, 142, 143, 144, 149, 150, 151, 152 }, //천 옷
			new int[] {0, 1, 2, 3, 4, 5, 6, 7, 8, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 32, 33, 34, 35, 36, 37, 38, 40, 41, 42, 43, 44, 45, 46, 47, 51, 56, 57, 58, 59, 60, 61, 62, 63, 64, 65, 66, 67, 68, 69, 70, 71, 73, 74, 75, 76, 77, 78, 79, 80, 81, 82, 83, 85, 86, 87, 88, 89, 90, 91, 92, 93, 94, 95, 96, 97, 98, 99, 100, 101, 115, 135, 136, 137, 138, 139, 149, 150, 151, 152 }, //가죽 갑옷
			new int[] {0, 1, 2, 3, 4, 5, 6, 7, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 32, 37, 38, 40, 42, 44, 51, 56, 57, 58, 59, 60, 61, 62, 63, 64, 65, 70, 71, 76, 77, 81, 86, 87, 88, 89, 93, 94, 95, 96, 99, 100, 101, 115, 119, 135, 136, 137, 138, 139, 141, 142, 143, 144, 149, 150, 151, 152 }, //스텃 갑옷
			new int[] {0, 1, 2, 3, 4, 5, 6, 7, 8, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 51, 56, 57, 58, 59, 60, 61, 62, 63, 64, 65, 66, 67, 68, 69, 70, 71, 73, 74, 75, 76, 77, 78, 79, 80, 81, 82, 83, 84, 85, 86, 87, 88, 89, 90, 91, 92, 93, 94, 95, 96, 97, 98, 99, 100, 101, 115, 119, 135, 136, 137, 138, 139, 141, 142, 143, 144, 149, 150, 151, 152 }, //뼈 갑옷
			new int[] {0, 1, 2, 3, 4, 5, 6, 7, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 32, 38, 40, 42, 44, 51, 56, 57, 58, 59, 60, 61, 62, 63, 65, 70, 76, 77, 81, 82, 87, 88, 89, 92, 93, 94, 95, 99, 100, 101, 115, 135, 136, 137, 138, 139, 149, 150, 151, 152 }, //링 갑옷(투구)
			new int[] {0, 1, 2, 3, 4, 5, 6, 7, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 32, 37, 38, 40, 42, 44, 51, 56, 57, 58, 59, 60, 61, 62, 63, 65, 70, 76, 77, 82, 87, 88, 89, 92, 93, 94, 95, 100, 101, 115, 119, 135, 136, 137, 138, 139, 141, 142, 143, 144, 149, 150, 151, 152 }, //체인 갑옷
			new int[] {0, 1, 2, 3, 4, 5, 6, 7, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 32, 37, 39, 40, 42, 44, 46, 47, 51, 56, 57, 58, 59, 60, 61, 62, 63, 65, 70, 76, 77, 82, 87, 88, 89, 90, 92, 93, 94, 95, 101, 115, 119, 135, 136, 137, 138, 141, 142, 143, 144, 149, 150, 151, 152 }, //플레이트 갑옷
			new int[] {0, 1, 2, 3, 4, 5, 6, 8, 12, 13, 14, 15, 16, 18, 19, 20, 21, 33, 34, 35, 36, 37, 38, 39, 41, 43, 45, 46, 47, 51, 56, 57, 58, 59, 60, 61, 62, 64, 65, 66, 67, 68, 69, 70, 71, 74, 75, 76, 77, 79, 81, 83, 84, 86, 87, 88, 89, 90, 93, 94, 95, 97, 98, 99, 101, 115, 135, 136, 137, 138, 149, 150, 151, 152 }, //나무 갑옷(18)
			new int[] {0, 1, 2, 3, 4, 5, 6, 7, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 37, 39, 40, 42, 51, 56, 57, 58, 59, 60, 61, 62, 63, 64, 65, 66, 67, 68, 70, 71, 73, 74, 76, 77, 78, 79, 80, 81, 82, 83, 84, 85, 86, 87, 88, 89, 90, 91, 92, 93, 94, 95, 96, 97, 98, 99, 101, 115 }, //전투 팔찌
			new int[] {0, 1, 2, 3, 4, 5, 6, 7, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 32, 37, 38, 39, 40, 42, 44, 51, 56, 57, 58, 59, 60, 61, 62, 63, 64, 65, 66, 67, 68, 70, 71, 73, 74, 76, 77, 78, 79, 80, 81, 82, 83, 84, 85, 86, 87, 88, 89, 90, 91, 92, 93, 94, 95, 96, 97, 98, 99, 101, 115, 132, 133, 134, 135, 136, 137, 138, 139 }, //전투 반지
			new int[] {0, 1, 2, 3, 4, 5, 6, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 51, 56, 57, 58, 59, 60, 61, 62, 63, 64, 65, 66, 67, 68, 70, 71, 73, 74, 76, 77, 78, 79, 80, 81, 82, 83, 84, 85, 86, 87, 88, 89, 90, 91, 92, 93, 94, 95, 96, 97, 98, 99, 100, 101, 115 }, //전투 목걸이
			new int[] {0, 1, 2, 3, 4, 5, 6, 7, 17, 32, 37, 38, 40, 42, 44, 56, 57, 58, 59, 60, 61, 62, 63, 64, 65, 66, 67, 68, 70, 71, 73, 74, 76, 77, 78, 79, 80, 81, 82, 83, 84, 85, 86, 87, 88, 89, 90, 91, 92, 93, 94, 95, 96, 97, 98, 99, 100, 101, 115 }, //전투 귀걸이(22)
			new int[] {0, 1, 2, 3, 4, 5, 6, 8, 12, 13, 14, 15, 16, 18, 19, 21, 37, 39, 41, 43, 46, 47, 51, 56, 57, 58, 59, 60, 61, 62, 64, 66, 68, 69, 71, 74, 75, 76, 79, 80, 82, 84, 86, 90, 92, 94, 97, 98, 101, 119, 140, 141, 142, 143, 144, 145, 146 }, //마법 팔찌
			new int[] {0, 1, 2, 3, 4, 5, 6, 8, 12, 13, 14, 15, 16, 18, 19, 21, 33, 34, 35, 36, 37, 39, 41, 43, 45, 46, 47, 51, 56, 57, 58, 59, 60, 61, 62, 64, 66, 68, 69, 71, 74, 75, 76, 79, 80, 82, 84, 86, 90, 92, 94, 97, 98, 101, 119, 139, 140, 141, 142, 143, 144, 145, 146 }, //마법 반지
			new int[] {0, 1, 2, 3, 4, 5, 6, 8, 12, 13, 14, 15, 16, 19, 21, 46, 47, 51, 56, 57, 58, 59, 60, 61, 62, 64, 66, 68, 69, 71, 74, 75, 76, 79, 80, 82, 84, 86, 90, 92, 94, 97, 98, 101, 119, }, //마법 목걸이
			new int[] {0, 1, 2, 3, 4, 5, 6, 8, 18, 33, 34, 35, 36, 37, 41, 43, 45, 46, 47, 56, 57, 58, 59, 60, 61, 62, 64, 66, 68, 69, 71, 74, 75, 76, 79, 80, 82, 84, 86, 90, 92, 94, 97, 98, 101, 119, 140, }, //마법 귀걸이(26)
		};
		#endregion
		//아이템 옵션 붙이기
		public static int NewEquipOptionList( Item equip, int itemoption, int itemvalue, int skilluse )
		{
			if( equip is IEquipOption )
			{
				IEquipOption item = equip as IEquipOption;
				//옵션 지정 코드
				AosAttributes primary = item.Attributes;
				AosWeaponAttributes weapon = item.WeaponAttributes;
				SAAbsorptionAttributes absorp = item.AbsorptionAttributes;
				ExtendedWeaponAttributes exweapon = item.ExtendedWeaponAttributes;
				AosSkillBonuses skill = item.SkillBonuses;
				AosArmorAttributes armor = item.ArmorAttributes;
				switch( itemoption )
				{
					case 0: //힘 증가
					{
						primary.BonusStr += itemvalue;
						break;
					}
					case 1: //민첩 증가
					{
						primary.BonusDex += itemvalue;
						break;
					}
					case 2: //지능 증가
					{
						primary.BonusInt += itemvalue;
						break;
					}
					case 3: //운 증가
					{
						primary.Luck += itemvalue;
						break;
					}
					case 4: //체력 증가
					{
						primary.BonusHits += itemvalue;
						break;
					}
					case 5: //기력 증가
					{
						primary.BonusStam += itemvalue;
						break;
					}
					case 6: //마나 증가
					{
						primary.BonusMana += itemvalue;
						break;
					}
					case 7: //물리 피해 증가%
					{
						primary.WeaponDamage += itemvalue;
						break;
					}
					case 8: //주문 피해 증가%
					{
						primary.SpellDamage += itemvalue;
						break;
					}
					case 9: //관통 피해 증가%
					{
						absorp.ResonancePierce += itemvalue;
						break;
					}
					case 10: //충격 피해 증가%
					{
						absorp.ResonanceKinetic += itemvalue;
						break;
					}
					case 11: //출혈 피해 증가%
					{
						absorp.ResonanceBleed += itemvalue;
						break;
					}
					case 12: //물리 저항%
					{
						weapon.ResistPhysicalBonus += itemvalue;
						break;
					}
					case 13: //화염 저항%
					{
						weapon.ResistFireBonus += itemvalue;
						break;
					}
					case 14: //냉기 저항%
					{
						weapon.ResistColdBonus += itemvalue;
						break;
					}
					case 15: //독 저항%
					{
						weapon.ResistPoisonBonus += itemvalue;
						break;
					}
					case 16: //에너지 저항%
					{
						weapon.ResistEnergyBonus += itemvalue;
						break;
					}
					case 17: //명중률 증가%
					{
						primary.AttackChance += itemvalue;
						break;
					}
					case 18: //방어율 증가%
					{
						primary.DefendChance += itemvalue;
						break;
					}
					case 19: //체력 회복
					{
						primary.RegenHits += itemvalue;
						break;
					}
					case 20: //기력 회복
					{
						primary.RegenStam += itemvalue;
						break;
					}
					case 21: //마나 회복
					{
						primary.RegenMana += itemvalue;
						break;
					}
					case 22: //물리 피해 증가%
					{
						primary.BalancedWeapon += itemvalue;
						break;
					}
					case 23: //화염 피해 증가%
					{
						absorp.ResonanceFire += itemvalue;
						break;
					}
					case 24: //냉기 피해 증가%
					{
						absorp.ResonanceCold += itemvalue;
						break;
					}
					case 25: //독 피해 증가%
					{
						absorp.ResonancePoison += itemvalue;
						break;
					}
					case 26: //에너지 피해 증가%
					{
						absorp.ResonanceEnergy += itemvalue;
						break;
					}
					case 27: //광역 물리 피해 증가%
					{
						weapon.HitPhysicalArea += itemvalue;
						break;
					}
					case 28: //광역 화염 피해 증가%
					{
						weapon.HitFireArea += itemvalue;
						break;
					}
					case 29: //광역 냉기 피해 증가%
					{
						weapon.HitColdArea += itemvalue;
						break;
					}
					case 30: //광역 독 피해 증가%
					{
						weapon.HitPoisonArea += itemvalue;
						break;
					}
					case 31: //광역 에너지 피해 증가%
					{
						weapon.HitEnergyArea += itemvalue;
						break;
					}
					case 32: //물리 피해 증가
					{
						absorp.EaterDamage += itemvalue;
						break;
					}
					case 33: //화염 피해 증가
					{
						absorp.EaterFire += itemvalue;
						break;
					}
					case 34: //냉기 피해 증가
					{
						absorp.EaterCold += itemvalue;
						break;
					}
					case 35: //독 피해 증가
					{
						absorp.EaterPoison += itemvalue;
						break;
					}
					case 36: //에너지 피해 증가
					{
						absorp.EaterEnergy += itemvalue;
						break;
					}
					case 37: //체력 흡수
					{
						weapon.HitLeechHits += itemvalue;
						break;
					}
					case 38: //기력 흡수
					{
						weapon.HitLeechStam += itemvalue;
						break;
					}
					case 39: //마나 흡수
					{
						weapon.HitLeechMana += itemvalue;
						break;
					}
					case 40: //공격 속도 증가
					{
						primary.WeaponSpeed += itemvalue;
						break;
					}
					case 41: // 시전 속도 증가
					{
						primary.CastSpeed += itemvalue;
						break;
					}
					case 42: //물리 치명타 확률 증가
					{
						primary.WeaponCritical += itemvalue;
						break;
					}
					case 43: //마법 치명타 확률 증가
					{
						primary.CastRecovery += itemvalue;
						break;
					}
					case 44: //물리 치명타 피해 증가
					{
						primary.Brittle += itemvalue;
						break;
					}
					case 45: //마법 치명타 피해 증가
					{
						primary.SpellChanneling += itemvalue;
						break;
					}
					case 46: //치유량 증가%
					{
						primary.EnhancePotions += itemvalue;
						break;
					}
					case 47: //치유량 증가
					{
						primary.HealBonus += itemvalue;
						break;
					}
					case 48: //관통 피해 증가
					{
						absorp.EaterPierce += itemvalue;
						break;
					}
					case 49: //충격 피해 증가
					{
						absorp.EaterKinetic += itemvalue;
						break;
					}
					case 50: //출혈 피해 증가
					{
						absorp.EaterBleed += itemvalue;
						break;
					}
					case 51: //금화 획득 증가%
					{
						primary.NightSight += itemvalue;
						break;
					}
					case 52: //화염 화살 공격%
					{
						weapon.HitMagicArrow += itemvalue;
						break;
					}
					case 53: //체력 손상 공격%
					{
						weapon.HitHarm += itemvalue;
						break;
					}
					case 54: //화염구 공격%
					{
						weapon.HitFireball += itemvalue;
						break;
					}
					case 55: //번개 공격%
					{
						weapon.HitLightning += itemvalue;
						break;
					}
					case 56: //영장류 피해 증가%
					{
						absorp.HumanoidDamage += itemvalue;
						break;
					}
					case 57: //언데드 피해 증가%
					{
						absorp.UndeadDamage += itemvalue;
						break;
					}
					case 58: //정령 피해 증가%
					{
						absorp.ElementalDamage += itemvalue;
						break;
					}
					case 59: //곤충 피해 증가%
					{
						absorp.ArachnidDamage += itemvalue;
						break;
					}
					case 60: //파충류 피해 증가%
					{
						absorp.ReptilianDamage += itemvalue;
						break;
					}
					case 61: //악마 피해 증가%
					{
						absorp.AbyssDamage += itemvalue;
						break;
					}
					case 62: //요정 피해 증가%
					{
						absorp.FeyDamage += itemvalue;
						break;
					}
					case 63: //해부학 스킬 증가%
					{
						skill.SetValues(skilluse, SkillName.Anatomy, (double)itemvalue * 0.0001 );
						skilluse++;
						break;
					}
					case 64: //동물지식 스킬 증가%
					{
						skill.SetValues(skilluse, SkillName.AnimalLore, (double)itemvalue * 0.0001 );
						skilluse++;
						break;
					}
					case 65: //방패술 스킬 증가%
					{
						skill.SetValues(skilluse, SkillName.Parry, (double)itemvalue * 0.0001 );
						skilluse++;
						break;
					}
					case 66: //평화연주 스킬 증가%
					{
						skill.SetValues(skilluse, SkillName.Peacemaking, (double)itemvalue * 0.0001 );
						skilluse++;
						break;
					}
					case 67: //은신감지 스킬 증가%
					{
						skill.SetValues(skilluse, SkillName.DetectHidden, (double)itemvalue * 0.0001 );
						skilluse++;
						break;
					}
					case 68: //불협화음 스킬 증가%
					{
						skill.SetValues(skilluse, SkillName.Discordance, (double)itemvalue * 0.0001 );
						skilluse++;
						break;
					}
					case 69: //지능평가 스킬 증가%
					{
						skill.SetValues(skilluse, SkillName.EvalInt, (double)itemvalue * 0.0001 );
						skilluse++;
						break;
					}
					case 70: //회복술 스킬 증가%
					{
						skill.SetValues(skilluse, SkillName.Healing, (double)itemvalue * 0.0001 );
						skilluse++;
						break;
					}
					case 71: //법의학 스킬 증가%
					{
						skill.SetValues(skilluse, SkillName.Forensics, (double)itemvalue * 0.0001 );
						skilluse++;
						break;
					}
					case 72: //목동술 스킬 증가%
					{
						skill.SetValues(skilluse, SkillName.Herding, (double)itemvalue * 0.0001 );
						skilluse++;
						break;
					}
					case 73: //은신 스킬 증가%
					{
						skill.SetValues(skilluse, SkillName.Hiding, (double)itemvalue * 0.0001 );
						skilluse++;
						break;
					}
					case 74: //도발연주 스킬 증가%
					{
						skill.SetValues(skilluse, SkillName.Provocation, (double)itemvalue * 0.0001 );
						skilluse++;
						break;
					}
					case 75: //마법학 스킬 증가%
					{
						skill.SetValues(skilluse, SkillName.Magery, (double)itemvalue * 0.0001 );
						skilluse++;
						break;
					}
					case 76: //마법저항 스킬 증가%
					{
						skill.SetValues(skilluse, SkillName.MagicResist, (double)itemvalue * 0.0001 );
						skilluse++;
						break;
					}
					case 77: //전술 스킬 증가%
					{
						skill.SetValues(skilluse, SkillName.Tactics, (double)itemvalue * 0.0001 );
						skilluse++;
						break;
					}
					case 78: //훔쳐보기 스킬 증가%
					{
						skill.SetValues(skilluse, SkillName.Snooping, (double)itemvalue * 0.0001 );
						skilluse++;
						break;
					}
					case 79: //음악연주 스킬 증가%
					{
						skill.SetValues(skilluse, SkillName.Musicianship, (double)itemvalue * 0.0001 );
						skilluse++;
						break;
					}
					case 80: //포이즈닝 스킬 증가%
					{
						skill.SetValues(skilluse, SkillName.Poisoning, (double)itemvalue * 0.0001 );
						skilluse++;
						break;
					}
					case 81: //궁술 스킬 증가%
					{
						skill.SetValues(skilluse, SkillName.Archery, (double)itemvalue * 0.0001 );
						skilluse++;
						break;
					}
					case 82: //영혼대화 스킬 증가%
					{
						skill.SetValues(skilluse, SkillName.SpiritSpeak, (double)itemvalue * 0.0001 );
						skilluse++;
						break;
					}
					case 83: //훔치기 스킬 증가%
					{
						skill.SetValues(skilluse, SkillName.Stealing, (double)itemvalue * 0.0001 );
						skilluse++;
						break;
					}
					case 84: //길들이기 스킬 증가%
					{
						skill.SetValues(skilluse, SkillName.AnimalTaming, (double)itemvalue * 0.0001 );
						skilluse++;
						break;
					}
					case 85: //반사신경 스킬 증가%
					{
						skill.SetValues(skilluse, SkillName.Tracking, (double)itemvalue * 0.0001 );
						skilluse++;
						break;
					}
					case 86: //수의학 스킬 증가%
					{
						skill.SetValues(skilluse, SkillName.Veterinary, (double)itemvalue * 0.0001 );
						skilluse++;
						break;
					}
					case 87: //검술 스킬 증가%
					{
						skill.SetValues(skilluse, SkillName.Swords, (double)itemvalue * 0.0001 );
						skilluse++;
						break;
					}
					case 88: //둔기술 스킬 증가%
					{
						skill.SetValues(skilluse, SkillName.Macing, (double)itemvalue * 0.0001 );
						skilluse++;
						break;
					}
					case 89: //펜싱 스킬 증가%
					{
						skill.SetValues(skilluse, SkillName.Fencing, (double)itemvalue * 0.0001 );
						skilluse++;
						break;
					}
					case 90: //명상 스킬 증가%
					{
						skill.SetValues(skilluse, SkillName.Meditation, (double)itemvalue * 0.0001 );
						skilluse++;
						break;
					}
					case 91: //은신이동 스킬 증가%
					{
						skill.SetValues(skilluse, SkillName.Stealth, (double)itemvalue * 0.0001 );
						skilluse++;
						break;
					}
					case 92: //강령술 스킬 증가%
					{
						skill.SetValues(skilluse, SkillName.Necromancy, (double)itemvalue * 0.0001 );
						skilluse++;
						break;
					}
					case 93: //집중 스킬 증가%
					{
						skill.SetValues(skilluse, SkillName.Focus, (double)itemvalue * 0.0001 );
						skilluse++;
						break;
					}
					case 94: //기사도 스킬 증가%
					{
						skill.SetValues(skilluse, SkillName.Chivalry, (double)itemvalue * 0.0001 );
						skilluse++;
						break;
					}
					case 95: //무사도 스킬 증가%
					{
						skill.SetValues(skilluse, SkillName.Bushido, (double)itemvalue * 0.0001 );
						skilluse++;
						break;
					}
					case 96: //암술 스킬 증가%
					{
						skill.SetValues(skilluse, SkillName.Ninjitsu, (double)itemvalue * 0.0001 );
						skilluse++;
						break;
					}
					case 97: //주문조합 스킬 증가%
					{
						skill.SetValues(skilluse, SkillName.Spellweaving, (double)itemvalue * 0.0001 );
						skilluse++;
						break;
					}
					case 98: //신비술 스킬 증가%
					{
						skill.SetValues(skilluse, SkillName.Mysticism, (double)itemvalue * 0.0001 );
						skilluse++;
						break;
					}
					case 99: //던지기 스킬 증가%
					{
						skill.SetValues(skilluse, SkillName.Throwing, (double)itemvalue * 0.0001 );
						skilluse++;
						break;
					}
					case 100: //무기 공격 반사%
					{
						primary.ReflectPhysical += itemvalue;
						break;
					}
					case 101: //전투 경험치%
					{
						primary.LowerAmmoCost += itemvalue;
						break;
					}
					case 102: //혼돈 피해%
					{
						exweapon.ChaosDamage += itemvalue;
						break;
					}
					case 103: //신성 피해%
					{
						exweapon.DirectDamage += itemvalue;
						break;
					}
					case 104: //무기 데미지 감소
					{
						armor.WeaponDefense += itemvalue;
						break;
					}
					case 105: //마법 데미지 감소
					{
						armor.MagicDefense += itemvalue;
						break;
					}
					case 106: //기절 시간 감소
					{
						armor.StunDefense += itemvalue;
						break;
					}
					case 107: //혼돈 피해
					{
						exweapon.ChaosPlus += itemvalue;
						break;
					}
					case 108: //신성 피해
					{
						exweapon.DirectPlus += itemvalue;
						break;
					}
					case 109: //방어력
					{
						armor.ShieldRecovery += itemvalue;
						break;
					}
					case 110: //전체 피격 데미지 감소
					{
						armor.AllDefenseBonus += itemvalue;
						break;
					}
					case 111: //어그로%
					{
						exweapon.AggroPointBonus += itemvalue;
						break;
					}
					case 112: //어그로
					{
						exweapon.AggroPoint += itemvalue;
						break;
					}
					case 113: //원소 저항력%
					{
						armor.ElementalResist += itemvalue;
						break;
					}
					case 114: //모든 저항력%
					{
						armor.AllResist += itemvalue;
						break;
					}
					case 115: //기력 감소%
					{
						armor.DefenseStam += itemvalue;
						break;
					}
					case 116: //시전 실패 감소%
					{
						absorp.CastingFocus += itemvalue;
						break;
					}
					case 117: //모든 피해%
					{
						weapon.UseBestSkill += itemvalue;
						break;
					}
					case 118: //모든 속도%
					{
						weapon.MageWeapon += itemvalue;
						break;
					}
					case 119: //마나 소모 감소%
					{
						primary.LowerManaCost += itemvalue;
						break;
					}
					case 120: //장비 요구치 감소
					{
						weapon.LowerStatReq += itemvalue;
						break;
					}
					case 121: //무기 피해
					{
						exweapon.BaseWeaponDamage += itemvalue;
						break;
					}
					case 122: //스펠 피해
					{
						exweapon.BaseSpellDamage += itemvalue;
						break;
					}
					case 123: //전체 피해
					{
						exweapon.BaseAllDamage += itemvalue;
						break;
					}
					case 124: //피격 시 물리 치명 확률 감소
					{
						weapon.BloodDrinker += itemvalue;
						break;
					}
					case 125: //피격 시 물리 치명 피해 감소
					{
						weapon.BattleLust += itemvalue;
						break;
					}
					case 126: //피격 시 마법 치명 확률 감소
					{
						weapon.HitFatigue += itemvalue;
						break;
					}
					case 127: //피격 시 마법 치명 피해 감소
					{
						weapon.HitManaDrain += itemvalue;
						break;
					}
					case 128: //붕대 사용 시 독 회복 확률
					{
						exweapon.Focus += itemvalue;
						break;
					}
					case 129: //독 저항성%
					{
						exweapon.HitSwarm += itemvalue;
						break;
					}
					case 130: //독 저항성
					{
						exweapon.Bane += itemvalue;
						break;
					}
					case 131: //함정 회피
					{
						exweapon.HitSparks += itemvalue;
						break;
					}
					case 132: //모든 특수기
					{
						exweapon.SPMAllBonus += itemvalue;
						break;
					}
					case 133: //첫 번째 특수기
					{
						exweapon.SPMFirstBonus += itemvalue;
						break;
					}
					case 134: //두 번째 특수기
					{
						exweapon.SPMSecondBonus += itemvalue;
						break;
					}
					case 135: //검 특수기
					{
						exweapon.SPMSwordBonus += itemvalue;
						break;
					}
					case 136: //둔기 특수기
					{
						exweapon.SPMMaceBonus += itemvalue;
						break;
					}
					case 137: //펜싱 특수기
					{
						exweapon.SPMFancingBonus += itemvalue;
						break;
					}
					case 138: //활&석궁 특수기
					{
						exweapon.SPMBowBonus += itemvalue;
						break;
					}
					case 139: //맨손 특수기
					{
						exweapon.SPMWrestling += itemvalue;
						break;
					}
					case 140: //모든 스펠
					{
						armor.MagicAllBonus += itemvalue;
						break;
					}
					case 141: //1써클 스펠
					{
						armor.MagicOneCircleBonus += itemvalue;
						break;
					}
					case 142: //2써클 스펠
					{
						armor.MagicTwoCircleBonus += itemvalue;
						break;
					}
					case 143: //3써클 스펠
					{
						armor.MagicThreeCircleBonus += itemvalue;
						break;
					}
					case 144: //4써클 스펠
					{
						armor.MagicFourCircleBonus += itemvalue;
						break;
					}
					case 145: //5써클 스펠
					{
						armor.MagicFiveCircleBonus += itemvalue;
						break;
					}
					case 146: //6써클 스펠
					{
						armor.MagicSixCircleBonus += itemvalue;
						break;
					}
					case 147: //7써클 스펠
					{
						armor.MagicSevenCircleBonus += itemvalue;
						break;
					}
					case 148: //8써클 스펠
					{
						armor.MagicEightCircleBonus += itemvalue;
						break;
					}
					case 149: //강령술 스펠
					{
						armor.MagicNecromancyBonus += itemvalue;
						break;
					}
					case 150: //원소술 스펠
					{
						armor.MagicElementalismBonus += itemvalue;
						break;
					}
					case 151: //신비술 스펠
					{
						armor.MagicMysticismBonus += itemvalue;
						break;
					}
					case 152: //기사도 스펠
					{
						armor.MagicChivalryBonus += itemvalue;
						break;
					}
					case 153: //화염 속성
					{
						if( equip is BaseWeapon )
						{
							BaseWeapon elementalweapon = equip as BaseWeapon;
							elementalweapon.AosElementDamages.Fire += itemvalue;
						}
						else
							Console.WriteLine("error Fire 153");
						break;
					}
					case 154: //냉기 속성
					{
						if( equip is BaseWeapon )
						{
							BaseWeapon elementalweapon = equip as BaseWeapon;
							elementalweapon.AosElementDamages.Cold += itemvalue;
						}
						else
							Console.WriteLine("error Cold 154");
						break;
					}
					case 155: //독 속성
					{
						if( equip is BaseWeapon )
						{
							BaseWeapon elementalweapon = equip as BaseWeapon;
							elementalweapon.AosElementDamages.Poison += itemvalue;
						}
						else
							Console.WriteLine("error Poison 155");
						break;
					}
					case 156: //에너지 속성
					{
						if( equip is BaseWeapon )
						{
							BaseWeapon elementalweapon = equip as BaseWeapon;
							elementalweapon.AosElementDamages.Energy += itemvalue;
						}
						else
							Console.WriteLine("error Energy 156");
						break;
					}
					case 157: //혼돈 속성
					{
						if( equip is BaseWeapon )
						{
							BaseWeapon elementalweapon = equip as BaseWeapon;
							elementalweapon.AosElementDamages.Chaos += itemvalue;
						}
						else
							Console.WriteLine("error Chaos 157");
						break;
					}
					case 158: //신성 속성
					{
						if( equip is BaseWeapon )
						{
							BaseWeapon elementalweapon = equip as BaseWeapon;
							elementalweapon.AosElementDamages.Direct += itemvalue;
						}
						else
							Console.WriteLine("error Direct 158");
						break;
					}
				}
			}
			return skilluse;
		}
		

		#region 아이템 제작
		//아이템 생성기
		public static int ItemCreator( Item item, double chance, PlayerMobile pm = null)
		{
			int rank = 0;
			//등급 결정
			if( item is IEquipOption )
			{
				IEquipOption equip = item as IEquipOption;
				equip.SuffixOption[1] = ItemRank(chance);
				rank = equip.SuffixOption[1];
				equip.PrefixOption[0] = 1000;
				if( equip.Identified )
					equip.Identified = false;

				//아이템 선택
				if( equip.SuffixOption[1] > 0 )
				{
					//유물 결정. 테스트코드로 브론즈 체크해 둠
					if( equip.Resource == CraftResource.Bronze || Utility.RandomDouble() < 0.001 )
					{
						Item artifact = Artifact_Select(item, equip.SuffixOption[1]);
						if( artifact != null )
						{
							IEquipOption artifactSave = artifact as IEquipOption;
							artifactSave.MaxHitPoints = equip.MaxHitPoints;
							artifactSave.HitPoints = equip.HitPoints;
							artifactSave.PlayerConstructed = equip.PlayerConstructed;
							artifactSave.Hue = equip.Hue;
							artifactSave.Resource = equip.Resource;
							artifactSave.Crafter = equip.Crafter;

							artifactSave.ItemPower = (ItemPower)Enum.ToObject(typeof(ItemPower), equip.SuffixOption[1] + 3);

							EquipOptionCreate(artifact);
							artifactSave.Map = item.Map;
							artifactSave.Location = item.Location;
							
							Misc.Util.NewItemDrop(item, artifact, pm);							
							
							item.Delete();
							
							return rank;
						}
					}
					//일반 아이템인 경우 옵션 결정
					ItemOptionSelect(item);
					equip.ItemPower = (ItemPower)Enum.ToObject(typeof(ItemPower), equip.SuffixOption[1] + 3);
					EquipOptionCreate(item);
				}
			}
			return rank;
		}
		#endregion
		
		#region 아이템 옵션
		
		//등급 랜덤 옵션 값 지정
		/*
		희귀 : 비중 10 ~ 30
		서사 : 비중 35 ~ 45
		영웅 : 비중 50 ~ 60
		전설 : 비중 65 ~ 80
		신화 : 비중 85 ~ 100
		
		최하급 : 5
		하급 : 4
		일반 : 3
		상급 : 2
		최상급 1
		지정

		희귀
		최하급 : 10 ~ 14
		하급 : 14 ~ 18
		일반 : 18 ~ 22
		상급 : 22 ~ 26
		최상급 26 ~ 30
		
		
		*/
		
		//옵션 선택
		public static int[] RankValue = 
		{
			10, 35, 50, 65, 85, 30, 45, 60, 80, 100
		};
		
		public static int OptionValueSelect( int rank, int line )
		{
			if( EquipRandomOption[line, 1] == 0 )
			{
				Console.WriteLine("line is " + line + "zero error" );
				return 0;
			}
			int dice = Utility.RandomMinMax(1, 15);
			double grade = 0.20;
			if( dice == 15 )
				grade = 1.00;
			else if( dice >= 13 )
				grade = 0.50;
			else if( dice >= 10 )
				grade = 0.33;
			else if( dice >= 6 )
				grade = 0.20;

			grade = ( RankValue[rank + 4] - RankValue[rank - 1] ) * grade; //신화 기준 최상급 15, 상급 7.5
			grade = Utility.RandomDouble() * grade + RankValue[rank - 1];
			grade = EquipRandomOption[line, 2] * grade * 0.01;
		
			Console.WriteLine("grade : " + grade );
			int selectValue = (int)grade;
			
			Console.WriteLine("selectValue1 : " + selectValue );
			Console.WriteLine("line : " + line );
			Console.WriteLine("line1 : " + EquipRandomOption[line, 1]  );
			Console.WriteLine("line2 : " + EquipRandomOption[line, 2]  );
			if( selectValue % EquipRandomOption[line, 1] != 0 )
			{
				selectValue += EquipRandomOption[line, 1];
			}
			selectValue /= EquipRandomOption[line, 1];
			selectValue *= EquipRandomOption[line, 1];
			Console.WriteLine("selectValue2 : " + selectValue );
			if( selectValue > EquipRandomOption[line, 2] )
				selectValue = EquipRandomOption[line, 2];
			Console.WriteLine("selectValue3 : " + selectValue );
			return selectValue;
		}
		
		
		//등급 지정
		public static int ItemRank( double chance )
		{
			/*
			몬스터 명성 / 100당 옵션 기대치 1로 계산
			제작술 스킬 1당 옵션 기대치 1로 계산
			장비학 스킬 1당 옵션 기대치 0.2로 계산
			고급일 시 옵션 기대치 값 50 증가
			
			옵션 값 0 ~ 4 : 일반 등급
			옵션 값 5 ~ 19 : 50% 확률로 희귀 등급, 50% 확률로 일반 등급
			옵션 값 20 ~ 49 : 50% 확률로 서사 등급, 50% 확률로 희귀 등급
			옵션 값 50 ~ 99 : 20% 확률로 영웅 등급, 50% 확률로 서사 등급, 30% 확률로 희귀 등급
			옵션 값 100 ~ 249 : 10% 확률로 전설 등급, 30% 확률로 영웅 등급, 40% 확률로 서사 등급, 20% 확률로 희귀 등급
			옵션 값 250 ~ 299 : 5% 확률로 신화 등급, 15% 확률로 전설 등급, 50% 확률로 영웅 등급, 30% 확률로 서사 등급
			옵션 값 300+ : 10% 확률로 신화 등급, 40% 확률로 전설 등급, 50% 확률로 영웅 등급
			*/		
			int rank = 0;
			if( chance >= 5 )
			{
				double dice = Utility.RandomDouble();
				if( chance >= 300 )
				{
					if( dice > 0.9 )
						rank = 5;
					else if( dice > 0.5 )
						rank = 4;
					else
						rank = 3;
				}
				else if( chance >= 250 )
				{
					if( dice > 0.95 )
						rank = 5;
					else if( dice > 0.8 )
						rank = 4;
					else if( dice > 0.3 )
						rank = 3;
					else
						rank = 2;
				}					
				else if( chance >= 100 )
				{
					if( dice > 0.9 )
						rank = 4;
					else if( dice > 0.6 )
						rank = 3;
					else if( dice > 0.2 )
						rank = 2;
					else
						rank = 1;
				}
				else if( chance >= 50 )
				{
					if( dice > 0.8 )
						rank = 3;
					else if( dice > 0.3 )
						rank = 2;
					else
						rank = 1;
				}
				else if( chance >= 20 )
				{
					if( dice > 0.5 )
						rank = 2;
					else
						rank = 1;
				}
				else
				{
					if( dice > 0.5 )
						rank = 1;
				}
			}		
		
			return rank;
		}
		
		public static void ItemOptionSelect( Item item )
		{
			//접두 11 ~ 30 : 1 ~ 20 옵션 리스트.
			//접미 11 ~ 30 : 1 ~ 20 옵션 저장값.
			//접두 61 ~ 70 : 기본 옵션 리스트.
			//접미 61 ~ 70 : 기본 옵션 저장값.
			//접미 1 : 랭크 레벨
			//접두 9 : 랭크 옵션
			//접미 9 : 랭크 값
				
				
			if( item is IEquipOption )
			{
				IEquipOption equip = item as IEquipOption;
				
				//옵션 미리 결정
				int selectLine = -1;
				selectLine = NewEquipNumber(item);

				//아티펙트가 아닐 때
				if( equip.SuffixOption[0] == 0 )
				{
					equip.SuffixOption[0] = 1;
					if( equip is BaseWeapon )
						equip.SuffixOption[0] = 3;
					else if( equip is BaseArmor )
						equip.SuffixOption[0] = 2;
						
					if( selectLine > -1 )
					{
						int[] suffle = EquipOptionType[selectLine];
						suffle = suffle.OrderBy(x => Utility.Random(suffle.Length)).ToArray();
						for( int i = 0; i < equip.SuffixOption[0]; ++i)
						{
							equip.PrefixOption[11 + i] = suffle[i];	//옵션 선택
							equip.SuffixOption[11 + i] = OptionValueSelect(equip.SuffixOption[1], equip.PrefixOption[11 + i]);	//옵션 값
							//Console.WriteLine(i + "번째 옵션은 " + equip.PrefixOption[11 + i] + " 값은 " + equip.SuffixOption[11 + i]);
						}
					}
					else
						equip.SuffixOption[0] = 0; //안전 코드. 옵션을 체크하지 않음
				}
				
				//랭크 옵션 처리
				if( equip.SuffixOption[1] > 0 )
				{
					//무기
					if( selectLine >= 0 && selectLine <= 8 )
					{
						equip.PrefixOption[9] = 123;
						equip.SuffixOption[9] = EquipStaticOption[0, equip.SuffixOption[1]];
					}
					//방어구
					else if( selectLine >= 10 && selectLine <= 18 )
					{
						equip.PrefixOption[9] = 4;
						equip.SuffixOption[9] = EquipStaticOption[1, equip.SuffixOption[1]];
					}
					
					//전투 악세사리
					else if( selectLine >= 19 && selectLine <= 22 )
					{
						equip.PrefixOption[9] = 5;
						equip.SuffixOption[9] = EquipStaticOption[2, equip.SuffixOption[1]];
					
					}
					//마법 악세사리
					else if( selectLine >= 13 && selectLine <= 26 )
					{
						equip.PrefixOption[9] = 6;
						equip.SuffixOption[9] = EquipStaticOption[3, equip.SuffixOption[1]];
					}
				}
			}
		}
		
		//1080578부터 시작
		public static int NewEquipNumber(Item equip)
		{
			int check = -1;
			if( equip is IEquipOption )
			{
				IEquipOption item = equip as IEquipOption;
				if( item is BaseWeapon )
				{
					BaseWeapon newmake = item as BaseWeapon;
					check = WeaponList(newmake);
				}
				
				else if( item is BaseArmor )
				{
					BaseArmor newmake = item as BaseArmor;
					check = ArmorList(newmake);
				}
				else if( item is BaseClothing )
				{
					BaseClothing newmake = item as BaseClothing;
					if( !(newmake.Layer == Layer.Neck || newmake.Layer == Layer.Gloves || newmake.Layer == Layer.Arms || newmake.Layer == Layer.Helm || newmake.Layer == Layer.Pants || newmake.Layer == Layer.InnerTorso ) )
					{
						check = -1;
					}
					else
						check = 11;
				}
				else if( item is BaseJewel )
				{
					BaseJewel newmake = item as BaseJewel;
					check = JewelList(newmake);					

				}
				else if( item is Spellbook )
				{
					check = 9;
				}
			}

			return check;
			
		}		
		
		public static int WeaponList( BaseWeapon newmake )
		{
			int check = -1;
			if( newmake.Skill is SkillName.Swords )
			{
				if( newmake is BaseAxe )
					check = 2;
				else if( newmake.Layer == Layer.TwoHanded )
					check = 1;
				else if( newmake.Layer == Layer.OneHanded )
					check = 0;
			}
			else if( newmake.Skill is SkillName.Macing )
			{
				if( newmake.Layer == Layer.TwoHanded )
					check = 4;
				else if( newmake.Layer == Layer.OneHanded )
					check = 3;
			}
			else if( newmake.Skill is SkillName.Fencing )
			{
				if( newmake.Layer == Layer.TwoHanded )
					check = 6;
				else if( newmake.Layer == Layer.OneHanded )
					check = 5;
			}
			else if( newmake is BaseRanged )
			{
				if( ((BaseRanged)newmake).AmmoType == typeof(Bolt) )
					check = 8;
				else if( ((BaseRanged)newmake).AmmoType == typeof(Arrow) )
					check = 7;
			}
			return check;
		}
		
		public static int ArmorList( BaseArmor newmake )
		{
			/*
			아머타입 10 : 방패 //탱커
			아머타입 11 : 천옷 //법사, 힐러
			아머타입 12 : 가죽 //도적
			아머타입 13 : 스텃 //아쳐
			아머타입 14 : 뼈 //서포터
			아머타입 15 : 링, 투구, 스톤 //근접 딜러
			아머타입 16 : 체인, 스톤 //근접 딜러
			아머타입 17 : 플레이트 //탱커
			아머타입 18 : 나무 //서포터
			*/
			int check = 1 + (int)newmake.MaterialType;

			if (newmake is BaseShield)
				check = 0;
			else if (check >= 5 && check <= 7)
				check = 2;
			else if (newmake is Helmet || newmake is Bascinet || newmake is CloseHelm || newmake is NorseHelm)
				check = 5;
			else if (check == 8)
				check = 5;
			else if (check == 9 || check == 13)
				check = 6;
			else if (check == 10)
				check = 7;
			else if (check == 12)
				check = 8;
			check += 10;			
			
			return check;
		}
		
		
		public static int JewelList(BaseJewel newmake)
		{
			/*
			악세타입 19 : 팔찌 
			아머타입 20 : 반지 
			아머타입 21 : 목걸이 
			아머타입 22 : 귀걸이 
			*/
			int check = 19;

			if( newmake.Layer == Layer.Ring )
				check = 20;
			else if( newmake.Layer == Layer.Neck )
				check = 21;
			else if( newmake.Layer == Layer.Earrings )
				check = 22;
			
			if( newmake is SilverEarrings || newmake is SilverRing || newmake is SilverBracelet || newmake is SilverNecklace )
				check += 4;
			
			return check;
			
		}
		
		//코드 변경. 미리 옵션 수 만큼 체크해서 옵션 값 붙임
		public static void EquipOptionCreate( Item equip )
		{
			if( equip is IEquipOption )
			{
				IEquipOption item = equip as IEquipOption;
				int skilluse = 0;
				
				//신규 아이템 제작 코드
				/*
				아이템 접두, 접미 체크
				접두 : 아이템 기본 옵션(티어 등)
				접미 : 아이템 옵션 저장소
				
				접두 0 : 신규 아이템 체크 유무. 기본 값 10
				접미 0 : 옵션 갯수
				접두 1 : 아이템 세부 내구도(10000 => 내구도 1 하락)
				접미 1 : 랭크 레벨
				접두 2 : 숙련도
				접미 2 : 숙련도 최대치
				접두 3 : 재련 레벨	
				접미 3 : 1재련 값
				접두 4 : 2재련 값
				접미 4 : 3재련 값
				접두 5 : 4재련 값
				접미 5 : 5재련 값
				
				접두 9 : 랭크 옵션
				접미 9 : 랭크 값
				접두 10 : 강화 레벨
				접미 10 : 강화 값
				
				
				접두 11 ~ 30 : 1 ~ 20 옵션 리스트.
				접미 11 ~ 30 : 1 ~ 20 옵션 저장값.
				
				접두 31 ~ 40 : 1 ~ 10 재련 리스트
				접미 31 ~ 40 : 1 ~ 10 재련 저장값
				
				접두 41 : 목록 string 값
				접미 41 : 최대 옵션 값
				접두 42 ~ 45 : 1 ~ 4 재료 리스트
				접미 42 ~ 45 : 1 ~ 4 재료 저장값
				접두 50 : 세트 옵션 번호(1번 부터 시작)
				접미 50 : 세트 요구 수
				접두 51 ~ 60 : 세트 옵션 리스트
				접미 51 ~ 60 : 세트 옵션 저장값
				
				접두 61 ~ 70 : 기본 옵션 리스트.
				접미 61 ~ 70 : 기본 옵션 저장값.
				
				스킬 붙을 시 지정 칸 0 ~ 9
				0 ~ 4 : 랜덤 및 고정 옵션
				5 ~ 7 : 기본 옵션
				8 ~ 9 : 세트 옵션
				*/					
				
				//랜덤 옵션
				if( item.SuffixOption[0] > 0 )
				{
					for( int i = 0; i < item.SuffixOption[0]; ++i )
						skilluse = NewEquipOptionList( equip, item.PrefixOption[i + 11], item.SuffixOption[i + 11], skilluse);
				}
				
				/*
				#region 재료 옵션 사용 안함
				//접두 41 ~ 45, 접미 41 ~ 45 구현 코드
				int resourceuse = UseResourceNumber((int)item.Resource);
				Console.WriteLine("check code : {0} ", check);
				if( check >= 23 && check <= 26 )
				{
					resourceuse += 7;
				}

				item.PrefixOption[41] = NewResourceOption[resourceuse, equipLine, 0];
				for( int i = 1; i < 5; ++i )
				{
					item.PrefixOption[41 + i] = NewResourceOption[resourceuse, equipLine, ( i * 2) -1];
					item.SuffixOption[41 + i] = NewResourceOption[resourceuse, equipLine, i * 2];
					if( item.PrefixOption[41 + i] != - 1 )
					{
						skilluse = NewEquipOptionList( equip, item.PrefixOption[i + 41], item.SuffixOption[ i + 41], skilluse);
					}
				}				
				#endregion
				*/
				
				#region 기본 옵션
				//접두 61 ~ 70, 접미 61 ~ 70 구현 코드
				skilluse = 5;
				for( int i = 0; i < 10; ++i )
				{
					if( item.SuffixOption[61 + i] == 0 )
						break;
					skilluse = NewEquipOptionList( equip, item.PrefixOption[i + 61], item.SuffixOption[i + 61], skilluse);
				}
				#endregion
			}
		}
		#endregion
	}
	
}