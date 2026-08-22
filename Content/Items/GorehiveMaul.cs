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

            // Lets the player turn mid swing. Without it a heavy 35 tick animation pins you
            // facing the way you started, so you cannot back away while swinging.
            Item.useTurn = true;
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
