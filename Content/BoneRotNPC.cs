using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VenninBeeMod.Content.Buffs;

namespace VenninBeeMod.Content
{
    /// <summary>
    /// Carries Bone Rot on enemies. The buff only raises the flag; the softened hit has to be
    /// applied where the game works out damage.
    /// </summary>
    public class BoneRotNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;

        public bool boneRot;

        public override void ResetEffects(NPC npc)
        {
            boneRot = false;
        }

        public override void ModifyHitPlayer(NPC npc, Player target, ref Player.HurtModifiers modifiers)
        {
            if (!boneRot)
            {
                return;
            }

            // SourceDamage is the attack's own damage before the target's defences, so this
            // weakens the attacker rather than toughening whoever it happens to be hitting.
            modifiers.SourceDamage *= 1f - BoneRot.AttackPenalty;
        }

        public override void DrawEffects(NPC npc, ref Color drawColor)
        {
            if (!boneRot)
            {
                return;
            }

            drawColor = Color.Lerp(drawColor, new Color(148, 200, 120), 0.3f);

            if (Main.rand.NextBool(14))
            {
                Dust rot = Dust.NewDustDirect(npc.position, npc.width, npc.height, DustID.Smoke,
                    0f, -0.5f, 150, new Color(140, 196, 112), 0.7f);
                rot.noGravity = true;
            }
        }
    }
}
