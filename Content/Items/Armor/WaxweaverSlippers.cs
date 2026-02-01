using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent.Creative;

namespace VenninBeeMod.Content.Items.Armor
{
    [AutoloadEquip(EquipType.Legs)]
    public class WaxweaverSlippers : ModItem
    {
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Waxweaver Slippers");
            // Tooltip.SetDefault("Increases maximum mana by 20");

            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
        }

        public override void SetDefaults()
        {
            Item.width = 22;
            Item.height = 18;
            Item.value = Item.buyPrice(silver: 70);
            Item.rare = ItemRarityID.Orange;
            Item.defense = 4;
        }

        public override void UpdateEquip(Player player)
        {
            player.statManaMax2 += 20;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.BeeWax, 8)
                .AddIngredient(ItemID.Stinger, 4)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
}