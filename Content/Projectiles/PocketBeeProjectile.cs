using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VenninBeeMod.Content.Projectiles
{
    public class PocketBeeProjectile : ModProjectile
    {
        private float rotationDirection;

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 4; // 4-frame animation
        }

        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 40; // Very short lifespan
            Projectile.tileCollide = true;
            Projectile.ignoreWater = false;
            Projectile.alpha = 0;
        }

        public override void OnSpawn(Terraria.DataStructures.IEntitySource source)
        {
            Projectile.rotation = Main.rand.NextFloat(MathHelper.TwoPi); // random initial rotation
            rotationDirection = Main.rand.NextBool() ? 1f : -1f; // random clockwise or counterclockwise
        }

        public override void AI()
        {
            // Animation
            Projectile.frameCounter++;
            if (Projectile.frameCounter >= 4)
            {
                Projectile.frameCounter = 0;
                Projectile.frame++;
                if (Projectile.frame >= Main.projFrames[Projectile.type])
                    Projectile.frame = 0;
            }

            // Light yellow glow
            Lighting.AddLight(Projectile.Center, 0.8f, 0.7f, 0.1f);

            // Fade out more slowly
            Projectile.alpha += 5;
            if (Projectile.alpha > 255)
                Projectile.alpha = 255;

            // Wobble movement
            Projectile.velocity.X *= 0.99f;
            Projectile.velocity.Y += 0.15f;

            // Rotate sprite
            Projectile.rotation += rotationDirection * 0.2f;
        }
    }
}
