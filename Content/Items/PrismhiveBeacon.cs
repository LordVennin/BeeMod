using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;
using VenninBeeMod.Content.Buffs;
using VenninBeeMod.Content.Projectiles;

namespace VenninBeeMod.Content.Items
{
    /// <summary>
    /// The mod's first hardmode sentry. Both existing ones stop in pre-hardmode, so summoners
    /// had a minion at this tier and nothing to plant.
    /// </summary>
    public class PrismhiveBeacon : ModItem
    {
        public override void SetStaticDefaults()
        {
            ItemID.Sets.GamepadWholeScreenUseRange[Item.type] = true;
            ItemID.Sets.LockOnIgnoresCollision[Item.type] = true;
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
        }

        public override void SetDefaults()
        {
            Item.damage = 26;
            Item.DamageType = DamageClass.Summon;
            Item.mana = 12;
            Item.width = 40;
            Item.height = 40;
            Item.useTime = 26;
            Item.useAnimation = 26;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.noMelee = true;
            Item.knockBack = 2f;
            Item.value = Item.sellPrice(gold: 2);
            Item.rare = ItemRarityID.LightRed;
            Item.UseSound = SoundID.Item44;
            Item.autoReuse = false;
            Item.sentry = true;
            Item.shoot = ModContent.ProjectileType<PrismhiveSentry>();
            Item.shootSpeed = 0f;
        }

        public override Vector2? HoldoutOffset()
        {
            return new Vector2(-6f, 0f);
        }

        public override bool Shoot(Player player, Terraria.DataStructures.EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // Granted before the hive exists so the sentry's own check passes on its first tick.
            // Cancelling it is how the player packs every planted hive away.
            player.AddBuff(ModContent.BuffType<PrismhiveBuff>(), 18000);

            // Same placement the mod's other sentries use: it hangs where the cursor is rather
            // than dropping to the floor, and UpdateMaxTurrets right after is what retires the
            // oldest one once you are over the cap.
            Projectile.NewProjectile(source, Main.MouseWorld, Vector2.Zero, type, damage, knockback, player.whoAmI);
            player.UpdateMaxTurrets();
            return false;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<Prismwax>(16)
                .AddIngredient<FracturedStinger>(4)
                .AddIngredient(ItemID.SoulofLight, 8)
                .AddIngredient(ItemID.CrystalShard, 10)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
}
