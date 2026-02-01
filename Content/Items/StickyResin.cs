using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Terraria.GameContent.Creative;

namespace VenninBeeMod.Content.Items
{
    public class StickyResin : ModItem
    {
        public override void SetStaticDefaults()
        {
            // Tooltip.SetDefault("A gooey substance left behind by wild bees.");
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 25;
        }

        public override void SetDefaults()
        {
            Item.width = 20;
            Item.height = 20;
            Item.maxStack = 999;
            Item.value = Item.buyPrice(copper: 10);
            Item.rare = ItemRarityID.White;
        }
    }
}