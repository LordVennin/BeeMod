
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;

namespace VenninBeeMod.Content.Projectiles
{
    public class HiveTurret : ModProjectile
    {
        private int contactCooldown = 0;
        private int shootTimer;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.MinionShot[Projectile.type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = 24;
            Projectile.height = 24;
            Projectile.scale = 1.5f;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.ignoreWater = false;
            Projectile.tileCollide = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.timeLeft = 5400; // 30 seconds
            Projectile.sentry = true;
            Projectile.aiStyle = 0;
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            // Stop movement on ground, but prevent despawn
            Projectile.velocity = Vector2.Zero;
            return false; // Do not kill the projectile
        }

        public override void AI()
        {
            if (contactCooldown > 0)
                contactCooldown--;

            Player player = Main.player[Projectile.owner];
            if (!player.active || player.dead)
            {
                Projectile.Kill();
                return;
            }

            // Gravity
            Projectile.velocity.Y += 0.2f;
            Projectile.velocity.Y = MathHelper.Clamp(Projectile.velocity.Y, -10f, 10f);

            // Stop moving after landing
            if (Projectile.velocity.Y == 0f && Projectile.oldVelocity.Y > 0f)
            {
                Projectile.velocity = Vector2.Zero;
            }

            float contactRadius = 30f;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && !npc.friendly && npc.CanBeChasedBy(this) && npc.Hitbox.Intersects(Projectile.Hitbox) && contactCooldown <= 0)
                {
                    contactCooldown = 30;
                    // Spawn a burst of bees around the hive
                    for (int j = 0; j < 2; j++)
                    {
                        Vector2 beeVelocity = new Vector2(Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-4f, -1f));
                        int bee = Projectile.NewProjectile(
                            Projectile.GetSource_FromThis(),
                            Projectile.Center,
                            beeVelocity,
                            ProjectileID.Bee,
                            Projectile.damage,
                            Projectile.knockBack,
                            Projectile.owner
                        );

                        if (Main.projectile.IndexInRange(bee))
                        {
                            Main.projectile[bee].friendly = true;
                            Main.projectile[bee].hostile = false;
                            Main.projectile[bee].DamageType = DamageClass.Summon;
                        }
                    }

                    SoundEngine.PlaySound(SoundID.NPCHit1, Projectile.position);
                    break; // prevent over-spawning from multiple NPCs
                }
            }

            // Timer
            shootTimer++;
            if (shootTimer >= 160) // once per second
            {
                shootTimer = 0;

                Vector2 direction = new Vector2(Main.rand.NextFloat(-1f, 1f), -1f).SafeNormalize(Vector2.UnitY) * 6f;

                int bee = Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    Projectile.Center,
                    direction,
                    ProjectileID.Bee,
                    Projectile.damage,
                    Projectile.knockBack,
                    Projectile.owner
                );

                if (Main.projectile.IndexInRange(bee))
                {
                    Main.projectile[bee].friendly = true;
                    Main.projectile[bee].hostile = false;
                    Main.projectile[bee].DamageType = DamageClass.Summon;
                }

                SoundEngine.PlaySound(SoundID.Item17, Projectile.position);
            }

          
        }
    }
}
