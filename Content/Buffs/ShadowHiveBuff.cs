using Terraria;
using Terraria.ModLoader;
using VenninBeeMod.Content.Projectiles;

namespace VenninBeeMod.Content.Buffs
{
    /// <summary>
    /// Marker buff for a placed Shadow Hive. Cancelling it is how the player packs the hives
    /// back up, since sentries otherwise sit out their full lifetime.
    /// </summary>
    public class ShadowHiveBuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.buffNoSave[Type] = true;
            Main.buffNoTimeDisplay[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            if (player.ownedProjectileCounts[ModContent.ProjectileType<ShadowHiveSentry>()] > 0)
            {
                player.buffTime[buffIndex] = 18000;
                return;
            }

            // No hives left standing, so the icon should not linger.
            player.DelBuff(buffIndex);
            buffIndex--;
        }
    }
}
