using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent.Creative;
using System;
using Microsoft.Xna.Framework.Graphics;

namespace VenninBeeMod.Content.Projectiles
{
    public class StingerBeeShakerProjectile : ModProjectile
    {
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
            Projectile.ownerHitCheck = true;
            Projectile.hide = false;
            Projectile.ignoreWater = true;

            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];

            if (Projectile.ai[1] == 0f)
                Projectile.ai[1] = player.itemAnimationMax;

            player.heldProj = Projectile.whoAmI;
            player.itemTime = 2;
            player.itemAnimation = 2;
            Projectile.direction = player.direction;

            // Always thrust upward (with a little wobble for life)
            Vector2 thrustDirection = new Vector2(0, -1f);
            thrustDirection = thrustDirection.RotatedBy(Math.Sin(Main.GameUpdateCount * 0.2f) * 0.05f); // slight wiggle

            float progress = Projectile.ai[0] / Projectile.ai[1];
            float distance = 50f * (float)Math.Sin(MathHelper.Pi * progress);
            float xOffset = 0f;
            if (Main.MouseWorld.X < player.Center.X)
            {
                // Mouse is to the left
                xOffset = -4f;  // Adjust as needed
            }
            else
            {
                // Mouse is to the right
                xOffset = 9f;  // Adjust as needed
            }

            Vector2 thrustOrigin = player.MountedCenter + new Vector2(xOffset, 0f);
            Projectile.Center = thrustOrigin + new Vector2(0f, -distance);
            Projectile.rotation = thrustDirection.ToRotation() + MathHelper.ToRadians(135f); // pointy spear angle

            // Optional: spawn bees while thrusting
            if (Main.myPlayer == Projectile.owner)
            {
                int beeCount = 1;
                float spawnChance = 0.04f;

                if (HasHivePack(player))
                {
                    beeCount = 1;
                    spawnChance = 0.08f;
                }

                if (Main.rand.NextFloat() < spawnChance)
                {
                    for (int i = 0; i < beeCount; i++)
                    {
                        Vector2 beeVelocity = new Vector2(Main.rand.NextFloat(-1.5f, 1.5f), -4f + Main.rand.NextFloat(-1f, 1f));
                        Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, beeVelocity,
                            ProjectileID.Bee, (int)(Projectile.damage * 0.2f), 0f, player.whoAmI);
                    }
                }
            }

            Projectile.ai[0] += 1f;
            if (Projectile.ai[0] >= Projectile.ai[1])
                Projectile.Kill();
        }

        public override void ModifyDamageHitbox(ref Rectangle hitbox)
        {
            // Shrink the hitbox inward — tune these numbers to your needs
            hitbox.Inflate(-20, -32); // width down by 40 total, height by 64 total
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            SpriteEffects effects = Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            Vector2 drawOrigin = new Vector2(texture.Width / 2f, texture.Height / 2f);
            Vector2 drawPosition = Projectile.Center - Main.screenPosition;

            // Push sprite visually downward to align base with hand
            Vector2 visualOffset = new Vector2(0f, 24f); // Adjust this value for fine tuning
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

            return false;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            // Define how far from center the spear's tip is — upward direction
            float tipOffset = 50f; // this reaches "up" from the player's hand

            // Get the point of the tip of the spear
            Vector2 spearTip = Projectile.Center + Vector2.Normalize(new Vector2(0f, -1f)) * tipOffset;

            // Define a small hitbox around the tip
            Rectangle tipHitbox = new Rectangle((int)spearTip.X - 8, (int)spearTip.Y - 8, 16, 16);

            return tipHitbox.Intersects(targetHitbox);
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