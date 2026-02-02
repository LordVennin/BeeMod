using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using VenninBeeMod.Content.Projectiles;

namespace VenninBeeMod.Content.Items
{
    public class StingerBlowpipe : ModItem
    {
        private const float BaseBurstChance = 0.2f;
        private const float HivePackBurstChance = 0.35f;
        private const float BaseConsumeChance = 0.75f;
        private const float HivePackConsumeChance = 0.5f;

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.damage = 15;
            Item.DamageType = DamageClass.Ranged;
            Item.width = 34;
            Item.height = 14;
            Item.useTime = 24;
            Item.useAnimation = 24;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.knockBack = 2f;
            Item.value = Item.buyPrice(silver: 45);
            Item.rare = ItemRarityID.Green;
            Item.UseSound = SoundID.Item63;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<HornetStingerProjectile>();
            Item.shootSpeed = 11f;
            Item.useAmmo = ItemID.Stinger;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.Stinger, 8)
                .AddIngredient(ItemID.JungleSpores, 5)
                .AddIngredient(ItemID.Vine, 2)
                .AddTile(TileID.Anvils)
                .Register();
        }

        public override bool Shoot(Player player, Terraria.DataStructures.EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            float burstChance = HasHivePack(player) ? HivePackBurstChance : BaseBurstChance;
            if (Main.rand.NextFloat() < burstChance)
            {
                int extraShots = Main.rand.Next(2, 4);
                for (int i = 0; i < extraShots; i++)
                {
                    Vector2 perturbedSpeed = velocity.RotatedByRandom(MathHelper.ToRadians(6f)) * Main.rand.NextFloat(0.9f, 1.1f);
                    Projectile.NewProjectile(source, position, perturbedSpeed, type, damage, knockback, player.whoAmI);
                }
            }

            return true;
        }

        public override bool CanConsumeAmmo(Item ammo, Player player)
        {
            float consumeChance = HasHivePack(player) ? HivePackConsumeChance : BaseConsumeChance;
            return Main.rand.NextFloat() < consumeChance;
        }

        private static bool HasHivePack(Player player)
        {
            for (int i = 3; i < 10; i++)
            {
                if (player.armor[i].type == ItemID.HiveBackpack)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
