using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using VenninBeeMod.Content.Buffs;

namespace VenninBeeMod.Content.Projectiles
{
    public class HiveheartMinion : ModProjectile
    {
        private const int StateIdle = 0;
        private const int StateAttack = 1;
        private const int StateReturn = 2;
        private const int StateHeal = 3;

        private const float AttackRange = 700f;
        private const float HealRange = 700f;

        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.Bee;

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = Main.projFrames[ProjectileID.Bee];
            ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true;
            ProjectileID.Sets.MinionSacrificable[Projectile.type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 30;
            Projectile.scale = 1.5f;
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

            float speed = 8f;
            float inertia = 20f;

            Vector2 idlePosition = player.Center + new Vector2(
                (float)System.Math.Sin(Main.GameUpdateCount * 0.05f + Projectile.whoAmI) * 30f,
                -60f
            );

            switch ((int)Projectile.ai[0])
            {
                case StateIdle:
                    {
                        NPC target = FindTarget(player);
                        if (target != null)
                        {
                            Projectile.localAI[0] = target.whoAmI;
                            Projectile.localAI[1] = 120f;
                            Projectile.ai[0] = StateAttack;
                        }
                        else
                        {
                            Vector2 swarmOffset = new Vector2(
                                (float)System.Math.Sin(Main.GameUpdateCount * 0.05f + Projectile.whoAmI) * 30f,
                                (float)System.Math.Cos(Main.GameUpdateCount * 0.1f + Projectile.whoAmI) * 10f
                            );
                            Vector2 hoverPos = player.Center + new Vector2(0, -60f) + swarmOffset;
                            Vector2 toHover = hoverPos - Projectile.Center;

                            Projectile.velocity = (Projectile.velocity * (inertia - 1) + toHover.SafeNormalize(Vector2.Zero) * speed) / inertia;
                        }
                    }
                    break;

                case StateAttack:
                    {
                        Projectile.localAI[1]--;
                        NPC target = GetStoredTarget();

                        if (target == null)
                        {
                            Projectile.ai[0] = StateReturn;
                            break;
                        }

                        Vector2 toTarget = target.Center - Projectile.Center;
                        Projectile.velocity = (Projectile.velocity * (inertia - 1) + toTarget.SafeNormalize(Vector2.Zero) * 10f) / inertia;

                        if (Projectile.localAI[1] <= 0f)
                        {
                            Projectile.ai[0] = StateReturn;
                        }
                    }
                    break;

                case StateReturn:
                    {
                        Vector2 toIdle = idlePosition - Projectile.Center;
                        Projectile.velocity = (Projectile.velocity * (inertia - 1) + toIdle.SafeNormalize(Vector2.Zero) * speed) / inertia;

                        if (toIdle.Length() < 20f)
                        {
                            Projectile.ai[0] = StateIdle;
                        }
                    }
                    break;

                case StateHeal:
                    {
                        Player healTarget = FindHealTarget(player);
                        if (healTarget == null)
                        {
                            Projectile.ai[0] = StateReturn;
                            break;
                        }

                        Vector2 toPlayer = healTarget.Center - Projectile.Center;
                        Projectile.velocity = (Projectile.velocity * (inertia - 1) + toPlayer.SafeNormalize(Vector2.Zero) * 9f) / inertia;

                        if (toPlayer.Length() < 20f)
                        {
                            int healAmount = System.Math.Max(1, (int)Projectile.ai[1]);
                            if (healTarget.statLife < healTarget.statLifeMax2)
                            {
                                healTarget.statLife = System.Math.Min(healTarget.statLife + healAmount, healTarget.statLifeMax2);
                                healTarget.HealEffect(healAmount);
                            }

                            Projectile.ai[1] = 0f;
                            Projectile.ai[0] = StateIdle;
                        }
                    }
                    break;
            }

            Projectile.frameCounter++;
            if (Projectile.frameCounter >= 6)
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

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Projectile.ai[0] != StateHeal)
            {
                Projectile.ai[1] = System.Math.Max(1, damageDone / 2);
                Projectile.ai[0] = StateHeal;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if ((int)Projectile.ai[0] != StateHeal)
            {
                return true;
            }

            Texture2D texture = ModContent.Request<Texture2D>("VenninBeeMod/Content/Projectiles/HiveHornetMinionHeart").Value;
            int frameHeight = texture.Height / Main.projFrames[Projectile.type];
            Rectangle frame = new Rectangle(0, frameHeight * Projectile.frame, texture.Width, frameHeight);
            Vector2 origin = frame.Size() / 2f;
            SpriteEffects effects = Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            Main.EntitySpriteDraw(
                texture,
                Projectile.Center - Main.screenPosition,
                frame,
                Projectile.GetAlpha(lightColor),
                Projectile.rotation,
                origin,
                Projectile.scale,
                effects,
                0f
            );

            return false;
        }

        private NPC GetStoredTarget()
        {
            int targetIndex = (int)Projectile.localAI[0];
            if (targetIndex >= 0 && targetIndex < Main.maxNPCs)
            {
                NPC target = Main.npc[targetIndex];
                if (target.active && !target.friendly && target.CanBeChasedBy(this))
                {
                    return target;
                }
            }

            return null;
        }

        private Player FindInjuredPlayer(Player owner)
        {
            Player closest = null;
            float closestDistance = HealRange;

            for (int i = 0; i < Main.maxPlayers; i++)
            {
                Player player = Main.player[i];
                if (!player.active || player.dead)
                {
                    continue;
                }

                if (owner.team == 0 && player.whoAmI != owner.whoAmI)
                {
                    continue;
                }

                if (owner.team != 0 && player.team != owner.team)
                {
                    continue;
                }

                if (player.statLife >= player.statLifeMax2)
                {
                    continue;
                }

                float distance = Vector2.Distance(Projectile.Center, player.Center);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closest = player;
                }
            }

            return closest;
        }

        private Player FindHealTarget(Player owner)
        {
            Player injuredPlayer = FindInjuredPlayer(owner);
            if (injuredPlayer != null)
            {
                return injuredPlayer;
            }

            Player closest = null;
            float closestDistance = HealRange;

            for (int i = 0; i < Main.maxPlayers; i++)
            {
                Player player = Main.player[i];
                if (!player.active || player.dead)
                {
                    continue;
                }

                if (owner.team == 0 && player.whoAmI != owner.whoAmI)
                {
                    continue;
                }

                if (owner.team != 0 && player.team != owner.team)
                {
                    continue;
                }

                float distance = Vector2.Distance(Projectile.Center, player.Center);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closest = player;
                }
            }

            return closest ?? owner;
        }
    }
}
