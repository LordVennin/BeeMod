using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VenninBeeMod.Content.Projectiles
{
    public class StingerburstArrow : ModProjectile
    {
        private const int ShardCount = 3;

        public override void SetDefaults()
        {
            Projectile.CloneDefaults(ProjectileID.WoodenArrowFriendly);
            Projectile.width = Math.Max(1, (int)(Projectile.width * 0.2f));
            Projectile.height = Math.Max(1, (int)(Projectile.height * 0.2f));
            Projectile.scale = 0.2f;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            SplitIntoShards();
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            SplitIntoShards();
            return true;
        }

        private void SplitIntoShards()
        {
            if (Projectile.localAI[0] == 1f || Main.myPlayer != Projectile.owner)
            {
                return;
            }

            Projectile.localAI[0] = 1f;

            for (int i = 0; i < ShardCount; i++)
            {
                Vector2 velocity = Projectile.velocity.RotatedByRandom(MathHelper.ToRadians(25f)) * Main.rand.NextFloat(0.6f, 0.9f);
                int damage = (int)(Projectile.damage * 0.5f);
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, velocity, ModContent.ProjectileType<StingerburstShard>(), damage, Projectile.knockBack * 0.6f, Projectile.owner);
            }
        }
    }
}
