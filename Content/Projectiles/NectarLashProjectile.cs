using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using VenninBeeMod.Content.Buffs;

namespace VenninBeeMod.Content.Projectiles
{
    /// <summary>
    /// The Nectar Lash's swing. Structure follows the vanilla whip pattern: the game drives the
    /// arc through WhipSettings and hands back the control points, and this fills in the art.
    /// </summary>
    public class NectarLashProjectile : ModProjectile
    {
        private const int TagDuration = 300;

        /// <summary>Frame layout of the strip: handle, three cord segments, then the tip.</summary>
        private const int HandleHeight = 26;
        private const int SegmentHeight = 16;
        private const int TipTop = 74;
        private const int TipHeight = 18;
        private const int FrameWidth = 10;

        /// <summary>Ticks of held channel per extra segment of reach.</summary>
        private const int ChargePerSegment = 14;
        private const int MaxChargeTime = 130;

        private float Timer
        {
            get => Projectile.ai[0];
            set => Projectile.ai[0] = value;
        }

        private float ChargeTime
        {
            get => Projectile.ai[1];
            set => Projectile.ai[1] = value;
        }

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.IsAWhip[Type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.DefaultToWhip();
        }

        /// <summary>
        /// Holding the swing feeds it: the cord grows and reaches further, and letting go early
        /// throws that away.
        /// </summary>
        public override bool PreAI()
        {
            Player owner = Main.player[Projectile.owner];

            if (!owner.channel || ChargeTime >= MaxChargeTime)
            {
                return true;
            }

            if (++ChargeTime % ChargePerSegment == 0)
            {
                Projectile.WhipSettings.Segments++;
            }

            Projectile.WhipSettings.RangeMultiplier += 1f / MaxChargeTime;

            owner.itemAnimation = owner.itemAnimationMax;
            owner.itemTime = owner.itemTimeMax;

            return false;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<NectarTag>(), TagDuration);

            // Standard whip behaviour: minions drop what they are doing and go for this one.
            Main.player[Projectile.owner].MinionAttackTargetNPC = target.whoAmI;

            // Later segments of the same swing are worth less, so the whip is a setup tool
            // rather than a damage weapon in its own right.
            Projectile.damage = (int)(Projectile.damage * 0.55f);
        }

        private void DrawCord(List<Vector2> points)
        {
            Texture2D texture = TextureAssets.FishingLine.Value;
            Rectangle frame = texture.Frame();
            Vector2 origin = new Vector2(frame.Width / 2f, 2f);

            Vector2 position = points[0];
            for (int i = 0; i < points.Count - 1; i++)
            {
                Vector2 span = points[i + 1] - points[i];
                float rotation = span.ToRotation() - MathHelper.PiOver2;
                Color colour = Lighting.GetColor(points[i].ToTileCoordinates(), new Color(236, 196, 96));
                Vector2 scale = new Vector2(1f, (span.Length() + 2f) / frame.Height);

                Main.EntitySpriteDraw(texture, position - Main.screenPosition, frame, colour,
                    rotation, origin, scale, SpriteEffects.None, 0);

                position += span;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            List<Vector2> points = new List<Vector2>();
            Projectile.FillWhipControlPoints(Projectile, points);

            DrawCord(points);

            SpriteEffects flip = Projectile.spriteDirection < 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Vector2 position = points[0];

            for (int i = 0; i < points.Count - 1; i++)
            {
                Rectangle frame = new Rectangle(0, 0, FrameWidth, HandleHeight);
                Vector2 origin = new Vector2(5f, 8f);
                float scale = 1f;

                if (i == points.Count - 2)
                {
                    // The tip swells through the middle of the swing and settles again.
                    frame.Y = TipTop;
                    frame.Height = TipHeight;

                    Projectile.GetWhipSettings(Projectile, out float flyOutTime, out int _, out float _);
                    float t = Timer / flyOutTime;
                    scale = MathHelper.Lerp(0.5f, 1.5f,
                        Utils.GetLerpValue(0.1f, 0.7f, t, true) * Utils.GetLerpValue(0.9f, 0.7f, t, true));
                }
                else if (i > 10)
                {
                    frame.Y = 58;
                    frame.Height = SegmentHeight;
                }
                else if (i > 5)
                {
                    frame.Y = 42;
                    frame.Height = SegmentHeight;
                }
                else if (i > 0)
                {
                    frame.Y = 26;
                    frame.Height = SegmentHeight;
                }

                Vector2 span = points[i + 1] - points[i];
                float rotation = span.ToRotation() - MathHelper.PiOver2;
                Color colour = Lighting.GetColor(points[i].ToTileCoordinates());

                Main.EntitySpriteDraw(texture, position - Main.screenPosition, frame, colour,
                    rotation, origin, scale, flip, 0);

                position += span;
            }

            return false;
        }
    }
}
