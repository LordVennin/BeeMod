using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.ID;
using System;

namespace VenninBeeMod.Content.Projectiles
{
    public class RoyalBeeOrbiter : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 4;
        }

        public override void SetDefaults()
        {
            Projectile.width = 12;
            Projectile.height = 12;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 999999;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.DamageType = DamageClass.Summon;
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            if (!player.active || player.dead)
            {
                Projectile.Kill();
                return;
            }

            Projectile.direction = player.direction;
            Projectile.spriteDirection = Projectile.direction * -1;

            // Read slot and total from ai[]
            int index = (int)Projectile.ai[0];
            int total = (int)Projectile.ai[1];

            float angle = MathHelper.TwoPi * index / total + Main.GameUpdateCount * 0.05f;
            float radius = 60f;

            Vector2 offset = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * radius;
            Projectile.Center = player.Center + offset;

            // Animation
            Projectile.frameCounter++;
            if (Projectile.frameCounter >= 5)
            {
                Projectile.frameCounter = 0;
                Projectile.frame = (Projectile.frame + 1) % 4;
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Player player = Main.player[Projectile.owner];
            player.statLife += 3;
            player.HealEffect(3);
        }
    }
}