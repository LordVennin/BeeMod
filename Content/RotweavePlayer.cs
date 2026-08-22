using Terraria.ModLoader;

namespace VenninBeeMod.Content
{
    public class RotweavePlayer : ModPlayer
    {
        public bool rotweave;

        public override void ResetEffects()
        {
            rotweave = false;
        }
    }
}
