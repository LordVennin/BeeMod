using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;
using VenninBeeMod.Content.Projectiles;

namespace VenninBeeMod.Content.Items
{
    public class BuzzKill : ModItem
    {
        private const int PelletCount = 15;

        // Half-angle of the cone, so pellets land anywhere in a ~68 degree spray.
        private const float SpreadDegrees = 34f;

        public override void SetStaticDefaults()
        {
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
        }

        public override void SetDefaults()
        {
            Item.damage = 13;
            Item.DamageType = DamageClass.Ranged;
            Item.width = 62;
            Item.height = 22;
            Item.useTime = 45;
            Item.useAnimation = 45;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.knockBack = 4.5f;
            Item.value = Item.buyPrice(gold: 3);
            Item.rare = ItemRarityID.LightRed;
            Item.UseSound = SoundID.Item36;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<BuzzKillPellet>();
            Item.shootSpeed = 9.5f;
            Item.useAmmo = AmmoID.Bullet;
        }

        public override Vector2? HoldoutOffset()
        {
            return new Vector2(-6f, 0f);
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.Boomstick, 1)
                .AddIngredient(ItemID.BeeWax, 12)
                .AddIngredient(ItemID.SoulofLight, 8)
                .AddIngredient(ItemID.Stinger, 15)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // Every bullet becomes honey shot, whatever ammo is loaded.
            int pelletType = ModContent.ProjectileType<BuzzKillPellet>();

            for (int i = 0; i < PelletCount; i++)
            {
                Vector2 pelletVelocity = velocity.RotatedByRandom(MathHelper.ToRadians(SpreadDegrees))
                    * Main.rand.NextFloat(0.72f, 1.18f);

                Projectile.NewProjectile(source, position, pelletVelocity, pelletType, damage, knockback, player.whoAmI);
            }

            // The whole spray is spawned above, so skip the default single shot.
            return false;
        }
    }
}
