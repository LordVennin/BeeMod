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
        private const float SwarmRadius = 62f;
        private const float DriftSpeed = 5.4f;
        private const float LeashRange = 900f;

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

            if (Vector2.Distance(Projectile.Center, owner.Center) > LeashRange)
            {
                Projectile.Center = owner.Center;
                Projectile.netUpdate = true;
            }

            Drift(owner);
            AnimateFrames();
            UpdateFacing();
        }

        /// <summary>
        /// Wanders around the owner on a couple of out-of-phase sine terms. Deliberately never
        /// scans for NPCs, so the swarm will not chase or aggravate anything on its own.
        /// </summary>
        private void Drift(Player owner)
        {
            float time = (Main.GameUpdateCount * 0.024f) + Phase;

            Vector2 wanderPoint = owner.Center
                + new Vector2((float)System.Math.Cos(time * 1.3f), (float)System.Math.Sin(time * 1.7f)) * SwarmRadius
                + new Vector2((float)System.Math.Sin(time * 2.3f), (float)System.Math.Cos(time * 0.9f)) * 20f;

            Vector2 toPoint = wanderPoint - Projectile.Center;
            Vector2 desiredVelocity = toPoint.Length() > 10f
                ? toPoint.SafeNormalize(Vector2.UnitX) * DriftSpeed
                : toPoint * 0.4f;

            Projectile.velocity = Vector2.Lerp(Projectile.velocity, desiredVelocity, 0.11f);
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
