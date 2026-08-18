using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VenninBeeMod.Content.Buffs
{
    public class ShadowPoison : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.buffNoSave[Type] = true;
            BuffID.Sets.LongerExpertDebuff[Type] = true;
        }

        public override void Update(NPC npc, ref int buffIndex)
        {
            npc.GetGlobalNPC<ShadowPoisonNPC>().shadowPoisoned = true;
        }
    }
}
