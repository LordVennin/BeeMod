using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VenninBeeMod.Content.Items
{
    public class StingerAmmoGlobalItem : GlobalItem
    {
        public override void SetDefaults(Item entity)
        {
            if (entity.type == ItemID.Stinger)
            {
                entity.ammo = ItemID.Stinger;
            }
        }
    }
}
