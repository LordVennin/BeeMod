using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace VenninBeeMod.Content.Projectiles
{
    /// <summary>
    /// Thrown out of a Honeycrystal Shard's burst when the wielder has a Hive Backpack on. It is
    /// lobbed rather than fired - the drop is severe, so the splinters rain down around the shard
    /// instead of carrying. Whatever it lands on, it pops, and the bees come out of the pop.
    /// </summary>
    public class HoneycrystalSplinter : ModProjectile
    {
        private const int BeeCount = 2;

        /// <summary>Heavy enough that the arc is a lob, not a throw.</summary>
        private const float Gravity = 0.55f;
        private const float MaxFallSpeed = 15f;
        private const float AirDrag = 0.99f;

        /// <summary>Backstop so a splinter thrown over a chasm still pops eventually.</summary>
        private const int Fuse = 150;

        public override void SetDefaults()
        {
            Projectile.width = 12;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 1;
            Projectile.timeLeft = Fuse;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = false;
        }

        public override void AI()
        {
            Projectile.velocity.X *= AirDrag;
            Projectile.velocity.Y += Gravity;
            if (Projectile.velocity.Y > MaxFallSpeed)
            {
                Projectile.velocity.Y = MaxFallSpeed;
            }

            // Points the way it is travelling, so the tip leads on the way down.
            Projectile.rotation = Projectile.velocity.ToRotation() - MathHelper.PiOver2;

            if (Main.rand.NextBool(5))
            {
                Dust trail = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Honey);
                trail.velocity *= 0.2f;
                trail.noGravity = true;
                trail.scale = 0.9f;
            }
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Projectile.Kill();
            return false;
        }

        /// <summary>
        /// Bees hatch out of the pop wherever that happens - ground, wall, or the enemy it
        /// clipped on the way down.
        /// </summary>
        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item27 with { Volume = 0.6f, Pitch = 0.4f }, Projectile.Center);

            if (Main.myPlayer == Projectile.owner)
            {
                int beeDamage = System.Math.Max(1, (int)(Projectile.damage * 0.8f));
                for (int i = 0; i < BeeCount; i++)
                {
                    Vector2 velocity = Main.rand.NextVector2Circular(3f, 3f) - new Vector2(0f, 1.5f);
                    int index = Projectile.NewProjectile(
                        Projectile.GetSource_FromThis(),
                        Projectile.Center,
                        velocity,
                        ModContent.ProjectileType<HoneycrystalBee>(),
                        beeDamage,
                        0f,
                        Projectile.owner);

                    if (index >= 0)
                    {
                        Main.projectile[index].netUpdate = true;
                    }
                }
            }

            for (int i = 0; i < 12; i++)
            {
                Dust shatter = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Honey);
                shatter.velocity = Main.rand.NextVector2Circular(2.2f, 2.2f);
                shatter.noGravity = true;
                shatter.scale = Main.rand.NextFloat(1f, 1.4f);
            }
        }
    }
}
