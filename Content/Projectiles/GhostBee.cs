using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VenninBeeMod.Content.Projectiles
{
    /// <summary>
    /// One bee of the Phantom Hivebow's firing line. It is a platform, not a weapon: it holds
    /// station above the cursor, spits two stingers down in a wide inverted V, then fades.
    /// </summary>
    public class GhostBee : ModProjectile
    {
        public const int Lifetime = 34;

        private const int FireDelay = 8;
        private const float StingerSpeed = 11f;
        private const float SpreadDegrees = 26f;

        private ref float Slot => ref Projectile.ai[0];
        private ref float Fired => ref Projectile.ai[1];

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 4;
        }

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;

            // Purely a firing platform, so it neither deals nor takes contact damage.
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = -1;
            Projectile.timeLeft = Lifetime;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.alpha = 90;
        }

        public override void AI()
        {
            int age = Lifetime - Projectile.timeLeft;

            // Gentle bob, offset per slot so the line is not in lockstep.
            float bob = (float)System.Math.Sin((age * 0.28f) + (Slot * 1.1f));
            Projectile.velocity = new Vector2(0f, bob * 0.32f);

            // Fade in at the start and back out at the end.
            if (age < 5)
            {
                Projectile.alpha = (int)MathHelper.Lerp(255f, 90f, age / 5f);
            }
            else if (Projectile.timeLeft < 8)
            {
                Projectile.alpha = (int)MathHelper.Lerp(255f, 90f, Projectile.timeLeft / 8f);
            }

            if (age >= FireDelay && Fired == 0f)
            {
                Fired = 1f;
                FireVolley();
            }

            AnimateFrames();
        }

        private void FireVolley()
        {
            if (Main.myPlayer != Projectile.owner)
            {
                return;
            }

            int stingerType = ModContent.ProjectileType<GhostStinger>();
            float spread = MathHelper.ToRadians(SpreadDegrees);

            // The inverted V: one arm down and left, the other down and right.
            foreach (float sign in new[] { -1f, 1f })
            {
                Vector2 shot = Vector2.UnitY.RotatedBy(spread * sign) * StingerSpeed;
                Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    Projectile.Center,
                    shot,
                    stingerType,
                    Projectile.damage,
                    Projectile.knockBack,
                    Projectile.owner);
            }
        }

        private void AnimateFrames()
        {
            Projectile.frameCounter++;
            if (Projectile.frameCounter >= 4)
            {
                Projectile.frameCounter = 0;
                Projectile.frame = (Projectile.frame + 1) % Main.projFrames[Projectile.type];
            }
        }

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 3; i++)
            {
                Dust fade = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height,
                    DustID.Smoke, 0f, 0f, 170, new Color(180, 235, 248), 0.6f);
                fade.noGravity = true;
            }
        }
    }
}
