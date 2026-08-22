using Terraria;
using Terraria.ModLoader;
using VenninBeeMod.Content.Buffs;

namespace VenninBeeMod.Content
{
    /// <summary>
    /// Pays out the Nectar Lash's tag, and drags on whatever is wearing it.
    /// </summary>
    public class NectarTagNPC : GlobalNPC
    {
        public override void ModifyHitByProjectile(NPC npc, Projectile projectile, ref NPC.HitModifiers modifiers)
        {
            if (!npc.HasBuff(ModContent.BuffType<NectarTag>()))
            {
                return;
            }

            // Traps and enemy projectiles are not the player's doing, so they collect nothing.
            if (projectile.npcProj || projectile.trap)
            {
                return;
            }

            // A vanilla whip only pays minions and sentries. This mod's bees are mostly plain
            // projectiles off a gun or a staff, so the tag would miss almost everything the mod
            // is about; IBeeProjectile brings them in.
            if (!projectile.IsMinionOrSentryRelated && !BeeProjectiles.IsBee(projectile.type))
            {
                return;
            }

            modifiers.FlatBonusDamage += NectarTag.TagDamage;
        }

        public override void PostAI(NPC npc)
        {
            if (!npc.HasBuff(ModContent.BuffType<NectarTag>()))
            {
                return;
            }

            // Walking the NPC back along its own movement is the cheapest honest slow: it works
            // on fliers and walkers alike without touching whatever AI is driving them.
            npc.position -= npc.velocity * NectarTag.SlowFactor;
        }
    }
}
