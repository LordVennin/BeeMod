using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Terraria.GameContent.Creative;
using VenninBeeMod.Content.Projectiles;

namespace VenninBeeMod.Content.Items
{
    public class RoyalHoneyBrooch : ModItem
    {
        public override void SetStaticDefaults()
        {
            // Tooltip.SetDefault("Spawns healing bees when you are injured");
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
        }

        public override void SetDefaults()
        {
            Item.width = 20;
            Item.height = 22;
            Item.accessory = true;
            Item.rare = ItemRarityID.Orange;
            Item.value = Item.buyPrice(gold: 1);
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<RoyalBeePlayer>().royalHoneyActive = true;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.BeeWax, 8)
                .AddIngredient(ItemID.HoneyBlock, 10)
                .AddIngredient(ItemID.BottledHoney, 1)
                .AddTile(TileID.WorkBenches)
                .Register();
        }
    }
}