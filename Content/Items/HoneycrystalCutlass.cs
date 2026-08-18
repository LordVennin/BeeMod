using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;
using VenninBeeMod.Content.Projectiles;

namespace VenninBeeMod.Content.Items
{
    public class HoneycrystalCutlass : ModItem
    {
        public override void SetStaticDefaults()
        {
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
        }

        public override void SetDefaults()
        {
            Item.damage = 48;
            Item.DamageType = DamageClass.Melee;
            // Must match the texture. Terraria places a held item from the unscaled
            // texture frame size, so an oversized sprite drifts off the hand no matter
            // what Item.scale says; the sprite itself is resized instead of scaled.
            Item.width = 77;
            Item.height = 77;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 5.5f;
            Item.value = Item.buyPrice(gold: 2);
            Item.rare = ItemRarityID.LightRed;
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<HoneycrystalShard>();
            Item.shootSpeed = 9f;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.CrystalShard, 12)
                .AddIngredient(ItemID.SoulofLight, 8)
                .AddIngredient(ItemID.Stinger, 10)
                .AddIngredient(ItemID.BeeWax, 8)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
}
