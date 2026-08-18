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
            Item.useAnimation = 5;
            Item.useTime = 5;
            Item.width = 32;
            Item.height = 32;
            Item.UseSound = SoundID.Item1;
            Item.DamageType = DamageClass.MeleeNoSpeed;
            Item.autoReuse = true;

            // The blade is a projectile, so the item itself is never drawn or swung.
            Item.noUseGraphic = true;
            Item.noMelee = true;

            Item.rare = ItemRarityID.Blue;
            Item.value = Item.sellPrice(gold: 1);

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
            // Shadow Scales only drop from the Eater of Worlds, so they are the gate on their
            // own. Deliberately free of Bee Wax and Honeycomb so it never waits on Queen Bee.
            CreateRecipe()
                .AddIngredient(ItemID.ShadowScale, 20)
                .AddIngredient(ItemID.DemoniteBar, 10)
                .AddIngredient<StickyResin>(25)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
}
