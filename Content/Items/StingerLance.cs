using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using VenninBeeMod.Content.Projectiles;
using Terraria.GameContent.Creative;
using System;

namespace VenninBeeMod.Content.Items
{
    public class StingerLance : ModItem
    {
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Stinger Lance");
            // Tooltip.SetDefault("Inflicts venom and occasionally releases a homing bee");
        }

        public override void SetDefaults()
        {

            Item.damage = 29;
            Item.DamageType = DamageClass.Melee;
            Item.width = 42;
            Item.height = 42;
            Item.useTime = 28;
            Item.useAnimation = 25;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 4.5f;
            Item.value = Item.buyPrice(gold: 1);
            Item.rare = ItemRarityID.Orange;
            Item.UseSound = SoundID.Item71;
            Item.autoReuse = true;
            Item.noUseGraphic = true;
            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<StingerLanceProjectile>();
            Item.shootSpeed = 3.7f;

        }

        public override bool CanUseItem(Player player)
        {

            if (player.altFunctionUse == 2) // Right-click
            {
                Item.useStyle = ItemUseStyleID.Shoot;
                Item.damage = 29;
                Item.useTime = 28;
                Item.useAnimation = 28;
                Item.channel = false;
                Item.noUseGraphic = true;
                Item.noMelee = true;
                Item.shoot = ModContent.ProjectileType<StingerLanceProjectile>();
                Item.shootSpeed = 3f; // Used to pass thrust direction
            }
            else // Left-click: thrusting spear
            {
                Item.useStyle = ItemUseStyleID.Shoot;
                Item.damage = 21;
                Item.useTime = 28;
                Item.useAnimation = 28;
                Item.channel = false;
                Item.noUseGraphic = true;
                Item.noMelee = true;
                Item.shoot = ModContent.ProjectileType<StingerBeeShakerProjectile>();
                Item.shootSpeed = 3.7f;
            }
            // Only one spear out at a time
            return player.ownedProjectileCounts[Item.shoot] < 1;
            return base.CanUseItem(player);
        }
        public override bool AltFunctionUse(Player player) => true;

        public override void ModifyWeaponDamage(Player player, ref StatModifier damage)
        {
            if (HasHivePack(player))
            {
                damage *= 1.15f; // 15% more damage
            }
        }

        private bool HasHivePack(Player player)
        {
            for (int i = 3; i < 10; i++) // accessory slots
            {
                if (player.armor[i].type == ItemID.HiveBackpack)
                    return true;
            }
            return false;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.BeeWax, 12)
                .AddIngredient(ItemID.Stinger, 8)
                .AddIngredient(ItemID.JungleSpores, 6)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
}