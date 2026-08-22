using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.Audio;
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
        /// <summary>Only four threads at a time; a fifth shot rips the oldest one out.</summary>
        private const int MaxLodged = 4;

        /// <summary>
        /// How long a barb stays in before it works itself loose - eight seconds. Without this
        /// a lodged barb kept refreshing its own timeLeft and fed indefinitely, so the only
        /// thing that ever ended one was a kill, the tether, or being evicted.
        /// </summary>
        private const int LodgedLifetime = 480;

        private const int PulseInterval = 45;

        /// <summary>Life comes back every other pulse, so one barb is a slow trickle.</summary>
        private const int PulsesPerDrain = 2;

        /// <summary>And a bee every third, so a full set puts out a steady drip of them.</summary>
        private const int PulsesPerBee = 3;

        /// <summary>How far the thread stretches before the barb tears loose.</summary>
        private const float TetherRange = 700f;

        private const float PulseDamageShare = 0.5f;
        private const float BeeDamageShare = 0.45f;

        /// <summary>Segments in the drawn thread. Enough for the sag to look like a curve.</summary>
        private const int ThreadSegments = 14;

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

            // One hit per enemy. Penetrate is unlimited so the barb survives biting, and without
            // this a barb that cannot lodge - in a dummy, say - would grind out a hit every
            // frame it overlapped.
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }

        /// <summary>
        /// Keeps the number of barbs actually in an enemy down to <see cref="MaxLodged"/>,
        /// counted across every enemy rather than per target. The one that has been in longest
        /// goes, which makes firing at a fresh target feel like moving a barb rather than losing
        /// one at random.
        /// </summary>
        /// <remarks>
        /// Run when a barb lodges, not when it is fired. Counting shots still in the air meant a
        /// barb that was travelling could displace one already attached, and then miss - and
        /// leaving them out of the count while evicting on spawn would have let a fifth land and
        /// stick. Counting at the moment of lodging is what makes the cap exact.
        /// </remarks>
        private void EvictOldestBarb()
        {
            if (Main.myPlayer != Projectile.owner)
            {
                return;
            }

            while (true)
            {
                int lodged = 0;
                Projectile oldest = null;

                for (int i = 0; i < Main.maxProjectiles; i++)
                {
                    Projectile other = Main.projectile[i];
                    if (!other.active || other.type != Projectile.type || other.owner != Projectile.owner
                        || other.ai[0] == 0f)
                    {
                        continue;
                    }

                    lodged++;
                    if (other.whoAmI != Projectile.whoAmI && (oldest == null || other.ai[1] > oldest.ai[1]))
                    {
                        oldest = other;
                    }
                }

                if (lodged <= MaxLodged || oldest == null)
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
            if (Lodged >= LodgedLifetime)
            {
                RipOut();
                return;
            }

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

            // Never lodges in something that cannot die, or the drain and the bees would be
            // free off a practice target.
            if (Host != 0f || !CombatTarget.IsReal(target))
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

            // Now that this one is in, retire whatever no longer fits.
            EvictOldestBarb();
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

        /// <summary>
        /// Draws the thread of blood running from a lodged barb back to its owner, which is the
        /// only readout the player gets that a barb is still feeding them.
        /// </summary>
        public override bool PreDraw(ref Color lightColor)
        {
            if (Host != 0f)
            {
                DrawThread();
            }

            return true;
        }

        private void DrawThread()
        {
            Player owner = Main.player[Projectile.owner];
            if (!owner.active || owner.dead)
            {
                return;
            }

            Vector2 start = Projectile.Center;
            Vector2 end = owner.MountedCenter;

            // Hangs under its own weight rather than running straight, so it reads as a thread
            // and not a laser.
            float sag = MathHelper.Clamp(Vector2.Distance(start, end) * 0.2f, 8f, 52f);
            Vector2 control = ((start + end) * 0.5f) + new Vector2(0f, sag);

            // Swells right after each pulse feeds, then settles, so you can see it working.
            float sinceFeed = (Lodged % PulseInterval) / PulseInterval;
            float swell = 1f + ((1f - sinceFeed) * (1f - sinceFeed) * 1.5f);

            Texture2D pixel = TextureAssets.MagicPixel.Value;
            Rectangle source = new Rectangle(0, 0, 1, 1);
            Vector2 previous = start;

            for (int i = 1; i <= ThreadSegments; i++)
            {
                float t = i / (float)ThreadSegments;
                Vector2 point = Vector2.Lerp(Vector2.Lerp(start, control, t), Vector2.Lerp(control, end, t), t);
                Vector2 span = point - previous;

                if (span.LengthSquared() < 0.01f)
                {
                    continue;
                }

                // Thickest at the wound, tapering towards the player.
                float thickness = MathHelper.Lerp(3.4f, 1.3f, t) * swell;
                Color tint = Color.Lerp(new Color(168, 22, 32), new Color(112, 14, 24), t) * 0.9f;

                Main.spriteBatch.Draw(pixel, previous - Main.screenPosition, source, tint,
                    span.ToRotation(), new Vector2(0f, 0.5f),
                    new Vector2(span.Length(), thickness), SpriteEffects.None, 0f);

                previous = point;
            }
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
