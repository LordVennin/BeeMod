using System.Collections.Generic;
using Terraria.ModLoader;
using VenninBeeMod.Content.Items;
using VenninBeeMod.Content.NPCs;

namespace VenninBeeMod.Content
{
    /// <summary>
    /// Boss Checklist does not pick bosses up from NPC.boss on its own; every entry has to be
    /// registered through its Mod.Call API, which is what this does.
    /// </summary>
    public class BossChecklistSupport : ModSystem
    {
        // Roughly between King Slime (1f) and Eye of Cthulhu (2f) on Boss Checklist's scale.
        private const float ProgressionValue = 1.5f;

        public override void PostSetupContent()
        {
            if (!ModLoader.TryGetMod("BossChecklist", out Mod bossChecklist))
            {
                return;
            }

            bossChecklist.Call(
                "LogBoss",
                Mod,
                nameof(TheSwarm),
                ProgressionValue,
                () => SwarmWorldSystem.downedTheSwarm,
                ModContent.NPCType<TheSwarm>(),
                new Dictionary<string, object>
                {
                    ["spawnItems"] = ModContent.ItemType<SwarmEffigy>(),
                    ["collectibles"] = new List<int> { ModContent.ItemType<StrayQueenBee>() },
                });
        }
    }
}
