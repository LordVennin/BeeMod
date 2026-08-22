using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VenninBeeMod.Content.Buffs
{
    /// <summary>
    /// The Nectar Lash's tag. Standard whip fare, except that in this mod it also pays out on
    /// bees, which are mostly not minions and would otherwise be left out entirely.
    /// </summary>
    public class NectarTag : ModBuff
    {
        public const int TagDamage = 6;

        /// <summary>How much of its own movement a coated enemy loses each frame.</summary>
        public const float SlowFactor = 0.22f;

        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = true;

            // Tag buffs are allowed onto enemies that shrug off ordinary debuffs, which is what
            // keeps a whip working against bosses.
            BuffID.Sets.IsATagBuff[Type] = true;
        }
    }
}
