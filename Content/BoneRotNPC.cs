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
        /// <summary>How far the rot will jump when it outlives its host.</summary>
        private const float SpreadRange = 220f;

        /// <summary>How close the wearer has to be for the jump to be theirs.</summary>
        private const float CarrierRange = 2000f;

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

        /// <summary>
        /// Hive Pack secret for the Rotweave Shroud: rot outlives what it killed and finds
        /// something else nearby to settle into.
        /// </summary>
        public override void OnKill(NPC npc)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                return;
            }

            if (!npc.HasBuff(ModContent.BuffType<BoneRot>()) || !AnyoneSpreadingRot(npc))
            {
                return;
            }

            NPC nearest = null;
            float nearestDistance = SpreadRange;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC other = Main.npc[i];
                if (other.whoAmI == npc.whoAmI || !CombatTarget.IsReal(other)
                    || other.HasBuff(ModContent.BuffType<BoneRot>()))
                {
                    continue;
                }

                float distance = Vector2.Distance(other.Center, npc.Center);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearest = other;
                }
            }

            nearest?.AddBuff(ModContent.BuffType<BoneRot>(), BoneRot.BeeDuration);
        }

        /// <summary>
        /// Whether anybody close enough to have caused this is wearing the shroud with a Hive
        /// Pack. Checked per player rather than off the local one, so a server gets it right.
        /// </summary>
        private static bool AnyoneSpreadingRot(NPC npc)
        {
            for (int i = 0; i < Main.maxPlayers; i++)
            {
                Player player = Main.player[i];
                if (!player.active || player.dead)
                {
                    continue;
                }

                if (Vector2.Distance(player.Center, npc.Center) > CarrierRange)
                {
                    continue;
                }

                if (player.GetModPlayer<RotweavePlayer>().rotweave && HivePack.IsEquipped(player))
                {
                    return true;
                }
            }

            return false;
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
