using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;
using VenninBeeMod.Content.Projectiles;

namespace VenninBeeMod.Content.Items
{
    public class SoulDripScepter : ModItem
    {
        public const int ManaPerBee = 4;
        public override void SetStaticDefaults()
        {
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
            ItemID.Sets.ItemsThatAllowRepeatedRightClick[Type] = true;
        }

        public override void SetDefaults()
        {
            Item.damage = 36;
            Item.DamageType = DamageClass.Magic;
            Item.mana = ManaPerBee;
            Item.width = 36;
            Item.height = 36;
            Item.useTime = 10;
            Item.useAnimation = 10;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.knockBack = 3f;
            Item.value = Item.buyPrice(gold: 2);
            Item.rare = ItemRarityID.LightRed;
            Item.UseSound = SoundID.Item20;
            Item.autoReuse = false;
            Item.channel = true;
            Item.shoot = ModContent.ProjectileType<QueenMurmurFocus>();
            Item.shootSpeed = 0f;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.CrystalShard, 12)
                .AddIngredient(ItemID.SoulofLight, 6)
                .AddIngredient(ItemID.SoulofNight, 6)
                .AddIngredient(ItemID.BottledHoney, 6)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }

        public override bool AltFunctionUse(Player player)
        {
            return true;
        }
    }
}
