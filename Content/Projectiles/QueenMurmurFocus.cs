using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using VenninBeeMod.Content.Items;

namespace VenninBeeMod.Content.Projectiles
{
    public class QueenMurmurFocus : ModProjectile
    {
        private const int MaxBees = 50;
        private const int BeeSpawnInterval = 6;
        private const int StreamInterval = 5;
        private const int ReleaseIgnoreFrames = 10;
        public override string Texture => "VenninBeeMod/Content/Projectiles/BeeFollowerMinion";

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 4;
        }

        public override void SetDefaults()
        {
            Projectile.width = 18;
            Projectile.height = 18;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 2;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.hide = true;
        }

        public override bool? CanDamage() => false;

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            if (!player.active || player.dead)
            {
                Projectile.Kill();
                return;
            }

            Projectile.Center = player.MountedCenter;
            Projectile.velocity = Vector2.Zero;

            bool isChanneling = player.channel && !player.noItems && !player.CCed;
            if (!isChanneling)
            {
                ReleaseBees(player);
                Projectile.Kill();
                return;
            }

            Projectile.timeLeft = 2;
            player.itemTime = 2;
            player.itemAnimation = 2;

            int beeCount = CountChargingBees();
            HandleStreamFire(player, beeCount);

            if (beeCount < MaxBees)
            {
                Projectile.localAI[0]++;
                if (Projectile.localAI[0] >= BeeSpawnInterval)
                {
                    Projectile.localAI[0] = 0f;
                    int manaCost = GetManaCost(player);
                    if (!player.CheckMana(manaCost, true))
                    {
                        ReleaseBees(player);
                        Projectile.Kill();
                        return;
                    }
                    SpawnBee(player);
                }
            }
        }

        private void HandleStreamFire(Player player, int beeCount)
        {
            bool wantsStream = Projectile.owner == Main.myPlayer && Main.mouseRight;
            if (!wantsStream || beeCount == 0)
            {
                Projectile.localAI[1] = 0f;
                return;
            }

            Projectile.localAI[1]++;
            if (Projectile.localAI[1] < StreamInterval)
            {
                return;
            }

            Projectile.localAI[1] = 0f;
            Vector2 targetDirection = (Main.MouseWorld - player.Center).SafeNormalize(Vector2.UnitX * player.direction);
            ReleaseSingleBee(player, targetDirection, 12f);
        }

        private void SpawnBee(Player player)
        {
            if (Main.myPlayer != Projectile.owner)
            {
                return;
            }

            Vector2 spawnOffset = Main.rand.NextVector2CircularEdge(32f, 32f);
            Vector2 velocity = spawnOffset.SafeNormalize(Vector2.UnitX) * Main.rand.NextFloat(2f, 4f);
            int type = ModContent.ProjectileType<QueenMurmurBee>();
            int bee = Projectile.NewProjectile(
                Projectile.GetSource_FromThis(),
                player.Center + spawnOffset,
                velocity,
                type,
                Projectile.damage,
                Projectile.knockBack,
                Projectile.owner
            );

            if (bee >= 0 && bee < Main.maxProjectiles)
            {
                Main.projectile[bee].netUpdate = true;
            }
        }

        private int GetManaCost(Player player)
        {
            Item heldItem = player.HeldItem;
            int baseCost = SoulDripScepter.ManaPerBee;
            if (heldItem != null && heldItem.type == ModContent.ItemType<SoulDripScepter>())
            {
                baseCost = heldItem.mana;
            }

            return (int)Math.Ceiling(baseCost * player.manaCost);
        }

        private int CountChargingBees()
        {
            int count = 0;
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile projectile = Main.projectile[i];
                if (projectile.active
                    && projectile.owner == Projectile.owner
                    && projectile.type == ModContent.ProjectileType<QueenMurmurBee>()
                    && projectile.ai[0] == 0f)
                {
                    count++;
                }
            }

            return count;
        }

        private void ReleaseBees(Player player)
        {
            if (Main.myPlayer != Projectile.owner)
            {
                return;
            }

            Vector2 targetDirection = (Main.MouseWorld - player.Center).SafeNormalize(Vector2.UnitX * player.direction);
            float speed = 12f;

            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile projectile = Main.projectile[i];
                if (!projectile.active || projectile.owner != Projectile.owner || projectile.type != ModContent.ProjectileType<QueenMurmurBee>())
                {
                    continue;
                }

                projectile.ai[0] = 1f;
                projectile.ai[1] = -ReleaseIgnoreFrames;
                projectile.timeLeft = 120;
                projectile.tileCollide = false;
                projectile.velocity = targetDirection.RotatedBy(Main.rand.NextFloat(-0.2f, 0.2f)) * speed;
                projectile.netUpdate = true;
            }
        }

        private bool ReleaseSingleBee(Player player, Vector2 direction, float speed)
        {
            if (Main.myPlayer != Projectile.owner)
            {
                return false;
            }

            int beeType = ModContent.ProjectileType<QueenMurmurBee>();
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile projectile = Main.projectile[i];
                if (!projectile.active || projectile.owner != Projectile.owner || projectile.type != beeType || projectile.ai[0] != 0f)
                {
                    continue;
                }

                projectile.ai[0] = 1f;
                projectile.ai[1] = -ReleaseIgnoreFrames;
                projectile.timeLeft = 120;
                projectile.tileCollide = false;
                projectile.velocity = direction * speed;
                projectile.netUpdate = true;
                return true;
            }

            return false;
        }
    }
}
