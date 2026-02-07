using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VenninBeeMod.Content.Items
{
    public class SwarmEffigy : ModItem
    {
        public override string Texture => "VenninBeeMod/Content/Items/StickyResin";

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 3;
        }

        public override void SetDefaults()
        {
            Item.width = 20;
            Item.height = 20;
            Item.maxStack = 20;
            Item.useAnimation = 30;
            Item.useTime = 30;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.consumable = true;
            Item.rare = ItemRarityID.White;
            Item.value = Item.buyPrice(silver: 1);
        }

        public override bool CanUseItem(Player player)
        {
            return Main.dayTime
                && player.ZoneOverworldHeight
                && !NPC.AnyNPCs(ModContent.NPCType<NPCs.TheSwarm>());
        }

        public override bool? UseItem(Player player)
        {
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                NPC.SpawnOnPlayer(player.whoAmI, ModContent.NPCType<NPCs.TheSwarm>());
            }
            else
            {
                NetMessage.SendData(MessageID.SpawnBossUseLicenseStartEvent, number: player.whoAmI, number2: ModContent.NPCType<NPCs.TheSwarm>());
            }

            return true;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<StickyResin>(8)
                .AddIngredient(ItemID.Sunflower, 2)
                .AddTile(TileID.WorkBenches)
                .Register();
        }
    }
}
