using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;

namespace VenninBeeMod.Content.Projectiles
{
    public class StingerburstShard : ModProjectile
    {
        private const int MaxBounces = 2;
        private const float BounceSpeedMultiplier = 1.25f;

        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.Stinger;

        public override void SetDefaults()
        {
            Projectile.CloneDefaults(ProjectileID.WoodenArrowFriendly);
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.aiStyle = 0;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 120;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Projectile.ai[0]++;
            if (Projectile.ai[0] > MaxBounces)
            {
                return true;
            }

            if (Projectile.velocity.X != oldVelocity.X)
            {
                Projectile.velocity.X = -oldVelocity.X;
            }

            if (Projectile.velocity.Y != oldVelocity.Y)
            {
                Projectile.velocity.Y = -oldVelocity.Y;
            }

            Projectile.velocity *= BounceSpeedMultiplier;
            Projectile.netUpdate = true;
            return false;
        }
    }
}
