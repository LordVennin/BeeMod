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
        // Trimmed sprite dimensions and offsets within the texture (pixels).
        private const int SpriteWidth = 10;
        private const int SpriteHeight = 14;
        private const int SpriteOffsetX = 22;
        private const int SpriteOffsetY = 9;
        private const int ExplodeFlag = 2;


        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Rectangle source = new Rectangle(SpriteOffsetX, SpriteOffsetY, SpriteWidth, SpriteHeight);
            Vector2 origin = new Vector2(SpriteWidth / 2f, SpriteHeight / 2f);
            Vector2 position = Projectile.Center - Main.screenPosition;
            SpriteEffects effects = Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            Main.spriteBatch.Draw(texture, position, source, lightColor, Projectile.rotation, origin, Projectile.scale, effects, 0f);
            return false;
        }

        public override void PostDraw(Color lightColor)
        {
            // Draw the actual hitbox rectangle the engine uses
            Rectangle hb = Projectile.Hitbox;
            hb.Location -= (Main.screenPosition).ToPoint();

            Main.spriteBatch.Draw(
                TextureAssets.MagicPixel.Value,
                hb,
                Color.Red * 0.5f
            );
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
        }

        public override void OnSpawn(IEntitySource source)
        {
            Vector2 center = Projectile.Center;
            Projectile.Resize(SpriteWidth, SpriteHeight);
            Projectile.Center = center;
            ApplySpriteHitboxAlignment();
        }

        public override void AI()
        {
            if (Projectile.localAI[0] == 0f)
            {
                Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
                return;
            }

            Projectile.velocity = Vector2.Zero;

            Projectile.localAI[1]++;
            if (Projectile.localAI[1] >= BurstDelay)
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
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, velocity, ProjectileID.Bee, damage, 0f, Projectile.owner);
                }
            }

            for (int i = 0; i < 8; i++)
            {
                int dust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Honey);
                Main.dust[dust].velocity *= 1.2f;
                Main.dust[dust].noGravity = true;
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
