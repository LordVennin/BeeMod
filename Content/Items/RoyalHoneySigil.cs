using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Terraria.GameContent.Creative;

namespace VenninBeeMod.Content.Items
{
    public class RoyalHoneySigil : ModItem
    {
        public override void SetStaticDefaults()
        {
            // Tooltip.SetDefault("Summons orbiting healing bees while injured");
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
        }

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 26;
            Item.accessory = true;
            Item.rare = ItemRarityID.LightRed;
            Item.value = Item.buyPrice(gold: 3);
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<RoyalBeePlayer>().sigilActive = true;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<RoyalHoneyBrooch>()
                .AddIngredient(ItemID.SoulofLight, 3)
                .AddIngredient(ItemID.SoulofNight, 3)
                .AddTile(TileID.TinkerersWorkbench)
                .Register();
        }
    }
}