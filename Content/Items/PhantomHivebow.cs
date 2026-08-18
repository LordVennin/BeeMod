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
        private const int FallbackArrowCost = 3;
        private const float FallbackDrop = 620f;
        private const float FallbackLateral = 420f;

        public override void SetStaticDefaults()
        {
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
        }

        public override void SetDefaults()
        {
            Item.damage = 18;
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
            // Judged on the projectile the ammo produces rather than the ammo's own id, so an
            // Endless Quiver counts as the wooden arrows it fires instead of being treated as
            // something exotic and dropped into the fallback volley.
            bool plainArrow = type == ProjectileID.WoodenArrowFriendly;

            if (plainArrow)
            {
                SummonGhostLine(player, source, damage, knockback);
                ConsumeRestOfVolley(player, source, type, ArrowsPerVolley - 1);
            }
            else
            {
                RainArrows(player, source, type, damage, knockback);
                ConsumeRestOfVolley(player, source, type, FallbackArrowCost - 1);
            }

            // Everything is spawned by hand above, so skip the default single arrow.
            return false;
        }

        /// <summary>
        /// Takes the rest of the volley's ammo on top of the one the game already took for this
        /// use. Mirrors the gate vanilla applies in PickAmmo: nothing is taken from ammo that is
        /// not consumable, and each arrow still gets its ammo conservation roll. Going around
        /// that is what was destroying Endless Quivers.
        /// </summary>
        private void ConsumeRestOfVolley(Player player, EntitySource_ItemUse_WithAmmo source, int projectileType, int extra)
        {
            if (extra <= 0)
            {
                return;
            }

            Item ammo = FindAmmoStack(player, source.AmmoItemIdUsed);
            if (ammo == null || !ammo.consumable)
            {
                return;
            }

            for (int i = 0; i < extra; i++)
            {
                if (ammo.stack <= 0)
                {
                    break;
                }

                if (player.IsAmmoFreeThisShot(Item, ammo, projectileType))
                {
                    continue;
                }

                ammo.stack--;
                if (ammo.stack <= 0)
                {
                    ammo.active = false;
                    ammo.TurnToAir();
                }
            }
        }

        /// <summary>
        /// Ammo slots first, then the rest of the inventory, matching the order the game picks
        /// ammo in so the volley eats from the same stack the first arrow came out of.
        /// </summary>
        private static Item FindAmmoStack(Player player, int ammoItemId)
        {
            if (ammoItemId <= ItemID.None)
            {
                return null;
            }

            for (int i = 54; i < 58; i++)
            {
                Item candidate = player.inventory[i];
                if (candidate.type == ammoItemId && candidate.stack > 0)
                {
                    return candidate;
                }
            }

            for (int i = 0; i < 54; i++)
            {
                Item candidate = player.inventory[i];
                if (candidate.type == ammoItemId && candidate.stack > 0)
                {
                    return candidate;
                }
            }

            return null;
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
        }

        /// <summary>
        /// Anything other than plain wooden arrows falls back to a plunging volley from off the
        /// top of the screen. The arrows start opposite the way the player faces, so turning
        /// around flips which shoulder they come over. Costs three of whatever is loaded.
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
