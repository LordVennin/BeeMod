using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VenninBeeMod.Content.Projectiles
{
    public class SwarmResinBeeProjectile : ModProjectile
    {
        public override string Texture => "VenninBeeMod/Content/NPCs/StickyResinBee";

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 4;
        }

        public override void SetDefaults()
        {
            Projectile.width = 12;
            Projectile.height = 12;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 240;
            Projectile.penetrate = 1;
            Projectile.alpha = 40;
        }

        public override void AI()
        {
            Projectile.frameCounter++;
            if (Projectile.frameCounter >= 5)
            {
                Projectile.frameCounter = 0;
                Projectile.frame = (Projectile.frame + 1) % Main.projFrames[Projectile.type];
            }

            int playerTarget = Player.FindClosest(Projectile.Center, Projectile.width, Projectile.height);
            Player player = Main.player[playerTarget];
            if (player.active && !player.dead)
            {
                Vector2 desiredVelocity = (player.Center - Projectile.Center).SafeNormalize(Vector2.UnitY) * 7f;
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, desiredVelocity, 0.06f);
            }

            Projectile.rotation = Projectile.velocity.X * 0.08f;
        }
    }
}
