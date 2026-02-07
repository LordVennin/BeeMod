using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VenninBeeMod.Content.NPCs
{
    public class SwarmBeeMinion : ModNPC
    {
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
            NPC.defense = 0;
            NPC.lifeMax = 22;
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

            Player player = Main.player[boss.target];
            if (!player.active || player.dead)
                player = Main.player[Player.FindClosest(NPC.Center, NPC.width, NPC.height)];

            bool launched = NPC.ai[1] == 1f;

            if (launched)
            {
                NPC.localAI[1]++;
                if (NPC.localAI[1] > 90f)
                    NPC.ai[1] = 0f;

                Vector2 chase = (player.Center - NPC.Center).SafeNormalize(Vector2.UnitY) * 8.5f;
                NPC.velocity = Vector2.Lerp(NPC.velocity, chase, 0.1f);
                NPC.spriteDirection = NPC.direction = (NPC.velocity.X > 0f).ToDirectionInt();
                NPC.timeLeft = 180;
                return;
            }

            if (NPC.localAI[0] == 0f)
            {
                NPC.localAI[0] = Main.rand.NextFloat(24f, 56f);
                NPC.netUpdate = true;
            }

            float orbitAngle = (Main.GameUpdateCount * 0.11f) + (NPC.whoAmI * 0.45f);
            float orbitRadius = NPC.localAI[0];
            Vector2 orbitPoint = boss.Center + orbitAngle.ToRotationVector2() * orbitRadius;

            Vector2 aggressivePoint = Vector2.Lerp(orbitPoint, player.Center, 0.2f);
            Vector2 toPoint = aggressivePoint - NPC.Center;
            NPC.velocity = Vector2.Lerp(NPC.velocity, toPoint.SafeNormalize(Vector2.UnitX) * 6.8f, 0.12f);

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
