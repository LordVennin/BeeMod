using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent.Creative;

namespace VenninBeeMod.Content.Items.Armor
{
    [AutoloadEquip(EquipType.Legs)]
    public class HivepiercerTarsi : ModItem
    {
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Hivepiercer Tarsi");
            // Tooltip.SetDefault("10% increased movement speed");

            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
        }

        public override void SetDefaults()
        {
            Item.width = 22;
            Item.height = 18;
            Item.value = Item.buyPrice(silver: 70);
            Item.rare = ItemRarityID.Orange;
            Item.defense = 5;
        }

        public override void UpdateEquip(Player player)
        {
            player.moveSpeed += 0.10f;
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
