using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using VenninBeeMod.Content.Projectiles;

namespace VenninBeeMod.Content
{
    public class StrayQueenBeePlayer : ModPlayer
    {
        public const int MaxStrayBees = 15;
        public const int StrayBeeDamage = 10;

        private const int HatchInterval = 75;

        public bool strayQueenBee;

        private int hatchTimer;

        public override void ResetEffects()
        {
            strayQueenBee = false;
        }

        public override void PostUpdate()
        {
            if (!strayQueenBee)
            {
                hatchTimer = 0;
                return;
            }

            if (Main.myPlayer != Player.whoAmI)
            {
                return;
            }

            int beeType = ModContent.ProjectileType<StrayBee>();
            if (Player.ownedProjectileCounts[beeType] >= MaxStrayBees)
            {
                return;
            }

            hatchTimer++;
            if (hatchTimer < HatchInterval)
            {
                return;
            }

            hatchTimer = 0;

            Vector2 spawnPosition = Player.Center + Main.rand.NextVector2Circular(28f, 28f);
            int index = Projectile.NewProjectile(
                Player.GetSource_FromThis(),
                spawnPosition,
                Main.rand.NextVector2Circular(2f, 2f),
                beeType,
                StrayBeeDamage,
                0f,
                Player.whoAmI,
                ai0: Main.rand.NextFloat(MathHelper.TwoPi));

            if (index >= 0)
            {
                Main.projectile[index].netUpdate = true;
            }
        }
    }
}
