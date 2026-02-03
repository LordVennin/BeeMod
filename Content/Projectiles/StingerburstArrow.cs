using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.Audio;
using Terraria.ModLoader;

namespace VenninBeeMod.Content.Projectiles
{
    public class StingerburstArrow : ModProjectile
    {
        private const int ShardCount = 3;
        private const int StickDuration = 150;
        private const float StuckFlag = 1f;
        private const float SplitFlag = 1f;

        public override void SetDefaults()
        {
            Projectile.CloneDefaults(ProjectileID.WoodenArrowFriendly);
            Projectile.width = Math.Max(1, (int)(Projectile.width * 0.18f));
            Projectile.height = Math.Max(1, (int)(Projectile.height * 0.18f));
            Projectile.scale = 0.24f;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
        }

        public override void PostAI()
        {
            if (Projectile.localAI[0] != StuckFlag)
            {
                return;
            }

            Projectile.velocity = Vector2.Zero;
            Projectile.aiStyle = 0;
            Projectile.localAI[1]++;

            if (Projectile.localAI[1] <= StickDuration)
            {
                Lighting.AddLight(Projectile.Center, 0.07f, 0.4f, 0.07f);
                if (Main.rand.NextBool(3))
                {
                    int dustSize = Math.Max(1, (int)(Math.Min(Projectile.width, Projectile.height) * 0.6f));
                    Vector2 dustPosition = Projectile.Center - new Vector2(dustSize * 0.5f);
                    int dust = Dust.NewDust(dustPosition, dustSize, dustSize, DustID.GreenTorch, 0f, 0f, 150, default, 1.1f);
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
            if (Projectile.localAI[0] == StuckFlag)
            {
                return false;
            }

            Vector2 normal = -oldVelocity.SafeNormalize(Vector2.UnitY);
            Projectile.localAI[0] = StuckFlag;
            Projectile.localAI[1] = 0f;
            Projectile.ai[0] = normal.ToRotation();
            Projectile.velocity = Vector2.Zero;
            Projectile.tileCollide = false;
            Projectile.aiStyle = 0;
            Projectile.netUpdate = true;
            return false;
        }

        private Vector2 GetShardBaseDirection()
        {
            if (Projectile.localAI[0] != StuckFlag)
            {
                return Projectile.velocity.SafeNormalize(Vector2.UnitX);
            }

            return Projectile.ai[0].ToRotationVector2();
        }

        private void SplitIntoShards(Vector2 baseDirection)
        {
            if (Projectile.ai[1] == SplitFlag || Main.myPlayer != Projectile.owner)
            {
                return;
            }

            Projectile.ai[1] = SplitFlag;
            SoundEngine.PlaySound(SoundID.Item14, Projectile.Center);

            for (int i = 0; i < ShardCount; i++)
            {
                Vector2 velocity = baseDirection.RotatedByRandom(MathHelper.ToRadians(25f)) * Main.rand.NextFloat(11f, 14f);
                int damage = (int)(Projectile.damage * 0.5f);
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, velocity, ModContent.ProjectileType<StingerburstShard>(), damage, Projectile.knockBack * 0.6f, Projectile.owner);
            }
        }
    }
}
