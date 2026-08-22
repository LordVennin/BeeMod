using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;
using VenninBeeMod.Content.Projectiles;

namespace VenninBeeMod.Content.Items
{
    public class HornetRift : ModItem
    {
        /// <summary>
        /// Mana per use cycle. Three cycles a second works out to roughly the 18 a second the
        /// channel was always meant to cost, and vanilla charges it without the drone having to.
        /// </summary>
        private const int BaseManaCost = 6;

        public override void SetStaticDefaults()
        {
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
            ItemID.Sets.ItemsThatAllowRepeatedRightClick[Type] = false;
        }

        public override void SetDefaults()
        {
            Item.damage = 9;
            Item.DamageType = DamageClass.Magic;
            Item.mana = BaseManaCost;
            Item.width = 44;
            Item.height = 44;
            // A normal weapon's cycle. A full second per use was an outlier - every staff in
            // the mod that sits in the hand properly is between 10 and 34 - and a long hold is
            // the other half of why this one drifted.
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.knockBack = 1.5f;
            Item.value = Item.sellPrice(gold: 1);
            Item.rare = ItemRarityID.Blue;
            Item.UseSound = SoundID.Item43;
            Item.autoReuse = true;

            // Held down rather than tapped; the drone lives only while this is true.
            Item.channel = true;

            Item.shoot = ModContent.ProjectileType<RiftBee>();
            Item.shootSpeed = 0f;
        }

        /// <summary>
        /// Without this the staff hangs off the hand instead of being gripped. Every Shoot style
        /// item in the mod that looks right defines one; this was the only one that did not.
        /// </summary>
        public override Vector2? HoldoutOffset()
        {
            return new Vector2(-6f, 0f);
        }

        /// <summary>
        /// Hive Pack secret: a point off the upkeep. It is charged every second the channel is
        /// held, so a single point adds up over a long cast.
        /// </summary>
        public override void UpdateInventory(Player player)
        {
            Item.mana = HivePack.IsEquipped(player) ? BaseManaCost - 1 : BaseManaCost;
        }

        public override void AddRecipes()
        {
            // Shadow Scales gate this behind the Eater of Worlds, same as the rest of the set.
            CreateRecipe()
                .AddIngredient(ItemID.ShadowScale, 20)
                .AddIngredient(ItemID.DemoniteBar, 10)
                .AddIngredient(ItemID.Stinger, 10)
                .AddIngredient<StickyResin>(20)
                .AddTile(TileID.Anvils)
                .Register();
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // Mana keeps draining every use cycle, but only ever one drone at a time.
            if (player.ownedProjectileCounts[type] < 1)
            {
                Projectile.NewProjectile(source, player.MountedCenter, Vector2.Zero, type, damage, knockback, player.whoAmI);
            }

            return false;
        }
    }
}
