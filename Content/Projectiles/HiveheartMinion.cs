using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VenninBeeMod.Content.Buffs;

namespace VenninBeeMod.Content.Projectiles
{
    public class HiveheartMinion : ModProjectile
    {
        private const int AttackCooldown = 60;
        private const float AttackRange = 600f;

        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.Bee;

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = Main.projFrames[ProjectileID.Bee];
            ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true;
            ProjectileID.Sets.MinionSacrificable[Projectile.type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.friendly = true;
            Projectile.minion = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.minionSlots = 1f;
            Projectile.timeLeft = 18000;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 20;
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            if (!player.active || player.dead || !player.GetModPlayer<HiveheartPlayer>().hiveheartActive)
            {
                Projectile.Kill();
                return;
            }

            Projectile.timeLeft = 2;

            if (Projectile.ai[0] > 0f)
            {
                Projectile.ai[0]--;
            }

            NPC target = FindTarget(player);
            Vector2 idlePosition = player.Center + new Vector2(
                (float)System.Math.Sin(Main.GameUpdateCount * 0.05f + Projectile.whoAmI) * 50f,
                -60f
            );

            Vector2 destination = target != null ? target.Center : idlePosition;
            float speed = target != null ? 10f : 7f;
            float inertia = 20f;

            Vector2 toDestination = destination - Projectile.Center;
            if (toDestination.Length() > 10f)
            {
                Projectile.velocity = (Projectile.velocity * (inertia - 1) + toDestination.SafeNormalize(Vector2.Zero) * speed) / inertia;
            }

            if (target != null && Projectile.ai[0] <= 0f)
            {
                Vector2 shootVelocity = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitY) * 8f;
                int damage = (int)(Projectile.damage * 0.6f);
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, shootVelocity, ModContent.ProjectileType<HealingBee>(), damage, Projectile.knockBack, Projectile.owner);
                Projectile.ai[0] = AttackCooldown;
            }

            Projectile.frameCounter++;
            if (Projectile.frameCounter >= 5)
            {
                Projectile.frameCounter = 0;
                Projectile.frame = (Projectile.frame + 1) % Main.projFrames[Projectile.type];
            }

            if (Projectile.velocity.X > 0.2f)
            {
                Projectile.spriteDirection = -1;
            }
            else if (Projectile.velocity.X < -0.2f)
            {
                Projectile.spriteDirection = 1;
            }
        }

        private NPC FindTarget(Player player)
        {
            if (player.HasMinionAttackTargetNPC)
            {
                NPC npc = Main.npc[player.MinionAttackTargetNPC];
                if (npc.CanBeChasedBy(this))
                {
                    return npc;
                }
            }

            NPC closest = null;
            float closestDistance = AttackRange;
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

            return closest;
        }
    }
}
