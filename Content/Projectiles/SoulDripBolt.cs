using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VenninBeeMod.Content.Projectiles
{
    public class SoulDripBolt : ModProjectile
    {
        private const int MaxChains = 2;
        private const float ChainRange = 420f;

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 240;
            Projectile.tileCollide = true;
            Projectile.aiStyle = 0;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            Lighting.AddLight(Projectile.Center, 0.35f, 0.25f, 0.1f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Projectile.ai[0] >= MaxChains)
            {
                return;
            }

            NPC nextTarget = FindNextTarget(target);
            if (nextTarget == null)
            {
                return;
            }

            Vector2 direction = (nextTarget.Center - target.Center).SafeNormalize(Vector2.UnitY);
            float speed = Projectile.velocity.Length();
            int damage = (int)(Projectile.damage * 0.8f);

            Projectile.NewProjectile(
                Projectile.GetSource_FromThis(),
                target.Center,
                direction * speed,
                Type,
                damage,
                Projectile.knockBack,
                Projectile.owner,
                Projectile.ai[0] + 1f,
                nextTarget.whoAmI
            );
        }

        private NPC FindNextTarget(NPC currentTarget)
        {
            NPC closest = null;
            float closestDistance = ChainRange;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.CanBeChasedBy(this) || npc.whoAmI == currentTarget.whoAmI)
                {
                    continue;
                }

                float distance = Vector2.Distance(currentTarget.Center, npc.Center);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closest = npc;
                }
            }

            return closest;
        }
    }
}
