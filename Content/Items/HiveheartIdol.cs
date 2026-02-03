using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;
using VenninBeeMod.Content.Buffs;
using VenninBeeMod.Content.Projectiles;
using Microsoft.Xna.Framework;

namespace VenninBeeMod.Content.Items
{
    public class HiveheartIdol : ModItem
    {
        public override void SetStaticDefaults()
        {
            ItemID.Sets.GamepadWholeScreenUseRange[Item.type] = true;
            ItemID.Sets.LockOnIgnoresCollision[Item.type] = true;
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
        }

        public override void SetDefaults()
        {
            Item.damage = 20;
            Item.DamageType = DamageClass.Summon;
            Item.mana = 10;
            Item.width = 64;
            Item.height = 64;
            Item.useTime = 36;
            Item.useAnimation = 28;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.noMelee = true;
            Item.knockBack = 2f;
            Item.value = Item.buyPrice(gold: 2);
            Item.rare = ItemRarityID.LightRed;
            Item.UseSound = SoundID.Item44;
            Item.shoot = ModContent.ProjectileType<HiveheartMinion>();
            Item.buffType = ModContent.BuffType<HiveheartBuff>();
            Item.shootSpeed = 4f;
        }

        public override Vector2? HoldoutOffset()
        {
            return new Vector2(-90, 0); // X moves it left/right, Y moves it up/down
        }

        public override bool Shoot(Player player, Terraria.DataStructures.EntitySource_ItemUse_WithAmmo source, Microsoft.Xna.Framework.Vector2 position, Microsoft.Xna.Framework.Vector2 velocity, int type, int damage, float knockback)
        {
            player.AddBuff(Item.buffType, 2);

            if (player.ownedProjectileCounts[type] >= player.maxMinions)
            {
                return false;
            }

            Projectile.NewProjectile(source, player.Center, Microsoft.Xna.Framework.Vector2.Zero, type, damage, knockback, player.whoAmI);
            return false;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.CrystalShard, 10)
                .AddIngredient(ItemID.SoulofLight, 8)
                .AddIngredient(ItemID.SoulofNight, 8)
                .AddIngredient(ItemID.Ruby, 3)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
}
