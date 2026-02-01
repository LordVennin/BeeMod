using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework;

namespace VenninBeeMod.Content
{
    public class QueensGuardPlayer : ModPlayer
    {
        public bool hasQueensGuardSet;

        public override void ResetEffects()
        {
            hasQueensGuardSet = false;
        }

        public override void PostUpdate()
        {
            if (hasQueensGuardSet)
            {
                bool hasBuff = Player.HasBuff(ModContent.BuffType<Buffs.QueensHoney>());

                if (Player.statLife <= Player.statLifeMax2 * 0.25f && !hasBuff)
                {
                    Player.AddBuff(ModContent.BuffType<Buffs.QueensHoney>(), 600);
                }
            }
        }

        public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (hasQueensGuardSet && item.DamageType == DamageClass.Melee && Main.myPlayer == Player.whoAmI)
            {
                TrySpawnBee(Player, target);
            }
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (hasQueensGuardSet && proj.DamageType == DamageClass.Melee && Main.myPlayer == Player.whoAmI)
            {
                TrySpawnBee(Player, target);
            }
        }

        private void TrySpawnBee(Player player, NPC target)
        {
            if (Main.rand.NextFloat() < 0.43f)
            {
                int beeCount = 1 + Main.rand.Next(2); // 2 or 3 bees
                float baseDamage = player.GetWeaponDamage(player.HeldItem);

                for (int i = 0; i < beeCount; i++)
                {
                    // Start from player center
                    Vector2 spawnPos = player.Center;

                    // Direction based on facing
                    float horizontalSpeed = Main.rand.NextFloat(4f, 6f);
                    float verticalOffset = Main.rand.NextFloat(-1f, 0.5f); // slight upward curve

                    Vector2 beeVelocity = new Vector2(player.direction * horizontalSpeed, verticalOffset);

                    Projectile.NewProjectile(
                        player.GetSource_OnHit(target),
                        spawnPos,
                        beeVelocity,
                        ProjectileID.Bee,
                        (int)(baseDamage * 0.5f),
                        0f,
                        player.whoAmI
                    );
                }
            }
        }
    }
}