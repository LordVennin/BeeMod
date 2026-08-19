using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VenninBeeMod.Content.Projectiles
{
    public class BuzzKillPellet : ModProjectile
    {
        private const float Gravity = 0.34f;
        private const float MaxFallSpeed = 16f;
        private const float AirDrag = 0.992f;
        private const float BeeDamageFactor = 0.7f;

        public override void SetDefaults()
        {
            Projectile.width = 8;
            Projectile.height = 8;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 300;
            Projectile.tileCollide = true;
            Projectile.aiStyle = 0;
        }

        public override void AI()
        {
            // Heavy dropoff: the shot arcs hard and the spray only reaches so far.
            Projectile.velocity.X *= AirDrag;
            Projectile.velocity.Y += Gravity;

            if (Projectile.velocity.Y > MaxFallSpeed)
            {
                Projectile.velocity.Y = MaxFallSpeed;
            }

            Lighting.AddLight(Projectile.Center, 0.45f, 0.35f, 0.08f);

            if (Main.rand.NextBool(8))
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Honey);
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Main.myPlayer != Projectile.owner)
            {
                return;
            }

            int beeDamage = (int)(Projectile.damage * BeeDamageFactor);
            if (beeDamage < 1)
            {
                beeDamage = 1;
            }

            // Pop the bee back out of the wound, then it turns and comes for whatever was hit.
            Vector2 burst = -Projectile.velocity.SafeNormalize(Vector2.UnitY).RotatedByRandom(MathHelper.ToRadians(50f))
                * Main.rand.NextFloat(3.5f, 6f);

            Projectile.NewProjectile(
                Projectile.GetSource_OnHit(target),
                Projectile.Center,
                burst,
                ModContent.ProjectileType<BuzzKillBee>(),
                beeDamage,
                0.5f,
                Projectile.owner,
                ai0: target.whoAmI);
        }

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 3; i++)
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Honey);
            }
        }
    }
}
