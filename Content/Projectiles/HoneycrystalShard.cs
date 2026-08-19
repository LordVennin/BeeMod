using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent;

namespace VenninBeeMod.Content.Projectiles
{
    public class HoneycrystalShard : ModProjectile
    {
        private const int BurstDelay = 120;
        private const int BeeCount = 3;
        private const int CountdownDustInterval = 6;
        private const int CountdownDustCount = 2;
        // Trimmed sprite dimensions and offsets within the texture (pixels).
        private const int SpriteWidth = 12;
        private const int SpriteHeight = 12;
        private const int SpriteOffsetX = 19;
        private const int SpriteOffsetY = 30;
        private const int ExplodeFlag = 1;
        private const int RotationInitFlag = 0;

        /// <summary>
        /// Hive Pack secret: instead of shattering on impact the shard hooks into whatever it hit
        /// and rides along as a barb, cooking off on a much shorter fuse. Same three bees, but
        /// they hatch inside the target rather than wherever the shard happened to stop.
        /// </summary>
        private const int BarbFuse = 45;
        private const int BarbDustInterval = 4;

        /// <summary>Index of the host NPC plus one, or 0 while the shard is not lodged.</summary>
        private ref float LodgedHost => ref Projectile.ai[2];

        private Vector2 lodgeOffset;


        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            float spriteCenterX = SpriteOffsetX + (SpriteWidth - 1) / 2f;
            float spriteCenterY = SpriteOffsetY + (SpriteHeight - 1) / 2f;
            Vector2 origin = new Vector2(spriteCenterX, spriteCenterY);
            Vector2 position = Projectile.Center - Main.screenPosition;
            SpriteEffects effects = Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            Main.spriteBatch.Draw(texture, position, null, lightColor, Projectile.rotation, origin, Projectile.scale, effects, 0f);
            return false;
        }



        public override void SetDefaults()
        {
			// Hitbox should match the sprite pixel dimensions.
			Projectile.width = SpriteWidth;
			Projectile.height = SpriteHeight;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 180;
            Projectile.aiStyle = 0;
            Projectile.tileCollide = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }

        public override void OnSpawn(IEntitySource source)
        {
            LodgedHost = 0f;
            lodgeOffset = Vector2.Zero;

            Vector2 center = Projectile.Center;
            Projectile.Resize(SpriteWidth, SpriteHeight);
            Projectile.Center = center;
            ApplySpriteHitboxAlignment();
        }

        public override void AI()
        {
            if (LodgedHost > 0f)
            {
                UpdateLodged();
                return;
            }

            if (Projectile.localAI[0] == 0f)
            {
                if (Projectile.ai[RotationInitFlag] == 0f)
                {
                    Projectile.rotation = Main.rand.NextFloat(MathHelper.TwoPi);
                    Projectile.ai[RotationInitFlag] = 1f;
                }

                Projectile.velocity.Y += 0.2f;
                return;
            }

            Projectile.velocity = Vector2.Zero;

            Projectile.ai[1]++;
            if (Projectile.ai[1] % CountdownDustInterval == 0f)
            {
                for (int i = 0; i < CountdownDustCount; i++)
                {
                    Vector2 dustOffset = Main.rand.NextVector2CircularEdge(Projectile.width * 0.6f, Projectile.height * 0.6f);
                    Dust dust = Dust.NewDustPerfect(Projectile.Center + dustOffset, DustID.Honey);
                    dust.velocity = dustOffset.SafeNormalize(Vector2.UnitY) * Main.rand.NextFloat(0.4f, 1.2f);
                    dust.noGravity = true;
                }
            }
            if (Projectile.ai[1] >= BurstDelay)
            {
                Explode();
            }
        }

        public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac)
        {
            width = SpriteWidth;
            height = SpriteHeight;
            return true;
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (Projectile.localAI[0] == 0f)
            {
                StickInPlace();
            }

            return false;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (LodgedHost == 0f && CanLodgeIn(target))
            {
                LodgeIn(target);
                return;
            }

            Explode();
        }

        private bool CanLodgeIn(NPC target)
        {
            return target.active
                && !target.dontTakeDamage
                && !target.friendly
                && HivePack.IsEquipped(Main.player[Projectile.owner]);
        }

        private void LodgeIn(NPC target)
        {
            LodgedHost = target.whoAmI + 1;
            lodgeOffset = Projectile.Center - target.Center;

            // Keep the barb inside the silhouette rather than pinned to the exact contact point,
            // which on a fast shard can be a hitbox-corner clip well off the sprite.
            float maxReach = System.Math.Min(target.width, target.height) * 0.35f;
            if (lodgeOffset.Length() > maxReach)
            {
                lodgeOffset = lodgeOffset.SafeNormalize(Vector2.UnitY) * maxReach;
            }

            Projectile.ai[1] = 0f;
            Projectile.velocity = Vector2.Zero;
            Projectile.friendly = false;
            Projectile.tileCollide = false;
            Projectile.localAI[0] = 1f;
            Projectile.timeLeft = BarbFuse + 30;
            Projectile.netUpdate = true;

            SoundEngine.PlaySound(SoundID.Item10, Projectile.Center);
        }

        private void UpdateLodged()
        {
            int index = (int)LodgedHost - 1;
            NPC host = index >= 0 && index < Main.maxNPCs ? Main.npc[index] : null;

            // Host gone means the bees have nothing to hatch into, so pop right there.
            if (host == null || !host.active || host.life <= 0)
            {
                Explode();
                return;
            }

            Projectile.Center = host.Center + lodgeOffset;
            Projectile.velocity = Vector2.Zero;
            Projectile.rotation += 0.08f;

            Projectile.ai[1]++;
            if (Projectile.ai[1] % BarbDustInterval == 0f)
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.Honey);
                dust.velocity = Main.rand.NextVector2Circular(1.2f, 1.2f);
                dust.noGravity = true;
            }

            if (Projectile.ai[1] >= BarbFuse)
            {
                Explode();
            }
        }

        public override void OnKill(int timeLeft)
        {
            Explode();
        }

        private void StickInPlace()
        {
            Projectile.localAI[0] = 1f;
            Projectile.velocity = Vector2.Zero;
            Projectile.tileCollide = false;
            Projectile.friendly = false;
            Projectile.netUpdate = true;
        }

        private void Explode()
        {
            if (Projectile.localAI[ExplodeFlag] == 1f)
            {
                return;
            }

            Projectile.localAI[ExplodeFlag] = 1f;
            SoundEngine.PlaySound(SoundID.Item14, Projectile.Center);

            if (Main.myPlayer == Projectile.owner)
            {
                for (int i = 0; i < BeeCount; i++)
                {
                    Vector2 velocity = Main.rand.NextVector2Circular(3f, 3f);
                    int damage = (int)(Projectile.damage * 0.4f);
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, velocity, ModContent.ProjectileType<HoneycrystalBee>(), damage, 0f, Projectile.owner);
                }
            }

            for (int i = 0; i < 20; i++)
            {
                int dust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Honey);
                Main.dust[dust].velocity = Main.rand.NextVector2Circular(2.4f, 2.4f);
                Main.dust[dust].noGravity = true;
                Main.dust[dust].scale = Main.rand.NextFloat(1.1f, 1.6f);
            }

            Projectile.Kill();
        }

        private void ApplySpriteHitboxAlignment()
        {
            float spriteCenterX = SpriteOffsetX + (SpriteWidth - 1) / 2f;
            float spriteCenterY = SpriteOffsetY + (SpriteHeight - 1) / 2f;
            float textureCenterX = TextureAssets.Projectile[Type].Width() / 2f;
            float textureCenterY = TextureAssets.Projectile[Type].Height() / 2f;

            DrawOriginOffsetX = (int)Math.Round(spriteCenterX - textureCenterX);
            DrawOriginOffsetY = (int)Math.Round(spriteCenterY - textureCenterY);
        }
    }
}
