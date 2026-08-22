using Microsoft.Xna.Framework;
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
        /// <summary>Which way the swing in progress was aimed, or 0 between swings.</summary>
        private int swingDirection;

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

            // Permits the facing to change during the animation. On its own this makes things
            // worse - movement then re-faces you every frame - but it stops the game undoing
            // the facing UseStyle sets below.
            Item.useTurn = true;
        }

        /// <summary>
        /// Aims each swing at the cursor and holds it there for the whole animation.
        /// </summary>
        /// <remarks>
        /// Neither value of <see cref="Item.useTurn"/> gets this right on its own. The swing
        /// inherits whatever facing movement last set, so walking left pinned every swing to the
        /// left however far right the cursor was. Committing to the cursor on the first frame
        /// and enforcing it after is what lets you back away from something and still hit it.
        /// This runs during ItemCheck, which is after movement has had its say, so it wins.
        /// </remarks>
        public override void UseStyle(Player player, Rectangle heldItemFrame)
        {
            if (player.whoAmI != Main.myPlayer)
            {
                return;
            }

            if (player.itemAnimation >= player.itemAnimationMax - 1 || swingDirection == 0)
            {
                swingDirection = Main.MouseWorld.X > player.MountedCenter.X ? 1 : -1;
            }

            player.direction = swingDirection;
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
