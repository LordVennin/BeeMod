using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent.Creative;

namespace VenninBeeMod.Content.Items.Armor
{
    [AutoloadEquip(EquipType.Body)]
    public class WaxweaverMantle : ModItem
    {
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Waxweaver Mantle");
             // Tooltip.SetDefault("6% increased magic damage.\nIncreases maximum mana by 20.");

            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
        }

        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 20;
            Item.value = Item.buyPrice(silver: 90);
            Item.rare = ItemRarityID.Orange;
            Item.defense = 5;
        }

        public override void UpdateEquip(Player player)
        {
            player.GetDamage(DamageClass.Magic) += 0.06f;
            player.statManaMax2 += 20;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.BeeWax, 10)
                .AddIngredient(ItemID.Stinger, 5)
                .AddIngredient(ItemID.HoneyBlock, 5)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
}