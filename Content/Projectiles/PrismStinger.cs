using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace VenninBeeMod.Content.Projectiles
{
    /// <summary>
    /// Fired by the Prismhive Beacon. It travels a short way as one shard, then refracts into
    /// three that fan out - so the beacon covers ground rather than picking one target.
    /// </summary>
    public class PrismStinger : ModProjectile
    {
        /// <summary>How far it flies before splitting, in ticks.</summary>
        private const int RefractDelay = 14;

        private const int Fragments = 3;
        private const float FragmentSpread = 17f;

        /// <summary>Set on the fragments so they do not refract again.</summary>
        private ref float IsFragment => ref Projectile.ai[0];

        private ref float Age => ref Projectile.ai[1];

        public override void SetDefaults()
        {
            Projectile.width = 8;
            Projectile.height = 8;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 180;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() - MathHelper.PiOver2;
            Lighting.AddLight(Projectile.Center, 0.3f, 0.45f, 0.6f);

            if (Main.rand.NextBool(4))
            {
                Dust trail = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height,
                    DustID.RainbowMk2, 0f, 0f, 120, new Color(190, 235, 255), 0.6f);
                trail.noGravity = true;
                trail.velocity *= 0.2f;
            }

            if (IsFragment != 0f)
            {
                return;
            }

            Age++;
            if (Age >= RefractDelay)
            {
                Refract();
            }
        }

        private void Refract()
        {
            if (Main.myPlayer == Projectile.owner)
            {
                for (int i = 0; i < Fragments; i++)
                {
                    float lean = MathHelper.Lerp(-FragmentSpread, FragmentSpread, i / (float)(Fragments - 1));
                    Vector2 velocity = Projectile.velocity.RotatedBy(MathHelper.ToRadians(lean));

                    int index = Projectile.NewProjectile(
                        Projectile.GetSource_FromThis(),
                        Projectile.Center,
                        velocity,
                        Projectile.type,
                        System.Math.Max(1, (int)(Projectile.damage * 0.6f)),
                        Projectile.knockBack * 0.5f,
                        Projectile.owner,
                        ai0: 1f);

                    if (index >= 0)
                    {
                        Main.projectile[index].netUpdate = true;
                    }
                }
            }

            SoundEngine.PlaySound(SoundID.Item27 with { Volume = 0.4f, Pitch = 0.6f }, Projectile.Center);
            Projectile.Kill();
        }

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 5; i++)
            {
                Dust shard = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height,
                    DustID.RainbowMk2, 0f, 0f, 100, new Color(210, 240, 255), 0.8f);
                shard.noGravity = true;
                shard.velocity = Main.rand.NextVector2Circular(2f, 2f);
            }
        }
    }
}
