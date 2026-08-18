using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VenninBeeMod.Content.Buffs;

namespace VenninBeeMod.Content.Projectiles
{
    /// <summary>
    /// Spawned in the target's blind spot by <see cref="Items.SilentSting"/>. It fades in, then
    /// drives straight through the enemy it was aimed at. If that enemy dies first the bee has
    /// nothing to stab and simply vanishes in a puff of smoke.
    /// </summary>
    public class NinjaBee : ModProjectile
    {
        private const int WindUp = 12;
        private const float StrikeSpeed = 18f;
        private const int PoisonDuration = 300;

        private ref float TargetIndex => ref Projectile.ai[0];
        private ref float Dismissed => ref Projectile.ai[1];
        private ref float Timer => ref Projectile.ai[2];

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 4;
        }

        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 240;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.alpha = 255;
        }

        public override void AI()
        {
            NPC target = ResolveTarget();
            if (target == null)
            {
                Dismiss();
                return;
            }

            Timer++;

            // Fades up out of nothing during the wind-up so the ambush reads visually.
            Projectile.alpha = (int)MathHelper.Clamp(255f - ((Timer / WindUp) * 255f), 0f, 255f);

            if (Timer < WindUp)
            {
                Projectile.velocity *= 0.85f;
            }
            else
            {
                Vector2 toTarget = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX);
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, toTarget * StrikeSpeed, 0.45f);
            }

            AnimateFrames();
            UpdateFacing();
        }

        private NPC ResolveTarget()
        {
            int index = (int)TargetIndex;
            if (index < 0 || index >= Main.maxNPCs)
            {
                return null;
            }

            NPC npc = Main.npc[index];
            return npc.active && npc.life > 0 && !npc.friendly ? npc : null;
        }

        private void Dismiss()
        {
            Dismissed = 1f;
            Projectile.Kill();
        }

        /// <summary>
        /// Only ever stabs the enemy it was sent after, so a bee cannot be soaked up by
        /// something that happens to wander between it and its mark.
        /// </summary>
        public override bool? CanHitNPC(NPC target)
        {
            return target.whoAmI == (int)TargetIndex ? null : false;
        }

        /// <summary>
        /// True damage. ScalingArmorPenetration is a fraction of the target's defense, so 1f
        /// discards all of it however armoured the enemy is. A flat ArmorPenetration would only
        /// count for half against NPCs.
        /// </summary>
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            modifiers.ScalingArmorPenetration += 1f;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<ShadowPoison>(), PoisonDuration);
        }

        public override void OnKill(int timeLeft)
        {
            if (Dismissed == 1f)
            {
                SmokeOut();
                return;
            }

            for (int i = 0; i < 5; i++)
            {
                Dust sting = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height,
                    DustID.Smoke, 0f, 0f, 90, new Color(140, 96, 210), 0.8f);
                sting.velocity = Main.rand.NextVector2Circular(1.6f, 1.6f);
                sting.noGravity = true;
            }
        }

        private void SmokeOut()
        {
            for (int i = 0; i < 6; i++)
            {
                Dust puff = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height,
                    DustID.Smoke, 0f, 0f, 130, default, 0.65f);
                puff.velocity = Main.rand.NextVector2Circular(0.7f, 0.7f) - (Vector2.UnitY * 0.25f);
                puff.noGravity = true;
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

        private void UpdateFacing()
        {
            Projectile.rotation = 0f;
            if (Projectile.velocity.X > 0.15f)
            {
                Projectile.spriteDirection = -1;
            }
            else if (Projectile.velocity.X < -0.15f)
            {
                Projectile.spriteDirection = 1;
            }
        }
    }
}
