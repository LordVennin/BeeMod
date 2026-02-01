using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using VenninBeeMod.Content.Projectiles;

namespace VenninBeeMod.Content.Items
{
    public class PocketBees : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 99;
        }

        public override void SetDefaults()
        {
            Item.damage = 4;
            Item.DamageType = DamageClass.Ranged;
            Item.width = 18;
            Item.height = 18;
            Item.maxStack = 999;
            Item.consumable = true;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useTime = 22;
            Item.useAnimation = 22;
            Item.noUseGraphic = true;
            Item.noMelee = true;
            Item.knockBack = 1f;
            Item.value = Item.buyPrice(copper: 50);
            Item.rare = ItemRarityID.White;
            Item.UseSound = SoundID.Item17;
            Item.shoot = ModContent.ProjectileType<PocketBeeProjectile>();
            Item.shootSpeed = 6f;
            Item.autoReuse = true;
        }

        public override bool Shoot(Player player, Terraria.DataStructures.EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            int beeCount = Main.rand.Next(10, 21);
            for (int i = 0; i < beeCount; i++)
            {
                Vector2 perturbedSpeed = velocity.RotatedByRandom(MathHelper.ToRadians(25)) * Main.rand.NextFloat(0.6f, 1.1f);
                Projectile.NewProjectile(source, position, perturbedSpeed, type, damage, knockback, player.whoAmI);
            }
            return false; // Prevents default single projectile
        }

        public override void AddRecipes()
        {
            CreateRecipe(15)
                .AddIngredient(ModContent.ItemType<StickyResin>(), 2)
                .AddTile(TileID.WorkBenches)
                .Register();
        }
    }
}
