using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VenninBeeMod.Content.NPCs
{
    public class SwarmBeeMinion : ModNPC
    {
        private const int OrbitingDefense = 10;
        private const int LaunchedDefense = -4;

        public override string Texture => "VenninBeeMod/Content/NPCs/StickyResinBee";

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = Main.npcFrameCount[ModContent.NPCType<StickyResinBee>()];
        }

        public override void SetDefaults()
        {
            NPC.width = 18;
            NPC.height = 18;
            NPC.aiStyle = -1;
            NPC.damage = 10;
            NPC.defense = OrbitingDefense;
            NPC.lifeMax = 19;
            NPC.knockBackResist = 0.6f;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.value = 0f;
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
            int bossIndex = (int)NPC.ai[0];
            if (bossIndex < 0 || bossIndex >= Main.maxNPCs)
            {
                NPC.active = false;
                return;
            }

            NPC boss = Main.npc[bossIndex];
            if (!boss.active || boss.type != ModContent.NPCType<TheSwarm>())
            {
                NPC.active = false;
                return;
            }

            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                int serverTarget = (int)NPC.ai[2];
                if (serverTarget < 0 || serverTarget >= Main.maxPlayers)
                    serverTarget = boss.target;

                Player serverTargetPlayer = Main.player[serverTarget];
                if (!serverTargetPlayer.active || serverTargetPlayer.dead)
                    serverTarget = Player.FindClosest(NPC.Center, NPC.width, NPC.height);

                if ((int)NPC.ai[2] != serverTarget)
                {
                    NPC.ai[2] = serverTarget;
                    NPC.netUpdate = true;
                }
            }

            int targetPlayerIndex = (int)NPC.ai[2];
            if (targetPlayerIndex < 0 || targetPlayerIndex >= Main.maxPlayers)
                targetPlayerIndex = boss.target;

            Player targetPlayer = Main.player[targetPlayerIndex];
            if (!targetPlayer.active || targetPlayer.dead)
                targetPlayer = Main.player[Player.FindClosest(NPC.Center, NPC.width, NPC.height)];

            bool launched = NPC.ai[1] == 1f;
            NPC.defense = launched ? LaunchedDefense : OrbitingDefense;

            if (launched)
            {
                NPC.ai[3]++;

                bool returningToBoss = NPC.ai[3] > 45f;
                if (NPC.ai[3] > 90f)
                {
                    NPC.ai[1] = 0f;
                    NPC.ai[3] = 0f;
                    NPC.netUpdate = true;
                }

                Vector2 flightTarget = returningToBoss ? boss.Center : targetPlayer.Center;
                float flightSpeed = returningToBoss ? 10f : 8.5f;
                float flightInertia = returningToBoss ? 0.18f : 0.1f;
                Vector2 chase = (flightTarget - NPC.Center).SafeNormalize(Vector2.UnitY) * flightSpeed;
                NPC.velocity = Vector2.Lerp(NPC.velocity, chase, flightInertia);
                NPC.spriteDirection = NPC.direction = (NPC.velocity.X > 0f).ToDirectionInt();
                NPC.timeLeft = 180;
                return;
            }

            float time = Main.GameUpdateCount;
            float baseAngle = (time * 0.075f) + (NPC.whoAmI * 0.13f);
            float chaosAngle = baseAngle + (float)System.Math.Sin((time * 0.11f) + (NPC.whoAmI * 0.37f)) * 0.85f;
            float radiusNoise = (float)System.Math.Sin((time * 0.09f) + (NPC.whoAmI * 1.21f));
            float radiusPulse = (float)System.Math.Cos((time * 0.16f) + (NPC.whoAmI * 0.73f)) * 6f;
            float radius = MathHelper.Clamp(22f + (radiusNoise * 18f) + radiusPulse, 10f, 46f);

            Vector2 swarmPoint = boss.Center + chaosAngle.ToRotationVector2() * radius;
            Vector2 toPoint = swarmPoint - NPC.Center;
            float seekSpeed = 6.2f + ((NPC.whoAmI % 7) * 0.2f);
            float inertia = 0.18f + ((NPC.whoAmI % 5) * 0.02f);
            NPC.velocity = Vector2.Lerp(NPC.velocity, toPoint.SafeNormalize(Vector2.UnitX) * seekSpeed, inertia);

            NPC.spriteDirection = NPC.direction = (NPC.velocity.X > 0f).ToDirectionInt();
            NPC.timeLeft = 60;
        }

        public override void OnKill()
        {
            int bossIndex = (int)NPC.ai[0];
            if (bossIndex < 0 || bossIndex >= Main.maxNPCs)
                return;

            NPC boss = Main.npc[bossIndex];
            if (!boss.active || boss.type != ModContent.NPCType<TheSwarm>())
                return;

            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                boss.SimpleStrikeNPC(18, 0);
                boss.netUpdate = true;
            }
        }
    }
}
