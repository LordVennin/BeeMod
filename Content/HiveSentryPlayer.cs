using Terraria;
using Terraria.ModLoader;
using VenninBeeMod.Content.Projectiles;

namespace VenninBeeMod.Content
{
    /// <summary>
    /// Hive Pack secret for the Makeshift Hive Staff: its hives only take up half a sentry slot,
    /// so you can field twice as many as your gear normally allows.
    /// </summary>
    /// <remarks>
    /// Vanilla sentry slots are whole numbers - <see cref="Player.maxTurrets"/> is an int and
    /// <c>Player.UpdateMaxTurrets</c> simply counts active sentries against it. So instead of
    /// making a hive cost 0.5, the capacity is raised by one for every two hives already out,
    /// which works out to the same thing. It also self-limits: with a base of B slots, hives can
    /// only reach 2B before the capacity stops growing, so this is a doubling and not the
    /// unlimited spam that skipping slot consumption outright would allow.
    /// </remarks>
    public class HiveSentryPlayer : ModPlayer
    {
        public override void PostUpdateEquips()
        {
            if (!HivePack.IsEquipped(Player))
            {
                return;
            }

            int hives = 0;
            int hiveType = ModContent.ProjectileType<HiveTurret>();

            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile projectile = Main.projectile[i];
                if (projectile.active && projectile.owner == Player.whoAmI && projectile.type == hiveType)
                {
                    hives++;
                }
            }

            // Round up, so the very first hive already pays for itself and a second one fits.
            Player.maxTurrets += (hives + 1) / 2;
        }
    }
}
