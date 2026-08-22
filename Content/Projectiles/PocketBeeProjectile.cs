using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VenninBeeMod.Content.Projectiles
{
    public class PocketBeeProjectile : ModProjectile, IBeeProjectile
    {
        private const int HomingDelay = 20;
        private const float HomingRange = 420f;
        private const float HomingSpeed = 9f;

        private float rotationDirection;

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 4; // 4-frame animation
        }

        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 40; // Very short lifespan
            Projectile.tileCollide = true;
            Projectile.ignoreWater = false;
            Projectile.alpha = 0;
        }

        public override void OnSpawn(Terraria.DataStructures.IEntitySource source)
        {
            Projectile.rotation = Main.rand.NextFloat(MathHelper.TwoPi); // random initial rotation
            rotationDirection = Main.rand.NextBool() ? 1f : -1f; // random clockwise or counterclockwise
        }

        public override void AI()
        {
            SeekWithHivePack();

            // Animation
            Projectile.frameCounter++;
            if (Projectile.frameCounter >= 4)
            {
                Projectile.frameCounter = 0;
                Projectile.frame++;
                if (Projectile.frame >= Main.projFrames[Projectile.type])
                    Projectile.frame = 0;
            }

            // Light yellow glow
            Lighting.AddLight(Projectile.Center, 0.8f, 0.7f, 0.1f);

            // Fade out more slowly
            Projectile.alpha += 5;
            if (Projectile.alpha > 255)
                Projectile.alpha = 255;

            // Wobble movement
            Projectile.velocity.X *= 0.99f;
            Projectile.velocity.Y += 0.15f;

            // Rotate sprite
            Projectile.rotation += rotationDirection * 0.2f;
        }
    
        /// <summary>
        /// With a Hive Pack the bees stop being dumb shot and turn hunter partway through their
        /// short flight, curving onto whatever is nearest.
        /// </summary>
        private void SeekWithHivePack()
        {
            int age = 40 - Projectile.timeLeft;
            if (age < HomingDelay || !HivePack.IsEquipped(Main.player[Projectile.owner]))
            {
                return;
            }

            NPC closest = null;
            float closestDistance = HomingRange;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.CanBeChasedBy(this))
                {
                    continue;
                }

                float distance = Vector2.Distance(Projectile.Center, npc.Center);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closest = npc;
                }
            }

            if (closest == null)
            {
                return;
            }

            Vector2 desired = (closest.Center - Projectile.Center).SafeNormalize(Vector2.UnitX) * HomingSpeed;
            Projectile.velocity = Vector2.Lerp(Projectile.velocity, desired, 0.16f);
        }
    }
}
