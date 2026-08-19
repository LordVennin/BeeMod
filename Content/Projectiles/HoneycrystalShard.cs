using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent;

namespace VenninBeeMod.Content.Projectiles
{
    /// <summary>
    /// Thrown off every Honeycrystal Cutlass swing. It sticks where it lands, cooks for a couple
    /// of seconds and bursts. Bare, that burst is just a spray of honey; with a Hive Backpack on
    /// it throws three splinters instead, and the bees come out of those.
    /// </summary>
    public class HoneycrystalShard : ModProjectile
    {
        private const int BurstDelay = 120;
        private const int SplinterCount = 3;
        private const int CountdownDustInterval = 6;
        private const int CountdownDustCount = 2;

        // Trimmed sprite dimensions and offsets within the texture (pixels).
        private const int SpriteWidth = 12;
        private const int SpriteHeight = 12;
        private const int SpriteOffsetX = 19;
        private const int SpriteOffsetY = 30;

        /// <summary>
        /// The hitbox is deliberately wider than the drawn crystal. A 12 pixel box on a shard
        /// that spends most of its life sitting on the floor slips straight past short enemies,
        /// so it gets a little reach in every direction. Tile collision still uses the sprite
        /// size (see <see cref="TileCollideStyle"/>) so it beds down flush against the ground.
        /// </summary>
        private const int HitboxSize = 20;

        private const int ExplodeFlag = 1;
        private const int RotationInitFlag = 0;

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
            Projectile.width = HitboxSize;
            Projectile.height = HitboxSize;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 180;
            Projectile.aiStyle = 0;
            Projectile.tileCollide = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }

        public override void AI()
        {
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
            hitboxCenterFrac = new Vector2(0.5f, 0.5f);
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

        public override void OnKill(int timeLeft)
        {
            Explode();
        }

        private void StickInPlace()
        {
            Projectile.localAI[0] = 1f;
            Projectile.velocity = Vector2.Zero;
            Projectile.tileCollide = false;

            // Stays live on the ground rather than going inert. A landed shard is still a hazard,
            // which is the only way short enemies ever run into one.
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

            if (Main.myPlayer == Projectile.owner && HivePack.IsEquipped(Main.player[Projectile.owner]))
            {
                ThrowSplinters();
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

        /// <summary>
        /// Hive Pack secret: the burst lobs three crystal splinters instead of hatching anything
        /// itself. They arc up and drop hard, and the bees come out of wherever they land.
        /// </summary>
        private void ThrowSplinters()
        {
            int splinterType = ModContent.ProjectileType<HoneycrystalSplinter>();
            int splinterDamage = System.Math.Max(1, (int)(Projectile.damage * 0.5f));

            for (int i = 0; i < SplinterCount; i++)
            {
                // Fanned upward so they scatter on the way down instead of landing in a stack.
                float lean = MathHelper.Lerp(-1.9f, 1.9f, i / (float)(SplinterCount - 1));
                Vector2 velocity = new Vector2(lean + Main.rand.NextFloat(-0.5f, 0.5f), Main.rand.NextFloat(-6.4f, -4.6f));

                int index = Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    Projectile.Center,
                    velocity,
                    splinterType,
                    splinterDamage,
                    0f,
                    Projectile.owner);

                if (index >= 0)
                {
                    Main.projectile[index].netUpdate = true;
                }
            }
        }
    }
}
