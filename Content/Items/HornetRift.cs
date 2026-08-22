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
        /// Mana per use cycle. The staff runs three cycles a second while held, so this is a
        /// third of what holding the channel actually costs per second. Vanilla charges it as
        /// part of the use, which is also what the drone reads as proof it is still paid for.
        /// </summary>
        private const int BaseManaCost = 11;

        public override void SetStaticDefaults()
        {
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
            ItemID.Sets.ItemsThatAllowRepeatedRightClick[Type] = false;
        }

        public override void SetDefaults()
        {
            Item.damage = 8;
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
            // Getting here means this use cycle was affordable and the mana has already been
            // spent, so this call is the receipt the drone waits on. Without it the drone only
            // watched the channel flag, which stays true on an empty mana bar, and the staff
            // kept firing for nothing.
            bool droneOut = false;

            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile other = Main.projectile[i];
                if (other.active && other.type == type && other.owner == player.whoAmI)
                {
                    other.ai[2] = 0f;
                    other.netUpdate = true;
                    droneOut = true;
                }
            }

            // One drone at a time; every cycle after the first just pays for the one that is out.
            if (!droneOut)
            {
                Projectile.NewProjectile(source, player.MountedCenter, Vector2.Zero, type, damage, knockback, player.whoAmI);
            }

            return false;
        }
    }
}
