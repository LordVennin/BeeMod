using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace VenninBeeMod.Content.Items
{
    /// <summary>
    /// Comb that set into crystal somewhere in the Hallow. The gating material for the mod's
    /// early hardmode gear, so the tier has something of its own to hunt rather than being
    /// built entirely out of vanilla Souls.
    /// </summary>
    public class Prismwax : ModItem
    {
        public override void SetStaticDefaults()
        {
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 25;
            ItemID.Sets.ItemNoGravity[Type] = false;
        }

        public override void SetDefaults()
        {
            Item.width = 22;
            Item.height = 22;
            Item.maxStack = Item.CommonMaxStack;
            Item.value = Item.sellPrice(silver: 8);
            Item.rare = ItemRarityID.LightRed;
        }
    }
}
