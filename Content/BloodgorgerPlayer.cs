using Terraria.ModLoader;

namespace VenninBeeMod.Content
{
    /// <summary>
    /// Holds the global cap on Bloodgorger lifesteal.
    /// </summary>
    /// <remarks>
    /// Sustain is the one thing that must not multiply with summon slots. Three gorgers should
    /// be three lots of damage, never three lots of healing, so the drain is rationed here on
    /// the player rather than per drone.
    /// </remarks>
    public class BloodgorgerPlayer : ModPlayer
    {
        /// <summary>One point of life per second, no matter how many drones are feeding.</summary>
        public const int DrainInterval = 60;

        private int drainCooldown;

        public override void PostUpdate()
        {
            if (drainCooldown > 0)
            {
                drainCooldown--;
            }
        }

        /// <summary>
        /// Claims the shared drain if it is off cooldown. Returns false if another drone already
        /// took it this second.
        /// </summary>
        public bool TryDrain()
        {
            if (drainCooldown > 0)
            {
                return false;
            }

            drainCooldown = DrainInterval;
            return true;
        }
    }
}
