using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
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
        private const float HoverCircleRadiusMin = 40f;
        private const float HoverCircleRadiusMax = 80f;
        private const float HoverCircleHeight = 60f;
        private const int HoverRetargetMinFrames = 45;
        private const int HoverRetargetMaxFrames = 90;

        private float hoverAngle;
        private float hoverRadius;
        private float hoverTimer;
        private bool hoverInitialized;

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
            if (!player.active || player.dead || !(player.GetModPlayer<HiveheartPlayer>().hiveheartActive || player.HasBuff(ModContent.BuffType<HiveheartBuff>())))
            {
                Projectile.Kill();
                return;
            }

            Projectile.timeLeft = 2;

            float speed = 8f;
            float inertia = 20f;

            GetMinionOrder(player, out int minionIndex, out int minionCount);
            UpdateHoverTarget(minionIndex);
            Vector2 idlePosition = player.Center + GetHoverOffset(minionIndex, minionCount);

            Projectile.friendly = (int)Projectile.ai[0] != StateHeal;

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
                            Vector2 hoverPos = player.Center + GetHoverOffset(minionIndex, minionCount);
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

                        if (Main.GameUpdateCount % 6 == 0)
                        {
                            Vector2 dustOffset = Main.rand.NextVector2Circular(10f, 10f);
                            Dust dust = Dust.NewDustPerfect(Projectile.Center + dustOffset, DustID.Honey);
                            dust.velocity = dustOffset.SafeNormalize(Vector2.UnitY) * Main.rand.NextFloat(0.3f, 0.8f);
                            dust.noGravity = true;
                            dust.scale = Main.rand.NextFloat(0.9f, 1.2f);
                            dust.color = Color.Red;
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
                                SoundEngine.PlaySound(SoundID.Item4, Projectile.Center);
                                SoundEngine.PlaySound(SoundID.Item9, Projectile.Center);
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
                Projectile.spriteDirection = 1;
            }
            else if (Projectile.velocity.X < -0.2f)
            {
                Projectile.spriteDirection = -1;
            }
        }

        private Vector2 GetHoverOffset(int minionIndex, int minionCount)
        {
            Vector2 circleOffset = new Vector2((float)System.Math.Cos(hoverAngle), (float)System.Math.Sin(hoverAngle)) * hoverRadius;
            circleOffset.Y = System.Math.Min(circleOffset.Y, 0f);

            float time = Main.GameUpdateCount;
            float bobX = (float)System.Math.Sin(time * 0.06f + minionIndex * 0.9f) * 6f;
            float bobY = (float)System.Math.Cos(time * 0.05f + minionIndex * 1.2f) * 4f;

            return new Vector2(circleOffset.X + bobX, -HoverCircleHeight + circleOffset.Y + bobY);
        }

        private void UpdateHoverTarget(int minionIndex)
        {
            if (!hoverInitialized)
            {
                float seed = (Projectile.whoAmI + minionIndex) * 0.93f;
                hoverAngle = seed;
                hoverRadius = HoverCircleRadiusMin + (Projectile.whoAmI % 7) * 6f;
                hoverTimer = HoverRetargetMinFrames + (Projectile.whoAmI % 11);
                hoverInitialized = true;
            }

            hoverTimer--;
            if (hoverTimer <= 0f)
            {
                hoverAngle = Main.rand.NextFloat(MathHelper.TwoPi);
                hoverRadius = Main.rand.NextFloat(HoverCircleRadiusMin, HoverCircleRadiusMax);
                hoverTimer = Main.rand.Next(HoverRetargetMinFrames, HoverRetargetMaxFrames + 1);
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

        public override bool? CanHitNPC(NPC target)
        {
            return (int)Projectile.ai[0] == StateHeal ? false : null;
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
            SpriteEffects effects = Projectile.spriteDirection == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

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

        private void GetMinionOrder(Player player, out int minionIndex, out int minionCount)
        {
            minionIndex = 0;
            minionCount = 0;

            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile proj = Main.projectile[i];
                if (!proj.active || proj.owner != player.whoAmI || proj.type != Projectile.type)
                {
                    continue;
                }

                if (proj.whoAmI == Projectile.whoAmI)
                {
                    minionIndex = minionCount;
                }

                minionCount++;
            }

            if (minionCount == 0)
            {
                minionCount = 1;
            }
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
