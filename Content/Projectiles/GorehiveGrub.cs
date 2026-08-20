using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace VenninBeeMod.Content.Projectiles
{
    /// <summary>
    /// Planted by the Gorehive Maul. It rides whatever it was hammered into, feeds for five
    /// seconds and then bursts out as a knot of bees. One grub per enemy - the maul is meant to
    /// be about picking your target, not about swinging faster.
    /// </summary>
    public class GorehiveGrub : ModProjectile
    {
        private const int FeedTime = 300;
        private const int BeeCount = 4;

        /// <summary>Share of the maul's hit that each bee carries.</summary>
        private const float BeeDamageShare = 0.35f;

        /// <summary>Index of the host NPC plus one.</summary>
        private ref float Host => ref Projectile.ai[0];

        private ref float FeedTimer => ref Projectile.ai[1];

        private Vector2 rideOffset;

        /// <summary>
        /// Plants a grub in <paramref name="target"/> unless one is already in there.
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
                if (other.active && other.type == grubType && other.owner == player.whoAmI
                    && (int)other.ai[0] - 1 == target.whoAmI)
                {
                    return;
                }
            }

            // The grub itself never hits anything, so its damage field is free to carry what
            // the bees inside it should be worth.
            int beeDamage = System.Math.Max(1, (int)(weaponDamage * BeeDamageShare));
            int index = Projectile.NewProjectile(
                player.GetSource_ItemUse(player.HeldItem),
                target.Center,
                Vector2.Zero,
                grubType,
                beeDamage,
                0f,
                player.whoAmI,
                ai0: target.whoAmI + 1);

            if (index >= 0)
            {
                Main.projectile[index].netUpdate = true;
            }

            SoundEngine.PlaySound(SoundID.NPCHit13 with { Volume = 0.5f }, target.Center);
        }

        public override void SetDefaults()
        {
            Projectile.width = 12;
            Projectile.height = 10;

            // Pure payload. The maul's swing is the damage; this is what comes after.
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = FeedTime + 60;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.netImportant = true;
        }

        public override void AI()
        {
            NPC host = ResolveHost();
            if (host == null)
            {
                // Nothing left to eat, so it comes out early rather than being wasted.
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

            // Twitches harder the closer it is to coming out.
            float progress = FeedTimer / FeedTime;
            Projectile.rotation = (float)System.Math.Sin(FeedTimer * (0.12f + (progress * 0.35f))) * (0.15f + (progress * 0.4f));
            Projectile.scale = 0.8f + (progress * 0.45f);

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

        /// <summary>
        /// Hive Pack secret: a grub whose host dies early still hatches. Without the pack an
        /// early kill wastes it, which is the cost of the maul being a slow, committed swing.
        /// </summary>
        private void Burst(NPC host)
        {
            if (host == null && !HivePack.IsEquipped(Main.player[Projectile.owner]))
            {
                Projectile.Kill();
                return;
            }

            if (Main.myPlayer == Projectile.owner)
            {
                int damage = System.Math.Max(1, Projectile.damage);
                int preferred = host != null ? host.whoAmI + 1 : 0;

                for (int i = 0; i < BeeCount; i++)
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

            SoundEngine.PlaySound(SoundID.NPCDeath13 with { Volume = 0.7f }, Projectile.Center);

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
