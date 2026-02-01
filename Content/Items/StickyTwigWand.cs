using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using VenninBeeMod.Content.Projectiles;
using Terraria.GameContent.Creative;
using System;
using Terraria.DataStructures;

namespace VenninBeeMod.Content.Items
{
    public class StickyTwigWand : ModItem
    {
        public override void SetStaticDefaults()
        {
            // Tooltip.SetDefault("Sprays a short-range burst of sticky honey. Slows enemies.");
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
        }

        public override void SetDefaults()
        {
            Item.damage = 9;
            Item.DamageType = DamageClass.Magic;
            Item.mana = 2;
            Item.width = 28;
            Item.height = 28;
            Item.scale = 1.3f;
            Item.useTime = 7;
            Item.useAnimation = 7;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 0.5f;
            Item.value = Item.buyPrice(silver: 10);
            Item.rare = ItemRarityID.White;
            Item.UseSound = SoundID.Item13;
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<StickyHoneySprayProjectile>();
            Item.shootSpeed = 6f;
            Item.channel = true;
        }

        public override void UseStyle(Player player, Rectangle heldItemFrame)
        {
            if (player.itemAnimation <= 0)
                return;

            if (player.whoAmI != Main.myPlayer)
                return; // Prevent flipping for other players in multiplayer

            Vector2 aim = Main.MouseWorld - player.MountedCenter;
            aim.Normalize();

            player.ChangeDir(aim.X > 0 ? 1 : -1);

            // Horizontal offset
            float xOffset = -6f * player.direction;

            // Dynamic vertical offset — higher when aiming downward
            float baseYOffset = -12f;
            float extraLift = MathHelper.Clamp(aim.Y, -1f, 1f) * -6f;
            float yOffset = baseYOffset + extraLift;

            player.itemLocation = player.MountedCenter + new Vector2(xOffset, yOffset);
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // Offset spawn position slightly forward along aim direction
            Vector2 aim = velocity.SafeNormalize(Vector2.UnitX);
            position += aim * 50f;

            Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);

            return false; // prevents vanilla from spawning the default projectile
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.Wood, 6)
                .AddIngredient(ModContent.ItemType<StickyResin>(), 12)
                .AddIngredient(ItemID.ManaCrystal, 1)
                .AddTile(TileID.WorkBenches)
                .Register();
        }
    }
}
