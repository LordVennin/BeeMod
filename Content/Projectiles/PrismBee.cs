using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VenninBeeMod.Content.Projectiles
{
    /// <summary>
    /// The bee let out by both the Prismhive Beacon and the Combcaster. It hunts anything close
    /// by and refracts a little light as it goes.
    /// </summary>
    public class PrismBee : ModProjectile, IBeeProjectile
    {
        private const float ChaseSpeed = 11f;
        private const float ChaseInertia = 7f;
        private const float SeekRange = 620f;
        private const int ArmDuration = 10;

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
            Projectile.DamageType = DamageClass.Summon;
            Projectile.penetrate = 2;
            Projectile.timeLeft = 300;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 20;
        }

        public override void AI()
        {
            ArmTimer++;
            Projectile.friendly = ArmTimer >= ArmDuration;

            if (!Projectile.friendly)
            {
                Projectile.velocity *= 0.94f;
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
                }
            }

            Projectile.frameCounter++;
            if (Projectile.frameCounter >= 4)
            {
                Projectile.frameCounter = 0;
                Projectile.frame = (Projectile.frame + 1) % Main.projFrames[Projectile.type];
            }

            Projectile.rotation = 0f;
            if (Projectile.velocity.X > 0.15f)
            {
                Projectile.spriteDirection = 1;
            }
            else if (Projectile.velocity.X < -0.15f)
            {
                Projectile.spriteDirection = -1;
            }

            Lighting.AddLight(Projectile.Center, 0.25f, 0.4f, 0.5f);

            if (Main.rand.NextBool(10))
            {
                Dust shimmer = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height,
                    DustID.RainbowMk2, 0f, 0f, 120, new Color(180, 230, 255), 0.7f);
                shimmer.noGravity = true;
            }
        }

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
    }
}
