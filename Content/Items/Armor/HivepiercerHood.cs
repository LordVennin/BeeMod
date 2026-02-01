using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent.Creative;

namespace VenninBeeMod.Content.Items.Armor
{
    [AutoloadEquip(EquipType.Head)]
    public class HivepiercerHood : ModItem
    {
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Hivepiercer Hood");
            // Tooltip.SetDefault("6% increased ranged critical strike chance");

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
            player.GetCritChance(DamageClass.Ranged) += 6f;
        }

        public override bool IsArmorSet(Item head, Item body, Item legs)
        {
            return body.type == ModContent.ItemType<HivepiercerShell>() &&
                   legs.type == ModContent.ItemType<HivepiercerTarsi>();
        }

        public override void UpdateArmorSet(Player player)
        {
            player.setBonus = "Summons a hornet that mimics your ranged attacks";
            player.AddBuff(ModContent.BuffType<Buffs.HiveMindBuff>(), 60);
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