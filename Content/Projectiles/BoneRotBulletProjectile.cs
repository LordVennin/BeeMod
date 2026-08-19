using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VenninBeeMod.Content.Buffs;

namespace VenninBeeMod.Content.Projectiles
{
    public class BoneRotBulletProjectile : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 6;
            Projectile.height = 12;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 600;
            Projectile.tileCollide = true;
            Projectile.alpha = 20;

            // Borrow the musket ball's flight so it handles and drops like a normal bullet.
            Projectile.aiStyle = ProjAIStyleID.Arrow;
            AIType = ProjectileID.Bullet;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<BoneRot>(), BoneRot.Duration);
        }

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 3; i++)
            {
                Dust rot = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height,
                    DustID.Bone, 0f, 0f, 110, default, 0.7f);
                rot.noGravity = true;
            }
        }
    }
}
