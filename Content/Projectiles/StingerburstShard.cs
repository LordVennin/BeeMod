using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VenninBeeMod.Content.Projectiles
{
    public class StingerburstShard : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.CloneDefaults(ProjectileID.WoodenArrowFriendly);
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 120;
        }
    }
}
