using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader;

namespace VenninBeeMod.Content.NPCs
{
    public class TheSwarm : ModNPC
    {
        private const int MaxSwarmBees = 75;
        private ref float AttackTimer => ref NPC.ai[0];
        private ref float AttackState => ref NPC.ai[1];
        private ref float HasDashed => ref NPC.ai[2];

        public override string Texture => "VenninBeeMod/Content/NPCs/StickyResinBee";

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = Main.npcFrameCount[ModContent.NPCType<StickyResinBee>()];
            NPCID.Sets.MPAllowedEnemies[NPC.type] = true;
            NPCID.Sets.ShouldBeCountedAsBoss[NPC.type] = true;
        }

        public override void SetDefaults()
        {
            NPC.width = 34;
            NPC.height = 34;
            NPC.aiStyle = -1;
            NPC.damage = 16;
            NPC.defense = 3;
            NPC.lifeMax = 540;
            NPC.knockBackResist = 0f;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.value = 0f;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath14;
            NPC.boss = true;
            NPC.npcSlots = 15f;
            Music = MusicID.Boss1;
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.Add(new FlavorTextBestiaryInfoElement("A resin-fed queen mimic wrapped in a cloud of enraged bees."));
        }

        public override void FindFrame(int frameHeight)
        {
            NPC.frameCounter += 0.2f;
            if (NPC.frameCounter >= Main.npcFrameCount[NPC.type])
                NPC.frameCounter = 0;

            NPC.frame.Y = (int)NPC.frameCounter * frameHeight;
        }

        public override void AI()
        {
            NPC.TargetClosest(faceTarget: false);
            Player player = Main.player[NPC.target];
            if (!player.active || player.dead)
            {
                NPC.velocity.Y -= 0.3f;
                if (NPC.timeLeft > 10)
                    NPC.timeLeft = 10;
                return;
            }

            if (!Main.dayTime)
            {
                NPC.velocity.Y -= 0.2f;
                if (NPC.timeLeft > 60)
                    NPC.timeLeft = 60;
            }

            float lifeRatio = NPC.life / (float)NPC.lifeMax;
            SpawnSwarmBees(MaxSwarmBees);

            AttackTimer++;
            if (AttackState == 0f)
            {
                DoHoverAndShoot(player, lifeRatio);
                if (AttackTimer >= 1500f)
                {
                    AttackTimer = 0f;
                    AttackState = 1f;
                    HasDashed = 0f;
                    NPC.netUpdate = true;
                }
            }
            else
            {
                DoSwoopAttack(player, lifeRatio);
                if (AttackTimer >= 72f)
                {
                    AttackTimer = 0f;
                    AttackState = 0f;
                    HasDashed = 0f;
                    NPC.netUpdate = true;
                }
            }

            NPC.rotation = NPC.velocity.X * 0.04f;
            NPC.spriteDirection = NPC.direction = (NPC.velocity.X > 0f).ToDirectionInt();
        }

        private void DoHoverAndShoot(Player player, float lifeRatio)
        {
            NPC.damage = 10;

            float hoverHeight = -340f + (1f - lifeRatio) * 70f;
            float lateralBob = (float)System.Math.Sin((AttackTimer * 0.055f) + (NPC.whoAmI * 0.9f)) * 86f;
            float verticalBob = (float)System.Math.Cos((AttackTimer * 0.082f) + (NPC.whoAmI * 0.47f)) * 22f;

            Vector2 hoverTarget = player.Center + new Vector2(lateralBob, hoverHeight + verticalBob);
            Vector2 toTarget = hoverTarget - NPC.Center;
            float speed = MathHelper.Lerp(5.2f, 3.5f, 1f - lifeRatio);
            NPC.velocity = Vector2.Lerp(NPC.velocity, toTarget.SafeNormalize(Vector2.UnitY) * speed, 0.1f);

            int fireRate = (int)MathHelper.Lerp(70f, 46f, 1f - lifeRatio);
            if (AttackTimer % fireRate == 0f && Main.netMode != NetmodeID.MultiplayerClient)
            {
                LaunchOrbitingBees(player);

                SoundEngine.PlaySound(SoundID.Item17, NPC.Center);
            }
        }

        private void LaunchOrbitingBees(Player player)
        {
            int launches = 0;
            int maxLaunches = Main.rand.Next(1, 3);
            Vector2 launchDirection = (player.Center - NPC.Center).SafeNormalize(Vector2.UnitY);

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC bee = Main.npc[i];
                if (!bee.active || bee.type != ModContent.NPCType<SwarmBeeMinion>())
                    continue;

                if ((int)bee.ai[0] != NPC.whoAmI || bee.ai[1] == 1f)
                    continue;

                bee.ai[1] = 1f;
                bee.ai[2] = player.whoAmI;
                bee.ai[3] = 0f;
                bee.velocity = launchDirection * Main.rand.NextFloat(8f, 10f);
                bee.netUpdate = true;

                launches++;
                if (launches >= maxLaunches)
                    break;
            }
        }

        private void DoSwoopAttack(Player player, float lifeRatio)
        {
            Vector2 prepPoint = player.Center + new Vector2(player.velocity.X * 20f, -270f + lifeRatio * 80f);
            if (AttackTimer < 28f)
            {
                Vector2 toPrep = prepPoint - NPC.Center;
                NPC.velocity = Vector2.Lerp(NPC.velocity, toPrep.SafeNormalize(Vector2.UnitY) * 7.5f, 0.12f);
                NPC.damage = 8;
                return;
            }

            if (HasDashed == 0f)
            {
                Vector2 swoopTarget = player.Center + new Vector2(player.velocity.X * 18f, 32f);
                NPC.velocity = (swoopTarget - NPC.Center).SafeNormalize(Vector2.UnitY) * 14.5f;
                NPC.damage = 24;
                HasDashed = 1f;
                SoundEngine.PlaySound(SoundID.Roar, NPC.Center);
                NPC.netUpdate = true;
            }

            NPC.damage = 24;
            NPC.velocity *= 0.99f;
        }

        private void SpawnSwarmBees(int desiredSwarm)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;

            int activeBees = 0;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC other = Main.npc[i];
                if (other.active && other.type == ModContent.NPCType<SwarmBeeMinion>() && (int)other.ai[0] == NPC.whoAmI)
                    activeBees++;
            }

            if (activeBees >= desiredSwarm)
                return;

            int toSpawn = desiredSwarm - activeBees;
            for (int i = 0; i < toSpawn; i++)
            {
                Vector2 spawnOffset = Main.rand.NextVector2Circular(90f, 90f);
                int idx = NPC.NewNPC(NPC.GetSource_FromAI(), (int)(NPC.Center.X + spawnOffset.X), (int)(NPC.Center.Y + spawnOffset.Y), ModContent.NPCType<SwarmBeeMinion>(), ai0: NPC.whoAmI);
                if (idx >= 0)
                {
                    Main.npc[idx].netUpdate = true;
                }
            }
        }

        public override bool CheckDead()
        {
            if (!Main.dayTime)
                return true;

            return base.CheckDead();
        }

        public override void OnKill()
        {
            int resinAmount = Main.rand.Next(28, 45);
            Item.NewItem(NPC.GetSource_Loot(), NPC.getRect(), ModContent.ItemType<Items.StickyResin>(), resinAmount);

            int silverAmount = Main.rand.Next(3, 7);
            Item.NewItem(NPC.GetSource_Loot(), NPC.getRect(), ItemID.SilverCoin, silverAmount);
        }

        public override bool? DrawHealthBar(byte hbPosition, ref float scale, ref Vector2 position)
        {
            return null;
        }
    }
}
