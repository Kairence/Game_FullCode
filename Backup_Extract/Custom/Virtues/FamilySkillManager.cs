using System;
using Server;

namespace Server.Misc
{
    public class FamilySkillNode
    {
        public int NodeID;
        public string Name;
        public int MaxLevel;
        public int[] OptIDs;
        public int[] OptValuesPerLevel;

        public FamilySkillNode(int id, string name, int maxLv, int[] optIDs, int[] optVals)
        {
            NodeID = id;
            Name = name;
            MaxLevel = maxLv;
            OptIDs = optIDs;
            OptValuesPerLevel = optVals;
        }
    }

    public static class FamilySkillManager
    {
        public static readonly FamilySkillNode[] Skills = new FamilySkillNode[601];

        public static int Skill(SkillName skillName)
        {
            return 77 + (int)skillName;
        }

        private static void SetNode(int id, string name, int maxLv, int[] optIDs, int[] optVals)
        {
            Skills[id] = new FamilySkillNode(id, name, maxLv, optIDs, optVals);
        }

        static FamilySkillManager()
        {
            // =========================================================
            // 1. 정직 (Honesty) : 401 ~ 425
            // =========================================================
            SetNode(401, "탐구심", 10, new int[] { CustomOption.Int, CustomOption.Stam }, new int[] { 100000, 100000 });
            SetNode(402, "지식의 갈망", 10, new int[] { CustomOption.Gold }, new int[] { 2500 });
            SetNode(403, "진실의 눈", 10, new int[] { CustomOption.Magic }, new int[] { 2500 });
            SetNode(404, "명석한 머리", 10, new int[] { CustomOption.Mana }, new int[] { 100000 });
            SetNode(405, "고대 문헌 해독", 5, new int[] { Skill(SkillName.EvalInt) }, new int[] { 20000 });
            SetNode(406, "물질의 진리", 5, new int[] { Skill(SkillName.Alchemy) }, new int[] { 20000 });
            SetNode(407, "기록자", 5, new int[] { Skill(SkillName.Inscribe) }, new int[] { 20000 });
            SetNode(408, "마법의 이해", 5, new int[] { Skill(SkillName.Magery) }, new int[] { 20000 });
            SetNode(409, "마나의 흐름", 5, new int[] { CustomOption.LowerManaCost }, new int[] { 2500 });
            SetNode(410, "매직 탐지", 5, new int[] { CustomOption.Magic }, new int[] { 5000 });
            SetNode(411, "진리 탐구", 5, new int[] { CustomOption.Luck }, new int[] { 250000 });
            SetNode(412, "장비 감정", 5, new int[] { Skill(SkillName.ItemID) }, new int[] { 20000 });
            SetNode(413, "독도법", 5, new int[] { Skill(SkillName.Cartography) }, new int[] { 20000 });
            SetNode(414, "학자의 인내", 5, new int[] { CustomOption.LowerAllCost }, new int[] { 1000 });
            SetNode(415, "자연의 섭리", 5, new int[] { Skill(SkillName.MagicResist) }, new int[] { 20000 });
            SetNode(416, "신비한 행운", 5, new int[] { CustomOption.Luck }, new int[] { 250000 });
            SetNode(417, "유물 수집가", 5, new int[] { CustomOption.Magic }, new int[] { 5000 });
            SetNode(418, "지혜의 샘", 5, new int[] { CustomOption.Int }, new int[] { 100000 });
            SetNode(419, "약점 간파", 5, new int[] { CustomOption.SpellCriChance }, new int[] { 5000 });
            SetNode(420, "지식의 축적", 5, new int[] { CustomOption.Gold }, new int[] { 5000 });
            SetNode(421, "통찰력", 5, new int[] { CustomOption.LowerAllCost }, new int[] { 1000 });
            SetNode(422, "진실의 방패", 5, new int[] { CustomOption.MagicArmor }, new int[] { 4000 });
            SetNode(423, "보물 감별사", 5, new int[] { CustomOption.Magic }, new int[] { 5000 });
            SetNode(424, "마나 집중", 5, new int[] { CustomOption.SpellSpeed }, new int[] { 6000 });
            SetNode(425, "궁극의 진리", 1, new int[] { CustomOption.Luck, CustomOption.Magic, CustomOption.Gold }, new int[] { 1250000, 25000, 25000 });

            // =========================================================
            // 2. 연민 (Compassion) : 426 ~ 450
            // =========================================================
            SetNode(426, "따뜻한 마음", 10, new int[] { CustomOption.HealPlus }, new int[] { 10000 });
            SetNode(427, "생명 존중", 10, new int[] { CustomOption.HitsRegen }, new int[] { 2500 });
            SetNode(428, "동물의 벗", 10, new int[] { Skill(SkillName.AnimalLore) }, new int[] { 20000 });
            SetNode(429, "치유의 손길", 10, new int[] { Skill(SkillName.Healing) }, new int[] { 20000 });
            SetNode(430, "응급 처치", 5, new int[] { Skill(SkillName.Anatomy) }, new int[] { 20000 });
            SetNode(431, "생명의 기운", 5, new int[] { CustomOption.Hits }, new int[] { 100000 });
            SetNode(432, "야수 교감", 5, new int[] { Skill(SkillName.AnimalTaming) }, new int[] { 20000 });
            SetNode(433, "선율의 이해", 5, new int[] { Skill(SkillName.Musicianship) }, new int[] { 20000 });
            SetNode(434, "수의학", 5, new int[] { Skill(SkillName.Veterinary) }, new int[] { 20000 });
            SetNode(435, "영혼의 대화", 5, new int[] { Skill(SkillName.SpiritSpeak) }, new int[] { 20000 });
            SetNode(436, "자애로운 빛", 5, new int[] { CustomOption.HealPlusPlus }, new int[] { 15000 });
            SetNode(437, "끝없는 체력", 5, new int[] { CustomOption.StamRegen }, new int[] { 5000 });
            SetNode(438, "희생정신", 5, new int[] { CustomOption.AllResist }, new int[] { 10000 });
            SetNode(439, "평화의 노래", 5, new int[] { Skill(SkillName.Peacemaking) }, new int[] { 20000 });
            SetNode(440, "진정의 외침", 5, new int[] { Skill(SkillName.Provocation) }, new int[] { 20000 });
            SetNode(441, "기적의 손", 5, new int[] { CustomOption.HitsGain }, new int[] { 5000 });
            SetNode(442, "은신처 제공", 5, new int[] { CustomOption.DefChance }, new int[] { 3700 });
            SetNode(443, "굳건한 유대", 5, new int[] { CustomOption.MagicArmor }, new int[] { 4000 });
            SetNode(444, "생명의 인도자", 5, new int[] { CustomOption.AllGain }, new int[] { 3000 });
            SetNode(445, "수호자의 헌신", 5, new int[] { CustomOption.DefChance }, new int[] { 3700 });
            SetNode(446, "자연의 축복", 5, new int[] { CustomOption.AllRegen }, new int[] { 2000 });
            SetNode(447, "불협화음", 5, new int[] { Skill(SkillName.Discordance) }, new int[] { 20000 });
            SetNode(448, "영혼의 위로", 5, new int[] { CustomOption.ManaRegen }, new int[] { 5000 });
            SetNode(449, "생명력 집중", 5, new int[] { CustomOption.HitsLeech }, new int[] { 2500 });
            SetNode(450, "연민의 화신", 1, new int[] { CustomOption.HealPlus, CustomOption.AllRegen, CustomOption.AllResist }, new int[] { 100000, 10000, 50000 });

            // =========================================================
            // 3. 용맹 (Valor) : 451 ~ 475
            // =========================================================
            SetNode(451, "완력 단련", 10, new int[] { CustomOption.Str }, new int[] { 100000 });
            SetNode(452, "체력 단련", 10, new int[] { CustomOption.Hits }, new int[] { 100000 });
            SetNode(453, "기본 전술", 10, new int[] { CustomOption.WeaponDamage }, new int[] { 5000 });
            SetNode(454, "신속 연마", 10, new int[] { CustomOption.SwingSpeed }, new int[] { 3000 });
            SetNode(455, "명중 보완", 5, new int[] { CustomOption.HitChance }, new int[] { 3700 });
            SetNode(456, "자상의 달인", 5, new int[] { Skill(SkillName.Fencing) }, new int[] { 20000 });
            SetNode(457, "둔기의 달인", 5, new int[] { Skill(SkillName.Macing) }, new int[] { 20000 });
            SetNode(458, "검의 달인", 5, new int[] { Skill(SkillName.Swords) }, new int[] { 20000 });
            SetNode(459, "흡혈귀의 손길", 5, new int[] { CustomOption.HitsLeech }, new int[] { 2500 });
            SetNode(460, "방패의 달인", 5, new int[] { Skill(SkillName.Parry) }, new int[] { 20000 });
            SetNode(461, "강철 피부", 5, new int[] { CustomOption.WeaponArmor }, new int[] { 4000 });
            SetNode(462, "급소 파악", 5, new int[] { CustomOption.WeaponCriChance }, new int[] { 5000 });
            SetNode(463, "무자비함", 5, new int[] { CustomOption.PhysPlus }, new int[] { 20000 });
            SetNode(464, "용맹한 일격", 5, new int[] { CustomOption.WeaponCriDamage }, new int[] { 15000 });
            SetNode(465, "전술적 우위", 5, new int[] { Skill(SkillName.Tactics) }, new int[] { 20000 });
            SetNode(466, "분노 폭발", 5, new int[] { CustomOption.SwingSpeed }, new int[] { 6000 });
            SetNode(467, "강타", 5, new int[] { CustomOption.WeaponDamage }, new int[] { 12000 });
            SetNode(468, "한계 돌파", 5, new int[] { CustomOption.Str }, new int[] { 100000 });
            SetNode(469, "갑옷 부수기", 5, new int[] { CustomOption.HitChance }, new int[] { 3700 });
            SetNode(470, "피의 축제", 5, new int[] { CustomOption.WeaponCriDamage }, new int[] { 15000 });
            SetNode(471, "충격 반사", 5, new int[] { CustomOption.WeaponReflect }, new int[] { 20000 });
            SetNode(472, "기력 탈취", 5, new int[] { CustomOption.StamLeech }, new int[] { 2500 });
            SetNode(473, "광전사", 5, new int[] { CustomOption.AllDamage }, new int[] { 7500 });
            SetNode(474, "투지", 5, new int[] { CustomOption.HitsRegen }, new int[] { 5000 });
            SetNode(475, "전장의 신", 1, new int[] { CustomOption.PhysPlus, CustomOption.SwingSpeed, CustomOption.WeaponCriChance }, new int[] { 100000, 30000, 25000 });

            // =========================================================
            // 4. 정의 (Justice) : 476 ~ 500
            // =========================================================
            SetNode(476, "심판의 무게", 10, new int[] { CustomOption.Str, CustomOption.Mana }, new int[] { 100000, 100000 });
            SetNode(477, "강인한 정신", 10, new int[] { CustomOption.Stam }, new int[] { 100000 });
            SetNode(478, "정확한 겨냥", 10, new int[] { CustomOption.HitChance }, new int[] { 1500 });
            SetNode(479, "불굴의 방패", 10, new int[] { CustomOption.AllArmor }, new int[] { 1500 });
            SetNode(480, "길잡이", 5, new int[] { Skill(SkillName.Tracking) }, new int[] { 20000 });
            SetNode(481, "원소 내성", 5, new int[] { CustomOption.AllResist }, new int[] { 10000 });
            SetNode(482, "궁수의 눈", 5, new int[] { Skill(SkillName.Archery) }, new int[] { 20000 });
            SetNode(483, "마물 도감", 5, new int[] { Skill(SkillName.AnimalLore) }, new int[] { 20000 });
            SetNode(484, "성기사의 길", 5, new int[] { Skill(SkillName.Chivalry) }, new int[] { 20000 });
            SetNode(485, "신성한 빛", 5, new int[] { CustomOption.HolyPlus }, new int[] { 20000 });
            SetNode(486, "연속 사격", 5, new int[] { CustomOption.SwingSpeed }, new int[] { 6000 });
            SetNode(487, "결계 파괴", 5, new int[] { Skill(SkillName.DetectHidden) }, new int[] { 20000 });
            SetNode(488, "독성 정화", 5, new int[] { CustomOption.PoisonResist }, new int[] { 20000 });
            SetNode(489, "관통 사격", 5, new int[] { CustomOption.WeaponDamage }, new int[] { 12000 });
            SetNode(490, "퇴마술", 5, new int[] { CustomOption.UndeadSlayer, CustomOption.DemonSlayer }, new int[] { 20000, 20000 });
            SetNode(491, "악의 추적자", 5, new int[] { CustomOption.Dex }, new int[] { 100000 });
            SetNode(492, "무법자 사냥", 5, new int[] { CustomOption.HumanoidSlayer }, new int[] { 20000 });
            SetNode(493, "마법 차단", 5, new int[] { CustomOption.MagicArmor }, new int[] { 4000 });
            SetNode(494, "절대 영역", 5, new int[] { CustomOption.EnergyResist, CustomOption.ColdResist }, new int[] { 20000, 20000 });
            SetNode(495, "파멸의 선고", 5, new int[] { CustomOption.ReptileSlayer, CustomOption.InsectSlayer }, new int[] { 20000, 20000 });
            SetNode(496, "이단 심문관", 5, new int[] { CustomOption.AllDamage }, new int[] { 7500 });
            SetNode(497, "매의 눈", 5, new int[] { CustomOption.WeaponCriChance }, new int[] { 5000 });
            SetNode(498, "단죄의 일격", 5, new int[] { CustomOption.WeaponCriDamage }, new int[] { 15000 });
            SetNode(499, "강철의 의지", 5, new int[] { CustomOption.DefChance }, new int[] { 3700 });
            SetNode(500, "정의의 심판관", 1, new int[] { CustomOption.UndeadSlayer, CustomOption.DemonSlayer, CustomOption.ReptileSlayer, CustomOption.InsectSlayer, CustomOption.HitChance }, new int[] { 100000, 100000, 100000, 100000, 18000 });

            // =========================================================
            // 5. 희생 (Sacrifice) : 501 ~ 525
            // =========================================================
            SetNode(501, "땀의 결실", 10, new int[] { CustomOption.Stam, CustomOption.Hits }, new int[] { 100000, 100000 });
            SetNode(502, "거인의 어깨", 10, new int[] { CustomOption.Str }, new int[] { 100000 });
            SetNode(503, "끝없는 인내", 10, new int[] { CustomOption.StamRegen }, new int[] { 2500 });
            SetNode(504, "굳은살", 10, new int[] { CustomOption.PhysResist }, new int[] { 10000 });
            SetNode(505, "단단한 가죽", 5, new int[] { CustomOption.AllArmor }, new int[] { 3000 });
            SetNode(506, "재료의 이해", 5, new int[] { CustomOption.LowerAllCost }, new int[] { 1000 });
            SetNode(507, "광맥의 발견", 5, new int[] { Skill(SkillName.Mining) }, new int[] { 20000 });
            SetNode(508, "대지의 숨결", 5, new int[] { Skill(SkillName.Blacksmith) }, new int[] { 20000 });
            SetNode(509, "깊은 뿌리", 5, new int[] { Skill(SkillName.Carpentry) }, new int[] { 20000 });
            SetNode(510, "숙련된 망치", 5, new int[] { Skill(SkillName.Tinkering) }, new int[] { 20000 });
            SetNode(511, "응급 처치", 5, new int[] { Skill(SkillName.Healing) }, new int[] { 20000 });
            SetNode(512, "기력 보존", 5, new int[] { CustomOption.LowerStamCost }, new int[] { 2500 });
            SetNode(513, "손재주", 5, new int[] { Skill(SkillName.Tailoring) }, new int[] { 20000 });
            SetNode(514, "대풍작", 5, new int[] { CustomOption.AllGain }, new int[] { 3000 });
            SetNode(515, "수목학", 5, new int[] { Skill(SkillName.Lumberjacking) }, new int[] { 20000 });
            SetNode(516, "마나 공유", 5, new int[] { CustomOption.ManaRegen }, new int[] { 5000 });
            SetNode(517, "철벽", 5, new int[] { CustomOption.DefChance }, new int[] { 3700 });
            SetNode(518, "불굴의 혼", 5, new int[] { CustomOption.HitsGain }, new int[] { 5000 });
            SetNode(519, "황금의 손", 5, new int[] { CustomOption.Luck }, new int[] { 250000 });
            SetNode(520, "헌신적인 삶", 5, new int[] { CustomOption.Gold }, new int[] { 5000 });
            SetNode(521, "전우애", 5, new int[] { CustomOption.HealPlus }, new int[] { 20000 });
            SetNode(522, "희귀목 탐지", 5, new int[] { Skill(SkillName.Fletching) }, new int[] { 20000 });
            SetNode(523, "완벽주의", 5, new int[] { CustomOption.LowerAllCost }, new int[] { 1000 });
            SetNode(524, "희생의 방패", 5, new int[] { CustomOption.WeaponReflect }, new int[] { 20000 });
            SetNode(525, "희생의 십자가", 1, new int[] { CustomOption.AllGain, CustomOption.LowerAllCost, CustomOption.Str }, new int[] { 15000, 5000, 1250000 });

            // =========================================================
            // 6. 명예 (Honor) : 526 ~ 550
            // =========================================================
            SetNode(526, "기사의 긍지", 10, new int[] { CustomOption.Str, CustomOption.Stam }, new int[] { 100000, 100000 });
            SetNode(527, "명예로운 상처", 10, new int[] { CustomOption.Hits }, new int[] { 100000 });
            SetNode(528, "고결한 정신", 10, new int[] { CustomOption.Mana }, new int[] { 100000 });
            SetNode(529, "승전보", 10, new int[] { CustomOption.Gold }, new int[] { 2500 });
            SetNode(530, "명성 축적", 5, new int[] { CustomOption.Luck }, new int[] { 250000 });
            SetNode(531, "신앙심", 5, new int[] { Skill(SkillName.Focus) }, new int[] { 20000 });
            SetNode(532, "급소 타격", 5, new int[] { CustomOption.WeaponCriChance }, new int[] { 5000 });
            SetNode(533, "칼날 반사", 5, new int[] { CustomOption.WeaponReflect }, new int[] { 20000 });
            SetNode(534, "역공", 5, new int[] { CustomOption.DefChance }, new int[] { 3700 });
            SetNode(535, "절대 방어", 5, new int[] { CustomOption.DefChance }, new int[] { 3700 });
            SetNode(536, "전술 훈련", 5, new int[] { Skill(SkillName.Tactics) }, new int[] { 20000 });
            SetNode(537, "전리품 탐색", 5, new int[] { CustomOption.Gold }, new int[] { 5000 });
            SetNode(538, "기사도", 5, new int[] { Skill(SkillName.Chivalry) }, new int[] { 20000 });
            SetNode(539, "일기당천", 5, new int[] { CustomOption.SwingSpeed }, new int[] { 6000 });
            SetNode(540, "무사도", 5, new int[] { Skill(SkillName.Bushido) }, new int[] { 20000 });
            SetNode(541, "무기 막기", 5, new int[] { Skill(SkillName.Parry) }, new int[] { 20000 });
            SetNode(542, "황금률", 5, new int[] { CustomOption.Gold }, new int[] { 5000 });
            SetNode(543, "기적의 행운", 5, new int[] { CustomOption.Luck }, new int[] { 250000 });
            SetNode(544, "기사의 맹세", 5, new int[] { CustomOption.HolyPlus }, new int[] { 20000 });
            SetNode(545, "명예로운 죽음", 5, new int[] { CustomOption.WeaponCriDamage }, new int[] { 15000 });
            SetNode(546, "무결점 전투", 5, new int[] { CustomOption.HitChance }, new int[] { 3700 });
            SetNode(547, "무념무상", 5, new int[] { CustomOption.StamRegen }, new int[] { 5000 });
            SetNode(548, "신성한 방패", 5, new int[] { CustomOption.MagicArmor }, new int[] { 4000 });
            SetNode(549, "영광의 상처", 5, new int[] { CustomOption.HitsRegen }, new int[] { 5000 });
            SetNode(550, "명예의 수호자", 1, new int[] { CustomOption.DefChance, CustomOption.Luck, CustomOption.Gold, CustomOption.AllDamage }, new int[] { 18000, 1250000, 25000, 37500 });

            // =========================================================
            // 7. 영성 (Spirituality) : 551 ~ 575
            // =========================================================
            SetNode(551, "지성 단련", 10, new int[] { CustomOption.Int }, new int[] { 100000 });
            SetNode(552, "마력 증폭", 10, new int[] { CustomOption.Mana }, new int[] { 100000 });
            SetNode(553, "집중력", 10, new int[] { CustomOption.ManaRegen }, new int[] { 2500 });
            SetNode(554, "영혼의 울림", 10, new int[] { CustomOption.SpellDamage }, new int[] { 5000 });
            SetNode(555, "주문 조합", 5, new int[] { Skill(SkillName.Spellweaving) }, new int[] { 20000 });
            SetNode(556, "강령술", 5, new int[] { Skill(SkillName.Necromancy) }, new int[] { 20000 });
            SetNode(557, "신비술", 5, new int[] { Skill(SkillName.Mysticism) }, new int[] { 20000 });
            SetNode(558, "마법학", 5, new int[] { Skill(SkillName.Magery) }, new int[] { 20000 });
            SetNode(559, "시전 집중", 5, new int[] { CustomOption.SpellSpeed }, new int[] { 6000 });
            SetNode(560, "영혼 대화", 5, new int[] { Skill(SkillName.SpiritSpeak) }, new int[] { 20000 });
            SetNode(561, "마법 저항", 5, new int[] { Skill(SkillName.MagicResist) }, new int[] { 20000 });
            SetNode(562, "마력 절약", 5, new int[] { CustomOption.LowerManaCost }, new int[] { 2500 });
            SetNode(563, "주문 극대화", 5, new int[] { CustomOption.SpellCriChance }, new int[] { 5000 });
            SetNode(564, "영적 교감", 5, new int[] { Skill(SkillName.Meditation) }, new int[] { 20000 });
            SetNode(565, "고속 시전", 5, new int[] { CustomOption.SpellSpeed }, new int[] { 6000 });
            SetNode(566, "혼돈의 마력", 5, new int[] { CustomOption.ChaosPlus }, new int[] { 20000 });
            SetNode(567, "마나 흡수", 5, new int[] { CustomOption.ManaLeech }, new int[] { 2500 });
            SetNode(568, "마법 보호", 5, new int[] { CustomOption.MagicArmor }, new int[] { 4000 });
            SetNode(569, "지능 평가", 5, new int[] { Skill(SkillName.EvalInt) }, new int[] { 20000 });
            SetNode(570, "파괴의 주문", 5, new int[] { CustomOption.SpellCriDamage }, new int[] { 15000 });
            SetNode(571, "초월적 존재", 5, new int[] { CustomOption.AllResist }, new int[] { 10000 });
            SetNode(572, "마법의 지배자", 5, new int[] { CustomOption.SpellDamage }, new int[] { 12000 }); // SpellPlus(29) -> SpellDamage(10)
            SetNode(573, "영혼 수확", 5, new int[] { CustomOption.ManaGain }, new int[] { 5000 });
            SetNode(574, "자연 치유", 5, new int[] { CustomOption.HitsRegen }, new int[] { 5000 });
            SetNode(575, "영성의 대마법사", 1, new int[] { CustomOption.SpellDamage, CustomOption.ManaRegen, CustomOption.SpellSpeed }, new int[] { 62500, 25000, 30000 });

            // =========================================================
            // 8. 겸손 (Humility) : 576 ~ 600
            // =========================================================
            SetNode(576, "기민한 육체", 10, new int[] { CustomOption.Dex }, new int[] { 100000 });
            SetNode(577, "인내하는 자", 10, new int[] { CustomOption.Stam }, new int[] { 100000 });
            SetNode(578, "비우는 마음", 10, new int[] { CustomOption.Str }, new int[] { 100000 });
            SetNode(579, "그림자 동화", 10, new int[] { CustomOption.AllSpeed }, new int[] { 1800 });
            SetNode(580, "은신술", 5, new int[] { Skill(SkillName.Hiding) }, new int[] { 20000 });
            SetNode(581, "암술", 5, new int[] { Skill(SkillName.Ninjitsu) }, new int[] { 20000 });
            SetNode(582, "그림자 이동", 5, new int[] { Skill(SkillName.Stealth) }, new int[] { 20000 });
            SetNode(583, "회피 기동", 5, new int[] { CustomOption.DefChance }, new int[] { 3700 });
            SetNode(584, "그림자 일격", 5, new int[] { CustomOption.WeaponCriChance }, new int[] { 5000 });
            SetNode(585, "독의 이해", 5, new int[] { Skill(SkillName.Poisoning) }, new int[] { 20000 });
            SetNode(586, "기력 회복", 5, new int[] { CustomOption.StamRegen }, new int[] { 5000 });
            SetNode(587, "깃털의 무게", 5, new int[] { CustomOption.LowerAllCost }, new int[] { 1000 });
            SetNode(588, "배후 공격", 5, new int[] { CustomOption.PhysPlus }, new int[] { 20000 });
            SetNode(589, "맹독", 5, new int[] { CustomOption.PoisonPlus }, new int[] { 20000 });
            SetNode(590, "무한한 체력", 5, new int[] { CustomOption.LowerStamCost }, new int[] { 2500 });
            SetNode(591, "환영 보법", 5, new int[] { CustomOption.AllArmor }, new int[] { 3000 });
            SetNode(592, "암살자의 눈", 5, new int[] { CustomOption.WeaponCriDamage }, new int[] { 15000 });
            SetNode(593, "마비독", 5, new int[] { CustomOption.PoisonResist }, new int[] { 20000 });
            SetNode(594, "무소유", 5, new int[] { CustomOption.AllGain }, new int[] { 3000 });
            SetNode(595, "생존 본능", 5, new int[] { CustomOption.HitsRegen }, new int[] { 5000 });
            SetNode(596, "탐욕 비우기", 5, new int[] { Skill(SkillName.Stealing) }, new int[] { 20000 });
            SetNode(597, "함정 해체", 5, new int[] { Skill(SkillName.RemoveTrap) }, new int[] { 20000 });
            SetNode(598, "어둠의 손길", 5, new int[] { CustomOption.HitChance }, new int[] { 3700 });
            SetNode(599, "반사 신경", 5, new int[] { CustomOption.Dex }, new int[] { 100000 });
            SetNode(600, "겸손의 그림자", 1, new int[] { CustomOption.AllResist, CustomOption.StamRegen, CustomOption.DefChance }, new int[] { 50000, 25000, 18000 });
        }
    }
}