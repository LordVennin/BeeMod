using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;
using VenninBeeMod.Content.Projectiles;

namespace VenninBeeMod.Content.Items
{
    public class PhantomHivebow : ModItem
    {
        private const int GhostBeeCount = 5;
        private const int ArrowsPerVolley = 5;

        private const float BeeSpacing = 44f;
        private const float HoverHeight = 132f;

        private const int FallbackArrowCount = 3;
        private const float FallbackDrop = 620f;
        private const float FallbackLateral = 420f;

        public override void SetStaticDefaults()
        {
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
        }

        public override void SetDefaults()
        {
            Item.damage = 17;
            Item.DamageType = DamageClass.Ranged;
            Item.width = 34;
            Item.height = 46;
            Item.useTime = 40;
            Item.useAnimation = 40;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.knockBack = 2f;
            Item.value = Item.sellPrice(gold: 1);
            Item.rare = ItemRarityID.Blue;
            Item.UseSound = SoundID.Item5;
            Item.autoReuse = true;
            Item.useAmmo = AmmoID.Arrow;
            Item.shoot = ProjectileID.WoodenArrowFriendly;
            Item.shootSpeed = 9f;
        }

        public override Vector2? HoldoutOffset()
        {
            return new Vector2(-4f, 0f);
        }

        public override void AddRecipes()
        {
            // Gated by Shadow Scales, which only the Eater of Worlds drops.
            CreateRecipe()
                .AddIngredient(ItemID.ShadowScale, 20)
                .AddIngredient(ItemID.DemoniteBar, 10)
                .AddIngredient(ItemID.Stinger, 12)
                .AddIngredient<StickyResin>(20)
                .AddTile(TileID.Anvils)
                .Register();
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (source.AmmoItemIdUsed == ItemID.WoodenArrow)
            {
                SummonGhostLine(player, source, damage, knockback);
            }
            else
            {
                RainArrows(player, source, type, damage, knockback);
            }

            // Everything is spawned by hand above, so skip the default single arrow.
            return false;
        }

        /// <summary>
        /// Hangs the firing line above the cursor. Each bee spits its own pair of stingers, so
        /// five bees put ten in the air for the five arrows the volley eats.
        /// </summary>
        private void SummonGhostLine(Player player, EntitySource_ItemUse_WithAmmo source, int damage, float knockback)
        {
            Vector2 anchor = Main.MouseWorld;
            int beeType = ModContent.ProjectileType<GhostBee>();

            for (int slot = 0; slot < GhostBeeCount; slot++)
            {
                float offset = (slot - ((GhostBeeCount - 1) / 2f)) * BeeSpacing;
                Vector2 station = anchor + new Vector2(offset, -HoverHeight);

                Projectile.NewProjectile(source, station, Vector2.Zero, beeType,
                    damage, knockback, player.whoAmI, ai0: slot);
            }

            // Vanilla already took one arrow for this use, so claim the rest of the volley.
            for (int i = 0; i < ArrowsPerVolley - 1; i++)
            {
                player.ConsumeItem(ItemID.WoodenArrow);
            }
        }

        /// <summary>
        /// Anything other than plain wooden arrows falls back to a plunging volley from off the
        /// top of the screen. The arrows start opposite the way the player faces, so turning
        /// around flips which shoulder they come over.
        /// </summary>
        private void RainArrows(Player player, EntitySource_ItemUse_WithAmmo source, int type, int damage, float knockback)
        {
            for (int i = 0; i < FallbackArrowCount; i++)
            {
                Vector2 target = Main.MouseWorld + Main.rand.NextVector2Circular(28f, 28f);

                float drop = FallbackDrop + Main.rand.NextFloat(0f, 180f);
                float lateral = (-FallbackLateral * player.direction) + Main.rand.NextFloat(-40f, 40f);

                Vector2 start = target + new Vector2(lateral, -drop);
                start.Y = Math.Max(start.Y, 120f);

                Vector2 arrowVelocity = (target - start).SafeNormalize(Vector2.UnitY) * Item.shootSpeed * 1.7f;

                Projectile.NewProjectile(source, start, arrowVelocity, type, damage, knockback, player.whoAmI);
            }
        }
    }
}
