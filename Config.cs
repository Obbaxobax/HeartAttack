using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.ComponentModel;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;

namespace ClientSideTest
{
    public class ServerConfig : ModConfig
    {
        public override ConfigScope Mode => ConfigScope.ServerSide;

        public static ServerConfig Instance => ModContent.GetInstance<ServerConfig>();


        [Header("ImportantValues")]

        [DefaultValue(3476)]
        [Description("Port used for the websocket.")]
        public int Port;

        [DefaultValue(90)]
        [Description("The value which will kill the player[s] if exceeded.")]
        public int HeartrateLimit;


        [Header("ExplosionConfig")]

        [Range(1, 100000)]
        [DefaultValue(2000)]
        [Description("The damage of the explosion.")]
        public int Damage;

        [DefaultValue(true)]
        [Description("Whether the explosion may also damage enemies or not.")]
        public bool KillEnemies;

        [DefaultValue(false)]
        [Description("Whether the explosion may also damage npc's or not.")]
        public bool KillNPCs;

        [DefaultValue(200)]
        [Range(1, 2000)]
        [Description("Size of the explosion. (I.E. 200 by 200)")]
        public int Size;


        [Header("Customization")]

        [DefaultValue("Your blood begins to boil.")]
        [Description("Text to be displayed when within 5 BPMs of death.")]
        public string WarningText;

        public Color WarningTextColor = Color.Red;

        [Description("Use [PLAYER] to display the name of the player.")]
        public List<string> DeathMessages = new List<string>() { "[PLAYER] blew up" };

        //Allow anyone to change the config
        public override bool AcceptClientChanges(ModConfig pendingConfig, int whoAmI, ref NetworkText message)
        {
            return true;
        }
    }
}
