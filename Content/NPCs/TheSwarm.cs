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
        private const int MaxSwarmBees = 16;
        private ref float AttackTimer => ref NPC.ai[0];
        private ref float AttackState => ref NPC.ai[1];

        public override string Texture => "VenninBeeMod/Content/NPCs/StickyResinBee";

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = Main.npcFrameCount[ModContent.NPCType<StickyResinBee>()];
            NPCID.Sets.MPAllowedEnemies[NPC.type] = true;
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
            int desiredSwarm = (int)MathHelper.Lerp(4f, MaxSwarmBees, lifeRatio);
            NPC.localAI[0] = desiredSwarm;
            SpawnSwarmBees(desiredSwarm);

            AttackTimer++;
            if (AttackState == 0f)
            {
                DoHoverAndShoot(player, lifeRatio);
                if (AttackTimer >= 150f)
                {
                    AttackTimer = 0f;
                    AttackState = 1f;
                    NPC.netUpdate = true;
                }
            }
            else
            {
                DoChargeAttack(player);
                if (AttackTimer >= 45f)
                {
                    AttackTimer = 0f;
                    AttackState = 0f;
                    NPC.netUpdate = true;
                }
            }

            NPC.rotation = NPC.velocity.X * 0.04f;
            NPC.spriteDirection = NPC.direction = (NPC.velocity.X > 0f).ToDirectionInt();
        }

        private void DoHoverAndShoot(Player player, float lifeRatio)
        {
            NPC.damage = 8;

            Vector2 hoverTarget = player.Center + new Vector2(player.direction * -140f, -120f + (1f - lifeRatio) * 90f);
            Vector2 toTarget = hoverTarget - NPC.Center;
            float speed = MathHelper.Lerp(4f, 2.3f, 1f - lifeRatio);
            NPC.velocity = Vector2.Lerp(NPC.velocity, toTarget.SafeNormalize(Vector2.UnitY) * speed, 0.08f);

            int fireRate = (int)MathHelper.Lerp(36f, 24f, 1f - lifeRatio);
            if (AttackTimer % fireRate == 0f && Main.netMode != NetmodeID.MultiplayerClient)
            {
                Vector2 vel = (player.Center - NPC.Center).SafeNormalize(Vector2.UnitX) * 5.5f;
                Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, vel, ModContent.ProjectileType<Projectiles.SwarmResinBeeProjectile>(), 12, 1f);
                SoundEngine.PlaySound(SoundID.Item17, NPC.Center);
            }
        }

        private void DoChargeAttack(Player player)
        {
            if (AttackTimer == 1f)
            {
                Vector2 dashVelocity = (player.Center - NPC.Center).SafeNormalize(Vector2.UnitY) * 11f;
                NPC.velocity = dashVelocity;
                SoundEngine.PlaySound(SoundID.Roar, NPC.Center);
            }

            NPC.damage = 20;
            NPC.velocity *= 0.985f;
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

            int toSpawn = Utils.Clamp(desiredSwarm - activeBees, 0, 2);
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
    }
}
