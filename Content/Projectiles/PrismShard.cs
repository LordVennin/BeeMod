using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VenninBeeMod.Content.Projectiles
{
    /// <summary>
    /// The Prism Drone's own shot. Shares the Prismhive Beacon's stinger art, since the drone
    /// is where the beacon's material comes from in the first place.
    /// </summary>
    public class PrismShard : ModProjectile
    {
        public override string Texture => "VenninBeeMod/Content/Projectiles/PrismStinger";

        public override void SetDefaults()
        {
            Projectile.width = 8;
            Projectile.height = 8;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 240;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() - MathHelper.PiOver2;
            Lighting.AddLight(Projectile.Center, 0.28f, 0.38f, 0.5f);

            if (Main.rand.NextBool(5))
            {
                Dust trail = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height,
                    DustID.RainbowMk2, 0f, 0f, 130, new Color(200, 180, 255), 0.6f);
                trail.noGravity = true;
                trail.velocity *= 0.2f;
            }
        }
    }
}
