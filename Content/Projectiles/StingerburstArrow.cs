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
        private const int StickDuration = 150;

        public override void SetDefaults()
        {
            Projectile.CloneDefaults(ProjectileID.WoodenArrowFriendly);
            Projectile.width = Math.Max(1, (int)(Projectile.width * 0.24f));
            Projectile.height = Math.Max(1, (int)(Projectile.height * 0.24f));
            Projectile.scale = 0.24f;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
        }

        public override void PostAI()
        {
            if (Projectile.ai[0] != 1f)
            {
                return;
            }

            Projectile.velocity = Vector2.Zero;
            Projectile.ai[1]++;

            if (Projectile.ai[1] <= StickDuration)
            {
                Lighting.AddLight(Projectile.Center, 0.05f, 0.35f, 0.05f);
                if (Main.rand.NextBool(3))
                {
                    int dust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.GreenTorch, 0f, 0f, 150, default, 1.1f);
                    Main.dust[dust].velocity *= 0.2f;
                    Main.dust[dust].noGravity = true;
                }

                return;
            }

            SplitIntoShards(GetShardBaseDirection());
            Projectile.Kill();
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            SplitIntoShards(Projectile.velocity.SafeNormalize(Vector2.UnitX));
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (Projectile.ai[0] == 1f)
            {
                return false;
            }

            Vector2 normal = -oldVelocity.SafeNormalize(Vector2.UnitY);
            Projectile.ai[0] = 1f;
            Projectile.ai[1] = 0f;
            Projectile.localAI[1] = normal.ToRotation();
            Projectile.velocity = Vector2.Zero;
            Projectile.tileCollide = false;
            Projectile.netUpdate = true;
            return false;
        }

        private Vector2 GetShardBaseDirection()
        {
            if (Projectile.ai[0] != 1f)
            {
                return Projectile.velocity.SafeNormalize(Vector2.UnitX);
            }

            return Projectile.localAI[1].ToRotationVector2();
        }

        private void SplitIntoShards(Vector2 baseDirection)
        {
            if (Projectile.localAI[0] == 1f || Main.myPlayer != Projectile.owner)
            {
                return;
            }

            Projectile.localAI[0] = 1f;

            for (int i = 0; i < ShardCount; i++)
            {
                Vector2 velocity = baseDirection.RotatedByRandom(MathHelper.ToRadians(25f)) * Main.rand.NextFloat(11f, 14f);
                int damage = (int)(Projectile.damage * 0.5f);
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, velocity, ModContent.ProjectileType<StingerburstShard>(), damage, Projectile.knockBack * 0.6f, Projectile.owner);
            }
        }
    }
}
