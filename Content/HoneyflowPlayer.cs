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
            if (!honeyflowActive)
            {
                return;
            }

            if (Player.honeyWet)
            {
                Player.honeyWet = false;
                Player.honey = false;
            }
        }
    }
}
