
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent;
using System;

namespace VenninBeeMod.Content.Projectiles
{
    public class HiveballProjectile : ModProjectile
    {
        private enum AIState
        {
            Spinning,
            LaunchingForward,
            Retracting
        }

        public override void SetStaticDefaults()
        {
        }

        public override void SetDefaults()
        {
            Projectile.width = 22;
            Projectile.height = 22;
            Projectile.scale = 1.2f;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.timeLeft = 600;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            float maxLaunchDistance = 240f;
            Vector2 toPlayer = player.MountedCenter - Projectile.Center;
            float distanceToPlayer = toPlayer.Length();

            if (Projectile.ai[0] == (float)AIState.Spinning)
            {
                Vector2 throwDirection = Main.MouseWorld - player.MountedCenter;

                // Clamp vertical aim a bit to avoid steep downward throws
                if (throwDirection.Y > 0)
                    throwDirection.Y *= 0.4f;

                if (!player.channel)
                {
                    Projectile.ai[0] = (float)AIState.LaunchingForward;
                    Projectile.ai[1] = 0f; // reset timer
                    Projectile.Center = player.MountedCenter; //  reset to player's hand
                    Vector2 launchDirection = Main.MouseWorld - player.MountedCenter;
                    Projectile.velocity = launchDirection.SafeNormalize(Vector2.UnitX) * 12f;
                    Projectile.netUpdate = true;
                }
                else
                {
                    float rotationSpeed = 0.4f;
                    float radius = 48f;
                    Projectile.Center = player.MountedCenter + radius * new Vector2((float)System.Math.Cos(Main.GameUpdateCount * rotationSpeed), (float)System.Math.Sin(Main.GameUpdateCount * rotationSpeed));
                    Projectile.velocity = Vector2.Zero;
                }
            }
            else if (Projectile.ai[0] == (float)AIState.LaunchingForward)
            {
                Projectile.ai[1]++; // frame counter for launch duration
                if (Projectile.ai[1] == 1f)
                {
                    ReleaseFlingBees();
                }

                if (player.channel)
                {
                    Projectile.velocity.X *= 0.98f;
                    Projectile.velocity.Y += 0.4f;
                }

                if (!player.channel && Projectile.ai[1] >= 20)
                {
                    Projectile.ai[0] = (float)AIState.Retracting;
                    Projectile.tileCollide = false;
                    Projectile.netUpdate = true;
                }

                if (distanceToPlayer > 800f)
                {
                    Projectile.ai[0] = (float)AIState.Retracting;
                    Projectile.tileCollide = false;
                    Projectile.netUpdate = true;
                }
            }
            else if (Projectile.ai[0] == (float)AIState.Retracting)
            {
                float retractSpeed = 18f;
                Vector2 direction = Vector2.Normalize(player.MountedCenter - Projectile.Center);
                Projectile.velocity = direction * retractSpeed;

                if (distanceToPlayer < 32f)
                {
                    Projectile.Kill();
                }
                else
                {
                    Projectile.timeLeft = 10; // keep alive until returned
                }
            }

            Projectile.rotation += 0.3f * (float)Projectile.direction;
        }

        private void ReleaseFlingBees()
        {
            int beeCount = Main.rand.Next(2, 4);
            for (int i = 0; i < beeCount; i++)
            {
                Vector2 velocity = Vector2.UnitY.RotatedByRandom(MathHelper.TwoPi) * Main.rand.NextFloat(3f, 5f);
                int bee = Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    Projectile.Center,
                    velocity,
                    ProjectileID.Bee,
                    Projectile.damage / 2,
                    0f,
                    Projectile.owner
                );

                if (Main.projectile.IndexInRange(bee))
                {
                    Main.projectile[bee].DamageType = DamageClass.Melee;
                    Main.projectile[bee].friendly = true;
                    Main.projectile[bee].hostile = false;
                }
            }
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            // Bounce X
            if (Math.Abs(oldVelocity.X) > 1f && Projectile.velocity.X != oldVelocity.X)
                Projectile.velocity.X = -oldVelocity.X * 0.6f;

            // Bounce Y
            if (Math.Abs(oldVelocity.Y) > 1f && Projectile.velocity.Y != oldVelocity.Y)
                Projectile.velocity.Y = -oldVelocity.Y * 0.6f;

            // Optional: play bounce sound
            Collision.HitTiles(Projectile.position + Projectile.velocity, Projectile.velocity, Projectile.width, Projectile.height);
            Terraria.Audio.SoundEngine.PlaySound(SoundID.Dig, Projectile.position);

            return false; // Don't kill the projectile
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D chainTexture = ModContent.Request<Texture2D>("VenninBeeMod/Content/Projectiles/HiveballProjectileChain").Value;
            Texture2D projectileTexture = TextureAssets.Projectile[Projectile.type].Value;

            Vector2 playerCenter = Main.player[Projectile.owner].MountedCenter;
            Vector2 center = Projectile.Center;
            Vector2 toPlayer = playerCenter - center;
            float rotation = toPlayer.ToRotation() - MathHelper.PiOver2;
            float length = toPlayer.Length();
            Vector2 unit = toPlayer.SafeNormalize(Vector2.Zero);
            Vector2 origin = new Vector2(chainTexture.Width / 2f, chainTexture.Height);

            for (float i = 0; i <= length; i += chainTexture.Height)
            {
                Vector2 drawPos = center + unit * i - Main.screenPosition;
                Main.spriteBatch.Draw(chainTexture, drawPos, null, Lighting.GetColor((drawPos + Main.screenPosition).ToTileCoordinates()), rotation, origin, 1f, SpriteEffects.None, 0f);
            }

           

            Main.EntitySpriteDraw(
                projectileTexture,
                Projectile.Center - Main.screenPosition,
                null,
                lightColor,
                Projectile.rotation,
                projectileTexture.Size() * .5f,
                1.5f,
                SpriteEffects.None,
                0f
            );

            return false;
        }

        public override void Kill(int timeLeft)
        {
            for (int i = 0; i < 5; i++)
            {
                int dust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Honey);
                Main.dust[dust].velocity *= 0.5f;
                Main.dust[dust].scale = 1.2f;
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Honey, 180);

            if (Main.rand.NextBool(3))
            {
                int bee = Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    Projectile.Center,
                    Vector2.UnitY.RotatedByRandom(MathHelper.TwoPi) * 3f,
                    ProjectileID.Bee,
                    Projectile.damage / 2,
                    0f,
                    Projectile.owner
                );

                if (Main.projectile.IndexInRange(bee))
                {
                    Main.projectile[bee].DamageType = DamageClass.Melee;
                    Main.projectile[bee].friendly = true;
                    Main.projectile[bee].hostile = false;
                }
            }
        }
    }
}
