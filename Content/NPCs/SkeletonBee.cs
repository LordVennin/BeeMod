using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using VenninBeeMod.Content.Buffs;
using VenninBeeMod.Content.Projectiles;

namespace VenninBeeMod.Content.NPCs
{
    /// <summary>
    /// Drifts the dungeon halls until somebody walks under it, then climbs to the ceiling, hangs
    /// there upside down and shoots down at them.
    /// </summary>
    public class SkeletonBee : ModNPC
    {
        private const float NoticeRange = 340f;
        private const float PerchTriggerRange = 420f;
        private const float LeaveRange = 620f;
        private const float GiveUpRange = 1400f;

        private const int PerchSearchSteps = 22;
        private const int ClimbTimeout = 200;
        private const int ShootInterval = 70;

        private const float ApproachSpeed = 4.2f;
        private const float ClimbSpeed = 5.5f;
        private const float StingerSpeed = 8.5f;

        private const float StateWander = 0f;
        private const float StateApproach = 1f;
        private const float StateClimb = 2f;
        private const float StatePerch = 3f;

        /// <summary>Surfaces worth gripping: overhead first, then either side.</summary>
        private static readonly Vector2[] PerchDirections =
        {
            -Vector2.UnitY,
            -Vector2.UnitX,
            Vector2.UnitX,
        };

        private ref float State => ref NPC.ai[0];
        private ref float Timer => ref NPC.ai[1];
        private ref float AnchorX => ref NPC.ai[2];
        private ref float AnchorY => ref NPC.ai[3];

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 4;
        }

        public override void SetDefaults()
        {
            NPC.width = 30;
            NPC.height = 30;
            NPC.aiStyle = -1;
            NPC.damage = 26;
            NPC.defense = 12;
            NPC.lifeMax = 70;
            NPC.knockBackResist = 0.35f;
            NPC.noGravity = true;
            NPC.noTileCollide = false;
            NPC.value = Item.buyPrice(silver: 4);
            NPC.HitSound = SoundID.NPCHit2;
            NPC.DeathSound = SoundID.NPCDeath2;
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            if (!spawnInfo.Player.ZoneDungeon || spawnInfo.PlayerInTown)
            {
                return 0f;
            }

            // Weight against a base pool entry of 1, so this is roughly one in ten of the
            // spawns the game rolls in the dungeon.
            return 0.1f;
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.Add(new FlavorTextBestiaryInfoElement(
                "Picked clean centuries ago and still hunting. It roosts on the ceiling and spits its own bones at you."));
        }

        public override void FindFrame(int frameHeight)
        {
            // Frozen mid-flap while roosting; the wings are only doing work in the air.
            if (State == StatePerch)
            {
                NPC.frame.Y = frameHeight;
                return;
            }

            NPC.frameCounter += 0.25f;
            if (NPC.frameCounter >= Main.npcFrameCount[NPC.type])
            {
                NPC.frameCounter = 0;
            }

            NPC.frame.Y = (int)NPC.frameCounter * frameHeight;
        }

        public override void AI()
        {
            NPC.TargetClosest(faceTarget: false);
            Player player = Main.player[NPC.target];

            if (State == StatePerch)
            {
                PerchAI(player);
            }
            else if (State == StateClimb)
            {
                ClimbAI(player);
            }
            else if (State == StateApproach)
            {
                ApproachAI(player);
            }
            else
            {
                WanderAI(player);
            }

            if (State != StatePerch)
            {
                NPC.spriteDirection = NPC.direction = (NPC.velocity.X > 0f).ToDirectionInt();
            }
        }

        private void WanderAI(Player player)
        {
            NPC.rotation = 0f;
            Timer++;

            // Lazy drifting, redirected every couple of seconds.
            if (Timer % 120f == 0f)
            {
                NPC.velocity = new Vector2(Main.rand.NextFloat(-2.2f, 2.2f), Main.rand.NextFloat(-1.4f, 1.4f));
                NPC.netUpdate = true;
            }

            float bob = (float)System.Math.Sin((Main.GameUpdateCount * 0.08f) + NPC.whoAmI) * 0.06f;
            NPC.velocity.Y += bob;
            NPC.velocity = Vector2.Clamp(NPC.velocity, new Vector2(-3f, -2.5f), new Vector2(3f, 2.5f));

            if (!Noticed(player))
            {
                return;
            }

            State = StateApproach;
            Timer = 0f;
            NPC.netUpdate = true;
        }

        private bool Noticed(Player player)
        {
            if (!player.active || player.dead)
            {
                return false;
            }

            if (Vector2.Distance(NPC.Center, player.Center) > NoticeRange)
            {
                return false;
            }

            return Collision.CanHitLine(NPC.position, NPC.width, NPC.height, player.position, player.width, player.height);
        }

        /// <summary>
        /// Closes on the player in the open, looking for something to grip once it is near
        /// enough. This is also where a bee that lost its perch ends up, so getting away from one
        /// only buys you the time it takes to fly back.
        /// </summary>
        private void ApproachAI(Player player)
        {
            NPC.rotation = 0f;

            if (!player.active || player.dead)
            {
                State = StateWander;
                Timer = 0f;
                return;
            }

            Timer++;

            Vector2 toPlayer = player.Center - NPC.Center;
            float distance = toPlayer.Length();

            NPC.velocity = Vector2.Lerp(NPC.velocity, toPlayer.SafeNormalize(Vector2.UnitX) * ApproachSpeed, 0.07f);

            if (distance > GiveUpRange)
            {
                State = StateWander;
                Timer = 0f;
                NPC.netUpdate = true;
                return;
            }

            // Only look for a surface every so often; the scan is not worth running every tick.
            if (distance <= PerchTriggerRange && Timer % 15f == 0f && TryFindPerch(out Vector2 anchor))
            {
                AnchorX = anchor.X;
                AnchorY = anchor.Y;
                State = StateClimb;
                Timer = 0f;
                NPC.netUpdate = true;
            }
        }

        private void ClimbAI(Player player)
        {
            NPC.rotation = 0f;
            Timer++;

            Vector2 anchor = new Vector2(AnchorX, AnchorY);
            Vector2 toAnchor = anchor - NPC.Center;

            NPC.velocity = Vector2.Lerp(NPC.velocity, toAnchor.SafeNormalize(-Vector2.UnitY) * ClimbSpeed, 0.16f);

            // Grip whatever it reaches first, whether that is the chosen spot or a surface it
            // brushed on the way.
            if (toAnchor.Length() < 12f || GripDirection() != Vector2.Zero)
            {
                State = StatePerch;
                AnchorX = NPC.Center.X;
                AnchorY = NPC.Center.Y;
                Timer = 0f;
                NPC.velocity = Vector2.Zero;
                NPC.netUpdate = true;
                return;
            }

            if (Timer > ClimbTimeout || !player.active || player.dead)
            {
                State = StateApproach;
                Timer = 0f;
                NPC.netUpdate = true;
            }
        }

        private void PerchAI(Player player)
        {
            Vector2 grip = GripDirection();

            bool lostPlayer = !player.active || player.dead || Vector2.Distance(NPC.Center, player.Center) > LeaveRange;

            if (grip == Vector2.Zero || lostPlayer)
            {
                // Peel off and go after them rather than sitting on a wall out of range.
                State = player.active && !player.dead ? StateApproach : StateWander;
                NPC.rotation = 0f;
                Timer = 0f;
                NPC.netUpdate = true;
                return;
            }

            // Hangs off whichever surface it caught, so a wall grip lies on its side and a
            // ceiling grip hangs upside down.
            NPC.rotation = grip.ToRotation() - MathHelper.PiOver2;
            NPC.velocity = Vector2.Zero;
            NPC.Center = new Vector2(AnchorX, AnchorY);

            Timer++;
            if (Timer < ShootInterval)
            {
                return;
            }

            Timer = 0f;

            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                return;
            }

            Vector2 muzzle = NPC.Center - (grip * 12f);
            Vector2 shot = (player.Center - muzzle).SafeNormalize(Vector2.UnitY) * StingerSpeed;

            Projectile.NewProjectile(
                NPC.GetSource_FromAI(),
                muzzle,
                shot.RotatedByRandom(MathHelper.ToRadians(6f)),
                ModContent.ProjectileType<SkeletonStinger>(),
                18,
                1f,
                Main.myPlayer);

            SoundEngine.PlaySound(SoundID.Item17, NPC.Center);
        }

        /// <summary>
        /// Which way the surface it is holding lies, or zero if it is not touching one. Doubles
        /// as the grip check, so mining the wall out drops it immediately.
        /// </summary>
        private Vector2 GripDirection()
        {
            foreach (Vector2 dir in PerchDirections)
            {
                if (Collision.SolidCollision(NPC.position + (dir * 6f), NPC.width, NPC.height))
                {
                    return dir;
                }
            }

            return Vector2.Zero;
        }

        /// <summary>
        /// Nearest solid surface overhead or to either side, as a point to fly to.
        /// </summary>
        private bool TryFindPerch(out Vector2 anchor)
        {
            anchor = Vector2.Zero;
            float best = float.MaxValue;

            foreach (Vector2 dir in PerchDirections)
            {
                for (int step = 1; step <= PerchSearchSteps; step++)
                {
                    float reach = step * 8f;
                    if (!Collision.SolidCollision(NPC.position + (dir * reach), NPC.width, NPC.height))
                    {
                        continue;
                    }

                    if (reach < best)
                    {
                        best = reach;
                        anchor = NPC.Center + (dir * (reach - 8f));
                    }

                    break;
                }
            }

            return best < float.MaxValue;
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo hurtInfo)
        {
            target.AddBuff(ModContent.BuffType<BoneRot>(), BoneRot.Duration);
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Items.TatteredBeeHide>(), 1, 1, 3));

            // The good bit, and the reason to keep hunting them.
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Items.HardenedBeeStinger>(), 10));
        }
    }
}
