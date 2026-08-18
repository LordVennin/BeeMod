using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VenninBeeMod.Content.Projectiles
{
    /// <summary>
    /// One arm of a ghost bee's downward volley. Sprite points straight down, so the rotation
    /// is offset by a quarter turn to line up with its travel.
    /// </summary>
    public class GhostStinger : ModProjectile
    {
        private const float Gravity = 0.14f;
        private const float MaxFallSpeed = 15f;

        public override void SetDefaults()
        {
            Projectile.width = 8;
            Projectile.height = 14;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 240;
            Projectile.tileCollide = true;
            Projectile.alpha = 40;
        }

        public override void AI()
        {
            Projectile.velocity.Y += Gravity;
            if (Projectile.velocity.Y > MaxFallSpeed)
            {
                Projectile.velocity.Y = MaxFallSpeed;
            }

            Projectile.rotation = Projectile.velocity.ToRotation() - MathHelper.PiOver2;

            Lighting.AddLight(Projectile.Center, 0.16f, 0.32f, 0.36f);

            if (Main.rand.NextBool(10))
            {
                Dust trail = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height,
                    DustID.Smoke, 0f, 0f, 160, new Color(150, 220, 240), 0.55f);
                trail.noGravity = true;
            }
        }

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 3; i++)
            {
                Dust burst = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height,
                    DustID.Smoke, 0f, 0f, 150, new Color(170, 230, 245), 0.6f);
                burst.noGravity = true;
            }
        }
    }
}
