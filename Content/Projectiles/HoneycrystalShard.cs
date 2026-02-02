using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace VenninBeeMod.Content.Projectiles
{
    public class HoneycrystalShard : ModProjectile
    {
        private const int BurstDelay = 30;
        private const int BeeCount = 3;
        private const float GravityStrength = 0.2f;
        private const float VisualScale = 0.65f;
        private const int StickAdjustSteps = 12;

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.scale = VisualScale;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 180;
            Projectile.aiStyle = 0;
            Projectile.tileCollide = true;
        }

        public override void OnSpawn(IEntitySource source)
        {
            Projectile.rotation = Main.rand.NextFloat(MathHelper.TwoPi);
        }

        public override void AI()
        {
            bool isStuck = Projectile.localAI[0] == 1f;

            if (!isStuck)
            {
                Projectile.velocity.Y += GravityStrength;
                return;
            }

            Projectile.velocity = Vector2.Zero;

            Projectile.localAI[1]++;
            if (Projectile.localAI[1] >= BurstDelay)
            {
                ExplodeIntoBees();
            }
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (Projectile.localAI[0] == 0f)
            {
                StickInPlace(oldVelocity);
            }

            return false;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Projectile.localAI[0] == 0f)
            {
                StickInPlace();
            }
        }

        private void StickInPlace(Vector2? collisionVelocity = null)
        {
            if (collisionVelocity.HasValue && collisionVelocity.Value != Vector2.Zero)
            {
                Vector2 step = collisionVelocity.Value.SafeNormalize(Vector2.Zero);
                Vector2 adjustedPosition = Projectile.position;

                for (int i = 0; i < StickAdjustSteps; i++)
                {
                    if (!Collision.SolidCollision(adjustedPosition, Projectile.width, Projectile.height))
                    {
                        break;
                    }

                    adjustedPosition -= step;
                }

                Projectile.position = adjustedPosition;
            }

            Projectile.localAI[0] = 1f;
            Projectile.velocity = Vector2.Zero;
            Projectile.tileCollide = false;
            Projectile.friendly = false;
            Projectile.netUpdate = true;
        }

        private void ExplodeIntoBees()
        {
            SoundEngine.PlaySound(SoundID.Item14, Projectile.position);

            if (Main.myPlayer == Projectile.owner)
            {
                for (int i = 0; i < BeeCount; i++)
                {
                    Vector2 velocity = Main.rand.NextVector2Circular(3f, 3f);
                    int damage = (int)(Projectile.damage * 0.4f);
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, velocity, ProjectileID.Bee, damage, 0f, Projectile.owner);
                }
            }

            for (int i = 0; i < 12; i++)
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Honey, 0f, 0f, 150, default, 1.2f);
                dust.velocity = Main.rand.NextVector2Circular(2.4f, 2.4f);
                dust.noGravity = false;
            }

            for (int i = 0; i < 8; i++)
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Honey, 0f, 0f, 100, default, 1.4f);
                dust.velocity = Main.rand.NextVector2Circular(3.2f, 3.2f);
                dust.noGravity = true;
            }

            Projectile.Kill();
        }
    }
}
