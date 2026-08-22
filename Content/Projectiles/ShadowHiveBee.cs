using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VenninBeeMod.Content.Projectiles
{
    /// <summary>
    /// A heavy drone released by the Shadow Hive. It never leaves its hive's ring and will only
    /// go after enemies standing inside it, weaving toward them rather than homing cleanly.
    /// Two stings and it is spent.
    /// </summary>
    public class ShadowHiveBee : ModProjectile, IBeeProjectile
    {
        private const int MaxStings = 3;

        private const float DriftSpeed = 3.4f;
        private const float WanderSpeed = 1.8f;
        private const float WobbleStrength = 2.1f;

        // How far inside the rim the drone starts being pulled back toward the hive.
        private const float EdgeMargin = 30f;

        private ref float Phase => ref Projectile.ai[0];
        private ref float HiveIndex => ref Projectile.ai[1];

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

            // Spent after two stings.
            Projectile.penetrate = MaxStings;

            Projectile.timeLeft = 990;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.alpha = 60;

            // Keeps stinging on its own clock instead of being gated by shared invincibility.
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 30;
        }

        public override void AI()
        {
            Projectile hive = ResolveHive();
            if (hive == null)
            {
                // The hive it belonged to is gone, so there is no ring left to hold.
                Projectile.Kill();
                return;
            }

            Vector2 anchor = hive.Center;
            float radius = ShadowHiveSentry.RadiusFor(hive);
            NPC target = FindTargetInRing(anchor, radius);

            Vector2 heading = target != null
                ? (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX) * DriftSpeed
                : Projectile.velocity.SafeNormalize(Vector2.UnitX) * WanderSpeed;

            heading = ApplyLeash(anchor, radius, heading);

            // Weave across the line of travel so the approach reads as a drunken bee.
            Vector2 sideways = new Vector2(-heading.Y, heading.X).SafeNormalize(Vector2.Zero);
            float wobble = (float)System.Math.Sin((Main.GameUpdateCount * 0.13f) + Phase) * WobbleStrength;

            Projectile.velocity = Vector2.Lerp(Projectile.velocity, heading + (sideways * wobble), 0.07f);

            ClampToRing(anchor, radius);
            AnimateFrames();
            UpdateFacing();

            if (Main.rand.NextBool(14))
            {
                Dust haze = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height,
                    DustID.Smoke, 0f, 0f, 150, new Color(150, 100, 210), 0.8f);
                haze.noGravity = true;
            }
        }

        /// <summary>
        /// Bends the drone's heading back inward as it nears the rim, ramping up with how far
        /// out it has drifted so the turn is gradual rather than a bounce.
        /// </summary>
        private Vector2 ApplyLeash(Vector2 anchor, float radius, Vector2 heading)
        {
            float leash = radius - EdgeMargin;
            float distance = Vector2.Distance(Projectile.Center, anchor);
            if (distance <= leash)
            {
                return heading;
            }

            float pull = MathHelper.Clamp((distance - leash) / EdgeMargin, 0f, 1f);
            Vector2 inward = (anchor - Projectile.Center).SafeNormalize(Vector2.UnitX) * DriftSpeed;
            return Vector2.Lerp(heading, inward, pull);
        }

        /// <summary>
        /// Hard backstop, so no amount of wobble or knockback can carry a drone out of the ring.
        /// </summary>
        private void ClampToRing(Vector2 anchor, float radius)
        {
            Vector2 offset = Projectile.Center - anchor;
            if (offset.Length() > radius)
            {
                Projectile.Center = anchor + (offset.SafeNormalize(Vector2.UnitX) * radius);
            }
        }

        private Projectile ResolveHive()
        {
            int index = (int)HiveIndex;
            if (index < 0 || index >= Main.maxProjectiles)
            {
                return null;
            }

            Projectile hive = Main.projectile[index];
            bool valid = hive.active
                && hive.owner == Projectile.owner
                && hive.type == ModContent.ProjectileType<ShadowHiveSentry>();

            return valid ? hive : null;
        }

        private NPC FindTargetInRing(Vector2 anchor, float radius)
        {
            NPC closest = null;
            float closestDistance = float.MaxValue;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.CanBeChasedBy(this))
                {
                    continue;
                }

                // Anything standing outside the hive's ring is simply not its problem.
                if (Vector2.Distance(npc.Center, anchor) > radius)
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

        /// <summary>
        /// Second guard on the ring, so a drone cannot clip something that has just stepped out.
        /// </summary>
        public override bool? CanHitNPC(NPC target)
        {
            Projectile hive = ResolveHive();
            if (hive == null)
            {
                return false;
            }

            return Vector2.Distance(target.Center, hive.Center) <= ShadowHiveSentry.RadiusFor(hive) ? null : false;
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

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 5; i++)
            {
                Dust spent = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height,
                    DustID.Smoke, 0f, 0f, 140, new Color(160, 110, 220), 0.75f);
                spent.noGravity = true;
            }
        }
    }
}
