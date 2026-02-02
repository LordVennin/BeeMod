using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent.Creative;

namespace VenninBeeMod.Content.Items.Armor
{
    [AutoloadEquip(EquipType.Head)]
    public class WaxweaverCowl : ModItem
    {
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Waxweaver Cowl");
            // Tooltip.SetDefault("6% increased magic critical strike chance");

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
            player.GetCritChance(DamageClass.Magic) += 6f;
        }

        public override bool IsArmorSet(Item head, Item body, Item legs)
        {
            return body.type == ModContent.ItemType<WaxweaverMantle>() &&
                   legs.type == ModContent.ItemType<WaxweaverSlippers>();
        }

        public override void UpdateArmorSet(Player player)
        {
            player.setBonus = "Leaves behind a trail of bees that damage enemies\nIncreases magic damage by 20%";
            player.GetDamage(DamageClass.Magic) += 0.20f;
            player.GetModPlayer<WaxweaverPlayer>().hasWaxweaverSet = true;
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
