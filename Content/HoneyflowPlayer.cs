using Terraria;
using Terraria.ModLoader;

namespace VenninBeeMod.Content
{
    public class HoneyflowPlayer : ModPlayer
    {
        public bool honeyflowActive;

        public override void ResetEffects()
        {
            honeyflowActive = false;
        }

        public override void PreUpdateMovement()
        {
            if (honeyflowActive && Player.honeyWet)
            {
                Player.ignoreWater = true;
                Player.honeyWet = false;
                Player.honey = false;
            }
        }

        public override void PostUpdateRunSpeeds()
        {
            if (honeyflowActive && Player.honeyWet)
            {
                Player.ignoreWater = true;
                Player.maxRunSpeed *= 4f;
                Player.runAcceleration *= 4f;
                Player.runSlowdown *= 4f;
            }
        }
    }
}
