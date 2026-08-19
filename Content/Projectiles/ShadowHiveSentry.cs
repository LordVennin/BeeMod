using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VenninBeeMod.Content.Buffs;

namespace VenninBeeMod.Content.Projectiles
{
    /// <summary>
    /// The Shadow Hive sentry. It hangs where it was placed, bobbing, steeping everything inside
    /// its ring in Shadow Poison and letting drones out a few at a time.
    /// </summary>
    public class ShadowHiveSentry : ModProjectile
    {
        public const float BaseAuraRadius = 247f;

        /// <summary>
        /// Hive Pack secret: the ring reaches 30 percent further, so the poison field and the
        /// patch the drones patrol both grow with it.
        /// </summary>
        private const float HivePackAuraScale = 1.3f;

        private const int PoisonInterval = 30;
        private const int PoisonDuration = 180;
        private const int BeeInterval = 120;
        private const int MaxBees = 4;
        private const float BobHeight = 7f;
        private const float BobSpeed = 0.045f;

        /// <summary>
        /// Ring size for a given hive. Drones read this off their own hive rather than assuming
        /// the base value, so a hive keeps its reach even if the pack comes off mid-fight.
        /// </summary>
        public static float RadiusFor(Projectile hive)
        {
            return HivePack.IsEquipped(Main.player[hive.owner]) ? BaseAuraRadius * HivePackAuraScale : BaseAuraRadius;
        }

        private float AuraRadius => RadiusFor(Projectile);

        private ref float HomeX => ref Projectile.ai[0];
        private ref float HomeY => ref Projectile.ai[1];

        private Asset<Texture2D> auraTexture;

        public override void SetDefaults()
        {
            Projectile.width = 96;
            Projectile.height = 96;

            // A structure, not a weapon: the ring and the drones do the work.
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
            Player owner = Main.player[Projectile.owner];

            // Cancelling the buff is the player's way of packing the hives away. Only the owner
            // judges this, so buff sync lag cannot make remote clients cull it early.
            if (Main.myPlayer == Projectile.owner && (!owner.active || owner.dead || !owner.HasBuff<ShadowHiveBuff>()))
            {
                Projectile.Kill();
                return;
            }

            if (HomeX == 0f && HomeY == 0f)
            {
                HomeX = Projectile.Center.X;
                HomeY = Projectile.Center.Y;
                Projectile.netUpdate = true;
            }

            Projectile.velocity = Vector2.Zero;

            float bob = (float)System.Math.Sin(Main.GameUpdateCount * BobSpeed) * BobHeight;
            Projectile.Center = new Vector2(HomeX, HomeY + bob);

            SpillParticles();

            if (Main.GameUpdateCount % PoisonInterval == 0)
            {
                PoisonTheRing();
            }

            if (Main.GameUpdateCount % BeeInterval == 0)
            {
                ReleaseDrone();
            }
        }

        /// <summary>
        /// Applied where the game is authoritative, otherwise the debuff would never reach
        /// anyone else in a multiplayer world.
        /// </summary>
        private void PoisonTheRing()
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                return;
            }

            int poison = ModContent.BuffType<ShadowPoison>();
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.CanBeChasedBy(this))
                {
                    continue;
                }

                if (Vector2.Distance(npc.Center, Projectile.Center) <= AuraRadius)
                {
                    npc.AddBuff(poison, PoisonDuration);
                }
            }
        }

        private void ReleaseDrone()
        {
            if (Main.myPlayer != Projectile.owner)
            {
                return;
            }

            Player owner = Main.player[Projectile.owner];
            int beeType = ModContent.ProjectileType<ShadowHiveBee>();
            if (owner.ownedProjectileCounts[beeType] >= MaxBees)
            {
                return;
            }

            Vector2 exit = Projectile.Center + new Vector2(0f, 30f);
            int index = Projectile.NewProjectile(
                Projectile.GetSource_FromThis(),
                exit,
                new Vector2(Main.rand.NextFloat(-1.4f, 1.4f), 1.6f),
                beeType,
                Projectile.damage,
                Projectile.knockBack,
                Projectile.owner,
                ai0: Main.rand.NextFloat(MathHelper.TwoPi),
                ai1: Projectile.whoAmI);

            if (index >= 0)
            {
                Main.projectile[index].netUpdate = true;
            }
        }

        private void SpillParticles()
        {
            // Motes drifting up out of the hive itself.
            if (Main.rand.NextBool(3))
            {
                Dust mote = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height,
                    DustID.Smoke, 0f, -1.2f, 120, new Color(158, 104, 226), 0.9f);
                mote.noGravity = true;
            }

            // A couple of specks tracing the edge of the ring so its reach is readable.
            for (int i = 0; i < 2; i++)
            {
                if (!Main.rand.NextBool(2))
                {
                    continue;
                }

                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 rim = Projectile.Center + (angle.ToRotationVector2() * AuraRadius);
                Dust edge = Dust.NewDustPerfect(rim, DustID.Smoke, Vector2.Zero, 110,
                    new Color(178, 128, 240), 0.75f);
                edge.noGravity = true;
                edge.velocity = angle.ToRotationVector2() * -0.35f;
            }
        }

        /// <summary>
        /// Two alpha discs sitting on the same spot compound into a blob dark enough to hide
        /// whatever is standing in it, so stacked hives elect a single one to draw the ring.
        /// Hives placed meaningfully apart each keep their own.
        /// </summary>
        private bool OwnsTheRing()
        {
            for (int i = 0; i < Projectile.whoAmI; i++)
            {
                Projectile other = Main.projectile[i];
                if (!other.active || other.type != Projectile.type)
                {
                    continue;
                }

                if (Vector2.Distance(other.Center, Projectile.Center) < AuraRadius * 0.5f)
                {
                    return false;
                }
            }

            return true;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (!OwnsTheRing())
            {
                return true;
            }

            auraTexture ??= ModContent.Request<Texture2D>(
                "VenninBeeMod/Content/Projectiles/ShadowAura", AssetRequestMode.ImmediateLoad);

            Texture2D aura = auraTexture.Value;
            float pulse = 0.95f + ((float)System.Math.Sin(Main.GameUpdateCount * 0.05f) * 0.05f);
            float scale = (AuraRadius * 2f * pulse) / aura.Width;

            // Alpha of zero on the tint keeps this reading as a glow under normal blending.
            Main.spriteBatch.Draw(
                aura,
                Projectile.Center - Main.screenPosition,
                null,
                new Color(126, 66, 204, 0) * 0.42f,
                0f,
                new Vector2(aura.Width, aura.Height) / 2f,
                scale,
                SpriteEffects.None,
                0f);

            return true;
        }
    }
}
