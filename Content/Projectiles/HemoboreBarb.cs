using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace VenninBeeMod.Content.Projectiles
{
    /// <summary>
    /// Fired by the Hemobore. It lodges in the first thing it hits and stays there, pulsing
    /// damage, feeding a thread of blood back to whoever fired it, and shedding a bee every so
    /// often so it is not purely single target.
    /// </summary>
    public class HemoboreBarb : ModProjectile
    {
        /// <summary>Only three threads at a time; a fourth shot rips the oldest one out.</summary>
        private const int MaxLodged = 3;

        private const int PulseInterval = 45;

        /// <summary>Life comes back every other pulse, so one barb is a slow trickle.</summary>
        private const int PulsesPerDrain = 2;

        /// <summary>And a bee every third, so three barbs put out a steady drip of them.</summary>
        private const int PulsesPerBee = 3;

        /// <summary>How far the thread stretches before the barb tears loose.</summary>
        private const float TetherRange = 700f;

        private const float PulseDamageShare = 0.5f;
        private const float BeeDamageShare = 0.45f;

        /// <summary>Index of the host NPC plus one, or 0 while still in flight.</summary>
        private ref float Host => ref Projectile.ai[0];

        /// <summary>Ticks lodged. Doubles as the age used to pick which barb to rip out.</summary>
        private ref float Lodged => ref Projectile.ai[1];

        private ref float PulseCount => ref Projectile.ai[2];

        private Vector2 rideOffset;

        public override void SetDefaults()
        {
            Projectile.width = 8;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            // Never spent on impact: CanHitNPC gates further hits once it has lodged, and a
            // penetrate of 1 would kill it the moment it bit.
            Projectile.penetrate = -1;
            Projectile.timeLeft = 900;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.netImportant = true;
        }

        public override void OnSpawn(IEntitySource source)
        {
            EvictOldestBarb();
        }

        /// <summary>
        /// Keeps the number of threads down to <see cref="MaxLodged"/>. The one that has been in
        /// longest goes, which makes firing at a fresh target feel like moving a barb rather than
        /// losing one at random.
        /// </summary>
        private void EvictOldestBarb()
        {
            if (Main.myPlayer != Projectile.owner)
            {
                return;
            }

            while (true)
            {
                int count = 0;
                Projectile oldest = null;

                for (int i = 0; i < Main.maxProjectiles; i++)
                {
                    Projectile other = Main.projectile[i];
                    if (!other.active || other.type != Projectile.type || other.owner != Projectile.owner)
                    {
                        continue;
                    }

                    count++;
                    if (other.whoAmI != Projectile.whoAmI && (oldest == null || other.ai[1] > oldest.ai[1]))
                    {
                        oldest = other;
                    }
                }

                if (count <= MaxLodged || oldest == null)
                {
                    return;
                }

                oldest.Kill();
            }
        }

        public override void AI()
        {
            if (Host == 0f)
            {
                Projectile.rotation = Projectile.velocity.ToRotation() - MathHelper.PiOver2;
                Projectile.velocity.Y += 0.06f;
                return;
            }

            NPC host = ResolveHost();
            Player owner = Main.player[Projectile.owner];

            if (host == null || !owner.active || owner.dead)
            {
                Projectile.Kill();
                return;
            }

            // Hive Pack secret: the thread never tears, however far you back off.
            bool unbreakable = HivePack.IsEquipped(owner);
            if (!unbreakable && Vector2.Distance(owner.Center, host.Center) > TetherRange)
            {
                RipOut();
                return;
            }

            Projectile.Center = host.Center + rideOffset;
            Projectile.velocity = Vector2.Zero;
            Projectile.timeLeft = 60;

            Lodged++;
            if (Lodged % PulseInterval == 0f)
            {
                Pulse(owner, host);
            }

            if (Main.rand.NextBool(6))
            {
                Dust seep = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height,
                    DustID.Blood, 0f, 0f, 90, default, 0.85f);
                seep.velocity *= 0.3f;
            }
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

        private void Pulse(Player owner, NPC host)
        {
            PulseCount++;

            if (Main.myPlayer != Projectile.owner)
            {
                return;
            }

            int pulseDamage = System.Math.Max(1, (int)(Projectile.damage * PulseDamageShare));
            owner.ApplyDamageToNPC(host, pulseDamage, 0f, 0, false);
            CrimsonBee.TryConfuse(host);

            if (PulseCount % PulsesPerDrain == 0f && owner.statLife < owner.statLifeMax2)
            {
                owner.statLife = System.Math.Min(owner.statLife + 1, owner.statLifeMax2);
                owner.HealEffect(1);
            }

            if (PulseCount % PulsesPerBee == 0f)
            {
                ShedBee(host);
            }
        }

        /// <summary>
        /// The barb coughs up a bee that goes looking for its own target. This is what stops the
        /// Hemobore being a pure single target leash.
        /// </summary>
        private void ShedBee(NPC host)
        {
            int damage = System.Math.Max(1, (int)(Projectile.damage * BeeDamageShare));
            int index = Projectile.NewProjectile(
                Projectile.GetSource_FromThis(),
                Projectile.Center,
                Main.rand.NextVector2CircularEdge(3.5f, 3.5f),
                ModContent.ProjectileType<BloodBee>(),
                damage,
                0f,
                Projectile.owner,
                ai0: host.whoAmI + 1);

            if (index >= 0)
            {
                Main.projectile[index].netUpdate = true;
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            CrimsonBee.TryConfuse(target);

            if (Host != 0f || !target.active || target.dontTakeDamage)
            {
                return;
            }

            Host = target.whoAmI + 1;
            rideOffset = Projectile.Center - target.Center;

            float reach = System.Math.Min(target.width, target.height) * 0.3f;
            if (rideOffset.Length() > reach)
            {
                rideOffset = rideOffset.SafeNormalize(Vector2.UnitY) * reach;
            }

            Lodged = 0f;
            PulseCount = 0f;
            Projectile.friendly = false;
            Projectile.tileCollide = false;
            Projectile.velocity = Vector2.Zero;
            Projectile.netUpdate = true;

            SoundEngine.PlaySound(SoundID.NPCHit13 with { Volume = 0.6f }, Projectile.Center);
        }

        /// <summary>
        /// Stops the barb counting as a hit so it can lodge instead of being spent.
        /// </summary>
        public override bool? CanHitNPC(NPC target)
        {
            return Host == 0f ? null : false;
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            return Host == 0f;
        }

        private void RipOut()
        {
            SoundEngine.PlaySound(SoundID.NPCHit18 with { Volume = 0.5f }, Projectile.Center);
            Projectile.Kill();
        }

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 6; i++)
            {
                Dust spray = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height,
                    DustID.Blood, 0f, 0f, 80, default, 1f);
                spray.velocity = Main.rand.NextVector2Circular(2f, 2f);
            }
        }
    }
}
