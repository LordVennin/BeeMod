using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System;

namespace VenninBeeMod.Content.Projectiles
{
    public class HoneyGlobProjectile : ModProjectile
    {
        private int stuckToNPC = -1;
        private Vector2 npcOffset = Vector2.Zero;
        private int dustTimer = 0;

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 1;
        }

        public override void SetDefaults()
        {
            Projectile.width = 24;
            Projectile.height = 24;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 300;
            Projectile.ignoreWater = false;
            Projectile.tileCollide = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 20;
            Projectile.aiStyle = 0;
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (Projectile.localAI[0] == 1f) return false;

            Projectile.localAI[0] = 1f;
            Projectile.tileCollide = false;
            Projectile.velocity = Vector2.Zero;
            Projectile.netUpdate = true;

            // Burst of honey dust on impact
            for (int i = 0; i < 10; i++)
            {
                int dust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Honey);
                Main.dust[dust].velocity *= 0.6f;
                Main.dust[dust].noGravity = true;
            }

            return false;
        }

        public override void AI()
        {
            if (Projectile.localAI[0] == 0f)
            {
                // Gravity while falling
                Projectile.velocity.Y += 0.15f;
                if (Projectile.velocity.Y > 12f)
                    Projectile.velocity.Y = 12f;

                // Initial rotation setup (only runs once)
                if (Projectile.ai[1] == 0f)
                {
                    Projectile.rotation = Main.rand.NextFloat(MathHelper.TwoPi);
                    Projectile.ai[1] = Main.rand.NextBool() ? 0.05f : -0.05f; // store spin speed/direction
                }

                // Spin while airborne
                Projectile.rotation += Projectile.ai[1];
            }
            else if (Projectile.localAI[0] == 1f || Projectile.localAI[0] == 2f)
            {
                // Stop rotation when stuck
                Projectile.rotation = 0f;

                // Stick to NPC if needed
                if (Projectile.localAI[0] == 2f && stuckToNPC >= 0 && Main.npc[stuckToNPC].active)
                {
                    Projectile.position = Main.npc[stuckToNPC].position + npcOffset;
                }
                else if (Projectile.localAI[0] == 2f)
                {
                    Projectile.localAI[0] = 1f; // fallback to ground mode
                }

                // Shrink
                Projectile.velocity = Vector2.Zero;
                Projectile.scale -= 0.005f;
                if (Projectile.scale <= 0.2f)
                {
                    Projectile.Kill();
                    return;
                }

                // Dust logic (with timer)
                dustTimer++;
                Player owner = Main.player[Projectile.owner];
                float range = HasHivePack(owner) ? 90f : 60f;

                if (dustTimer >= 6)
                {
                    dustTimer = 0;

                    for (int i = 0; i < 2; i++)
                    {
                        Vector2 dustPos;

                        if (Projectile.localAI[0] == 2f && stuckToNPC >= 0 && Main.npc[stuckToNPC].active)
                        {
                            NPC target = Main.npc[stuckToNPC];
                            float x = Main.rand.NextFloat(target.width);
                            float y = Main.rand.NextFloat(target.height);
                            dustPos = new Vector2(target.position.X + x, target.position.Y + y);
                        }
                        else if (Projectile.localAI[0] == 1f)
                        {
                            Vector2 offset = Main.rand.NextVector2Circular(range, range);
                            dustPos = Projectile.Center + offset;
                        }
                        else
                        {
                            float radius = 12f * Projectile.scale;
                            Vector2 offset = Main.rand.NextVector2Circular(radius, radius);
                            dustPos = Projectile.Center + offset;
                        }

                        int dust = Dust.NewDust(dustPos, 0, 0, DustID.Honey);
                        Main.dust[dust].velocity *= 0.2f;
                        Main.dust[dust].scale = 1.2f;
                        Main.dust[dust].noGravity = true;
                    }
                }

                // Buff nearby players
                foreach (Player plr in Main.player)
                {
                    if (plr.active && plr.Distance(Projectile.Center) <= range)
                    {
                        plr.AddBuff(BuffID.Honey, 30);
                    }
                }
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Player player = Main.player[Projectile.owner];

            if (HasHivePack(player) && Projectile.localAI[0] == 0f)
            {
                stuckToNPC = target.whoAmI;
                npcOffset = Projectile.position - target.position; // store relative position
                Projectile.localAI[0] = 2f;
                Projectile.tileCollide = false;
                Projectile.velocity = Vector2.Zero;
                Projectile.netUpdate = true;
            }
            else
            {
                target.AddBuff(BuffID.Slow, 60); // fallback: just slow on hit
            }
        }

        private bool HasHivePack(Player player)
        {
            for (int i = 3; i < 10; i++) // accessory slots
            {
                if (player.armor[i].type == ItemID.HiveBackpack)
                    return true;
            }
            return false;
        }

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 8; i++)
            {
                int dust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Honey);
                Main.dust[dust].velocity *= 1.5f;
                Main.dust[dust].scale = 1.2f;
            }
        }
    }
}