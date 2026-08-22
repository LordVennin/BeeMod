using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;
using VenninBeeMod.Content;

namespace VenninBeeMod.Content.Items
{
    /// <summary>
    /// Skeleton Bee hide stitched over its own bones. The rot it carried in life keeps working
    /// through whatever your swarm touches, and enemies pay you less attention while you are
    /// wearing something that died a long time ago.
    /// </summary>
    public class RotweaveShroud : ModItem
    {
        /// <summary>
        /// Roughly three quarters of a Putrid Scent. Enough to matter for a summoner standing
        /// behind their bees, not enough to make you invisible.
        /// </summary>
        private const int AggroReduction = 300;

        public override void SetStaticDefaults()
        {
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
        }

        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.accessory = true;
            Item.rare = ItemRarityID.Orange;
            Item.value = Item.sellPrice(gold: 1);
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<RotweavePlayer>().rotweave = true;
            player.aggro -= AggroReduction;
        }

        public override void AddRecipes()
        {
            // Hide is the Skeleton Bee's common drop, so this is the reason to keep hunting them
            // once you have the stingers you wanted.
            CreateRecipe()
                .AddIngredient<TatteredBeeHide>(8)
                .AddIngredient(ItemID.Bone, 25)
                .AddIngredient<StickyResin>(15)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
}
