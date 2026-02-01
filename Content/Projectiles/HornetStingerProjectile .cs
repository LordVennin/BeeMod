using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;

namespace VenninBeeMod.Content.Projectiles
{
    public class HornetStingerProjectile : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.Stinger;

        public override void SetStaticDefaults()
        {
            //DisplayName.SetDefault("Hornet Stinger");
        }

        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 300;
            Projectile.aiStyle = 0; // removed arrow AI to eliminate gravity
            Projectile.extraUpdates = 1;
        }

        public override void AI()
        {
            // Optional: rotate in flight
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Grass,
            Projectile.velocity.X * 0.2f, Projectile.velocity.Y * 0.2f, 100, default, .6f);
        }

        public override bool CanHitPlayer(Player target)
        {
            // Don't damage players at all
            return false;
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Terraria.Audio.SoundEngine.PlaySound(SoundID.Dig, Projectile.position);
            return true; // allow default collision behavior (destroy on impact)
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Optional: poison effect
            target.AddBuff(BuffID.Poisoned, 120);
        }
    }
}