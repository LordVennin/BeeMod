using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VenninBeeMod.Content.Projectiles
{
    public class BuzzKillBee : ModProjectile, IBeeProjectile
    {
        private const float ChaseSpeed = 11f;
        private const float ChaseInertia = 9f;
        private const float RetargetRange = 700f;
        private const int ArmDuration = 16;

        // Set by BuzzKillPellet to the NPC the pellet hit.
        private ref float TargetIndex => ref Projectile.ai[0];
        private ref float ArmTimer => ref Projectile.ai[1];

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
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 300;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
        }

        public override void AI()
        {
            ArmTimer++;

            // Brief wind-up so the bee peels off the wound instead of instantly re-hitting it.
            bool armed = ArmTimer >= ArmDuration;
            Projectile.friendly = armed;

            if (!armed)
            {
                Projectile.velocity *= 0.93f;
            }
            else
            {
                NPC target = ResolveTarget();
                if (target != null)
                {
                    Vector2 desiredVelocity = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitY) * ChaseSpeed;
                    Projectile.velocity = (Projectile.velocity * (ChaseInertia - 1f) + desiredVelocity) / ChaseInertia;
                }
                else
                {
                    Projectile.velocity *= 0.97f;
                    Projectile.velocity.Y += 0.06f;
                }
            }

            AnimateFrames();
            UpdateFacing();

            if (Main.rand.NextBool(14))
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Honey);
            }
        }

        /// <summary>
        /// Sticks to the NPC the pellet hit. Only once that one is gone does the bee
        /// look for something else, so it does not just die with a wasted charge.
        /// </summary>
        private NPC ResolveTarget()
        {
            int index = (int)TargetIndex;
            if (index >= 0 && index < Main.maxNPCs)
            {
                NPC locked = Main.npc[index];
                if (locked.CanBeChasedBy(this))
                {
                    return locked;
                }
            }

            NPC closest = null;
            float closestDistance = RetargetRange;

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

            if (closest != null)
            {
                TargetIndex = closest.whoAmI;
            }

            return closest;
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
    }
}
