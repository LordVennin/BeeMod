using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.UI.BigProgressBar;
using Terraria.ModLoader;

namespace VenninBeeMod.Content.NPCs
{
    /// <summary>
    /// The big screen-bottom health bar used by <see cref="TheSwarm"/>.
    /// No custom bar texture is supplied, so tModLoader falls back to the vanilla
    /// bar frame and we only re-tint it and swap the icon for a buzzing bee.
    /// </summary>
    public class TheSwarmBossBar : ModBossBar
    {
        private const int IconFrameCount = 4;
        private const int IconFrameSize = 16;
        private const int IconFrameTicks = 5;

        private static readonly Color HealthyHoney = new Color(255, 224, 130);
        private static readonly Color EnragedHoney = new Color(255, 122, 68);

        private Asset<Texture2D> iconTexture;

        public override Asset<Texture2D> GetIconTexture(ref Rectangle? iconFrame)
        {
            iconTexture ??= ModContent.Request<Texture2D>("VenninBeeMod/Content/NPCs/StickyResinBee", AssetRequestMode.ImmediateLoad);

            // Cycle the bee sprite so the icon keeps buzzing while the fight is live.
            int frame = (int)(Main.GameUpdateCount / IconFrameTicks) % IconFrameCount;
            iconFrame = new Rectangle(0, frame * IconFrameSize, IconFrameSize, IconFrameSize);

            return iconTexture;
        }

        public override bool? ModifyInfo(ref BigProgressBarInfo info, ref float life, ref float lifeMax, ref float shield, ref float shieldMax)
        {
            NPC npc = Main.npc[info.npcIndexToAimAt];
            if (!npc.active || npc.type != ModContent.NPCType<TheSwarm>())
                return false;

            life = Utils.Clamp(npc.life, 0f, npc.lifeMax);
            lifeMax = npc.lifeMax;
            return true;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, NPC npc, ref BossBarDrawParams drawParams)
        {
            float lifeRatio = drawParams.LifeMax > 0f ? MathHelper.Clamp(drawParams.Life / drawParams.LifeMax, 0f, 1f) : 0f;

            // Honey gold while the swarm is thick, angry orange once it has been thinned out.
            Color barColor = Color.Lerp(EnragedHoney, HealthyHoney, lifeRatio);

            // Agitated shimmer over the last fifth of the fight.
            if (lifeRatio < 0.2f)
            {
                float pulse = 0.85f + (float)System.Math.Sin(Main.GameUpdateCount * 0.2f) * 0.15f;
                barColor = Color.Lerp(barColor, Color.White, 1f - pulse);
            }

            drawParams.BarColor = barColor;
            drawParams.IconColor = Color.White;

            // The bee frame is 16x16, the icon slot is 26x28.
            drawParams.IconScale = 1.5f;

            return true;
        }
    }
}
