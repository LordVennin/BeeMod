using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VenninBeeMod.Content.Projectiles
{
    public class HoneycrystalBee : ModProjectile
    {
        private const int DustInterval = 6;
        private const int DustCount = 2;

        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.Bee;

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = Main.projFrames[ProjectileID.Bee];
        }

        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.friendly = true;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 240;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.aiStyle = 36;
            Projectile.tileCollide = true;
        }

        public override void AI()
        {
            Projectile.frameCounter++;
            if (Projectile.frameCounter >= 5)
            {
                Projectile.frameCounter = 0;
                Projectile.frame = (Projectile.frame + 1) % Main.projFrames[Projectile.type];
            }

            if (Projectile.velocity.X > 0.2f)
            {
                Projectile.spriteDirection = -1;
            }
            else if (Projectile.velocity.X < -0.2f)
            {
                Projectile.spriteDirection = 1;
            }

            if (Projectile.timeLeft % DustInterval == 0)
            {
                for (int i = 0; i < DustCount; i++)
                {
                    Vector2 offset = Main.rand.NextVector2Circular(Projectile.width * 0.4f, Projectile.height * 0.4f);
                    Dust dust = Dust.NewDustPerfect(Projectile.Center + offset, DustID.CrystalShard);
                    dust.velocity = offset.SafeNormalize(Vector2.UnitY) * Main.rand.NextFloat(0.3f, 0.9f);
                    dust.noGravity = true;
                    dust.scale = Main.rand.NextFloat(0.8f, 1.1f);
                    dust.color = new Color(110, 190, 255, 200);
                }
            }
        }
    }
}
