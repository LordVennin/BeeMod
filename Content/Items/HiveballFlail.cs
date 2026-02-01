using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using VenninBeeMod.Content.Projectiles;
using Terraria.DataStructures;

namespace VenninBeeMod.Content.Items
{
    public class HiveballFlail : ModItem
    {
        public override void SetStaticDefaults()
        {
            // Tooltip.SetDefault("A sticky hive on a string. It thuds... and it stings.");
        }

        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.damage = 10;
            Item.knockBack = 4f;
            Item.useTime = 40;
            Item.useAnimation = 40;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.DamageType = DamageClass.Melee;
            Item.noMelee = true; // handled by projectile
            Item.noUseGraphic = true;
            Item.channel = true;
            Item.shoot = ModContent.ProjectileType<HiveballProjectile>();
            Item.shootSpeed = 12f;
            Item.value = Item.buyPrice(silver: 5);
            Item.rare = ItemRarityID.White;
            Item.UseSound = SoundID.Item1;
        }

        private bool HasHivePack(Player player)
        {
            for (int i = 3; i < 10; i++)
                if (player.armor[i].type == ItemID.HiveBackpack)
                    return true;

            return false;
        }


        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (HasHivePack(player))
            {
                Vector2 offsetDir = velocity.SafeNormalize(Vector2.UnitX).RotatedBy(MathHelper.PiOver2); // perpendicular
                float spacing = 12f; // distance between each hive

                for (int i = -1; i <= 1; i++)
                {
                    Vector2 spawnOffset = offsetDir * i * spacing;
                    Vector2 perturbedVelocity = velocity.RotatedBy(MathHelper.ToRadians(10f * i));
                    Projectile.NewProjectile(source, position + spawnOffset, perturbedVelocity, type, damage, knockback, player.whoAmI);
                }
                return false; // Prevent default single shot
            }

            return true; // Fire single flail normally
        }


        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<StickyResin>(), 12)
                .AddIngredient(ItemID.Wood, 5)
                .AddRecipeGroup(RecipeGroupID.IronBar, 6)
                .AddTile(TileID.WorkBenches)
                .Register();
        }
    }
}