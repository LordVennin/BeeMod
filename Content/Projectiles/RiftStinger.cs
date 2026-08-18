using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VenninBeeMod.Content.Projectiles
{
    /// <summary>
    /// A stinger spat through the rift. Flies flat and fast; the sprite points down, so the
    /// rotation carries a quarter-turn offset.
    /// </summary>
    public class RiftStinger : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 8;
            Projectile.height = 14;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 150;
            Projectile.tileCollide = true;
            Projectile.alpha = 30;
            Projectile.extraUpdates = 1;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() - MathHelper.PiOver2;
            Lighting.AddLight(Projectile.Center, 0.28f, 0.14f, 0.4f);

            if (Main.rand.NextBool(6))
            {
                Dust trail = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height,
                    DustID.Smoke, 0f, 0f, 150, new Color(168, 116, 236), 0.6f);
                trail.noGravity = true;
            }
        }

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 4; i++)
            {
                Dust burst = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height,
                    DustID.Smoke, 0f, 0f, 140, new Color(190, 150, 246), 0.7f);
                burst.noGravity = true;
            }
        }
    }
}
