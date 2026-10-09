import re

with open(r'Scripts/Custom/City Economy System/NPC Group/VirtualAdventurerSystem.cs', 'r', encoding='utf-8-sig') as f:
    content = f.read()

# Add Magincia block in the first loop
target1 = r'''foreach \(TownEconomy town in allTowns\)
                \{
                    if \(town != null && town\.Facet == CurrentMap && town\.VendorCount > 0\)
                    \{'''

replacement1 = r'''foreach (TownEconomy town in allTowns)
                {
                    if (town != null && (town.TownName == "Magincia" || town.Name == "Magincia")) continue; // [기획] 마진시아로의 피난/게이트 완전 차단

                    if (town != null && town.Facet == CurrentMap && town.VendorCount > 0)
                    {'''

content = re.sub(target1, replacement1, content)

# Add Magincia block in the fallback loop
target2 = r'''foreach \(TownEconomy town in allTowns\)
                      \{
                          if \(town != null && town\.Facet == CurrentMap\)
                          \{'''

replacement2 = r'''foreach (TownEconomy town in allTowns)
                      {
                          if (town != null && (town.TownName == "Magincia" || town.Name == "Magincia")) continue; // [기획] 마진시아로의 피난/게이트 완전 차단

                          if (town != null && town.Facet == CurrentMap)
                          {'''

content = re.sub(target2, replacement2, content)

with open(r'Scripts/Custom/City Economy System/NPC Group/VirtualAdventurerSystem.cs', 'w', encoding='utf-8-sig') as f:
    f.write(content)

print("Patch applied successfully.")
