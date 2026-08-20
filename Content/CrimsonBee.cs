using Terraria;
using Terraria.ID;

namespace VenninBeeMod.Content
{
    /// <summary>
    /// Shared behaviour for the Crimson set. Where the Corruption weapons lean on Shadow Poison
    /// ticking away in the background, these ones scatter enemies: every crimson sting has a
    /// small chance to leave the target reeling.
    /// </summary>
    /// <remarks>
    /// This uses vanilla <see cref="BuffID.Confused"/> on purpose. It already reverses enemy
    /// movement and, more importantly, it already carries the per-NPC immunity tables - which
    /// means nearly every boss shrugs it off. Treat the confusion as a crowd bonus only and
    /// never as part of a weapon's damage budget.
    /// </remarks>
    public static class CrimsonBee
    {
        /// <summary>Roughly one sting in twelve. These weapons throw bees in bunches.</summary>
        public const int ConfusionOdds = 12;

        public const int ConfusionDuration = 120;

        public static void TryConfuse(NPC target)
        {
            if (CombatTarget.IsReal(target) && Main.rand.NextBool(ConfusionOdds))
            {
                // NPC.AddBuff honours immunity tables and syncs itself in multiplayer.
                target.AddBuff(BuffID.Confused, ConfusionDuration);
            }
        }
    }
}
