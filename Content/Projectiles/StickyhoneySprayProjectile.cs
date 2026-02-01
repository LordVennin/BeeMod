using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VenninBeeMod.Content.Buffs;

namespace VenninBeeMod.Content.Projectiles
{
    public class StickyHoneySprayProjectile : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 1;
        }

        public override void SetDefaults()
        {
            Projectile.width = 12;
            Projectile.height = 12;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 3;
            Projectile.timeLeft = 45;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.extraUpdates = 1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 19;
            Projectile.ownerHitCheck = false;
        }

        public override void AI()
        {
            // Apply gravity
            Projectile.velocity.Y += 0.08f;

            // Light yellow glow
            Lighting.AddLight(Projectile.Center, 0.8f, 0.7f, 0.1f);

            // Dust spray effect (increased frequency)
            for (int i = 0; i < 2; i++)
            {
                if (Main.rand.NextBool(2))
                {
                    int dust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Honey);
                    Main.dust[dust].velocity *= 0.3f;
                    Main.dust[dust].scale = 1.1f;
                    Main.dust[dust].noGravity = true;
                }
            }

            Player owner = Main.player[Projectile.owner];

            for (int i = 0; i < Main.maxPlayers; i++)
            {
                Player player = Main.player[i];

                if (!player.active || player.dead || player.whoAmI == owner.whoAmI)
                    continue;

                if (Projectile.Hitbox.Intersects(player.Hitbox))
                {
                    if (HasHivePack(owner))
                        player.AddBuff(ModContent.BuffType<QueensHoney>(), 60);
                    else
                        player.AddBuff(BuffID.Honey, 120);
                }
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

        public override bool PreDraw(ref Color lightColor) => false;
    }
}
