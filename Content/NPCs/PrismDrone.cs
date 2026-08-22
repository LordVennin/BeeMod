using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using VenninBeeMod.Content.Projectiles;

namespace VenninBeeMod.Content.NPCs
{
    /// <summary>
    /// A bee that went over to the Hallow when hardmode broke the world open. It keeps its
    /// distance and works you over with refracted shards rather than closing in, which is what
    /// makes it different to hunt from the mod's other bees.
    /// </summary>
    public class PrismDrone : ModNPC
    {
        private const float HoverRange = 260f;
        private const float HoverSpeed = 6.5f;
        private const int ShootInterval = 100;
        private const int VolleySize = 3;
        private const float VolleySpread = 16f;
        private const float ShardSpeed = 9.5f;
        private const float GiveUpRange = 1600f;

        private ref float Timer => ref NPC.ai[0];
        private ref float Drift => ref NPC.ai[1];

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 4;
        }

        public override void SetDefaults()
        {
            NPC.width = 32;
            NPC.height = 26;
            NPC.aiStyle = -1;
            NPC.damage = 34;
            NPC.defense = 16;
            NPC.lifeMax = 150;
            NPC.knockBackResist = 0.4f;
            NPC.noGravity = true;
            NPC.noTileCollide = false;
            NPC.value = Item.buyPrice(silver: 30);
            NPC.HitSound = SoundID.NPCHit4;
            NPC.DeathSound = SoundID.NPCDeath6;
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            // Hardmode only, and only where the Hallow reaches - which is what makes this a
            // reason to go somewhere rather than a reskin of the surface bee.
            if (!Main.hardMode || !spawnInfo.Player.ZoneHallow || spawnInfo.PlayerInTown)
            {
                return 0f;
            }

            return 0.14f;
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.Add(new FlavorTextBestiaryInfoElement(
                "Its comb set into crystal when the Hallow took the hive. It has been splitting light ever since."));
        }

        public override void FindFrame(int frameHeight)
        {
            NPC.frameCounter += 0.3f;
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

            Lighting.AddLight(NPC.Center, 0.3f, 0.45f, 0.6f);

            if (!player.active || player.dead || Vector2.Distance(NPC.Center, player.Center) > GiveUpRange)
            {
                NPC.velocity.Y -= 0.1f;
                if (NPC.timeLeft > 60)
                {
                    NPC.timeLeft = 60;
                }

                return;
            }

            Timer++;
            Drift += 0.03f;

            // Holds station off to one side at range and slides around rather than charging.
            float side = (float)System.Math.Sin(Drift + NPC.whoAmI) * HoverRange;
            float lift = -70f + ((float)System.Math.Cos(Drift * 1.4f) * 46f);
            Vector2 station = player.Center + new Vector2(side, lift);

            Vector2 toStation = station - NPC.Center;
            NPC.velocity = Vector2.Lerp(NPC.velocity, toStation.SafeNormalize(Vector2.UnitY) * HoverSpeed, 0.06f);

            NPC.rotation = NPC.velocity.X * 0.03f;
            NPC.spriteDirection = NPC.direction = (NPC.velocity.X > 0f).ToDirectionInt();

            if (Timer % ShootInterval == 0f && Main.netMode != NetmodeID.MultiplayerClient)
            {
                FireVolley(player);
            }

            if (Main.rand.NextBool(8))
            {
                Dust shimmer = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height,
                    DustID.RainbowMk2, 0f, 0f, 140, new Color(190, 230, 255), 0.7f);
                shimmer.noGravity = true;
            }
        }

        private void FireVolley(Player player)
        {
            Vector2 aim = (player.Center - NPC.Center).SafeNormalize(Vector2.UnitY);

            for (int i = 0; i < VolleySize; i++)
            {
                float lean = MathHelper.Lerp(-VolleySpread, VolleySpread, i / (float)(VolleySize - 1));
                Vector2 velocity = aim.RotatedBy(MathHelper.ToRadians(lean)) * ShardSpeed;

                Projectile.NewProjectile(
                    NPC.GetSource_FromAI(),
                    NPC.Center,
                    velocity,
                    ModContent.ProjectileType<PrismShard>(),
                    22,
                    2f,
                    Main.myPlayer);
            }

            SoundEngine.PlaySound(SoundID.Item27 with { Volume = 0.6f }, NPC.Center);
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Items.Prismwax>(), 1, 1, 3));
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Items.FracturedStinger>(), 8));
        }
    }
}
