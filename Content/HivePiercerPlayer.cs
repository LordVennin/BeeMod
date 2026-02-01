using Terraria;
using Terraria.ModLoader;
using System;
using Microsoft.Xna.Framework;

namespace VenninBeeMod.Content
{
    public class HivepiercerPlayer : ModPlayer
    {
        public bool hiveHornetActive;

        public bool hasHiveMind;

        public Vector2 syncedAimDirection = Vector2.UnitX;

        public override void PreUpdate()
        {
            if (Main.myPlayer == Player.whoAmI)
            {
                syncedAimDirection = (Main.MouseWorld - Player.Center).SafeNormalize(Vector2.UnitX);
            }
        }

        public int hornetShootCooldown;

        public override void PostUpdate()
        {
            if (hornetShootCooldown > 0)
                hornetShootCooldown--;
        }

        public override void ResetEffects()
        {
            hasHiveMind = false; // Reset every tick
        }

        public override void UpdateDead()
        {
            hasHiveMind = false; // Ensure flag is off on death
        }

    }
}
