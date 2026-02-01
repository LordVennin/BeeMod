using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VenninBeeMod.Content.Projectiles
{
    public class HoneySlashProjectile : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 40;
            Projectile.height = 40;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 3;
            Projectile.timeLeft = 30;
            Projectile.tileCollide = false;
            Projectile.alpha = 60;
            Projectile.light = 0.2f;
        }

        public override void AI()
        {
            Projectile.rotation += 0.4f * Projectile.direction;

            // Fade out and slow down near the end
            if (Projectile.timeLeft < 10)
            {
                Projectile.velocity *= 0.85f; // gradually slow down
                Projectile.alpha += 25;       // fade out
                if (Projectile.alpha > 255)
                    Projectile.alpha = 255;
            }

            // Dust trail for honey look
            for (int i = 0; i < 2; i++)
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Honey);
                dust.velocity *= 0.3f;
                dust.scale = 1.1f;
                dust.noGravity = true;
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Player player = Main.player[Projectile.owner];

            if (HasHivePack(player))
                target.AddBuff(BuffID.Chilled, 60);
            else
                target.AddBuff(BuffID.Slow, 60);
        }

        private bool HasHivePack(Player player)
        {
            for (int i = 3; i < 10; i++)
                if (player.armor[i].type == ItemID.HiveBackpack)
                    return true;
            return false;
        }
    }
}