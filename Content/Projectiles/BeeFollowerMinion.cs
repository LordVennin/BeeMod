using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System;
using VenninBeeMod.Content.Buffs;

namespace VenninBeeMod.Content.Projectiles
{
    public class BeeFollowerMinion : ModProjectile
    {
        const int STATE_IDLE = 0;
        const int STATE_ATTACK = 1;
        const int STATE_RETURN = 2;

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 4;
            ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.minion = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.penetrate = -1;
            Projectile.ArmorPenetration = 4;
            Projectile.timeLeft = 18000;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            //Projectile.minionSlots = 0.15f;
            Projectile.netImportant = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];

            if (HasHivePack(Main.player[Projectile.owner]))
            {
                Projectile.minionSlots = 0.25f; // Less slot cost with Hive Pack
            }
            else
            {
                Projectile.minionSlots = 0.33f; // Normal cost
            }


            if (!player.active || player.dead || !player.HasBuff(ModContent.BuffType<BeeSwarmBuff>()))
            {
                Projectile.Kill();
                return;
            }

            Projectile.timeLeft = 2;

            float speed = 8f;
            float inertia = 20f;

            Vector2 idlePos = player.Center + new Vector2(0, -50f);
            idlePos.X += (float)Math.Sin(Main.GameUpdateCount * 0.05f + Projectile.whoAmI) * 30f;

            switch ((int)Projectile.ai[0])
            {
                case STATE_IDLE:
                    {
                        NPC target = FindTarget();
                        if (target != null)
                        {
                            Vector2 toTarget = target.Center - Projectile.Center;
                            float angleOffset = Main.rand.NextFloat(-1.2f, 1.2f);
                            Vector2 flyDir = toTarget.RotatedBy(angleOffset).SafeNormalize(Vector2.UnitX);
                            Vector2 flyTo = target.Center + flyDir * 90f;

                            Projectile.localAI[0] = target.whoAmI;
                            Projectile.localAI[1] = 75 + Main.rand.Next(20);
                            Projectile.ai[0] = STATE_ATTACK;
                            Projectile.ai[1] = flyTo.X;
                            Projectile.ai[2] = flyTo.Y;
                        }
                        else
                        {
                            // Swarming idle movement with light wobble
                            Vector2 swarmOffset = new Vector2(
                                (float)Math.Sin(Main.GameUpdateCount * 0.05f + Projectile.whoAmI) * 30f,
                                (float)Math.Cos(Main.GameUpdateCount * 0.1f + Projectile.whoAmI) * 10f
                            );
                            Vector2 hoverPos = player.Center + new Vector2(0, -50f) + swarmOffset;
                            Vector2 toHover = hoverPos - Projectile.Center;

                            Projectile.velocity = (Projectile.velocity * (inertia - 1) + toHover.SafeNormalize(Vector2.Zero) * speed) / inertia;
                        }
                    }
                    break;

                case STATE_ATTACK:
                    {
                        Projectile.localAI[1]--;

                        NPC target = Projectile.localAI[0] >= 0 && Projectile.localAI[0] < Main.maxNPCs
                            ? Main.npc[(int)Projectile.localAI[0]]
                            : null;

                        Vector2 flyTo = new Vector2(Projectile.ai[1], Projectile.ai[2]);
                        Vector2 toFly = flyTo - Projectile.Center;

                        Projectile.velocity = (Projectile.velocity * (inertia - 1) + toFly.SafeNormalize(Vector2.Zero) * 8f) / inertia;

                        if (toFly.Length() < 16f || Projectile.localAI[1] <= 0 || target == null || !target.active || target.friendly)
                        {
                            Projectile.ai[0] = STATE_RETURN;
                        }
                    }
                    break;

                case STATE_RETURN:
                    {
                        Vector2 toIdle = idlePos - Projectile.Center;
                        Projectile.velocity = (Projectile.velocity * (inertia - 1) + toIdle.SafeNormalize(Vector2.Zero) * speed) / inertia;

                        if (toIdle.Length() < 20f)
                        {
                            Projectile.ai[0] = STATE_IDLE;
                        }
                    }
                    break;
            }

            // Animation
            Projectile.frameCounter++;
            if (Projectile.frameCounter >= 6)
            {
                Projectile.frameCounter = 0;
                Projectile.frame = (Projectile.frame + 1) % Main.projFrames[Projectile.type];
            }

            // Sprite direction
            if (Projectile.velocity.X > 0.2f)
                Projectile.spriteDirection = -1;
            else if (Projectile.velocity.X < -0.2f)
                Projectile.spriteDirection = 1;
        }

        private NPC FindTarget()
        {
            NPC closest = null;
            float distance = 700f;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.CanBeChasedBy(this) && Vector2.Distance(Projectile.Center, npc.Center) < distance)
                {
                    distance = Vector2.Distance(Projectile.Center, npc.Center);
                    closest = npc;
                }
            }

            return closest;
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            Player player = Main.player[Projectile.owner];
            if (HasHivePack(player))
            {
                modifiers.FinalDamage *= 1.10f;
            }
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