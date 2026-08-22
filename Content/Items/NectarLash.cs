using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;
using VenninBeeMod.Content.Projectiles;

namespace VenninBeeMod.Content.Items
{
    /// <summary>
    /// The mod's first whip. Early hardmode had no summoner tool between the Hiveheart Idol and
    /// nothing at all, and a bee mod without a whip was a strange gap.
    /// </summary>
    public class NectarLash : ModItem
    {
        public override void SetStaticDefaults()
        {
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
        }

        public override void SetDefaults()
        {
            // Set out longhand rather than through Item.DefaultToWhip, so the numbers are
            // visible and nothing depends on remembering that helper's argument order.
            Item.DamageType = DamageClass.SummonMeleeSpeed;
            Item.damage = 24;
            Item.knockBack = 3f;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.width = 36;
            Item.height = 36;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.channel = true;
            Item.value = Item.sellPrice(gold: 2);
            Item.rare = ItemRarityID.LightRed;
            Item.UseSound = SoundID.Item152;
            Item.shoot = ModContent.ProjectileType<NectarLashProjectile>();

            // On a whip this is reach rather than speed.
            Item.shootSpeed = 4.2f;
        }

        public override bool MeleePrefix()
        {
            return true;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<Prismwax>(12)
                .AddIngredient(ItemID.Stinger, 15)
                .AddIngredient(ItemID.SoulofLight, 5)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
}
