using System;
using Server;
using Server.Commands;
using System.Linq;

namespace Server.Misc
{
    public class TestSpawnCommand
    {
        public static void Initialize()
        {
            CommandSystem.Register("TestSpawn", AccessLevel.GameMaster, new CommandEventHandler(TestSpawn_OnCommand));
        }

        [Usage("TestSpawn")]
        private static void TestSpawn_OnCommand(CommandEventArgs e)
        {
            try
            {
                var zone = DungeonManager.ZoneList.FirstOrDefault(z => z.RCode == RegionCode.Trammel_Dungeon_Covetous_TortureChambers);
                if (zone == null) { e.Mobile.SendMessage("Zone not found."); return; }

                e.Mobile.SendMessage($"Zone: {zone.GroupName} Max: {zone.MaxPopulation} Act: {zone.ActiveMonsters.Count} IsAct: {zone.IsActive}");

                Rectangle2D bounds = zone.AreaBounds[0];

                int pass = 0, failImpass = 0, failRegion = 0;
                for (int i = 0; i < 50; i++)
                {
                    int x = Utility.RandomMinMax(bounds.X, bounds.X + bounds.Width);
                    int y = Utility.RandomMinMax(bounds.Y, bounds.Y + bounds.Height);
                    int z = zone.Facet.GetAverageZ(x, y); 
                    var land = zone.Facet.Tiles.GetLandTile(x, y);
                    bool isImpassable = land.Ignored || TileData.LandTable[land.ID & 0x3FFF].Flags.HasFlag(TileFlag.Impassable);

                    if (isImpassable) { failImpass++; continue; }

                    Region r = Region.Find(new Point3D(x, y, z), zone.Facet);
                    if (r != null && !r.IsDefault) { pass++; continue; }
                    failRegion++;
                }

                e.Mobile.SendMessage($"[Test Terrain] Pass: {pass}, Impassable: {failImpass}, NotInRegion: {failRegion}");

                if (zone.SpawnProfileStrings == null || zone.SpawnProfileStrings.Count == 0)
                {
                    e.Mobile.SendMessage("[Test Type] Error: SpawnProfileStrings is empty!");
                }
                else
                {
                    if (zone.SpawnProfileStrings.TryGetValue(1, out var list))
                    {
                        e.Mobile.SendMessage($"[Test Type] T1 Mobs: {string.Join(", ", list)}");
                        var t = ScriptCompiler.FindTypeByName(list[0].Trim());
                        e.Mobile.SendMessage($"[Test Type] FindType: {(t == null ? "NULL" : t.Name)}");
                    }
                }
            }
            catch (Exception ex)
            {
                e.Mobile.SendMessage($"Error: {ex.Message}");
            }
        }
    }
}
