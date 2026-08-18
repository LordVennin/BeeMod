using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;
using VenninBeeMod.Content.Buffs;
using VenninBeeMod.Content.Projectiles;

namespace VenninBeeMod.Content.Items
{
    public class ShadowHiveStaff : ModItem
    {
        public override void SetStaticDefaults()
        {
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
        }

        public override void SetDefaults()
        {
            Item.damage = 14;
            Item.DamageType = DamageClass.Summon;
            Item.mana = 12;
            Item.width = 40;
            Item.height = 40;
            Item.useTime = 26;
            Item.useAnimation = 26;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.noMelee = true;
            Item.knockBack = 2f;
            Item.value = Item.sellPrice(gold: 1);
            Item.rare = ItemRarityID.Blue;
            Item.UseSound = SoundID.Item44;
            Item.autoReuse = false;
            Item.sentry = true;
            Item.shoot = ModContent.ProjectileType<ShadowHiveSentry>();
            Item.shootSpeed = 0f;
        }

        public override void AddRecipes()
        {
            // Shadow Scales gate this behind the Eater of Worlds, same as the rest of the set.
            CreateRecipe()
                .AddIngredient(ItemID.ShadowScale, 25)
                .AddIngredient(ItemID.DemoniteBar, 12)
                .AddIngredient<StickyResin>(30)
                .AddTile(TileID.Anvils)
                .Register();
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // Granted before the hive exists so the sentry's own buff check passes on its first
            // tick. Cancelling this buff despawns every hive the player has out.
            player.AddBuff(ModContent.BuffType<ShadowHiveBuff>(), 18000);

            // Hangs in the air exactly where the cursor is, rather than dropping to the ground.
            Projectile.NewProjectile(source, Main.MouseWorld, Vector2.Zero, type, damage, knockback, player.whoAmI);

            // Required right after spawning a sentry so the oldest one retires past the cap.
            player.UpdateMaxTurrets();

            return false;
        }
    }
}
