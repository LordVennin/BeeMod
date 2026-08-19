using Terraria;
using Terraria.ModLoader;
using VenninBeeMod.Content.Buffs;

namespace VenninBeeMod.Content
{
    public class BoneRotPlayer : ModPlayer
    {
        public bool boneRot;

        public override void ResetEffects()
        {
            boneRot = false;
        }

        public override void PostUpdateEquips()
        {
            if (boneRot)
            {
                Player.GetDamage(DamageClass.Generic) *= 1f - BoneRot.AttackPenalty;
            }
        }
    }
}
