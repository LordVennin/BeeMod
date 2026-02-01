using Terraria;
using Terraria.ModLoader;
using Terraria.ID;

namespace VenninBeeMod.Content.Buffs
{
    public class QueensHoney : ModBuff
    {
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Queen's Honey");
            // Description.SetDefault("Greatly increased life regeneration");

            Main.buffNoSave[Type] = true;
            Main.buffNoTimeDisplay[Type] = false;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            // Stronger regen effect than regular honey
            player.lifeRegen += 7; // 6 extra life per second (60 ticks = 1 sec)
            player.lifeRegenTime = 0;

            // Optional: add visuals
            if (Main.rand.NextBool(4))
            {
                int dust = Dust.NewDust(player.position, player.width, player.height, DustID.Honey);
                Main.dust[dust].noGravity = true;
                Main.dust[dust].velocity *= 0.5f;
            }
        }
    }
}