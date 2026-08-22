using Terraria;
using Terraria.ModLoader;
using VenninBeeMod.Content.Buffs;

namespace VenninBeeMod.Content
{
    /// <summary>
    /// Puts Bone Rot on whatever the wearer's bees sting, for as long as the Rotweave Shroud is
    /// equipped.
    /// </summary>
    /// <remarks>
    /// This is deliberately a GlobalProjectile keyed off <see cref="IBeeProjectile"/> rather
    /// than an edit to each bee. There are eighteen bee classes, and every one of them would
    /// otherwise need to learn about an accessory it has nothing to do with.
    /// </remarks>
    public class BeeRotProjectile : GlobalProjectile
    {
        public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!BeeProjectiles.IsBee(projectile.type))
            {
                return;
            }

            if (projectile.owner < 0 || projectile.owner >= Main.maxPlayers)
            {
                return;
            }

            Player owner = Main.player[projectile.owner];
            if (!owner.active || !owner.GetModPlayer<RotweavePlayer>().rotweave)
            {
                return;
            }

            // Same gate as every other on-hit payload: nothing works on a practice target.
            if (!CombatTarget.IsReal(target))
            {
                return;
            }

            target.AddBuff(ModContent.BuffType<BoneRot>(), BoneRot.BeeDuration);
        }
    }
}
