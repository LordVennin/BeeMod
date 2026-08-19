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
        private const float LeaveRange = 760f;
        private const int CeilingSearchTicks = 200;
        private const int ShootInterval = 70;
        private const float StingerSpeed = 8.5f;

        private const float StateWander = 0f;
        private const float StateClimb = 1f;
        private const float StatePerch = 2f;

        private ref float State => ref NPC.ai[0];
        private ref float Timer => ref NPC.ai[1];
        private ref float PerchY => ref NPC.ai[2];
        private ref float ShootTimer => ref NPC.ai[3];

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
            NPC.lifeMax = 130;
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

            // Deliberately thin; it is meant to be an occasional nasty surprise.
            return 0.035f;
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
            else
            {
                WanderAI(player);
            }

            NPC.spriteDirection = NPC.direction = (NPC.velocity.X > 0f).ToDirectionInt();
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

            float bob = (float)System.Math.Sin(Main.GameUpdateCount * 0.08f + NPC.whoAmI) * 0.06f;
            NPC.velocity.Y += bob;
            NPC.velocity = Vector2.Clamp(NPC.velocity, new Vector2(-3f, -2.5f), new Vector2(3f, 2.5f));

            if (!Noticed(player))
            {
                return;
            }

            State = StateClimb;
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

        private void ClimbAI(Player player)
        {
            NPC.rotation = 0f;
            Timer++;

            NPC.velocity.X *= 0.9f;
            NPC.velocity.Y = -5f;

            // Roost as soon as there is something solid directly overhead.
            if (Collision.SolidCollision(NPC.position - new Vector2(0f, 6f), NPC.width, NPC.height))
            {
                State = StatePerch;
                PerchY = NPC.Center.Y;
                Timer = 0f;
                ShootTimer = 0f;
                NPC.velocity = Vector2.Zero;
                NPC.netUpdate = true;
                return;
            }

            // No ceiling worth reaching, so give up and go back to drifting.
            if (Timer > CeilingSearchTicks || !player.active || player.dead)
            {
                State = StateWander;
                Timer = 0f;
                NPC.netUpdate = true;
            }
        }

        private void PerchAI(Player player)
        {
            // Hanging from its feet.
            NPC.rotation = MathHelper.Pi;
            NPC.velocity = Vector2.Zero;
            NPC.Center = new Vector2(NPC.Center.X, PerchY);

            bool lostGrip = !Collision.SolidCollision(NPC.position - new Vector2(0f, 6f), NPC.width, NPC.height);
            bool lostPlayer = !player.active || player.dead || Vector2.Distance(NPC.Center, player.Center) > LeaveRange;

            if (lostGrip || lostPlayer)
            {
                State = StateWander;
                NPC.rotation = 0f;
                Timer = 0f;
                NPC.netUpdate = true;
                return;
            }

            ShootTimer++;
            if (ShootTimer < ShootInterval)
            {
                return;
            }

            ShootTimer = 0f;

            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                return;
            }

            Vector2 shot = (player.Center - NPC.Center).SafeNormalize(Vector2.UnitY) * StingerSpeed;
            Projectile.NewProjectile(
                NPC.GetSource_FromAI(),
                NPC.Center + new Vector2(0f, 10f),
                shot.RotatedByRandom(MathHelper.ToRadians(6f)),
                ModContent.ProjectileType<SkeletonStinger>(),
                18,
                1f,
                Main.myPlayer);

            SoundEngine.PlaySound(SoundID.Item17, NPC.Center);
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
