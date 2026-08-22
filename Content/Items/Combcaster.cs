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
    /// Magic was the thinnest class at this tier with only the Soul Drip Scepter, which is a
    /// fast direct-fire wand. This is the opposite: you place combs and they do the shooting.
    /// </summary>
    public class Combcaster : ModItem
    {
        public override void SetStaticDefaults()
        {
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
        }

        public override void SetDefaults()
        {
            Item.damage = 32;
            Item.DamageType = DamageClass.Magic;
            Item.mana = 12;
            Item.width = 40;
            Item.height = 40;
            Item.useTime = 34;
            Item.useAnimation = 34;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.knockBack = 2f;
            Item.value = Item.sellPrice(gold: 2);
            Item.rare = ItemRarityID.LightRed;
            Item.UseSound = SoundID.Item43;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<CombTurret>();

            // Lobbed rather than fired, so placing one is an act of aim.
            Item.shootSpeed = 9f;
        }

        public override Vector2? HoldoutOffset()
        {
            return new Vector2(-4f, 0f);
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            CombTurret.RetireOldest(player);
            Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
            return false;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<Prismwax>(14)
                .AddIngredient<FracturedStinger>(3)
                .AddIngredient(ItemID.SoulofLight, 8)
                .AddIngredient(ItemID.BeeWax, 10)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
}
