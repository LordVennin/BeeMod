using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent.Creative;

namespace VenninBeeMod.Content.Items.Armor
{
    [AutoloadEquip(EquipType.Head)]
    public class QueensGuardMask : ModItem
    {
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Queen's Guard Mask");
            // Tooltip.SetDefault("4% increased melee critical strike chance");

            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
        }

        public override void SetDefaults()
        {
            Item.width = 22;
            Item.height = 20;
            Item.value = Item.buyPrice(silver: 60);
            Item.rare = ItemRarityID.Orange;
            Item.defense = 4;
        }

        public override void UpdateEquip(Player player)
        {
            player.GetCritChance(DamageClass.Melee) += 4f;
        }

        public override bool IsArmorSet(Item head, Item body, Item legs)
        {
            return body.type == ModContent.ItemType<QueensGuardCuirass>() &&
                   legs.type == ModContent.ItemType<QueensGuardGreaves>();
        }

        public override void UpdateArmorSet(Player player)
        {
            player.setBonus = "At or below 25% health, gain Queen's Honey.\nMelee attacks have a chance to spawn bees.";
            player.GetModPlayer<QueensGuardPlayer>().hasQueensGuardSet = true;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.BeeWax, 6)
                .AddIngredient(ItemID.Stinger, 3)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }

}