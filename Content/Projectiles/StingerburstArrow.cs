using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace VenninBeeMod.Content.Projectiles
{
    public class StingerburstArrow : ModProjectile
    {
        private const float HitboxScale = 0.08f;
        private const int SpriteWidth = 100;
        private const int SpriteHeight = 100;
        private const int ShardCount = 3;
        private const int StickDuration = 150;
        private const float StuckFlag = 1f;
        private const float SplitFlag = 1f;
        private int hitboxWidth;
        private int hitboxHeight;

        public override void SetDefaults()
        {
            Projectile.CloneDefaults(ProjectileID.WoodenArrowFriendly);
            hitboxWidth = Math.Max(1, (int)(Projectile.width * HitboxScale));
            hitboxHeight = Math.Max(1, (int)(Projectile.height * HitboxScale));
            Projectile.width = hitboxWidth;
            Projectile.height = hitboxHeight;
            Projectile.scale = 0.26f;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
        }

        public override void OnSpawn(IEntitySource source)
        {
            Vector2 center = Projectile.Center;
            Projectile.Resize(hitboxWidth, hitboxHeight);
            Projectile.Center = center;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Vector2 origin = new Vector2(texture.Width / 2f, texture.Height / 2f);
            Vector2 position = Projectile.Center - Main.screenPosition;
            SpriteEffects effects = Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            Main.spriteBatch.Draw(texture, position, null, lightColor, Projectile.rotation, origin, Projectile.scale, effects, 0f);
            return false;
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
                    int dustSize = Math.Max(3, (int)(Math.Min(SpriteWidth, SpriteHeight) * 0.08f));
                    Vector2 dustOffset = Main.rand.NextVector2Circular(dustSize * 0.6f, dustSize * 0.6f);
                    Vector2 dustPosition = Projectile.Center + dustOffset - new Vector2(dustSize * 0.5f);
                    int dust = Dust.NewDust(dustPosition, dustSize, dustSize, DustID.GreenTorch, 0f, 0f, 150, default, 1.7f);
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
            SpawnSplitDust();

            for (int i = 0; i < ShardCount; i++)
            {
                Vector2 velocity = baseDirection.RotatedByRandom(MathHelper.ToRadians(25f)) * Main.rand.NextFloat(11f, 14f);
                int damage = (int)(Projectile.damage * 0.5f);
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, velocity, ModContent.ProjectileType<StingerburstShard>(), damage, Projectile.knockBack * 0.6f, Projectile.owner);
            }
        }

        private void SpawnSplitDust()
        {
            for (int i = 0; i < 16; i++)
            {
                Vector2 dustOffset = Main.rand.NextVector2Circular(10f, 10f);
                Dust dust = Dust.NewDustPerfect(Projectile.Center + dustOffset, DustID.GreenTorch);
                dust.velocity = dustOffset.SafeNormalize(Vector2.UnitY) * Main.rand.NextFloat(1.5f, 3.2f);
                dust.noGravity = true;
                dust.scale = Main.rand.NextFloat(1.4f, 2.1f);
            }
        }

    }
}
