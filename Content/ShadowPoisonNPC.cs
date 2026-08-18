using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VenninBeeMod.Content
{
    /// <summary>
    /// Carries the Shadow Poison damage over time. The buff itself only raises the flag; the
    /// actual life drain has to run through UpdateLifeRegen.
    /// </summary>
    public class ShadowPoisonNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;

        public bool shadowPoisoned;

        public override void ResetEffects(NPC npc)
        {
            shadowPoisoned = false;
        }

        public override void UpdateLifeRegen(NPC npc, ref int damage)
        {
            if (!shadowPoisoned)
            {
                return;
            }

            if (npc.lifeRegen > 0)
            {
                npc.lifeRegen = 0;
            }

            // lifeRegen is health per tick times 120, so subtracting 2 is exactly 1 health
            // per second. The damage parameter is only the number floating over the NPC.
            npc.lifeRegen -= 2;

            if (damage < 1)
            {
                damage = 1;
            }
        }

        public override void DrawEffects(NPC npc, ref Color drawColor)
        {
            if (!shadowPoisoned)
            {
                return;
            }

            drawColor = Color.Lerp(drawColor, new Color(122, 84, 184), 0.35f);

            if (Main.rand.NextBool(12))
            {
                Dust haze = Dust.NewDustDirect(npc.position, npc.width, npc.height, DustID.Smoke,
                    0f, -0.6f, 140, new Color(132, 92, 202), 0.7f);
                haze.noGravity = true;
            }
        }
    }
}
