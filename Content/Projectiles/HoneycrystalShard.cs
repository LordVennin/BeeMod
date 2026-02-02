using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace VenninBeeMod.Content.Projectiles
{
    public class HoneycrystalShard : ModProjectile
    {
        private const int BurstDelay = 120;
        private const int BeeCount = 3;
		// Trimmed sprite dimensions (pixels)
		private const int SpriteWidth = 35;
		private const int SpriteHeight = 44;
        private const int ExplodeFlag = 2;

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
    }
}
