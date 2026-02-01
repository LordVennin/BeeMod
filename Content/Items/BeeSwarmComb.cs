using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using VenninBeeMod.Content.Projectiles; // You'll create a BeeMinion.cs later
using Terraria.GameContent.Creative;
using Terraria.DataStructures;
using VenninBeeMod.Content.Buffs;

namespace VenninBeeMod.Content.Items
{
    public class BeeSwarmComb : ModItem
    {
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Bee Swarm Staff");
            // Tooltip.SetDefault("Summons bees to protect you\nHive Pack increases their number and damage");
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
            ItemID.Sets.GamepadWholeScreenUseRange[Item.type] = true;
            ItemID.Sets.LockOnIgnoresCollision[Item.type] = true;
        }

        public override void SetDefaults()
        {
            Item.damage = 7;
            Item.DamageType = DamageClass.Summon;
            Item.mana = 10;
            Item.width = 32;
            Item.height = 32;
            Item.useTime = 36;
            Item.useAnimation = 36;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.noMelee = true;
            Item.knockBack = .2f;
            Item.value = Item.buyPrice(silver: 60);
            Item.rare = ItemRarityID.Orange;
            Item.UseSound = SoundID.Item44;
            Item.shoot = ModContent.ProjectileType<BeeFollowerMinion>();
            Item.buffType = ModContent.BuffType<BeeSwarmBuff>(); ; // You’ll replace this with a custom buff if needed
            Item.shootSpeed = 0f;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            int beeCount = HasHivePack(player) ? 3 : 2;
            float hiveMult = HasHivePack(player) ? 1.1f : 1f;

            player.AddBuff(Item.buffType, 2);

            
            
                Projectile.NewProjectile(source, player.Center, Vector2.Zero, type, (int)(damage * hiveMult), knockback, player.whoAmI);
            

            return false;
        }

        public override void UseStyle(Player player, Rectangle heldItemFrame)
        {
            // No forward push needed for centered sprite
            float horizontalOffset = -10f;
            float verticalOffset = 5f; // Raise to match hand level

            Vector2 offset = new Vector2(player.direction * horizontalOffset, verticalOffset);
            player.itemLocation = player.MountedCenter + offset;
        }

        private bool HasHivePack(Player player)
        {
            for (int i = 3; i < 10; i++)
                if (player.armor[i].type == ItemID.HiveBackpack)
                    return true;

            return false;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.BeeWax, 10)
                .AddIngredient(ItemID.Stinger, 5)
                .AddIngredient(ItemID.HoneyBlock, 10)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
}