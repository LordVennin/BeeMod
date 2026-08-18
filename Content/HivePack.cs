using Terraria;
using Terraria.ID;

namespace VenninBeeMod.Content
{
    /// <summary>
    /// Shared check for the vanilla Hive Pack, which several pieces of this mod treat as a
    /// quiet upgrade trigger.
    /// </summary>
    public static class HivePack
    {
        public static bool IsEquipped(Player player)
        {
            for (int i = 3; i < 10; i++)
            {
                if (player.armor[i].type == ItemID.HiveBackpack)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
