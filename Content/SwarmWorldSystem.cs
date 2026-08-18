using System.IO;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace VenninBeeMod.Content
{
    /// <summary>
    /// Tracks whether The Swarm has been beaten. Boss Checklist and similar mods need a
    /// persistent downed flag to read; the vanilla NPC.boss field only marks the fight itself.
    /// </summary>
    public class SwarmWorldSystem : ModSystem
    {
        public static bool downedTheSwarm;

        public override void OnWorldLoad()
        {
            downedTheSwarm = false;
        }

        public override void OnWorldUnload()
        {
            downedTheSwarm = false;
        }

        public override void SaveWorldData(TagCompound tag)
        {
            if (downedTheSwarm)
            {
                tag["downedTheSwarm"] = true;
            }
        }

        public override void LoadWorldData(TagCompound tag)
        {
            downedTheSwarm = tag.ContainsKey("downedTheSwarm");
        }

        public override void NetSend(BinaryWriter writer)
        {
            BitsByte flags = new BitsByte();
            flags[0] = downedTheSwarm;
            writer.Write(flags);
        }

        public override void NetReceive(BinaryReader reader)
        {
            BitsByte flags = reader.ReadByte();
            downedTheSwarm = flags[0];
        }
    }
}
