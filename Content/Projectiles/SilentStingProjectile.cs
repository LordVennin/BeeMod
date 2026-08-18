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

        // How far past the hitbox the blade actually reaches, in multiples of its velocity.
        private const float ReachSteps = 9f;

        // Reach past the target before the ambusher materialises.
        private const float AmbushGap = 44f;

        // Ceiling on the Hive Pack volley, so a dense spawn event cannot flood the screen.
        private const int MaxAmbushTargets = 24;

        public float CollisionWidth => 14f * Projectile.scale;

        public int Timer
        {
            get => (int)Projectile.ai[0];
            set => Projectile.ai[0] = value;
        }

        private ref float BeeSent => ref Projectile.ai[1];

        public override string Texture => "VenninBeeMod/Content/Items/SilentSting";

        public override void SetDefaults()
        {
            Projectile.Size = new Vector2(22);
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.scale = 1f;
            Projectile.DamageType = DamageClass.Melee;
            // Left off on purpose. Vanilla's version runs its line of sight check against the
            // target's centre, which on a large boss can sit inside terrain even while the part
            // you are stabbing is in the open. Colliding does the same check against the
            // nearest point instead.
            Projectile.ownerHitCheck = false;

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
            Vector2 end = start + (Projectile.velocity * ReachSteps);

            float collisionPoint = 0f;
            bool reached = Collision.CheckAABBvLineCollision(
                targetHitbox.TopLeft(), targetHitbox.Size(), start, end, CollisionWidth, ref collisionPoint);

            if (!reached)
            {
                // A hitbox large enough to swallow the whole blade is never crossed by the swept
                // line, so a point blank stab into a boss could register as a clean miss. Treat
                // the blade sitting inside the target as a hit.
                reached = targetHitbox.Contains((int)start.X, (int)start.Y)
                    || targetHitbox.Contains((int)end.X, (int)end.Y);
            }

            if (!reached)
            {
                return false;
            }

            // Line of sight to the nearest point of the target rather than its centre, so a
            // boss whose middle is buried in terrain can still be stabbed where it is exposed.
            Player player = Main.player[Projectile.owner];
            Vector2 nearest = new Vector2(
                MathHelper.Clamp(start.X, targetHitbox.Left, targetHitbox.Right),
                MathHelper.Clamp(start.Y, targetHitbox.Top, targetHitbox.Bottom));

            return Collision.CanHitLine(player.position, player.width, player.height, nearest, 1, 1);
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
