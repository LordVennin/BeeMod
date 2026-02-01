using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.ID;

namespace VenninBeeMod.Content
{
    public class WaxweaverPlayer : ModPlayer
    {
        public bool hasWaxweaverSet;
        private int beeTrailTimer = 0;

        public override void ResetEffects()
        {
            hasWaxweaverSet = false;
        }

        public override void PostUpdate()
        {
            if (hasWaxweaverSet && Player.velocity.Length() > 1f)
            {
                beeTrailTimer--;
                if (beeTrailTimer <= 0)
                {
                    beeTrailTimer = 8; // Adjust for faster/slower trail rate

                    Vector2 spawnPos = Player.Center + new Vector2(-Player.direction * 10f, 8f);

                    float angle = Main.rand.NextFloat(MathHelper.ToRadians(100), MathHelper.ToRadians(260)); // from ~left-up to right-up
                    Vector2 chosenVelocity = angle.ToRotationVector2() * Main.rand.NextFloat(0.8f, 2f);

                    

                    int count = 0;
                    for (int i = 0; i < Main.maxProjectiles; i++)
                    {
                        if (Main.projectile[i].active &&
                            Main.projectile[i].type == ModContent.ProjectileType<Projectiles.BeeTrail>() &&
                            Main.projectile[i].owner == Player.whoAmI)
                        {
                            count++;
                            if (count >= 40)
                                return; // don't spawn more
                        }
                    }


                    int bee = Projectile.NewProjectile(
                        Player.GetSource_FromThis(),
                        spawnPos,
                        chosenVelocity,
                        ModContent.ProjectileType<Projectiles.BeeTrail>(), // You'll create this next
                        5, // Damage
                        0f,
                        Player.whoAmI
                    );

                    if (Main.projectile.IndexInRange(bee))
                    {
                        var proj = Main.projectile[bee];
                        proj.friendly = true;
                        proj.hostile = false;
                        proj.tileCollide = false;
                        proj.timeLeft = 160;
                    }
                }
            }
        }
    }
}