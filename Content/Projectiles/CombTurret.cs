using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace VenninBeeMod.Content.Projectiles
{
    /// <summary>
    /// A comb lobbed out of the Combcaster. It sticks where it lands - floor, wall or ceiling -
    /// and spits bees at anything nearby until it runs dry. Three at a time, so the weapon is
    /// about where you put them rather than how fast you cast.
    /// </summary>
    public class CombTurret : ModProjectile
    {
        public const int MaxCombs = 3;

        private const int Lifetime = 480;
        private const int BeeInterval = 60;

        /// <summary>Two a spit. One comb was too slight to be worth the cast on its own.</summary>
        private const int BeesPerSpit = 2;
        private const float Range = 420f;
        private const float BeeDamageShare = 0.55f;
        private const float Gravity = 0.32f;
        private const float MaxFallSpeed = 12f;

        private const int StuckFlag = 0;

        private ref float Age => ref Projectile.ai[0];

        /// <summary>
        /// Kills the longest-lived comb once the player is at the cap, so a new cast always
        /// places rather than silently doing nothing.
        /// </summary>
        public static void RetireOldest(Player player)
        {
            if (Main.myPlayer != player.whoAmI)
            {
                return;
            }

            int type = ModContent.ProjectileType<CombTurret>();
            while (true)
            {
                int count = 0;
                Projectile oldest = null;

                for (int i = 0; i < Main.maxProjectiles; i++)
                {
                    Projectile other = Main.projectile[i];
                    if (!other.active || other.type != type || other.owner != player.whoAmI)
                    {
                        continue;
                    }

                    count++;
                    if (oldest == null || other.ai[0] > oldest.ai[0])
                    {
                        oldest = other;
                    }
                }

                if (count < MaxCombs || oldest == null)
                {
                    return;
                }

                oldest.Kill();
            }
        }

        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;

            // The bees carry the damage; the comb is a nest.
            Projectile.friendly = false;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = -1;
            Projectile.timeLeft = Lifetime;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.netImportant = true;
        }

        public override void AI()
        {
            Age++;

            if (Projectile.localAI[StuckFlag] == 0f)
            {
                Projectile.velocity.Y += Gravity;
                if (Projectile.velocity.Y > MaxFallSpeed)
                {
                    Projectile.velocity.Y = MaxFallSpeed;
                }

                Projectile.rotation += 0.16f;
                return;
            }

            Projectile.velocity = Vector2.Zero;
            Lighting.AddLight(Projectile.Center, 0.35f, 0.3f, 0.12f);

            // Fades out over the last second so its going is not a surprise.
            Projectile.alpha = Projectile.timeLeft < 60 ? (int)(255 * (1f - (Projectile.timeLeft / 60f))) : 0;

            if (Age % BeeInterval == 0f)
            {
                SpitBee();
            }

            if (Main.rand.NextBool(12))
            {
                Dust mote = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height,
                    DustID.Honey, 0f, -0.4f, 120, default, 0.8f);
                mote.noGravity = true;
            }
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (Projectile.localAI[StuckFlag] == 1f)
            {
                return false;
            }

            Projectile.localAI[StuckFlag] = 1f;
            Projectile.velocity = Vector2.Zero;
            Projectile.tileCollide = false;
            Projectile.rotation = 0f;
            Projectile.netUpdate = true;

            SoundEngine.PlaySound(SoundID.Dig with { Pitch = 0.5f }, Projectile.Center);
            return false;
        }

        private void SpitBee()
        {
            if (Main.myPlayer != Projectile.owner)
            {
                return;
            }

            NPC target = FindTarget();
            if (target == null)
            {
                return;
            }

            Vector2 aim = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitY);
            int damage = System.Math.Max(1, (int)(Projectile.damage * BeeDamageShare));

            for (int i = 0; i < BeesPerSpit; i++)
            {
                // Fanned slightly so the pair does not fly as one dot.
                Vector2 launch = aim.RotatedBy(MathHelper.ToRadians(Main.rand.NextFloat(-22f, 22f))) * 4f;

                int index = Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    Projectile.Center,
                    launch,
                    ModContent.ProjectileType<PrismBee>(),
                    damage,
                    Projectile.knockBack,
                    Projectile.owner,
                    ai0: target.whoAmI + 1);

                if (index >= 0)
                {
                    Main.projectile[index].netUpdate = true;
                }
            }
        }

        private NPC FindTarget()
        {
            NPC closest = null;
            float closestDistance = Range;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.CanBeChasedBy(this))
                {
                    continue;
                }

                float distance = Vector2.Distance(Projectile.Center, npc.Center);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closest = npc;
                }
            }

            return closest;
        }

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 10; i++)
            {
                Dust crumb = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height,
                    DustID.Honey, 0f, 0f, 100, default, 1f);
                crumb.velocity = Main.rand.NextVector2Circular(2f, 2f);
            }
        }
    }
}
