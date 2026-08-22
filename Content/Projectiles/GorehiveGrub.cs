using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace VenninBeeMod.Content.Projectiles
{
    /// <summary>
    /// Planted by the Gorehive Maul. It rides what it was hammered into, chews while it is in
    /// there, and comes out as a knot of bees. Keep hitting the same enemy and the grub grows,
    /// which is what makes the maul's slow swing worth committing to one target.
    /// </summary>
    public class GorehiveGrub : ModProjectile
    {
        private const int FeedTime = 300;

        /// <summary>Bees at one stack, plus one more for each stack after.</summary>
        private const int BaseBees = 2;

        private const int MaxStacks = 4;

        /// <summary>Hive Pack secret: it can be fed half again as full.</summary>
        private const int HivePackMaxStacks = 6;

        private const float BeeDamageShare = 0.3f;

        /// <summary>Share of the maul's hit the grub gnaws off per stack, per bite.</summary>
        private const float GnawShare = 0.1f;
        private const int GnawInterval = 30;

        /// <summary>A fresh hit hurries the grub along rather than resetting it.</summary>
        private const int FeedBonusPerStack = 40;

        private ref float Host => ref Projectile.ai[0];
        private ref float FeedTimer => ref Projectile.ai[1];
        private ref float Stacks => ref Projectile.ai[2];

        private Vector2 rideOffset;

        public static int MaxStacksFor(Player player)
        {
            return HivePack.IsEquipped(player) ? HivePackMaxStacks : MaxStacks;
        }

        /// <summary>
        /// Plants a grub, or feeds the one already in there.
        /// </summary>
        public static void Implant(Player player, NPC target, int weaponDamage)
        {
            if (!CombatTarget.IsReal(target))
            {
                return;
            }

            int grubType = ModContent.ProjectileType<GorehiveGrub>();
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile other = Main.projectile[i];
                if (!other.active || other.type != grubType || other.owner != player.whoAmI
                    || (int)other.ai[0] - 1 != target.whoAmI)
                {
                    continue;
                }

                // Already one in there, so this swing feeds it instead of being wasted.
                if (other.ai[2] < MaxStacksFor(player))
                {
                    other.ai[2]++;
                    other.ai[1] += FeedBonusPerStack;
                    other.damage = System.Math.Max(other.damage, weaponDamage);
                    other.netUpdate = true;

                    SoundEngine.PlaySound(SoundID.Item17 with { Volume = 0.4f, Pitch = 0.5f }, target.Center);
                }

                return;
            }

            // The grub never hits anything itself, so its damage field carries the maul's hit
            // for the gnawing and the brood to be worked out from.
            int index = Projectile.NewProjectile(
                player.GetSource_ItemUse(player.HeldItem),
                target.Center,
                Vector2.Zero,
                grubType,
                System.Math.Max(1, weaponDamage),
                0f,
                player.whoAmI,
                ai0: target.whoAmI + 1,
                ai1: 0f);

            if (index >= 0)
            {
                Main.projectile[index].ai[2] = 1f;
                Main.projectile[index].netUpdate = true;
            }

            SoundEngine.PlaySound(SoundID.Item17 with { Volume = 0.5f, Pitch = -0.3f }, target.Center);
        }

        public override void SetDefaults()
        {
            Projectile.width = 12;
            Projectile.height = 10;

            // It does its damage by gnawing rather than by collision.
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = FeedTime + 120;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.netImportant = true;
        }

        public override void AI()
        {
            NPC host = ResolveHost();
            if (host == null)
            {
                // The host died. The brood comes out then and there, which is the point of
                // planting one before a kill rather than after.
                Burst(null);
                return;
            }

            if (rideOffset == Vector2.Zero)
            {
                rideOffset = new Vector2(
                    Main.rand.NextFloat(-1f, 1f) * host.width * 0.22f,
                    Main.rand.NextFloat(-1f, 1f) * host.height * 0.22f);
            }

            Projectile.Center = host.Center + rideOffset;
            Projectile.velocity = Vector2.Zero;

            FeedTimer++;

            float progress = MathHelper.Clamp(FeedTimer / FeedTime, 0f, 1f);
            float fatness = 1f + ((Stacks - 1f) * 0.16f);

            Projectile.rotation = (float)System.Math.Sin(FeedTimer * (0.12f + (progress * 0.35f)))
                * (0.15f + (progress * 0.4f));
            Projectile.scale = (0.8f + (progress * 0.45f)) * fatness;

            if (FeedTimer % GnawInterval == 0f)
            {
                Gnaw(host);
            }

            if (Main.rand.NextFloat() < 0.05f + (progress * 0.2f))
            {
                Dust seep = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height,
                    DustID.Blood, 0f, 0f, 90, default, 0.9f);
                seep.velocity *= 0.4f;
            }

            if (FeedTimer >= FeedTime)
            {
                Burst(host);
            }
        }

        /// <summary>
        /// The grub eating its way out, worth more the fatter it is. This is most of what makes
        /// the maul's damage arrive at all between its slow swings.
        /// </summary>
        private void Gnaw(NPC host)
        {
            if (Main.myPlayer != Projectile.owner)
            {
                return;
            }

            Player owner = Main.player[Projectile.owner];
            if (!owner.active || owner.dead)
            {
                return;
            }

            int bite = System.Math.Max(1, (int)(Projectile.damage * GnawShare * Stacks));
            owner.ApplyDamageToNPC(host, bite, 0f, 0, false);
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

        private void Burst(NPC host)
        {
            if (Main.myPlayer == Projectile.owner)
            {
                int count = BaseBees + (int)System.Math.Max(1f, Stacks);
                int damage = System.Math.Max(1, (int)(Projectile.damage * BeeDamageShare));
                int preferred = host != null ? host.whoAmI + 1 : 0;

                for (int i = 0; i < count; i++)
                {
                    Vector2 velocity = Main.rand.NextVector2CircularEdge(4.5f, 4.5f) * Main.rand.NextFloat(0.6f, 1f);
                    int index = Projectile.NewProjectile(
                        Projectile.GetSource_FromThis(),
                        Projectile.Center,
                        velocity,
                        ModContent.ProjectileType<BloodBee>(),
                        damage,
                        1f,
                        Projectile.owner,
                        ai0: preferred);

                    if (index >= 0)
                    {
                        Main.projectile[index].netUpdate = true;
                    }
                }
            }

            SoundEngine.PlaySound(SoundID.Item14 with { Volume = 0.5f, Pitch = 0.4f }, Projectile.Center);

            for (int i = 0; i < 16; i++)
            {
                Dust gore = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height,
                    DustID.Blood, 0f, 0f, 80, default, 1.2f);
                gore.velocity = Main.rand.NextVector2Circular(3f, 3f);
            }

            Projectile.Kill();
        }
    }
}
