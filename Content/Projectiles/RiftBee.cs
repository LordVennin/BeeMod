using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using VenninBeeMod.Content.Items;

namespace VenninBeeMod.Content.Projectiles
{
    /// <summary>
    /// The drone conjured by the Hornet Rift. It holds station ahead of its caster, tears a rift
    /// open in front of itself and spits stingers through it at the cursor. The longer the
    /// channel is held the faster it fires, so letting go throws the wind-up away.
    /// </summary>
    public class RiftBee : ModProjectile
    {
        // Fire interval walks from the slow end to the fast end over ChargeTime.
        private const float SlowInterval = 14f;
        private const float FastInterval = 4f;
        private const float ChargeTime = 90f;

        // Volleys are 1 to 3 stingers, so the interval is slower than a single shot would want.
        private const int MinVolley = 1;
        private const int MaxVolley = 3;

        // The item's mana cost is charged once per second for as long as the channel is held.
        private const int ManaDrainInterval = 60;

        private const float HoverDistance = 54f;
        private const float StingerSpeed = 13f;
        private const float ConeDegrees = 25f;

        private ref float Charge => ref Projectile.ai[0];
        private ref float FireTimer => ref Projectile.ai[1];
        private ref float ManaTimer => ref Projectile.ai[2];

        private float VolleyCount;

        private Asset<Texture2D> portalTexture;

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 4;
        }

        public override void SetDefaults()
        {
            Projectile.width = 32;
            Projectile.height = 32;

            // The stingers carry the damage; the drone is a conduit.
            Projectile.friendly = false;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 20;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.netImportant = true;
        }

        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];
            if (!StillChannelling(owner))
            {
                Projectile.Kill();
                return;
            }

            if (!PayUpkeep(owner))
            {
                Projectile.Kill();
                return;
            }

            // Pinning the animation is what holds the channel open, but it also stops the item
            // from cycling its own use, which is why upkeep is charged here instead.
            Projectile.timeLeft = 6;
            owner.itemTime = 2;
            owner.itemAnimation = 2;

            HoldStation(owner);

            Charge = MathHelper.Clamp(Charge + 1f, 0f, ChargeTime);

            FireTimer++;
            if (FireTimer >= CurrentInterval())
            {
                FireTimer = 0f;
                SpitStinger(owner);

                // Every few volleys only; one buzz per stinger would be unbearable at full rate.
                VolleyCount++;
                if (VolleyCount % 3f == 0f)
                {
                    SoundEngine.PlaySound(SoundID.Item17 with { Volume = 0.32f, PitchVariance = 0.4f }, Projectile.Center);
                }
            }

            AnimateFrames();
            UpdateFacing(owner);
            SpillParticles();
        }

        /// <summary>
        /// Charges the staff's mana cost once a second and reports whether the caster could
        /// afford it. Mana is client side, so only the owner runs this.
        /// </summary>
        private bool PayUpkeep(Player owner)
        {
            if (Main.myPlayer != Projectile.owner)
            {
                return true;
            }

            ManaTimer++;
            if (ManaTimer < ManaDrainInterval)
            {
                return true;
            }

            ManaTimer = 0f;

            // Passing -1 charges the held item's own cost, with the player's reductions applied.
            return owner.CheckMana(owner.HeldItem, -1, true, false);
        }

        private bool StillChannelling(Player owner)
        {
            return owner.active
                && !owner.dead
                && !owner.noItems
                && !owner.CCed
                && owner.channel
                && owner.HeldItem.type == ModContent.ItemType<HornetRift>();
        }

        private float CurrentInterval()
        {
            float progress = Charge / ChargeTime;
            return MathHelper.Lerp(SlowInterval, FastInterval, progress);
        }

        /// <summary>
        /// Sits between the caster and the cursor so the rift always faces the way you are
        /// aiming, with a slow bob to keep it alive.
        /// </summary>
        private void HoldStation(Player owner)
        {
            Vector2 aim = (AimPoint(owner) - owner.MountedCenter).SafeNormalize(new Vector2(owner.direction, 0f));
            float bob = (float)System.Math.Sin(Main.GameUpdateCount * 0.09f) * 5f;

            Vector2 station = owner.MountedCenter + (aim * HoverDistance) + new Vector2(0f, bob - 10f);
            Projectile.Center = Vector2.Lerp(Projectile.Center, station, 0.28f);
            Projectile.velocity = Vector2.Zero;
        }

        private static Vector2 AimPoint(Player owner)
        {
            // Only the controlling client knows where the cursor is.
            return Main.myPlayer == owner.whoAmI
                ? Main.MouseWorld
                : owner.Center + new Vector2(owner.direction * 200f, 0f);
        }

        private void SpitStinger(Player owner)
        {
            if (Main.myPlayer != Projectile.owner)
            {
                return;
            }

            Vector2 aim = (AimPoint(owner) - Projectile.Center).SafeNormalize(new Vector2(owner.direction, 0f));
            Vector2 mouth = Projectile.Center + (aim * 22f);

            Vector2 spraySide = new Vector2(-aim.Y, aim.X);

            int volley = Main.rand.Next(MinVolley, MaxVolley + 1);
            for (int i = 0; i < volley; i++)
            {
                // Chaotic cone: angle and speed both jitter, so the stream never looks ruled.
                Vector2 shot = aim.RotatedByRandom(MathHelper.ToRadians(ConeDegrees))
                    * (StingerSpeed * Main.rand.NextFloat(0.78f, 1.24f));

                // Scatter the muzzle across the rift so bursts do not stack on one point.
                Vector2 origin = mouth + (spraySide * Main.rand.NextFloat(-7f, 7f));

                int index = Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    origin,
                    shot,
                    ModContent.ProjectileType<RiftStinger>(),
                    Projectile.damage,
                    Projectile.knockBack,
                    Projectile.owner);

                if (index >= 0)
                {
                    Main.projectile[index].netUpdate = true;
                }
            }
        }

        private void SpillParticles()
        {
            if (!Main.rand.NextBool(3))
            {
                return;
            }

            float progress = Charge / ChargeTime;
            Vector2 rim = Projectile.Center + (Main.rand.NextFloat(MathHelper.TwoPi).ToRotationVector2() * 22f);
            Dust mote = Dust.NewDustPerfect(rim, DustID.Smoke, Vector2.Zero, 130,
                new Color(158, 104, 226), 0.6f + (progress * 0.5f));
            mote.noGravity = true;
            mote.velocity = (Projectile.Center - rim).SafeNormalize(Vector2.Zero) * 1.4f;
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

        private void UpdateFacing(Player owner)
        {
            Projectile.rotation = 0f;
            Vector2 aim = AimPoint(owner) - Projectile.Center;
            Projectile.spriteDirection = aim.X >= 0f ? -1 : 1;
        }

        /// <summary>
        /// Draws the rift behind the drone. It spins up and swells as the channel charges, which
        /// is the only readout the player gets for how fast the stingers are coming.
        /// </summary>
        public override bool PreDraw(ref Color lightColor)
        {
            portalTexture ??= ModContent.Request<Texture2D>(
                "VenninBeeMod/Content/Projectiles/RiftPortal", AssetRequestMode.ImmediateLoad);

            Texture2D portal = portalTexture.Value;
            float progress = Charge / ChargeTime;
            float scale = 0.55f + (progress * 0.5f);
            float spin = Main.GameUpdateCount * (0.02f + (progress * 0.06f));

            Player owner = Main.player[Projectile.owner];
            Vector2 aim = (AimPoint(owner) - Projectile.Center).SafeNormalize(new Vector2(owner.direction, 0f));
            Vector2 drawAt = Projectile.Center + (aim * 20f) - Main.screenPosition;

            Main.spriteBatch.Draw(
                portal,
                drawAt,
                null,
                new Color(190, 140, 250, 0) * (0.55f + (progress * 0.35f)),
                spin,
                new Vector2(portal.Width, portal.Height) / 2f,
                scale,
                SpriteEffects.None,
                0f);

            return true;
        }
    }
}
