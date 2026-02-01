using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using VenninBeeMod.Content.Projectiles;
using Terraria.GameContent.Creative;
using System;

namespace VenninBeeMod.Content.Items
{
    public class HoneyDripperWand : ModItem
    {
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Honeydripper Wand");
            // Tooltip.SetDefault("Fires sticky honey globs that may spawn bees on hit\nHive Pack boosts effects");
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
        }

        public override void SetDefaults()
        {
            Item.damage = 19;
            Item.DamageType = DamageClass.Magic;
            Item.mana = 15;
            Item.width = 30;
            Item.height = 30;
            Item.scale = 2f;
            Item.useTime = 55;
            Item.useAnimation = 55;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 2f;
            Item.value = Item.buyPrice(silver: 80);
            Item.rare = ItemRarityID.Orange;
            Item.UseSound = SoundID.Item21;
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<HoneyGlobProjectile>(); // Will implement next
            Item.shootSpeed = 7f;
        }

        public override void UseStyle(Player player, Rectangle heldItemFrame)
        {
            Vector2 aimDirection = Main.MouseWorld - player.MountedCenter;
            float rotation = aimDirection.ToRotation();
            player.ChangeDir(Math.Sign(aimDirection.X));

            if (player.direction == -1)
                rotation += MathHelper.Pi;

            player.itemRotation = rotation;

            // Offset relative to aim direction — keeps the wand aligned
            float offsetDistance = -32f; // tweak to tighten or loosen distance from hand
            Vector2 offset = aimDirection.SafeNormalize(Vector2.UnitX) * offsetDistance;

            // Adjust up or down slightly for better hand connection
            offset.Y += -18f;

            player.itemLocation = player.MountedCenter + offset;
        }



        public override void ModifyWeaponDamage(Player player, ref StatModifier damage)
        {
            if (HasHivePack(player))
            {
                damage *= 1.15f;
            }
        }

        private bool HasHivePack(Player player)
        {
            for (int i = 3; i < 10; i++)
            {
                if (player.armor[i].type == ItemID.HiveBackpack)
                    return true;
            }
            return false;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.BeeWax, 10)
                .AddIngredient(ItemID.HoneyBlock, 15)
                .AddIngredient(ItemID.Stinger, 6)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
}
