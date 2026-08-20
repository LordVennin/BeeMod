using Terraria;
using Terraria.ID;

namespace VenninBeeMod.Content
{
    /// <summary>
    /// One place to ask whether an NPC is something you can actually kill.
    /// </summary>
    /// <remarks>
    /// Every on-hit payload in the mod has to gate on this. A target dummy can be hit forever
    /// without dying, so anything that spawns bees, plants a grub or feeds life back would be
    /// farmable off a practice target otherwise.
    /// </remarks>
    public static class CombatTarget
    {
        public static bool IsReal(NPC npc)
        {
            return npc.active
                && npc.life > 0
                && !npc.friendly
                && !npc.immortal
                && !npc.dontTakeDamage
                && npc.type != NPCID.TargetDummy;
        }
    }
}
