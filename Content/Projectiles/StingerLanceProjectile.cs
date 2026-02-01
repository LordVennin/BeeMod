using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System;
using Microsoft.Xna.Framework.Graphics;


namespace VenninBeeMod.Content.Projectiles
{
    public class StingerLanceProjectile : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            // Use localization file for name if needed
        }

        public override void SetDefaults()
        {
            Projectile.width = 64;
            Projectile.height = 64;
            Projectile.scale = 2f;
            Projectile.aiStyle = 0;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.tileCollide = false;
            Projectile.ownerHitCheck = true; // Important for melee collision
            Projectile.hide = false;
            Projectile.ignoreWater = true;

            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 13;
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            int duration = player.itemAnimationMax;

            player.heldProj = Projectile.whoAmI;

            if (Projectile.timeLeft > duration)
            {
                Projectile.timeLeft = duration;
            }

            // Store direction in velocity
            Projectile.velocity = Vector2.Normalize(Projectile.velocity);

            float halfDuration = duration * 0.5f;
            float progress;

            if (Projectile.timeLeft < halfDuration)
                progress = Projectile.timeLeft / halfDuration;
            else
                progress = (duration - Projectile.timeLeft) / halfDuration;

            float holdoutMin = 24f;
            float holdoutMax = 96f;

            // Move the spear smoothly from min to max range
            Projectile.Center = player.MountedCenter + Vector2.SmoothStep(
                Projectile.velocity * holdoutMin,
                Projectile.velocity * holdoutMax,
                progress
            );

            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.ToRadians(135f);

            // Rotate the spear based on facing direction
            if (Projectile.spriteDirection == -1)
                Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.ToRadians(45f);
            else
                Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.ToRadians(135f);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            SpriteEffects effects = Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            Vector2 drawOrigin = new Vector2(texture.Width / 2f, texture.Height / 2f);
            Vector2 drawPosition = Projectile.Center - Main.screenPosition;

            // Offset the sprite backward visually (e.g., 16 pixels in the opposite direction of travel)
            Vector2 visualOffset = -Vector2.Normalize(Projectile.velocity) * 16f; // tweak value as needed
            drawPosition += visualOffset;

            Main.EntitySpriteDraw(
                texture,
                drawPosition,
                null,
                lightColor,
                Projectile.rotation,
                drawOrigin,
                Projectile.scale,
                effects,
                0f
            );

            return false; // skip default draw
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float tipOffset = 50f;
            Vector2 spearTip = Projectile.Center + Vector2.Normalize(Projectile.velocity) * tipOffset;

            // Scaled sprite is visually 128x128, so a 36x36 tip hitbox feels right
            Rectangle tipHitbox = new Rectangle((int)spearTip.X - 18, (int)spearTip.Y - 18, 36, 36);

            return tipHitbox.Intersects(targetHitbox);
        }





        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Player player = Main.player[Projectile.owner];

            if (HasHivePack(player))
            {
              target.AddBuff(BuffID.Poisoned, 240); // 4 seconds of Venom
            }

            
        }

        private bool HasHivePack(Player player)
        {
            for (int i = 3; i < 10; i++) // accessory slots
            {
                if (player.armor[i].type == ItemID.HiveBackpack)
                    return true;
            }
            return false;
        }

    }
}