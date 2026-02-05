using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VenninBeeMod.Content.Projectiles
{
    public class QueenMurmurBee : ModProjectile
    {
        public override string Texture => "VenninBeeMod/Content/Projectiles/BeeFollowerMinion";

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 4;
        }

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 180;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];

            if (!player.active || player.dead)
            {
                Projectile.Kill();
                return;
            }

            if (Projectile.ai[0] == 0f)
            {
                ChargingAI(player);
            }
            else
            {
                ReleasedAI();
            }

            AnimateFrames();
        }

        private void ChargingAI(Player player)
        {
            if (Projectile.localAI[0] == 0f)
            {
                Projectile.localAI[0] = Main.rand.NextFloat(0f, MathHelper.TwoPi);
                Projectile.localAI[1] = Main.rand.NextFloat(26f, 52f);
            }

            float angle = Projectile.localAI[0] + Main.GameUpdateCount * 0.18f;
            float radius = Projectile.localAI[1] + (float)System.Math.Sin(Main.GameUpdateCount * 0.25f + Projectile.whoAmI) * 8f;
            Vector2 targetPosition = player.Center + angle.ToRotationVector2() * radius;
            Vector2 toTarget = targetPosition - Projectile.Center;

            Vector2 desiredVelocity = toTarget * 0.25f;
            Vector2 jitter = Main.rand.NextVector2Circular(1.4f, 1.4f);
            Projectile.velocity = Vector2.Lerp(Projectile.velocity, desiredVelocity, 0.35f) + jitter * 0.1f;
            UpdateFacing();
            Projectile.tileCollide = false;
            Projectile.timeLeft = 2;
        }

        private void ReleasedAI()
        {
            Projectile.ai[1]++;
            Projectile.tileCollide = Projectile.ai[1] >= 0f;
            UpdateFacing();
        }

        private void AnimateFrames()
        {
            Projectile.frameCounter++;
            if (Projectile.frameCounter >= 6)
            {
                Projectile.frameCounter = 0;
                Projectile.frame = (Projectile.frame + 1) % Main.projFrames[Projectile.type];
            }
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            return true;
        }

        private void UpdateFacing()
        {
            Projectile.rotation = 0f;
            if (Projectile.velocity.X > 0.15f)
            {
                Projectile.spriteDirection = -1;
            }
            else if (Projectile.velocity.X < -0.15f)
            {
                Projectile.spriteDirection = 1;
            }
        }
    }
}
