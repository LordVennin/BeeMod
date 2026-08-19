using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VenninBeeMod.Content.Buffs;

namespace VenninBeeMod.Content.Projectiles
{
    /// <summary>
    /// Spat down at the player by a roosting <see cref="NPCs.SkeletonBee"/>.
    /// </summary>
    public class SkeletonStinger : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 8;
            Projectile.height = 14;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 300;
            Projectile.tileCollide = true;
            Projectile.alpha = 20;
        }

        public override void AI()
        {
            Projectile.velocity.Y += 0.08f;
            Projectile.rotation = Projectile.velocity.ToRotation() - MathHelper.PiOver2;

            if (Main.rand.NextBool(9))
            {
                Dust trail = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height,
                    DustID.Smoke, 0f, 0f, 160, new Color(150, 200, 130), 0.5f);
                trail.noGravity = true;
            }
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            target.AddBuff(ModContent.BuffType<BoneRot>(), BoneRot.Duration);
        }

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 4; i++)
            {
                Dust shard = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height,
                    DustID.Bone, 0f, 0f, 100, default, 0.8f);
                shard.noGravity = true;
            }
        }
    }
}
