using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent.Creative;

namespace VenninBeeMod.Content.Items.Armor
{
    [AutoloadEquip(EquipType.Body)]
    public class QueensGuardCuirass : ModItem
    {
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Queen's Guard Cuirass");
            // Tooltip.SetDefault("5% increased melee damage");

            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
        }

        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 20;
            Item.value = Item.buyPrice(silver: 90);
            Item.rare = ItemRarityID.Orange;
            Item.defense = 7;
        }

        public override void UpdateEquip(Player player)
        {
            player.GetDamage(DamageClass.Melee) += 0.05f;
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