using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VenninBeeMod.Content.Projectiles
{
    /// <summary>
    /// A loose bee from the Stray Queen Bee accessory. It never looks for a target, it just
    /// mills around its owner; anything that walks into it gets stung once and the bee is gone.
    /// </summary>
    public class StrayBee : ModProjectile
    {
        private const float MinimumDistance = 20f;

        private ref float Phase => ref Projectile.ai[0];

        public override string Texture => "VenninBeeMod/Content/Projectiles/BeeFollowerMinion";

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 4;
        }

        public override void SetDefaults()
        {
            Projectile.width = 12;
            Projectile.height = 12;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Generic;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 120;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.netImportant = true;
        }

        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];
            if (!owner.active || owner.dead || !owner.GetModPlayer<StrayQueenBeePlayer>().strayQueenBee)
            {
                Projectile.Kill();
                return;
            }

            // Kept alive by the accessory rather than by a countdown, so unequipping clears them.
            Projectile.timeLeft = 2;

            // Re-read every tick so putting the Hive Pack on or off retunes bees already in flight.
            Projectile.damage = StrayQueenBeePlayer.CurrentBeeDamage(owner);

            SwarmAround(owner);
            AnimateFrames();
            UpdateFacing();
        }

        /// <summary>
        /// Parks the bee on a point measured from the player rather than steering it toward one.
        /// The swarm therefore travels with the player instead of chasing them, and everything
        /// you see moving is the drift of the offset itself.
        /// <para/> Deliberately never scans for NPCs, so the swarm will not chase or aggravate
        /// anything on its own.
        /// </summary>
        private void SwarmAround(Player owner)
        {
            float seed = Phase;
            float time = Main.GameUpdateCount * 0.02f;

            // Each bee draws its own radii, speeds and phases out of its spawn seed, so the
            // cloud scatters instead of every bee tracing the same shared ellipse.
            float radiusX = 30f + (Scatter(seed, 1) * 40f);
            float radiusY = 26f + (Scatter(seed, 2) * 36f);
            float speedX = 0.6f + (Scatter(seed, 3) * 1.5f);
            float speedY = 0.6f + (Scatter(seed, 4) * 1.5f);
            float phaseX = Scatter(seed, 5) * MathHelper.TwoPi;
            float phaseY = Scatter(seed, 6) * MathHelper.TwoPi;

            // Pulls some bees in tight and lets others range wide, filling the cloud out
            // instead of leaving every bee on the same rim.
            float spread = 0.55f + (Scatter(seed, 7) * 0.45f);

            Vector2 offset = new Vector2(
                ((float)System.Math.Sin((time * speedX) + phaseX) * radiusX)
                    + ((float)System.Math.Sin((time * speedX * 2.7f) + phaseY) * 12f),
                ((float)System.Math.Cos((time * speedY) + phaseY) * radiusY)
                    + ((float)System.Math.Cos((time * speedY * 3.1f) + phaseX) * 12f));

            offset *= spread;

            // Keeps the odd bee from parking inside the player sprite when its two wobble
            // terms happen to cancel out.
            float distance = offset.Length();
            if (distance < MinimumDistance)
            {
                offset = distance > 0.01f
                    ? offset * (MinimumDistance / distance)
                    : new Vector2(MinimumDistance, 0f);
            }

            offset.Y -= 12f;

            Vector2 swarmPoint = owner.Center + offset;

            // Velocity is applied to position after AI runs, so this lands the bee exactly on
            // its point this tick while still leaving a sensible facing direction behind.
            Projectile.velocity = swarmPoint - Projectile.Center;
        }

        /// <summary>
        /// Deterministic 0-1 value from the bee's spawn seed. Beats Main.rand here because the
        /// seed travels with the projectile, so every client shakes out the same flight path.
        /// </summary>
        private static float Scatter(float seed, int salt)
        {
            double value = System.Math.Sin((seed * (12.9898 + (salt * 7.233))) + (salt * 43.7)) * 43758.5453;
            return (float)(value - System.Math.Floor(value));
        }

        private void AnimateFrames()
        {
            Projectile.frameCounter++;
            if (Projectile.frameCounter >= 5)
            {
                Projectile.frameCounter = 0;
                Projectile.frame = (Projectile.frame + 1) % Main.projFrames[Projectile.type];
            }
        }

        private void UpdateFacing()
        {
            Projectile.rotation = 0f;
            if (Projectile.velocity.X > 0.15f)
            {
                Projectile.spriteDirection = -1;
            }
            else if (Projectile.velocity.X < -0.15f)
            {
                Projectile.spriteDirection = 1;
            }
        }

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 4; i++)
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Honey);
            }
        }
    }
}
