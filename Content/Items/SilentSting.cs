using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;
using VenninBeeMod.Content.Projectiles;

namespace VenninBeeMod.Content.Items
{
    public class SilentSting : ModItem
    {
        public const int NinjaBeeDamage = 2;

        public override void SetStaticDefaults()
        {
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
        }

        public override void SetDefaults()
        {
            Item.damage = 9;
            Item.knockBack = 1.5f;
            Item.useStyle = ItemUseStyleID.Rapier;
            Item.useAnimation = 8;
            Item.useTime = 8;
            Item.width = 32;
            Item.height = 32;
            Item.UseSound = SoundID.Item1;
            Item.DamageType = DamageClass.MeleeNoSpeed;
            Item.autoReuse = true;

            // The blade is a projectile, so the item itself is never drawn or swung.
            Item.noUseGraphic = true;
            Item.noMelee = true;

            Item.rare = ItemRarityID.Pink;
            Item.value = Item.sellPrice(gold: 3);

            Item.shoot = ModContent.ProjectileType<SilentStingProjectile>();
            Item.shootSpeed = 2.4f;
        }

        // noUseGraphic weapons are not treated as melee for prefixes without this.
        public override bool MeleePrefix()
        {
            return true;
        }

        public override void AddRecipes()
        {
            // Deliberately free of Bee Wax and Honeycomb so it does not gate behind Queen Bee.
            CreateRecipe()
                .AddIngredient(ItemID.HallowedBar, 8)
                .AddIngredient(ItemID.SoulofNight, 10)
                .AddIngredient(ItemID.Stinger, 15)
                .AddIngredient<StickyResin>(20)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
}
