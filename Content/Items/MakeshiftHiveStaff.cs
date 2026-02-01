
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent.Creative;
using VenninBeeMod.Content.Projectiles;
using VenninBeeMod.Content.Items; // for StickyResin
using Terraria.DataStructures;

namespace VenninBeeMod.Content.Items
{
    public class MakeshiftHiveStaff : ModItem
    {
        public override void SetStaticDefaults()
        {
            ItemID.Sets.GamepadWholeScreenUseRange[Item.type] = true;
            ItemID.Sets.LockOnIgnoresCollision[Item.type] = true;
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Item.type] = 1;
        }

        public override void SetDefaults()
        {
            Item.damage = 5;
            Item.DamageType = DamageClass.Summon;
            Item.mana = 10;
            Item.width = 32;
            Item.height = 32;
            Item.useTime = 28;
            Item.useAnimation = 28;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.noMelee = true;
            Item.knockBack = 1f;
            Item.value = Item.buyPrice(silver: 10);
            Item.rare = ItemRarityID.White;
            Item.UseSound = SoundID.Item44;
            Item.sentry = true;
            Item.shoot = ModContent.ProjectileType<HiveTurret>();
            Item.shootSpeed = 0f;
            Item.autoReuse = false;
        }

        public override Vector2? HoldoutOffset()
        {
            return new Vector2(-30, 0); // X moves it left/right, Y moves it up/down
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // Count all sentries
            int activeSentries = 0;

            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile proj = Main.projectile[i];
                if (proj.active && proj.owner == player.whoAmI && proj.sentry)
                {
                    activeSentries++;
                    if (activeSentries >= player.maxTurrets)
                    {
                        proj.Kill(); // remove oldest sentry, regardless of type
                        break;
                    }
                }
            }

            // Spawn new hive sentry
            Projectile.NewProjectile(source, Main.MouseWorld, Vector2.Zero, type, damage, knockback, player.whoAmI);
            return false;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddRecipeGroup(RecipeGroupID.IronBar, 2)
                .AddIngredient(ModContent.ItemType<StickyResin>(), 8)
                .AddIngredient(ItemID.Wood, 6)
                .AddIngredient(ItemID.Rope, 4)
                .AddTile(TileID.WorkBenches)
                .Register();
        }
    }
}
