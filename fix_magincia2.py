import re
with open(r'Scripts/Custom/City Economy System/NPC Group/VirtualAdventurerSystem.cs', 'r', encoding='utf-8-sig') as f:
    content = f.read()

target = r'''System\.Linq\.Enumerable\.Where\(TownEconomyManager\.Towns\.Values, t => t\.Facet == CurrentMap\)'''
replacement = r'''System.Linq.Enumerable.Where(TownEconomyManager.Towns.Values, t => t.Facet == CurrentMap && t.TownName != "Magincia" && t.Name != "Magincia")'''

content = re.sub(target, replacement, content)

with open(r'Scripts/Custom/City Economy System/NPC Group/VirtualAdventurerSystem.cs', 'w', encoding='utf-8-sig') as f:
    f.write(content)
