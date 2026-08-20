using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace VenninBeeMod.Content.Projectiles
{
    /// <summary>
    /// Sticks to the first thing it touches, swells, then ruptures for a burst of damage and a
    /// knot of bees. Landing another cyst on a target that already has one feeds it instead of
    /// starting a second, so the payoff is in committing casts to one enemy.
    /// </summary>
    public class BroodCyst : ModProjectile
    {
        private const int SwellTime = 150;
        private const int MaxStacks = 3;

        private const float BaseRadius = 80f;
        private const float RadiusPerStack = 30f;

        /// <summary>Extra rupture damage each stack past the first is worth.</summary>
        private const float DamagePerStack = 0.5f;

        private const int BaseBees = 3;
        private const int BeesPerStack = 2;
        private const float BeeDamageShare = 0.4f;

        /// <summary>Frames the rupture hitbox stays open for.</summary>
        private const int RuptureFrames = 6;

        private const int RuptureFlag = 0;

        /// <summary>Index of the host NPC plus one, or 0 while in flight.</summary>
        private ref float Host => ref Projectile.ai[0];

        private ref float Stacks => ref Projectile.ai[1];
        private ref float SwellTimer => ref Projectile.ai[2];

        private Vector2 rideOffset;

        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 12;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 400;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.netImportant = true;

            // The rupture is one big pulse, so nothing should be hit twice by it.
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }

        public override void AI()
        {
            if (Projectile.localAI[RuptureFlag] == 1f)
            {
                // The rupture hitbox is open. Nothing to do but let it expire.
                Projectile.velocity = Vector2.Zero;
                return;
            }

            if (Host == 0f)
            {
                Projectile.velocity.Y += 0.14f;
                Projectile.rotation += 0.05f;
                Projectile.scale = 0.75f;
                return;
            }

            NPC host = ResolveHost();
            if (host == null)
            {
                Rupture();
                return;
            }

            Projectile.Center = host.Center + rideOffset;
            Projectile.velocity = Vector2.Zero;
            Projectile.timeLeft = 60;

            SwellTimer++;
            float progress = SwellTimer / SwellTime;

            // Swells with the fuse and with how many casts have gone into it.
            float stackBulk = 1f + ((EffectiveStacks() - 1) * 0.35f);
            Projectile.scale = (0.7f + (progress * 0.7f)) * stackBulk;
            Projectile.rotation = (float)System.Math.Sin(SwellTimer * 0.09f) * 0.12f;

            if (Main.rand.NextFloat() < 0.08f + (progress * 0.25f))
            {
                Dust swell = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height,
                    DustID.Blood, 0f, 0f, 90, default, 0.9f + progress);
                swell.velocity *= 0.35f;
                swell.noGravity = true;
            }

            if (SwellTimer >= SwellTime)
            {
                Rupture();
            }
        }

        /// <summary>
        /// Hive Pack secret: every cyst counts for one more than it is, so the full payload
        /// arrives a cast early.
        /// </summary>
        private float EffectiveStacks()
        {
            float stacks = System.Math.Max(1f, Stacks);
            if (HivePack.IsEquipped(Main.player[Projectile.owner]))
            {
                stacks = System.Math.Min(MaxStacks, stacks + 1f);
            }

            return stacks;
        }

        private NPC ResolveHost()
        {
            int index = (int)Host - 1;
            if (index < 0 || index >= Main.maxNPCs)
            {
                return null;
            }

            NPC host = Main.npc[index];
            return host.active && host.life > 0 ? host : null;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Projectile.localAI[RuptureFlag] == 1f)
            {
                CrimsonBee.TryConfuse(target);
                return;
            }

            if (Host != 0f || !target.active || target.dontTakeDamage)
            {
                return;
            }

            // Feed an existing cyst rather than starting a rival one.
            Projectile existing = FindCystOn(target);
            if (existing != null)
            {
                if (existing.ai[1] < MaxStacks)
                {
                    existing.ai[1] = System.Math.Min(MaxStacks, existing.ai[1] + 1f);

                    // A fresh cast buys the swell a little more time to show the growth.
                    existing.ai[2] = System.Math.Max(0f, existing.ai[2] - 40f);
                    existing.netUpdate = true;
                    SoundEngine.PlaySound(SoundID.NPCHit13 with { Volume = 0.6f, Pitch = 0.3f }, target.Center);
                }

                Projectile.Kill();
                return;
            }

            Attach(target);
        }

        private Projectile FindCystOn(NPC target)
        {
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile other = Main.projectile[i];
                if (!other.active || other.type != Projectile.type || other.owner != Projectile.owner
                    || other.whoAmI == Projectile.whoAmI)
                {
                    continue;
                }

                if ((int)other.ai[0] - 1 == target.whoAmI && other.localAI[RuptureFlag] == 0f)
                {
                    return other;
                }
            }

            return null;
        }

        private void Attach(NPC target)
        {
            Host = target.whoAmI + 1;
            Stacks = 1f;
            SwellTimer = 0f;
            rideOffset = Projectile.Center - target.Center;

            float reach = System.Math.Min(target.width, target.height) * 0.28f;
            if (rideOffset.Length() > reach)
            {
                rideOffset = rideOffset.SafeNormalize(Vector2.UnitY) * reach;
            }

            Projectile.friendly = false;
            Projectile.tileCollide = false;
            Projectile.velocity = Vector2.Zero;
            Projectile.netUpdate = true;

            SoundEngine.PlaySound(SoundID.NPCHit13 with { Volume = 0.6f }, target.Center);
        }

        /// <summary>
        /// A stuck cyst is inert until it goes off, so it should not be eating hits meant for
        /// anything else.
        /// </summary>
        public override bool? CanHitNPC(NPC target)
        {
            if (Projectile.localAI[RuptureFlag] == 1f)
            {
                return null;
            }

            return Host == 0f ? null : false;
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (Host != 0f || Projectile.localAI[RuptureFlag] == 1f)
            {
                return false;
            }

            // A cyst that hits the ground still goes off, just for less.
            Rupture();
            return false;
        }

        /// <summary>
        /// Last resort for a cyst that expires without ever going off - a flight that hit
        /// nothing, say. The hitbox cannot be opened from here because the projectile is
        /// already on its way out, so this is bees and gore only.
        /// </summary>
        public override void OnKill(int timeLeft)
        {
            Rupture();
        }

        /// <summary>
        /// Blows the hitbox out to the rupture radius for a handful of frames rather than
        /// spawning a separate explosion. Local immunity is off, so everything caught takes it
        /// exactly once.
        /// </summary>
        private void Rupture()
        {
            if (Projectile.localAI[RuptureFlag] == 1f)
            {
                return;
            }

            Projectile.localAI[RuptureFlag] = 1f;

            float stacks = EffectiveStacks();
            float radius = BaseRadius + (RadiusPerStack * stacks);
            int baseDamage = Projectile.damage;
            int burstDamage = System.Math.Max(1, (int)(baseDamage * (1f + (DamagePerStack * (stacks - 1f)))));

            Vector2 center = Projectile.Center;
            Projectile.Resize((int)(radius * 2f), (int)(radius * 2f));
            Projectile.Center = center;
            Projectile.damage = burstDamage;
            Projectile.knockBack = 6f;
            Projectile.friendly = true;
            Projectile.tileCollide = false;
            Projectile.timeLeft = RuptureFrames;
            Projectile.alpha = 255;

            // The host was already struck once when the cyst grafted on, and local immunity is
            // set to one hit per NPC for the rupture's sake. Clearing it is what lets the
            // rupture damage the very enemy it grew inside.
            Projectile.ResetLocalNPCHitImmunity();
            Projectile.netUpdate = true;

            SoundEngine.PlaySound(SoundID.NPCDeath13 with { Volume = 0.85f }, center);
            SpawnBees(center, stacks, baseDamage);
            SprayGore(center, radius);
        }

        private void SpawnBees(Vector2 center, float stacks, int baseDamage)
        {
            if (Main.myPlayer != Projectile.owner)
            {
                return;
            }

            int count = BaseBees + (int)(BeesPerStack * stacks);
            int damage = System.Math.Max(1, (int)(baseDamage * BeeDamageShare));
            int preferred = (int)Host;

            for (int i = 0; i < count; i++)
            {
                Vector2 velocity = Main.rand.NextVector2CircularEdge(5f, 5f) * Main.rand.NextFloat(0.6f, 1f);
                int index = Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    center,
                    velocity,
                    ModContent.ProjectileType<BloodBee>(),
                    damage,
                    0f,
                    Projectile.owner,
                    ai0: preferred);

                if (index >= 0)
                {
                    Main.projectile[index].netUpdate = true;
                }
            }
        }

        private void SprayGore(Vector2 center, float radius)
        {
            for (int i = 0; i < 40; i++)
            {
                Vector2 offset = Main.rand.NextVector2CircularEdge(radius * 0.9f, radius * 0.9f);
                Dust gore = Dust.NewDustPerfect(center + (offset * Main.rand.NextFloat(0.25f, 1f)),
                    DustID.Blood, Vector2.Zero, 70, default, Main.rand.NextFloat(1f, 1.7f));
                gore.velocity = offset.SafeNormalize(Vector2.UnitY) * Main.rand.NextFloat(1.5f, 4.5f);
            }
        }
    }
}
