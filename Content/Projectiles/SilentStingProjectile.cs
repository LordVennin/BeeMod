using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;

namespace VenninBeeMod.Content.Projectiles
{
    /// <summary>
    /// The stab itself. 1.4.4 shortswords are projectiles rather than a held sprite, so the
    /// blade, its reach and its hit detection all live here. Structure follows the vanilla
    /// shortsword setup: the hitbox sits near the player and the collision line is plotted out
    /// toward the tip.
    /// </summary>
    public class SilentStingProjectile : ModProjectile
    {
        public const int FadeInDuration = 4;
        public const int FadeOutDuration = 4;
        public const int TotalDuration = 10;

        private const int SpriteSize = 32;

        // Reach past the target before the ambusher materialises.
        private const float AmbushGap = 44f;

        // Ceiling on the Hive Pack volley, so a dense spawn event cannot flood the screen.
        private const int MaxAmbushTargets = 24;

        public float CollisionWidth => 10f * Projectile.scale;

        public int Timer
        {
            get => (int)Projectile.ai[0];
            set => Projectile.ai[0] = value;
        }

        private ref float BeeSent => ref Projectile.ai[1];

        public override string Texture => "VenninBeeMod/Content/Items/SilentSting";

        public override void SetDefaults()
        {
            Projectile.Size = new Vector2(18);
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.scale = 1f;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.ownerHitCheck = true;
            Projectile.extraUpdates = 1;

            // Local immunity means the blade keeps its own per-enemy cooldown and never stamps
            // the shared invincibility window, so stabs land at the weapon's own rate instead
            // of being throttled by whatever hit the enemy last. -1 is once per enemy per stab.
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
            Projectile.timeLeft = 360;
            Projectile.hide = true;
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];

            Timer += 1;
            if (Timer >= TotalDuration)
            {
                Projectile.Kill();
                return;
            }

            player.heldProj = Projectile.whoAmI;

            Projectile.Opacity = Utils.GetLerpValue(0f, FadeInDuration, Timer, clamped: true)
                * Utils.GetLerpValue(TotalDuration, TotalDuration - FadeOutDuration, Timer, clamped: true);

            Vector2 playerCenter = player.RotatedRelativePoint(player.MountedCenter, reverseRotation: false, addGfxOffY: false);
            Projectile.Center = playerCenter + (Projectile.velocity * (Timer - 1f));

            Projectile.spriteDirection = (Vector2.Dot(Projectile.velocity, Vector2.UnitX) >= 0f).ToDirectionInt();
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2
                - (MathHelper.PiOver4 * Projectile.spriteDirection);

            SetVisualOffsets();
        }

        private void SetVisualOffsets()
        {
            const int HalfSpriteWidth = SpriteSize / 2;
            const int HalfSpriteHeight = SpriteSize / 2;

            int halfProjWidth = Projectile.width / 2;
            int halfProjHeight = Projectile.height / 2;

            DrawOriginOffsetX = 0;
            DrawOffsetX = -(HalfSpriteWidth - halfProjWidth);
            DrawOriginOffsetY = -(HalfSpriteHeight - halfProjHeight);
        }

        public override bool ShouldUpdatePosition()
        {
            return false;
        }

        public override void CutTiles()
        {
            DelegateMethods.tilecut_0 = TileCuttingContext.AttackProjectile;
            Vector2 start = Projectile.Center;
            Vector2 end = start + (Projectile.velocity.SafeNormalize(-Vector2.UnitY) * 10f);
            Utils.PlotTileLine(start, end, CollisionWidth, DelegateMethods.CutTiles);
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            Vector2 start = Projectile.Center;
            Vector2 end = start + (Projectile.velocity * 6f);
            float collisionPoint = 0f;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), start, end, CollisionWidth, ref collisionPoint);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // One volley per stab, so holding the dagger down does not bury the screen in bees.
            if (Main.myPlayer != Projectile.owner || BeeSent != 0f)
            {
                return;
            }

            if (!CanBeAmbushed(target))
            {
                return;
            }

            BeeSent = 1f;

            Player player = Main.player[Projectile.owner];

            // Quiet Hive Pack bonus: the whole visible screen gets ambushed, not just the
            // enemy under the blade. Undocumented in the tooltip on purpose.
            if (HivePack.IsEquipped(player))
            {
                AmbushEverythingOnScreen(player);
                return;
            }

            SendAmbusher(player, target);
        }

        /// <summary>
        /// Target dummies and anything else that cannot actually be hurt are skipped, so the
        /// swarm cannot be farmed off a practice target.
        /// </summary>
        private static bool CanBeAmbushed(NPC npc)
        {
            return npc.active
                && !npc.immortal
                && !npc.dontTakeDamage
                && npc.type != NPCID.TargetDummy
                && npc.life > 0;
        }

        private void AmbushEverythingOnScreen(Player player)
        {
            Rectangle screen = new Rectangle(
                (int)Main.screenPosition.X,
                (int)Main.screenPosition.Y,
                Main.screenWidth,
                Main.screenHeight);

            int sent = 0;
            for (int i = 0; i < Main.maxNPCs && sent < MaxAmbushTargets; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.CanBeChasedBy(this) || !CanBeAmbushed(npc) || !npc.getRect().Intersects(screen))
                {
                    continue;
                }

                SendAmbusher(player, npc);
                sent++;
            }
        }

        private void SendAmbusher(Player player, NPC target)
        {
            Vector2 awayFromPlayer = (target.Center - player.Center).SafeNormalize(Vector2.UnitX);

            // Drop the bee on the far side of the target, so it comes back through the blind spot.
            float behind = (Math.Max(target.width, target.height) * 0.5f) + AmbushGap;
            Vector2 spawnPosition = target.Center + (awayFromPlayer * behind);

            Projectile.NewProjectile(
                Projectile.GetSource_OnHit(target),
                spawnPosition,
                Vector2.Zero,
                ModContent.ProjectileType<NinjaBee>(),
                Items.SilentSting.NinjaBeeDamage,
                0f,
                Projectile.owner,
                ai0: target.whoAmI);
        }
    }
}
