using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace VenninBeeMod.Content.Projectiles
{
    /// <summary>
    /// A crystal hive planted where you point it. It picks a target and spits a shard that
    /// refracts into three on the way, and lets a bee out now and then to chase down whatever
    /// the shards cannot reach.
    /// </summary>
    /// <remarks>
    /// Deliberately unlike the Shadow Hive, which is the mod's other hive sentry: that one is a
    /// static ring you fight inside, this one is a turret that shoots outward.
    /// </remarks>
    public class PrismhiveSentry : ModProjectile
    {
        private const float Range = 620f;
        private const int ShootInterval = 55;
        private const int BeeInterval = 210;
        private const int MaxBees = 2;
        private const float ShardSpeed = 12f;
        private const float BobHeight = 4f;
        private const float BobSpeed = 0.05f;

        private ref float HomeX => ref Projectile.ai[0];
        private ref float HomeY => ref Projectile.ai[1];

        public override void SetDefaults()
        {
            Projectile.width = 48;
            Projectile.height = 48;

            // The shards carry the damage; the hive is a mount for them.
            Projectile.friendly = false;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.sentry = true;
            Projectile.timeLeft = Projectile.SentryLifeTime;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.netImportant = true;
        }

        public override void AI()
        {
            if (HomeX == 0f && HomeY == 0f)
            {
                HomeX = Projectile.Center.X;
                HomeY = Projectile.Center.Y;
                Projectile.netUpdate = true;
            }

            float bob = (float)System.Math.Sin(Main.GameUpdateCount * BobSpeed) * BobHeight;
            Projectile.Center = new Vector2(HomeX, HomeY + bob);
            Projectile.velocity = Vector2.Zero;

            Lighting.AddLight(Projectile.Center, 0.4f, 0.55f, 0.7f);

            if (Main.rand.NextBool(4))
            {
                Dust shimmer = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height,
                    DustID.RainbowMk2, 0f, -0.6f, 140, new Color(200, 235, 255), 0.8f);
                shimmer.noGravity = true;
            }

            NPC target = FindTarget();
            if (target == null)
            {
                return;
            }

            if (Main.GameUpdateCount % ShootInterval == 0)
            {
                FireShard(target);
            }

            if (Main.GameUpdateCount % BeeInterval == 0)
            {
                ReleaseBee(target);
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
                if (distance < closestDistance
                    && Collision.CanHitLine(Projectile.position, Projectile.width, Projectile.height,
                        npc.position, npc.width, npc.height))
                {
                    closestDistance = distance;
                    closest = npc;
                }
            }

            return closest;
        }

        private void FireShard(NPC target)
        {
            if (Main.myPlayer != Projectile.owner)
            {
                return;
            }

            Vector2 aim = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitY) * ShardSpeed;
            int index = Projectile.NewProjectile(
                Projectile.GetSource_FromThis(),
                Projectile.Center,
                aim,
                ModContent.ProjectileType<PrismStinger>(),
                Projectile.damage,
                Projectile.knockBack,
                Projectile.owner);

            if (index >= 0)
            {
                Main.projectile[index].netUpdate = true;
            }

            SoundEngine.PlaySound(SoundID.Item9 with { Volume = 0.4f }, Projectile.Center);
        }

        private void ReleaseBee(NPC target)
        {
            if (Main.myPlayer != Projectile.owner)
            {
                return;
            }

            Player owner = Main.player[Projectile.owner];
            int beeType = ModContent.ProjectileType<PrismBee>();
            if (owner.ownedProjectileCounts[beeType] >= MaxBees)
            {
                return;
            }

            int index = Projectile.NewProjectile(
                Projectile.GetSource_FromThis(),
                Projectile.Center + new Vector2(0f, 16f),
                new Vector2(Main.rand.NextFloat(-2f, 2f), 1.5f),
                beeType,
                System.Math.Max(1, (int)(Projectile.damage * 0.7f)),
                Projectile.knockBack,
                Projectile.owner,
                ai0: target.whoAmI + 1);

            if (index >= 0)
            {
                Main.projectile[index].netUpdate = true;
            }
        }
    }
}
