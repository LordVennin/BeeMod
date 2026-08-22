using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VenninBeeMod.Content.Projectiles
{
    /// <summary>
    /// The bee every Crimson weapon eventually produces - out of a burst grub, a lodged barb or
    /// a ruptured cyst. It hunts whatever is nearest and has a small chance to leave its target
    /// reeling.
    /// </summary>
    /// <remarks>
    /// Deliberately <see cref="DamageClass.Generic"/>. Each of the four weapons is a different
    /// class, and the damage handed to a bee has already been scaled by whichever weapon made
    /// it, so scaling it a second time by the bee's own class would double-dip.
    /// </remarks>
    public class BloodBee : ModProjectile, IBeeProjectile
    {
        private const float ChaseSpeed = 10f;
        private const float ChaseInertia = 8f;
        private const float SeekRange = 760f;
        private const int ArmDuration = 12;

        /// <summary>Set by whatever spawned the bee, as the NPC index plus one.</summary>
        private ref float PreferredTarget => ref Projectile.ai[0];

        private ref float ArmTimer => ref Projectile.ai[1];

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
            Projectile.timeLeft = 260;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
        }

        public override void AI()
        {
            ArmTimer++;

            // Short wind-up so a bee born inside an enemy peels out before it counts as a hit.
            bool armed = ArmTimer >= ArmDuration;
            Projectile.friendly = armed;

            if (!armed)
            {
                Projectile.velocity *= 0.9f;
            }
            else
            {
                NPC target = ResolveTarget();
                if (target != null)
                {
                    Vector2 desired = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitY) * ChaseSpeed;
                    Projectile.velocity = ((Projectile.velocity * (ChaseInertia - 1f)) + desired) / ChaseInertia;
                }
                else
                {
                    Projectile.velocity *= 0.98f;
                    Projectile.velocity.Y += 0.05f;
                }
            }

            AnimateFrames();
            UpdateFacing();

            if (Main.rand.NextBool(12))
            {
                Dust trail = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height,
                    DustID.Blood, 0f, 0f, 120, default, 0.8f);
                trail.noGravity = true;
            }
        }

        /// <summary>
        /// Goes for whatever made it first, then anything else in range. Bees born out of a
        /// corpse should still be worth something.
        /// </summary>
        private NPC ResolveTarget()
        {
            int index = (int)PreferredTarget - 1;
            if (index >= 0 && index < Main.maxNPCs && Main.npc[index].CanBeChasedBy(this))
            {
                return Main.npc[index];
            }

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

            if (closest != null)
            {
                PreferredTarget = closest.whoAmI + 1;
            }

            return closest;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            CrimsonBee.TryConfuse(target);
        }

        private void AnimateFrames()
        {
            Projectile.frameCounter++;
            if (Projectile.frameCounter >= 4)
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
                Projectile.spriteDirection = 1;
            }
            else if (Projectile.velocity.X < -0.15f)
            {
                Projectile.spriteDirection = -1;
            }
        }

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 4; i++)
            {
                Dust spent = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height,
                    DustID.Blood, 0f, 0f, 100, default, 0.9f);
                spent.noGravity = true;
            }
        }
    }
}
