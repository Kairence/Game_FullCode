using System;
using System.Collections.Generic;
using System.Linq;
using Server;
using Server.Items;
using Server.Mobiles;
using Server.Network;

namespace Server.Misc
{
    // 1. 모험가 파티의 현재 행동 상태
    public enum AdventurerState 
    { 
        Resting,    // 마을/유적에서 정비 및 휴식 중
        Traveling,  // 거점 간 실제 이동 중 (길 위)
        Exploring   // 던전/유적 내부에서 가상 전투 중 
    }

    // 2. 장소(노드)의 성격 정의
    public enum WorldNodeType 
    { 
        Town,       // 마을 (여관 이용 가능, 보급 가능)
        Dungeon,    // 던전 (사냥 가능, 위험도 높음)
        Ruin        // 유적 (야외 캠핑 가능, 임시 거점)
    }

    // 3. 월드 노드 정보: 지리적 좌표와 난이도 데이터
    public class WorldNode
    {
        public string Name { get; set; }
        public WorldNodeType Type { get; set; }
        public Map NodeMap { get; set; }
        public Point3D EntranceLoc { get; set; } // 외부 입구 좌표 (이동 시간 계산의 기준)
        public Point3D TargetLoc { get; set; }   // 내부 활동 좌표 (사냥/상자 스폰 기준)
        public int Difficulty { get; set; }      // 권장 전투력 및 난이도 (1~10)

        public WorldNode(string name, WorldNodeType type, Map map, Point3D ext, Point3D ins, int diff)
        {
            Name = name;
            Type = type;
            NodeMap = map;
            EntranceLoc = ext;
            TargetLoc = ins;
            Difficulty = diff;
        }
    }

    // 4. 가상 모험가 파티 (Team System): 이동과 의사결정의 주체
    public class AdventurerParty
    {
        public List<VirtualAdventurer> Members { get; set; } // 파티원 명단
        public AdventurerState State { get; set; }           // 파티 전체 상태
        public WorldNode CurrentNode { get; set; }           // 현재 위치한 노드
        public WorldNode TargetNode { get; set; }            // 목표로 하는 노드
        public int TravelHoursRemaining { get; set; }        // 이동 완료까지 남은 가상 시간

        public AdventurerParty(WorldNode startNode)
        {
            Members = new List<VirtualAdventurer>();
            CurrentNode = startNode;
            State = AdventurerState.Resting;
        }

        // 파티원 전체의 시너지를 포함한 종합 전투력 계산
        public int GetTotalPower()
        {
            if (Members.Count == 0) return 0;
            // 인원수가 많을수록 보너스 적용 (1명: 1.0배, 5명: 1.5배)
            double synergy = 1.0 + (Members.Count * 0.1);
            return (int)(Members.Sum(m => m.CombatPower) * synergy);
        }

        // 파티의 시간당 루틴 (Hourly Tick)
        public void HourlyRoutine(TownEconomy town)
        {
            if (Members.Count == 0) return;

            // 각 멤버들의 개인 생존 및 상태 업데이트 실행
            // (개별 멤버의 HourlyRoutine 내에서 UpdateSurvival 등이 실행됨)
            foreach (var m in Members)
            {
                m.HourlyRoutine(town, 0); 
            }

            // 파티 단위 상태 머신 처리
            switch (State)
            {
                case AdventurerState.Traveling:
                    TravelHoursRemaining--;
                    if (TravelHoursRemaining <= 0) ReachDestination();
                    break;

                case AdventurerState.Resting:
                    // 모든 멤버가 90% 이상의 체력을 회복하고 스트레스가 낮아지면 다음 행선지 결정
                    if (Members.TrueForAll(m => m.HP >= m.MaxHP * 0.9 && m.Stress < 20))
                    {
                        DecideNextDestination();
                    }
                    break;

                case AdventurerState.Exploring:
                    // 사냥 중일 때는 개별 멤버의 가상 전투 로직에서 상태 변화를 감지함
                    break;
            }
        }

        // 목적지 설정 및 실제 좌표 기반 거리 계산
        public void SetDestination(WorldNode target)
        {
            if (target == null) return;
            TargetNode = target;

			Point3D p1 = CurrentNode.EntranceLoc;
			Point3D p2 = TargetNode.EntranceLoc;

            // 실제 월드 좌표 간 거리 계산 (Utility.GetDistance 이용)
            int dist = (int)Math.Sqrt(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2));

            // 이동 속도 설정: 시간당 80타일 기준 (브리튼-베스퍼 약 2400타일 = 30시간)
            const int TilesPerHour = 80;
            TravelHoursRemaining = Math.Max(1, dist / TilesPerHour);
            
            State = AdventurerState.Traveling;
        }

        private void ReachDestination()
        {
            CurrentNode = TargetNode;
            // 도착지가 마을이면 휴식 모드, 아니면 즉시 탐험 모드 진입
            State = (CurrentNode.Type == WorldNodeType.Town) ? AdventurerState.Resting : AdventurerState.Exploring;
        }

        private void DecideNextDestination()
        {
            // AI에 의해 새로운 던전이나 마을을 탐색하는 로직이 들어갈 자리입니다.
        }
    }
	// ⚔️ 가상 모험가 클래스: 개인의 능력치와 배낭 관리
    public class VirtualAdventurer : VirtualAgent
    {
        // --- [1. 기본 및 전투 스탯] ---
        public int CombatSkill { get; set; }      // 전투 숙련도 (0~100, 성장형)
        public int EquipmentTier { get; set; }    // 장비 강화 단계 (마을에서 골드로 강화)
        public int HP { get; set; }               // 현재 체력
        public int MaxHP { get; set; }            // 최대 체력

        // --- [2. 생존 및 숙련도 스탯] ---
        public int CampingSkill { get; set; }     // 캠핑 스킬 (야외 휴식 효율 결정)
        public double Experience { get; set; }    // 종합 경험치 (이동/전투 시 누적)
        public double PrepMultiplier { get; set; } // 원정 거리에 따른 물품 비축 배율 (1.2~2.0)

        // --- [3. 배낭 및 생존 물품] ---
        public int FoodRations { get; set; }      // 보존식(식량) 개수
        public int HealingPotions { get; set; }   // 체력 회복 포션 개수
        public int Bandages { get; set; }         // 붕대 개수
        public bool HasBedroll { get; set; }      // 캠핑용 침낭 보유 여부
        public bool IsRestingAtInn { get; set; }  // 현재 여관 숙박 중인지 여부

        // --- [4. 파티 및 파워 계산] ---
        public AdventurerParty Party { get; set; } 
        // 장비와 스킬을 합산한 개인 전투력
        public int CombatPower => CombatSkill + (EquipmentTier * 50);

        // ==============================================================================
        // 🏁 생성자: 초기 능력치 및 아이템 세팅
        // ==============================================================================
        public VirtualAdventurer(NpcJobClass job, NpcRank rank) : base(job, rank)
        {
            // 계급(Rank)이 높을수록 기본 체력이 높음
            MaxHP = 100 + ((int)rank * 50);
            HP = MaxHP;

            // 계급에 따른 초기 스킬 분배 (초보 50~100, 장인 200~400 등)
            CombatSkill = ((int)rank + 1) * Utility.RandomMinMax(50, 100);
            CampingSkill = Utility.RandomMinMax(10, 50); // 캠핑은 처음엔 서투름
            
            // 성장 데이터 초기화
            EquipmentTier = 1;
            Experience = 0.0;
            PrepMultiplier = 1.0; 

            // 초기 보급품 (마을에서 스폰될 때 기본적으로 가지고 있는 양)
            FoodRations = 5;
            HealingPotions = 3;
            Bandages = 10;
            HasBedroll = true;   // 모든 모험가는 침낭을 기본 소지
            IsRestingAtInn = false;
        }

        // (HourlyRoutine 및 Handle 메서드들은 파트 3에서 이어집니다)
// ==============================================================================
        // ⏰ 1. 시간당 루틴 메인 (HourlyRoutine)
        // ==============================================================================
        public override void HourlyRoutine(TownEconomy town, int currentHour)
        {
            if (Party == null) return;

            // 공통 생존 자원 및 회복 업데이트 (모든 상태에서 공통 실행)
            UpdateSurvival(); 

            // 파티의 현재 상태에 따른 개별 행동 처리
            switch (Party.State)
            {
                case AdventurerState.Resting:
                    HandleResting(town);
                    break;

                case AdventurerState.Traveling:
                    HandleTraveling();
                    break;

                case AdventurerState.Exploring:
                    HandleExploring(town);
                    break;
            }
        }

        // ==============================================================================
        // 🍖 2. 생존 및 회복 엔진 (UpdateSurvival)
        // ==============================================================================
        private void UpdateSurvival()
        {
            // [A] 배고픔 및 식량 소모 로직
            this.Hunger += 2;
            if (this.Hunger >= 50)
            {
                if (FoodRations > 0)
                {
                    FoodRations--;
                    this.Hunger = 0;
                    this.HP = Math.Min(MaxHP, HP + 5); // 보존식을 먹으면 체력 소폭 회복
                }
                else
                {
                    this.HP -= 5;       // 식량이 없으면 굶주림 피해
                    this.Stress += 5;    // 배고픔으로 인한 스트레스
                }
            }

            // [B] 상태별 체력/스트레스 회복 (또는 피로 누적)
            if (Party.State == AdventurerState.Resting)
            {
                if (Party.CurrentNode.Type == WorldNodeType.Town)
                {
                    // 마을 휴식: 여관 숙박 여부에 따른 차등 회복
                    int recoverHP = IsRestingAtInn ? 20 : 5;
                    int reduceStress = IsRestingAtInn ? 10 : 2;

                    this.HP += recoverHP;
                    this.Stress -= reduceStress;
                }
                else
                {
                    // 야외 캠핑: 캠핑 스킬과 배드롤이 생존을 결정
                    bool success = Utility.Random(100) < this.CampingSkill;
                    int bedrollBonus = HasBedroll ? 4 : 0;

                    if (success)
                    {
                        this.HP += 8 + bedrollBonus;
                        this.Stress -= 5 + (HasBedroll ? 2 : 0);
                        CheckSkillGain("Camping", 0.1); // 캠핑 성공 시 스킬 상승
                    }
                    else
                    {
                        this.HP += 2 + bedrollBonus;
                        this.Stress += 1; // 캠핑 실패 시 오히려 스트레스 증가
                        CheckSkillGain("Camping", 0.05);
                    }
                }
            }
            else
            {
                // 이동 중이거나 사냥 중일 때의 물리적 소모
                this.HP -= 2;
                this.Stress += 1;
            }

            // 수치 하한/상한 보정 및 사망 판정
            this.HP = Math.Max(0, Math.Min(MaxHP, HP));
            this.Stress = Math.Max(0, Math.Min(100, this.Stress));

            if (this.HP <= 0) Die();
        }

        // ==============================================================================
        // 🚶 3. 상태별 핸들러 (Traveling / Resting / Exploring)
        // ==============================================================================
        private void HandleTraveling()
        {
            // 이동 중에는 노정의 경험을 통해 스킬이 조금씩 상승
            CheckSkillGain("Combat", 0.05);
            CheckSkillGain("Camping", 0.03);
        }

        private void HandleResting(TownEconomy town)
        {
            // 마을에서 휴식 중이라면 상점을 이용해 정비(ProcessJob) 수행
            if (Party.CurrentNode.Type == WorldNodeType.Town)
            {
                ProcessJob(town);
            }
        }

        private void HandleExploring(TownEconomy town)
        {
            // 던전 탐험 중에는 매 시간 가상 전투 시도
            VirtualCombatTick();

            // 퇴각(후퇴) 조건 감시: 체력 40% 미만, 포션 고갈, 혹은 멘탈 붕괴
            if (this.HP < MaxHP * 0.4 || HealingPotions == 0 || this.Stress > 85)
            {
                ReturnToSafety();
            }
        }
		// ==============================================================================
        // 🛒 1. 마을 정비 및 거리 기반 비축 (ProcessJob)
        // ==============================================================================
        protected override void ProcessJob(TownEconomy town)
        {
            // [A] 다음 목적지 거리에 따른 비축 배율 계산
            if (Party != null && Party.TargetNode != null)
            {

				Point3D p1 = Party.CurrentNode.EntranceLoc;
				Point3D p2 = Party.TargetNode.EntranceLoc;

				// 실제 월드 좌표 간 거리 계산 (Utility.GetDistance 이용)
				int dist = (int)Math.Sqrt(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2));
			
                // 3000타일 기준 최대 2.0배 비축
                this.PrepMultiplier = Math.Min(2.0, 1.2 + (dist / 3000.0));
            }

            // [B] 소모품 대량 구매 (PrepMultiplier 적용)
            int foodT = (int)(10 * PrepMultiplier);
            if (FoodRations < foodT) 
                if (TryBuyItem(town, typeof(BreadLoaf), foodT - FoodRations, out _)) 
                    FoodRations = foodT;

            int potT = (int)(5 * PrepMultiplier);
            if (HealingPotions < potT) 
                if (TryBuyItem(town, typeof(GreaterHealPotion), potT - HealingPotions, out _)) 
                    HealingPotions = potT;

            int bandT = (int)(20 * PrepMultiplier);
            if (Bandages < bandT) 
                if (TryBuyItem(town, typeof(Bandage), bandT - Bandages, out _)) 
                    Bandages = bandT;

            // [C] 필수 장비 및 여관 숙박
            if (!HasBedroll) 
                if (TryBuyItem(town, typeof(Bedroll), 1, out _)) HasBedroll = true;

            if (this.HP < MaxHP * 0.6 || this.Stress > 40)
            {
                if (this.Gold >= 50) 
                { 
                    this.Gold -= 50; 
                    town.Wealth += 50; 
                    IsRestingAtInn = true; 
                    this.Stress = Math.Max(0, this.Stress - 10); 
                }
            }
            else IsRestingAtInn = false;

            // [D] 장비 강화 (성장)
            int upgradeCost = EquipmentTier * 2000;
            if (this.Gold >= upgradeCost)
            {
                this.Gold -= upgradeCost;
                town.Wealth += upgradeCost;
                EquipmentTier++;
            }
        }

        // ==============================================================================
        // ⚔️ 2. 가상 전투 엔진 (VirtualCombatTick)
        // ==============================================================================
        private void VirtualCombatTick()
        {
            WorldNode node = Party.CurrentNode;
            
            // GM(개발자) 관전을 허용하는 유저 감지 로직
            if (IsNormalPlayerNearby(node.NodeMap, node.TargetLoc, 50)) return;

            // 주변 몬스터 탐색
            BaseCreature monster = FindMonster(node.NodeMap, node.TargetLoc, 20);
            if (monster == null) return;

            int partyPower = Party.GetTotalPower();
            int monsterPower = monster.HitsMax + (monster.Fame / 10);

            if (Utility.Random(partyPower) >= Utility.Random(monsterPower))
            {
                // [승리] 몬스터 실제 사망 및 전리품 획득
                monster.Kill();
                this.Gold += monster.Fame / 10;
                this.Stress += 2;
                CheckSkillGain("Combat", 0.1); // 실전 승리로 스킬 상승
            }
            else
            {
                // [패배] 부상 및 스트레스
                this.HP -= Utility.RandomMinMax(10, 30);
                this.Stress += 10;

                // 위기 상황 시 포션 사용
                if (this.HP < MaxHP * 0.5 && HealingPotions > 0)
                {
                    HealingPotions--;
                    this.HP += 30;
                }
            }
        }

        // ==============================================================================
        // 📈 3. 스킬 및 경험치 통합 관리 (CheckSkillGain)
        // ==============================================================================
        public void CheckSkillGain(string skill, double chance)
        {
            if (Utility.RandomDouble() < chance)
            {
                if (skill == "Combat" && CombatSkill < 100) CombatSkill++;
                else if (skill == "Camping" && CampingSkill < 100) CampingSkill++;
                
                Experience += 0.5; // 행동에 따른 종합 경험치 누적
            }
        }

        // ==============================================================================
        // 💀 4. 사망 및 전리품 상자 생성 (Die & Chest)
        // ==============================================================================
        private void Die()
        {
            // 사망한 위치에 락픽용 보물상자 생성
            SpawnAdventurerChest(Party.CurrentNode.NodeMap, Party.CurrentNode.TargetLoc);
            
            // 파티에서 영구 제외
            Party.Members.Remove(this);
            // (이후 마을에서 새로운 NPC로 대체 생성되는 로직은 별도 처리)
        }

        private void SpawnAdventurerChest(Map map, Point3D loc)
        {
            MetalGoldenChest chest = new MetalGoldenChest();
            chest.Locked = true;
            // 장비 티어가 높을수록 자물쇠 난이도 상승
            chest.LockLevel = chest.RequiredSkill = (this.EquipmentTier * 20) + 10;
            
            // 소지 골드의 절반을 상자에 봉인
            chest.DropItem(new Gold(this.Gold / 2));
            if (Utility.RandomBool()) chest.DropItem(new StarSapphire(Utility.RandomMinMax(1, 3)));
            
            chest.MoveToWorld(loc, map);
        }

        private void ReturnToSafety()
        {
            // 파티 상태를 이동으로 변경하고 목표를 가장 가까운 마을로 재설정
            Party.State = AdventurerState.Traveling;
        }

        // --- 헬퍼 메서드 (GM 감지 및 몬스터 탐색) ---
        private bool IsNormalPlayerNearby(Map map, Point3D loc, int radius)
        {
            IPooledEnumerable eable = map.GetClientsInRange(loc, radius);
            foreach (NetState state in eable)
            {
                if (state.Mobile != null && state.Mobile.AccessLevel == AccessLevel.Player)
                { eable.Free(); return true; }
            }
            eable.Free(); return false;
        }

        private BaseCreature FindMonster(Map map, Point3D loc, int radius)
        {
            IPooledEnumerable eable = map.GetObjectsInRange(loc, radius);
            foreach (object obj in eable)
            {
                if (obj is BaseCreature bc && !bc.Controlled && bc.IsEnemy(null))
                { eable.Free(); return bc; }
            }
            eable.Free(); return null;
        }
    }
}