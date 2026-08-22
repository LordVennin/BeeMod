using System.Collections.Generic;
using Terraria.ModLoader;

namespace VenninBeeMod.Content
{
    /// <summary>
    /// The set of projectile types that are bees, collected once at load from everything that
    /// declares <see cref="IBeeProjectile"/>.
    /// </summary>
    public class BeeProjectiles : ModSystem
    {
        private static readonly HashSet<int> types = new HashSet<int>();

        public static bool IsBee(int type)
        {
            return types.Contains(type);
        }

        public override void PostSetupContent()
        {
            types.Clear();

            foreach (ModProjectile projectile in Mod.GetContent<ModProjectile>())
            {
                if (projectile is IBeeProjectile)
                {
                    types.Add(projectile.Type);
                }
            }
        }

        public override void Unload()
        {
            types.Clear();
        }
    }
}
