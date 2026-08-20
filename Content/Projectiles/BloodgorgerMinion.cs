using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using VenninBeeMod.Content.Buffs;

namespace VenninBeeMod.Content.Projectiles
{
    /// <summary>
    /// A parasite, not a fighter. It picks one enemy, clamps on and drains, hitting harder the
    /// longer it stays. Losing the target throws all of that away.
    /// </summary>
    /// <remarks>
    /// Three things keep this honest with several summon slots. The ramp is shallow and per
    /// drone. The lifesteal is rationed globally by <see cref="BloodgorgerPlayer"/>, so more
    /// drones never means more healing. And drones prefer an enemy nobody else is on, so a
    /// squad spreads across a room instead of stacking on one target.
    /// </remarks>
    public class BloodgorgerMinion : ModProjectile
    {
        private const int StateSeek = 0;
        private const int StateLatched = 1;

        private const float SeekRange = 900f;
        private const float LeashRange = 1100f;
        private const float SeekSpeed = 9f;
        private const float SeekInertia = 14f;

        /// <summary>Ticks attached to reach the top of the ramp.</summary>
        private const float RampTime = 300f;

        /// <summary>Damage multiplier at full ramp. 8 base becomes 14.</summary>
        private const float MaxRamp = 1.75f;

        private const int LatchHitCooldown = 30;

        private ref float State => ref Projectile.ai[0];
        private ref float Target => ref Projectile.ai[1];
        private ref float RampTicks => ref Projectile.ai[2];

        private Vector2 biteOffset;
        private float hoverPhase;

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 4;
            ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true;
            ProjectileID.Sets.MinionSacrificable[Projectile.type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = 24;
            Projectile.height = 20;
            Projectile.friendly = true;
            Projectile.minion = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.minionSlots = 1f;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 18000;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.netImportant = true;

            // Bites on its own clock instead of waiting on shared invincibility.
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = LatchHitCooldown;
        }

        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];
            if (!owner.active || owner.dead)
            {
                Projectile.Kill();
                return;
            }

            if (!owner.HasBuff(ModContent.BuffType<BloodgorgerBuff>()))
            {
                Projectile.Kill();
                return;
            }

            Projectile.timeLeft = 2;
            hoverPhase += 0.08f;

            if ((int)State == StateLatched)
            {
                LatchedAI(owner);
            }
            else
            {
                SeekAI(owner);
            }

            AnimateFrames();
            UpdateFacing();
        }

        private void SeekAI(Player owner)
        {
            NPC target = KeepOrPickTarget(owner);
            if (target == null)
            {
                Hover(owner);
                return;
            }

            Vector2 desired = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitY) * SeekSpeed;
            Projectile.velocity = ((Projectile.velocity * (SeekInertia - 1f)) + desired) / SeekInertia;
        }

        /// <summary>
        /// Holds onto the enemy this drone already called, and only goes looking when that one
        /// is gone.
        /// </summary>
        /// <remarks>
        /// The stickiness is half of what stops drones piling onto one enemy. A drone that
        /// re-picked every frame would keep flipping to whatever happened to be nearest, and
        /// since they all launch from the same place that is the same enemy for all of them.
        /// Projectiles update in index order, so the first drone writes its claim before the
        /// next one looks.
        /// </remarks>
        private NPC KeepOrPickTarget(Player owner)
        {
            NPC commanded = owner.HasMinionAttackTargetNPC ? Main.npc[owner.MinionAttackTargetNPC] : null;
            if (commanded != null && commanded.CanBeChasedBy(this))
            {
                Target = commanded.whoAmI + 1;
                return commanded;
            }

            NPC current = ResolveHost();
            if (current != null && current.CanBeChasedBy(this)
                && Vector2.Distance(Projectile.Center, current.Center) <= LeashRange)
            {
                return current;
            }

            NPC picked = ChooseTarget();
            Target = picked != null ? picked.whoAmI + 1 : 0f;
            return picked;
        }

        private void LatchedAI(Player owner)
        {
            NPC host = ResolveHost();
            if (host == null || Vector2.Distance(owner.Center, host.Center) > LeashRange)
            {
                Detach();
                return;
            }

            Projectile.Center = host.Center + biteOffset;
            Projectile.velocity = Vector2.Zero;

            RampTicks = System.Math.Min(RampTime, RampTicks + 1f);

            // Ripples along the drone as it gorges, keyed to how full it is.
            float fullness = RampTicks / RampTime;
            Projectile.rotation = (float)System.Math.Sin(hoverPhase * 2f) * (0.05f + (fullness * 0.12f));
            Projectile.scale = 1f + (fullness * 0.18f);

            if (Main.rand.NextFloat() < 0.08f + (fullness * 0.15f))
            {
                Dust feed = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height,
                    DustID.Blood, 0f, 0f, 90, default, 0.9f);
                feed.velocity *= 0.4f;
            }
        }

        /// <summary>
        /// Drifts behind the player when there is nothing to feed on.
        /// </summary>
        private void Hover(Player owner)
        {
            GetSwarmOrder(out int index, out int count);
            float spread = count > 1 ? (index - ((count - 1) * 0.5f)) * 34f : 0f;

            Vector2 station = owner.MountedCenter
                + new Vector2(-owner.direction * 46f + spread, -38f + ((float)System.Math.Sin(hoverPhase) * 7f));

            Vector2 desired = (station - Projectile.Center) * 0.14f;
            Projectile.velocity = Vector2.Lerp(Projectile.velocity, desired, 0.25f);
            Projectile.rotation = Projectile.velocity.X * 0.02f;
            Projectile.scale = 1f;
        }

        private void GetSwarmOrder(out int index, out int count)
        {
            index = 0;
            count = 0;
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile other = Main.projectile[i];
                if (!other.active || other.type != Projectile.type || other.owner != Projectile.owner)
                {
                    continue;
                }

                if (other.whoAmI < Projectile.whoAmI)
                {
                    index++;
                }

                count++;
            }
        }

        /// <summary>
        /// Nearest enemy, but an enemy nobody else has called wins over a closer one that is
        /// already spoken for. Falls back to a taken one when that is all there is, so a single
        /// enemy still gets the whole squad.
        /// </summary>
        private NPC ChooseTarget()
        {
            NPC free = null;
            NPC taken = null;
            float freeDistance = SeekRange;
            float takenDistance = SeekRange;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.CanBeChasedBy(this))
                {
                    continue;
                }

                float distance = Vector2.Distance(Projectile.Center, npc.Center);
                if (IsClaimed(npc))
                {
                    if (distance < takenDistance)
                    {
                        takenDistance = distance;
                        taken = npc;
                    }
                }
                else if (distance < freeDistance)
                {
                    freeDistance = distance;
                    free = npc;
                }
            }

            return free ?? taken;
        }

        /// <summary>
        /// Whether another drone has called this enemy - on the way to it as well as sitting on
        /// it. Counting only the latched ones was the bug: all the drones launch together and
        /// none of them is latched yet, so they all picked the same nearest enemy and arrived
        /// on top of each other.
        /// </summary>
        private bool IsClaimed(NPC npc)
        {
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile other = Main.projectile[i];
                if (!other.active || other.type != Projectile.type || other.owner != Projectile.owner
                    || other.whoAmI == Projectile.whoAmI)
                {
                    continue;
                }

                if ((int)other.ai[1] - 1 == npc.whoAmI)
                {
                    return true;
                }
            }

            return false;
        }

        private NPC ResolveHost()
        {
            int index = (int)Target - 1;
            if (index < 0 || index >= Main.maxNPCs)
            {
                return null;
            }

            NPC host = Main.npc[index];
            return host.active && host.life > 0 && !host.friendly ? host : null;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Nothing feeds off a practice target.
            if (!CombatTarget.IsReal(target))
            {
                return;
            }

            CrimsonBee.TryConfuse(target);
            Drain();

            if ((int)State == StateLatched)
            {
                return;
            }

            // Clamp onto our own claim, or onto something no other drone has called. Brushing
            // past somebody else's target on the way to ours should not end the trip.
            if ((int)Target - 1 != target.whoAmI && IsClaimed(target))
            {
                return;
            }

            Latch(target);
        }

        private void Latch(NPC target)
        {
            State = StateLatched;
            Target = target.whoAmI + 1;
            biteOffset = Projectile.Center - target.Center;

            float reach = System.Math.Min(target.width, target.height) * 0.3f;
            if (biteOffset.Length() > reach)
            {
                biteOffset = biteOffset.SafeNormalize(Vector2.UnitY) * reach;
            }

            Projectile.velocity = Vector2.Zero;
            Projectile.netUpdate = true;
        }

        /// <summary>
        /// Hive Pack secret: the drone keeps what it had built up, so being shaken off is a
        /// pause rather than a reset.
        /// </summary>
        private void Detach()
        {
            State = StateSeek;
            Target = 0f;

            if (!HivePack.IsEquipped(Main.player[Projectile.owner]))
            {
                RampTicks = 0f;
            }

            Projectile.scale = 1f;
            Projectile.netUpdate = true;

            SoundEngine.PlaySound(SoundID.NPCHit18 with { Volume = 0.4f }, Projectile.Center);
        }

        /// <summary>
        /// Rations the shared drain. Whichever drone bites first that second gets it.
        /// </summary>
        private void Drain()
        {
            if (Main.myPlayer != Projectile.owner)
            {
                return;
            }

            Player owner = Main.player[Projectile.owner];
            if (owner.statLife >= owner.statLifeMax2)
            {
                return;
            }

            if (!owner.GetModPlayer<BloodgorgerPlayer>().TryDrain())
            {
                return;
            }

            owner.statLife = System.Math.Min(owner.statLife + 1, owner.statLifeMax2);
            owner.HealEffect(1);
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            // The ramp is the whole weapon. A drone that has just arrived is genuinely feeble.
            float fullness = RampTicks / RampTime;
            modifiers.SourceDamage *= 1f + ((MaxRamp - 1f) * fullness);
        }

        private void AnimateFrames()
        {
            Projectile.frameCounter++;
            if (Projectile.frameCounter >= 5)
            {
                Projectile.frameCounter = 0;
                Projectile.frame = (Projectile.frame + 1) % Main.projFrames[Projectile.type];
            }
        }

        private void UpdateFacing()
        {
            if (Projectile.velocity.X > 0.15f)
            {
                Projectile.spriteDirection = 1;
            }
            else if (Projectile.velocity.X < -0.15f)
            {
                Projectile.spriteDirection = -1;
            }
        }
    }
}
