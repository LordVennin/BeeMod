using Terraria;
using Terraria.ModLoader;
using VenninBeeMod.Content.Projectiles;

namespace VenninBeeMod.Content.Buffs
{
    /// <summary>
    /// Marker buff for a planted Prismhive, matching the Shadow Hive. Cancelling it packs the
    /// hives away, which is otherwise impossible short of waiting out the sentry timer.
    /// </summary>
    public class PrismhiveBuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.buffNoSave[Type] = true;
            Main.buffNoTimeDisplay[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            if (player.ownedProjectileCounts[ModContent.ProjectileType<PrismhiveSentry>()] > 0)
            {
                player.buffTime[buffIndex] = 18000;
                return;
            }

            // Nothing standing, so the icon should not linger.
            player.DelBuff(buffIndex);
            buffIndex--;
        }
    }
}
