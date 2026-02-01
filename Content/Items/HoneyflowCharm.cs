using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent.Creative;
using VenninBeeMod.Content;

namespace VenninBeeMod.Content.Items
{
    public class HoneyflowCharm : ModItem
    {
        public override void SetStaticDefaults()
        {
            // Tooltip.SetDefault("Move through honey at normal speed and gain life regeneration");
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
        }

        public override void SetDefaults()
        {
            Item.width = 20;
            Item.height = 20;
            Item.accessory = true;
            Item.rare = ItemRarityID.Green;
            Item.value = Item.buyPrice(silver: 75);
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.lifeRegen += 1;
            player.GetModPlayer<HoneyflowPlayer>().honeyflowActive = true;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.HoneyBlock, 20)
                .AddIngredient(ItemID.BottledHoney, 2)
                .AddIngredient(ItemID.Stinger, 3)
                .AddTile(TileID.WorkBenches)
                .Register();
        }
    }
}
