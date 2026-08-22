using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;
using VenninBeeMod.Content.Projectiles;

namespace VenninBeeMod.Content.Items
{
    /// <summary>
    /// The Crimson answer to Silent Sting, and its opposite in every way: slow, heavy and
    /// telegraphed. It does not spawn bees on impact - it plants a grub that eats its way out.
    /// </summary>
    public class GorehiveMaul : ModItem
    {
        public override void SetStaticDefaults()
        {
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
        }

        public override void SetDefaults()
        {
            Item.damage = 26;
            Item.DamageType = DamageClass.Melee;

            // Matches the texture. Held items are placed from the unscaled frame size, so the
            // sprite is drawn at the size it should hang at rather than scaled at draw time.
            Item.width = 52;
            Item.height = 52;

            Item.useTime = 35;
            Item.useAnimation = 35;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 7f;
            Item.value = Item.sellPrice(gold: 1);
            Item.rare = ItemRarityID.Blue;
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            Item.scale = 1.1f;

            // Left at its default of false on purpose: the facing is frozen for the whole
            // animation, so nothing can flip the swing halfway through. CanUseItem below is
            // what points it at the cursor before the swing starts.
        }

        /// <summary>
        /// Points the swing at the cursor, once, just before it starts.
        /// </summary>
        /// <remarks>
        /// The timing is the whole point. Doing this from UseStyle flipped the facing after the
        /// game had already worked the swing arc out from the old facing, so the arc and the
        /// mirrored sprite disagreed and you got a second weapon swinging the wrong way. Setting
        /// it before the animation begins leaves the two in step, and useTurn being false means
        /// nothing changes it again until the swing is over.
        ///
        /// The documentation asks that this hook avoid side effects because the use might not
        /// happen. Turning to face the cursor is harmless if the swing is called off, and this
        /// item has no mana or ammo cost that could call it off.
        /// </remarks>
        public override bool CanUseItem(Player player)
        {
            if (player.whoAmI == Main.myPlayer)
            {
                player.direction = Main.MouseWorld.X > player.MountedCenter.X ? 1 : -1;
            }

            return true;
        }

        public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Main.myPlayer != player.whoAmI)
            {
                return;
            }

            // SourceDamage rather than Item.damage, so the grub's brood scales with whatever
            // melee gear the swing was actually made with.
            GorehiveGrub.Implant(player, target, hit.SourceDamage);
        }

        public override void AddRecipes()
        {
            // Tissue Sample gates this behind the Brain of Cthulhu, mirroring the Corruption set.
            CreateRecipe()
                .AddIngredient(ItemID.TissueSample, 20)
                .AddIngredient(ItemID.CrimtaneBar, 10)
                .AddIngredient<StickyResin>(25)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
}
