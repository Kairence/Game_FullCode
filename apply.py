import codecs

path = r'C:\Kairence_UO\4.0\Scripts\Skills\AnimalLore.cs'
content = codecs.open(path, 'r', encoding='utf-8-sig').read()

start_idx = content.find('public AnimalLoreGump(BaseCreature c) : base(')

new_gump = codecs.open('new_gump.txt', 'r', encoding='utf-8-sig').read()

gem_func = '''
        private static string GetGemFullName(int gemIndex)
        {
            switch (gemIndex)
            {
                case 0: return "별무늬 사파이어";
                case 1: return "에메랄드";
                case 2: return "사파이어";
                case 3: return "루비";
                case 4: return "황수정";
                case 5: return "자수정";
                case 6: return "전기석";
                case 7: return "호박";
                case 8: return "다이아몬드";
                case 99: return "전체(공용)";
                default: return "알수없음";
            }
        }
    }
}
'''

new_content = content[:start_idx] + new_gump + gem_func
codecs.open(path, 'w', encoding='utf-8-sig').write(new_content)
print('Patch applied successfully.')
