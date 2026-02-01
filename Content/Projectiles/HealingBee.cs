using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.ID;

namespace VenninBeeMod.Content.Projectiles
{
    public class HealingBee : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 4;
        }

        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.friendly = true;
            Projectile.penetrate = Main.rand.Next(2, 4); // Randomly 2 or 3 hits
            Projectile.timeLeft = 280;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.aiStyle = 36; // Bee-like behavior
            Projectile.tileCollide = true; // Collides with tiles
        }

        public override void AI()
        {
            // Animate frames
            Projectile.frameCounter++;
            if (Projectile.frameCounter >= 5)
            {
                Projectile.frameCounter = 0;
                Projectile.frame = (Projectile.frame + 1) % 4;
            }

            if (Projectile.velocity.X > 0.2f)
                Projectile.spriteDirection = -1;
            else if (Projectile.velocity.X < -0.2f)
                Projectile.spriteDirection = 1;

        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Player player = Main.player[Projectile.owner];
            player.statLife += 3;
            player.HealEffect(3);
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            // Just bounce off surfaces instead of dying
            if (Projectile.velocity.X != oldVelocity.X)
                Projectile.velocity.X = -oldVelocity.X * 0.75f;

            if (Projectile.velocity.Y != oldVelocity.Y)
                Projectile.velocity.Y = -oldVelocity.Y * 0.75f;

            return false; // Don’t kill the projectile
        }
    }
}