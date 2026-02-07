using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace VenninBeeMod.Content.NPCs
{
    public class StickyResinBee : ModNPC
    {
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 4;
        }

        public override void SetDefaults()
        {
            NPC.aiStyle = -1;
            NPC.width = 18;
            NPC.height = 18;
            NPC.damage = 3;
            NPC.defense = 2;
            NPC.lifeMax = 15;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.value = 25f;
            NPC.knockBackResist = 0.8f;
            NPC.noGravity = true;
            NPC.noTileCollide = false;
            NPC.chaseable = true;
            Banner = NPC.type;
            NPC.friendly = false;
            BannerItem = ModContent.ItemType<Items.StickyResin>();
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            if (!spawnInfo.Player.ZoneOverworldHeight)
                return 0f; // only spawn on surface

            Point playerTile = spawnInfo.Player.Center.ToTileCoordinates();
            int radius = 40;
            int sunflowerCount = 0;

            for (int x = -radius; x <= radius; x++)
            {
                for (int y = -radius; y <= radius; y++)
                {
                    int checkX = playerTile.X + x;
                    int checkY = playerTile.Y + y;

                    if (WorldGen.InWorld(checkX, checkY) && Main.tile[checkX, checkY].TileType == TileID.Sunflower)
                    {
                        sunflowerCount++;
                    }
                }
            }

            // Limit sunflower effect to 5 for balance
            sunflowerCount = Utils.Clamp(sunflowerCount, 0, 20);

            float baseSpawnChance = 0.09f; // base chance anywhere on surface
            float bonus = 0.03f * sunflowerCount;

            return baseSpawnChance + bonus;
        }

        
        public override void FindFrame(int frameHeight)
        {
            NPC.frameCounter += 0.15;
            if (NPC.frameCounter >= Main.npcFrameCount[NPC.type])
                NPC.frameCounter = 0;

            NPC.frame.Y = (int)(NPC.frameCounter) * frameHeight;
        }



        public override void AI()
        {
            Vector2 targetPos = NPC.Center;
            Point npcTile = NPC.Center.ToTileCoordinates();
            int radius = 40;
            bool foundSunflower = false;

            Vector2 currentTarget = new Vector2(NPC.localAI[0], NPC.localAI[1]);
            float distanceToTarget = Vector2.Distance(NPC.Center, currentTarget);

            // Check if previously selected flower tile is still a sunflower
            int flowerTileX = (int)(NPC.localAI[0] / 16);
            int flowerTileY = (int)(NPC.localAI[1] / 16);
            bool flowerStillValid = WorldGen.InWorld(flowerTileX, flowerTileY) &&
                                    Main.tile[flowerTileX, flowerTileY].TileType == TileID.Sunflower;

            // ... your existing AI ...

            if (distanceToTarget < 16f || NPC.localAI[2] <= 0f || !flowerStillValid)
            {
                NPC.localAI[2] = 180f;

                var flowerPositions = new List<Point>();

                for (int x = -radius; x <= radius; x++)
                {
                    for (int y = -radius; y <= radius; y++)
                    {
                        int checkX = npcTile.X + x;
                        int checkY = npcTile.Y + y;

                        if (WorldGen.InWorld(checkX, checkY) &&
                            Main.tile[checkX, checkY].TileType == TileID.Sunflower)
                        {
                            flowerPositions.Add(new Point(checkX, checkY));
                        }
                    }
                }

                if (flowerPositions.Count > 0)
                {
                    Point chosen = flowerPositions[Main.rand.Next(flowerPositions.Count)];
                    float xOffset = Main.rand.NextFloat(-6f, 6f);
                    float yOffset = Main.rand.NextFloat(-16f, -16f);
                    NPC.localAI[0] = chosen.X * 16 + 8 + xOffset;
                    NPC.localAI[1] = chosen.Y * 16 + yOffset;
                    foundSunflower = true;
                }
            }
            else
            {
                foundSunflower = flowerStillValid;
                NPC.localAI[2] -= 1f;
            }

            if (foundSunflower)
            {
                NPC.damage = 3;
                NPC.friendly = false;
                NPC.target = -1;

                targetPos = new Vector2(NPC.localAI[0], NPC.localAI[1]);
                float wiggle = (float)Math.Sin(Main.GameUpdateCount * 0.1f + NPC.whoAmI) * 0.6f;
                Vector2 hoverOffset = new Vector2(wiggle, (float)Math.Sin(Main.GameUpdateCount * 0.05f + NPC.whoAmI) * 1.2f);
                Vector2 direction = (targetPos + hoverOffset) - NPC.Center;
                float speed = 2.4f;
                NPC.velocity = Vector2.Lerp(NPC.velocity, direction.SafeNormalize(Vector2.Zero) * speed, 0.08f);
            }
            else
            {
                NPC.friendly = false;
                NPC.damage = 3;
                NPC.TargetClosest(true);

                if (NPC.HasValidTarget)
                {
                    Player player = Main.player[NPC.target];
                    Vector2 toPlayer = player.Center - NPC.Center;

                    // Aim a bit past the player's center so the bee doesn't lock perfectly inside them.
                    Vector2 attackTarget = player.Center + toPlayer.SafeNormalize(Vector2.Zero) * 22f;
                    Vector2 toAttackTarget = attackTarget - NPC.Center;

                    // Slightly slower attack movement keeps the bee from over-sticking on contact.
                    float speed = 3.5f;
                    NPC.velocity = Vector2.Lerp(NPC.velocity, toAttackTarget.SafeNormalize(Vector2.Zero) * speed, 0.1f);
                }
                else
                {
                    NPC.velocity *= 0.95f;
                }
            }

            NPC.spriteDirection = NPC.direction = (NPC.velocity.X > 0).ToDirectionInt();
        }

        public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers)
        {
            modifiers.Knockback *= 0f;
        }


        public override void OnKill()
        {
            if (Main.rand.NextFloat() < 0.75f)
                Item.NewItem(NPC.GetSource_Loot(), NPC.getRect(), ModContent.ItemType<Items.StickyResin>());

            if (Main.rand.NextFloat() < 0.50f)
                Item.NewItem(NPC.GetSource_Loot(), NPC.getRect(), ModContent.ItemType<Items.StickyResin>());

            if (Main.rand.NextFloat() < 0.30f)
                Item.NewItem(NPC.GetSource_Loot(), NPC.getRect(), ModContent.ItemType<Items.StickyResin>());
        }
    }
}
