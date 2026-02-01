using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using System;
using VenninBeeMod.Content.Projectiles;

namespace VenninBeeMod.Content.Items
{
    public class HoneyFangBlade : ModItem
    {
        public override void SetStaticDefaults()
        {
            // Tooltip.SetDefault("Fires a honey slash that slows enemies\nBee spawn chance increased with Hive Pack");
        }

        public override void SetDefaults()
        {
            Item.damage = 46;
            Item.DamageType = DamageClass.Melee;
            Item.width = 44;
            Item.height = 44;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 5f;
            Item.value = Item.buyPrice(gold: 4);
            Item.rare = ItemRarityID.LightRed;
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<HoneySlashProjectile>();
            Item.shootSpeed = 9f;
        }

        public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone)
        {
            float beeChance = HasHivePack(player) ? 0.5f : 0.2f;

            if (Main.myPlayer == player.whoAmI && Main.rand.NextFloat() < beeChance)
            {
                Vector2 velocity = Vector2.Normalize(Main.rand.NextVector2Circular(1f, 1f)) * 5f;
                Projectile.NewProjectile(
                    player.GetSource_OnHit(target),
                    target.Center,
                    velocity,
                    ProjectileID.Bee,
                    12,
                    1f,
                    player.whoAmI
                );
            }
        }

        private bool HasHivePack(Player player)
        {
            for (int i = 3; i < 10; i++)
                if (player.armor[i].type == ItemID.HiveBackpack)
                    return true;
            return false;
        }

        public override void UseStyle(Player player, Rectangle heldItemFrame)
        {
            Vector2 offset = Vector2.Zero;

            if (player.direction == 1)
            {
                // Facing right: shift left and down
                offset = new Vector2(-2f, 2f);
            }
            else
            {
                // Facing left: shift right and down
                offset = new Vector2(2f, 2f);
            }

            player.itemLocation = player.MountedCenter + offset;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.BeeKeeper)
                .AddIngredient(ItemID.CrystalShard, 10)
                .AddIngredient(ItemID.Stinger, 8)
                .AddIngredient(ItemID.HellstoneBar, 15)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
}