using Terraria;
using Terraria.ModLoader;
using Terraria.ID;

namespace VenninBeeMod.Content.Buffs
{
    public class BeeSwarmBuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Bee Swarm");
            // Description.SetDefault("Summoned bees are protecting you!");
            Main.buffNoSave[Type] = true;
            Main.buffNoTimeDisplay[Type] = false;
            Main.vanityPet[Type] = false;
            Main.debuff[Type] = false;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            player.buffTime[buffIndex] = 18000; // Keep buff alive while bee exists
            player.GetModPlayer<BeeSwarmPlayer>().beeSwarmActive = true;
        }
    }

    public class BeeSwarmPlayer : ModPlayer
    {
        public bool beeSwarmActive;

        public override void ResetEffects()
        {
            beeSwarmActive = false;
        }
    }
}