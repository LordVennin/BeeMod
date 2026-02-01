using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using VenninBeeMod.Content.Projectiles;

namespace VenninBeeMod.Content
{
    public class RoyalBeePlayer : ModPlayer
    {
        public bool royalHoneyActive;
        public bool sigilActive;

        private int beeTimer;
        private const int sigilBeeCount = 6;

        public override void ResetEffects()
        {
            royalHoneyActive = false;
            sigilActive = false;
        }

        public override void PostUpdate()
        {
            if (royalHoneyActive && Player.statLife < Player.statLifeMax2 / 2)
            {
                beeTimer++;
                if (beeTimer >= 180) // 5 seconds
                {
                    beeTimer = 0;

                    if (Main.myPlayer == Player.whoAmI)
                    {
                        Vector2 spawnPos = Player.Center + new Vector2(Main.rand.NextFloat(-20, 20), -16);
                        Vector2 velocity = new Vector2(Main.rand.NextFloat(-1f, 1f), -3f);

                        Projectile.NewProjectile(Player.GetSource_FromThis(), spawnPos, velocity,
                            ModContent.ProjectileType<HealingBee>(), 6, 0f, Player.whoAmI);
                    }
                }
            }
            else
            {
                beeTimer = 0;
            }

            // Updated logic to ensure despawn if accessory is unequipped
            bool shouldMaintainBees = sigilActive && Player.statLife < Player.statLifeMax2 / 2;

            if (shouldMaintainBees)
            {
                if (Main.myPlayer == Player.whoAmI && Player.ownedProjectileCounts[ModContent.ProjectileType<RoyalBeeOrbiter>()] < sigilBeeCount)
                {
                    for (int i = 0; i < sigilBeeCount; i++)
                    {
                        int proj = Projectile.NewProjectile(Player.GetSource_FromThis(), Player.Center, Vector2.Zero,
                            ModContent.ProjectileType<RoyalBeeOrbiter>(), 10, 0f, Player.whoAmI, i, sigilBeeCount);
                        Main.projectile[proj].netUpdate = true;
                    }
                }
            }
            else
            {
                for (int i = 0; i < Main.maxProjectiles; i++)
                {
                    Projectile p = Main.projectile[i];
                    if (p.active && p.owner == Player.whoAmI && p.type == ModContent.ProjectileType<RoyalBeeOrbiter>())
                    {
                        p.Kill();
                    }
                }
            }
        }
    }
}