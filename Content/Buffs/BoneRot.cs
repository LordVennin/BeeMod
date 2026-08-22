using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VenninBeeMod.Content.Buffs
{
    /// <summary>
    /// Rots whatever is carrying it: everything afflicted hits 15 percent softer, players and
    /// enemies alike.
    /// </summary>
    public class BoneRot : ModBuff
    {
        public const float AttackPenalty = 0.15f;
        public const int Duration = 240;

        /// <summary>
        /// What a single bee sting is worth. Much shorter than a bullet's, because the Rotweave
        /// Shroud lands dozens of these a fight rather than one at a time.
        /// </summary>
        public const int BeeDuration = 120;

        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.buffNoSave[Type] = true;
            Main.pvpBuff[Type] = true;
            BuffID.Sets.LongerExpertDebuff[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            player.GetModPlayer<BoneRotPlayer>().boneRot = true;
        }

        public override void Update(NPC npc, ref int buffIndex)
        {
            npc.GetGlobalNPC<BoneRotNPC>().boneRot = true;
        }
    }
}
