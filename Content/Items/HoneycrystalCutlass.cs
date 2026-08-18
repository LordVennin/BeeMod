using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;
using VenninBeeMod.Content.Projectiles;

namespace VenninBeeMod.Content.Items
{
    public class HoneycrystalCutlass : ModItem
    {
        /// <summary>
        /// How far up the blade to walk the swing pivot, in texture pixels. Terraria swings a
        /// sword around the bottom corner of its sprite, which on this blade is the tip of the
        /// pommel, so the character appears to pinch it by the very end. This lifts the pivot
        /// onto the grip instead.
        /// </summary>
        private const float GripOffset = 8f;

        public override void SetStaticDefaults()
        {
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
        }

        public override void SetDefaults()
        {
            Item.damage = 48;
            Item.DamageType = DamageClass.Melee;
            // Must match the texture. Terraria places a held item from the unscaled
            // texture frame size, so an oversized sprite drifts off the hand no matter
            // what Item.scale says; the sprite itself is resized instead of scaled.
            Item.width = 83;
            Item.height = 83;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 5.5f;
            Item.value = Item.buyPrice(gold: 2);
            Item.rare = ItemRarityID.LightRed;
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<HoneycrystalShard>();
            Item.shootSpeed = 9f;
        }

        public override bool ModifyItemDraw(ref PlayerDrawSet drawInfo, ref DrawData drawData, ref DrawData? coloredDrawData, ref DrawData? glowMaskDrawData)
        {
            Player player = drawInfo.drawPlayer;

            // The draw origin is in texture space, so this offset rides along with the swing
            // rotation for free. It has to be mirrored on X when facing left and on Y under
            // reversed gravity, because the sprite is flipped on exactly those axes.
            Vector2 gripShift = new Vector2(GripOffset * player.direction, -GripOffset * player.gravDir);

            drawData.origin += gripShift;
            coloredDrawData = ShiftOrigin(coloredDrawData, gripShift);
            glowMaskDrawData = ShiftOrigin(glowMaskDrawData, gripShift);

            return true;
        }

        private static DrawData? ShiftOrigin(DrawData? data, Vector2 shift)
        {
            if (!data.HasValue)
            {
                return null;
            }

            DrawData shifted = data.Value;
            shifted.origin += shift;
            return shifted;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.CrystalShard, 12)
                .AddIngredient(ItemID.SoulofLight, 8)
                .AddIngredient(ItemID.Stinger, 10)
                .AddIngredient(ItemID.BeeWax, 8)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
}
