using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VenninBeeMod.Content.Projectiles
{
    /// <summary>
    /// A heavy drone released by the Shadow Hive. It picks the nearest enemy and lumbers toward
    /// it on a weaving line rather than homing cleanly, and hurts on contact.
    /// </summary>
    public class ShadowHiveBee : ModProjectile
    {
        private const float DriftSpeed = 3.4f;
        private const float WanderSpeed = 1.8f;
        private const float SeekRange = 900f;
        private const float WobbleStrength = 2.1f;

        private ref float Phase => ref Projectile.ai[0];

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 4;
        }

        public override void SetDefaults()
        {
            Projectile.width = 26;
            Projectile.height = 26;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 900;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.alpha = 60;

            // Keeps stinging on its own clock instead of being gated by shared invincibility.
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 30;
        }

        public override void AI()
        {
            NPC target = FindTarget();

            Vector2 heading;
            if (target != null)
            {
                heading = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX) * DriftSpeed;
            }
            else
            {
                // Nothing to chase, so mill about on its own heading.
                heading = Projectile.velocity.SafeNormalize(Vector2.UnitX) * WanderSpeed;
            }

            // Weave across the line of travel so the approach reads as a drunken bee.
            Vector2 sideways = new Vector2(-heading.Y, heading.X).SafeNormalize(Vector2.Zero);
            float wobble = (float)System.Math.Sin((Main.GameUpdateCount * 0.13f) + Phase) * WobbleStrength;

            Projectile.velocity = Vector2.Lerp(Projectile.velocity, heading + (sideways * wobble), 0.07f);

            AnimateFrames();
            UpdateFacing();

            if (Main.rand.NextBool(14))
            {
                Dust haze = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height,
                    DustID.Smoke, 0f, 0f, 150, new Color(150, 100, 210), 0.8f);
                haze.noGravity = true;
            }
        }

        private NPC FindTarget()
        {
            NPC closest = null;
            float closestDistance = SeekRange;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.CanBeChasedBy(this))
                {
                    continue;
                }

                float distance = Vector2.Distance(Projectile.Center, npc.Center);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closest = npc;
                }
            }

            return closest;
        }

        private void AnimateFrames()
        {
            Projectile.frameCounter++;
            if (Projectile.frameCounter >= 6)
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
    }
}
