using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework;

namespace VenninBeeMod.Content.Projectiles
{
    public class BeeTrail : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Bee Trail");
            Main.projFrames[Projectile.type] = Main.projFrames[ProjectileID.Bee];
        }

        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.Bee;

        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 38000;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 45;
        }

        public override void AI()
        {
            // Start fading out after 120 ticks (2 seconds)
            if (Projectile.timeLeft < 60)
            {
                Projectile.alpha += 8;
                if (Projectile.alpha > 255)
                    Projectile.alpha = 255;
            }

            Projectile.rotation += Projectile.velocity.X * 0.05f;

            // Slowly float upward
            Projectile.velocity *= 0.95f;
            Projectile.velocity.Y -= 0.002f;

            // Animate like the bee sprite
            Projectile.frameCounter++;
            if (Projectile.frameCounter >= 6)
            {
                Projectile.frameCounter = 0;
                Projectile.frame = (Projectile.frame + 1) % Main.projFrames[Projectile.type];
            }

            // Create light and dust
            Lighting.AddLight(Projectile.Center, 0.8f, 0.7f, 0.2f);

            if (Main.rand.NextBool(6)) // less frequent
            {
                int dust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Honey);
                Main.dust[dust].noGravity = true;
                Main.dust[dust].scale = 0.8f;
                Main.dust[dust].velocity *= 0.5f; // slower movement
            }
        }
    }
}