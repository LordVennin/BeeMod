using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using VenninBeeMod.Content.Projectiles;
using Microsoft.Xna.Framework;

namespace VenninBeeMod.Content.Buffs
{
    public class HiveMindBuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Hive Mind");
            // Description.SetDefault("A hornet fights alongside you");

            Main.buffNoSave[Type] = true;
            Main.buffNoTimeDisplay[Type] = false;
        }

        private Projectile FindHornet(Player player)
        {
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile proj = Main.projectile[i];
                if (proj.active && proj.owner == player.whoAmI && proj.type == ModContent.ProjectileType<HiveHornetMinion>())
                    return proj;
            }
            return null;
        }
        public override void Update(Player player, ref int buffIndex)
        {
            player.GetModPlayer<HivepiercerPlayer>().hasHiveMind = true;

            if (player.whoAmI == Main.myPlayer)
            {
                //  Ensure the hornet minion is summoned
                if (player.ownedProjectileCounts[ModContent.ProjectileType<HiveHornetMinion>()] < 1)
                {
                    Projectile.NewProjectile(
                        player.GetSource_Buff(buffIndex),
                        player.Center,
                        Vector2.Zero,
                        ModContent.ProjectileType<HiveHornetMinion>(),
                        0,
                        0f,
                        player.whoAmI
                    );
                }

                //  Fire projectile from the hornet if cooldown is ready
                HivepiercerPlayer modPlayer = player.GetModPlayer<HivepiercerPlayer>();
                Item heldItem = player.HeldItem;

                if (modPlayer.hornetShootCooldown <= 0 &&
                    heldItem.DamageType == DamageClass.Ranged &&
                    player.itemTime == player.HeldItem.useTime)
                {
                    Projectile hornet = FindHornet(player);
                    if (hornet != null)
                    {
                        Vector2 shootDirection = (Main.MouseWorld - hornet.Center).SafeNormalize(Vector2.UnitX);
                        Vector2 spawnOffset = shootDirection * 6f + new Vector2(0f, 4f); // 4 pixels lower
                        Vector2 spawnPosition = hornet.Center + spawnOffset;
                        int damage = (int)(player.GetWeaponDamage(heldItem) * 0.9f);


                        Projectile.NewProjectile(
                            hornet.GetSource_FromThis(),
                            spawnPosition,
                            shootDirection * 10f,
                            ModContent.ProjectileType<HornetStingerProjectile>(),
                            damage,
                            1f,
                            player.whoAmI
                        );

                        modPlayer.hornetShootCooldown = 2;
                    }
                }
            }
        }
    }

}
