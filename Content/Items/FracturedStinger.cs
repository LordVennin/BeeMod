using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace VenninBeeMod.Content.Items
{
    /// <summary>
    /// The rare half of a Prism Drone's drop, and the reason to keep hunting them once you have
    /// the wax you needed.
    /// </summary>
    public class FracturedStinger : ModItem
    {
        public override void SetStaticDefaults()
        {
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 5;
        }

        public override void SetDefaults()
        {
            Item.width = 12;
            Item.height = 26;
            Item.maxStack = Item.CommonMaxStack;
            Item.value = Item.sellPrice(silver: 40);
            Item.rare = ItemRarityID.LightRed;
        }
    }
}
