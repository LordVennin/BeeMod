using Terraria;
using Terraria.ModLoader;

namespace VenninBeeMod.Content.Buffs
{
    public class HiveheartBuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.buffNoSave[Type] = true;
            Main.buffNoTimeDisplay[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            player.buffTime[buffIndex] = 18000;
            player.GetModPlayer<HiveheartPlayer>().hiveheartActive = true;
        }
    }

    public class HiveheartPlayer : ModPlayer
    {
        public bool hiveheartActive;

        public override void ResetEffects()
        {
            hiveheartActive = false;
        }
    }
}
