using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using System;
using Terraria.Audio;

namespace VenninBeeMod.Content.Projectiles
{
    public class HiveHornetMinion : ModProjectile
    {
        private int lastItemUseTime = -1;
        private bool hasFiredThisUse = false;
        private int shootCooldown = 0;
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.Hornet;

        private int lastShotAnimation = 0;

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = Main.projFrames[ProjectileID.Hornet];
            Main.projPet[Projectile.type] = true;

            ProjectileID.Sets.MinionSacrificable[Projectile.type] = true;
            ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = 34;
            Projectile.height = 26;
            Projectile.friendly = true;
            Projectile.minion = true;
            Projectile.minionSlots = 0f; // Doesn't consume minion slots
            Projectile.timeLeft = 18000;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];

            if (!player.active || player.dead || !player.HasBuff(ModContent.BuffType<Buffs.HiveMindBuff>()))
            {
                Projectile.Kill();
                return;
            }

            Projectile.timeLeft = 2;

            // Hover above player, add vertical sway
            float baseHoverHeight = -50f;
            float swayAmplitude = 6f;
            float swaySpeed = 0.08f;

            Vector2 idleOffset = new Vector2(
                0f,
                baseHoverHeight + (float)Math.Sin(Main.GameUpdateCount * swaySpeed) * swayAmplitude
            );

            Vector2 idlePosition = player.Center + idleOffset;
            Vector2 toIdle = idlePosition - Projectile.Center;
            float distance = toIdle.Length();

            // Calculate dynamic speed: faster when far, slower when close
            float maxSpeed = 10f;
            float minSpeed = 2f;
            float dynamicSpeed = MathHelper.Lerp(minSpeed, maxSpeed, Utils.Clamp(distance / 200f, 0f, 1f));

            Vector2 moveVelocity = Vector2.Zero;
            if (distance > 1f)
            {
                moveVelocity = toIdle.SafeNormalize(Vector2.Zero) * dynamicSpeed;
            }

            float inertia = 20f;
            Projectile.velocity = (Projectile.velocity * (inertia - 1) + moveVelocity) / inertia;

            // Always face the player's direction
            Projectile.spriteDirection = player.direction * -1;

            // Optional hover rotation
            Projectile.rotation = (float)Math.Sin(Main.GameUpdateCount * 0.05f) * 0.1f;

            // Animate frames
            Projectile.frameCounter++;
            if (Projectile.frameCounter >= 6)
            {
                Projectile.frameCounter = 0;
                Projectile.frame = (Projectile.frame + 1) % Main.projFrames[Projectile.type];
            }

            // --- Fire logic synced with player shot ---

            // Decrement cooldown each frame
            if (shootCooldown > 0)
                shootCooldown--;
        }
    }
}
