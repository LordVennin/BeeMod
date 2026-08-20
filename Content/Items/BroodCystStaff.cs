using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;
using VenninBeeMod.Content.Projectiles;

namespace VenninBeeMod.Content.Items
{
    /// <summary>
    /// The Hornet Rift is a held stream; this is the opposite. One heavy cast, a slow fuse, and
    /// a rupture. Land more than one on the same target and the rupture gets a great deal worse.
    /// </summary>
    public class BroodCystStaff : ModItem
    {
        public override void SetStaticDefaults()
        {
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
        }

        public override void SetDefaults()
        {
            Item.damage = 22;
            Item.DamageType = DamageClass.Magic;
            Item.mana = 14;
            Item.width = 42;
            Item.height = 42;
            Item.useTime = 40;
            Item.useAnimation = 40;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.knockBack = 1f;
            Item.value = Item.sellPrice(gold: 1);
            Item.rare = ItemRarityID.Blue;
            Item.UseSound = SoundID.Item44;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<BroodCyst>();

            // Slow on purpose. The cyst is a lobbed thing you have to aim ahead of.
            Item.shootSpeed = 7f;
        }

        public override Vector2? HoldoutOffset()
        {
            return new Vector2(-4f, 0f);
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.TissueSample, 25)
                .AddIngredient(ItemID.CrimtaneBar, 12)
                .AddIngredient(ItemID.Stinger, 10)
                .AddIngredient<StickyResin>(20)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
}
