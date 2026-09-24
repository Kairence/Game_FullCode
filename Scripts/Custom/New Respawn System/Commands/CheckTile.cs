using System;
using Server;
using Server.Commands;

namespace Server.Misc
{
    public class CheckTileCommand
    {
        public static void Initialize()
        {
            CommandSystem.Register("CheckTile", AccessLevel.GameMaster, new CommandEventHandler(CheckTile_OnCommand));
        }

        [Usage("CheckTile")]
        private static void CheckTile_OnCommand(CommandEventArgs e)
        {
            try
            {
                int id = 585;
                bool isImp = TileData.LandTable[id & 0x3FFF].Flags.HasFlag(TileFlag.Impassable);
                e.Mobile.SendMessage($"Tile {id} Impassable: {isImp}");
            }
            catch (Exception ex)
            {
                e.Mobile.SendMessage($"Error: {ex.Message}");
            }
        }
    }
}
