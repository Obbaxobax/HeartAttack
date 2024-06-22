using ClientSideTest;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using tModPorter;
using Microsoft.Xna.Framework;
using Terraria.ID;
using HeartAttack.Content.Projectiles;
using System.Text.RegularExpressions;
using Terraria.UI;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.GameContent.UI.Elements;
using Microsoft.Xna.Framework.Graphics;
using Terraria.ModLoader.UI.Elements;
using System.Threading.Tasks;
using System;
using Terraria.Utilities;

namespace HeartAttack
{
    public class HeartAttack : ModSystem
    {
        public static int baseHeartRateLimit;

        internal void writeText(string text)
        {
            Main.NewText(text);
        }

        private bool past = false;
        private bool bloodBoiling = false;

        public override void OnWorldUnload()
        {
            //Server.server.Stop();
            Server.keepRunning = false;
        }

        public int rate;
        public override void UpdateUI(GameTime gameTime)
        {
            //Check if the client is getting data directly, or from server
            if (Server.data != null)
            {
                //If client is connected to watch, send data to server
                var resultString = Regex.Match(Server.data, @"\d+").Value;
                rate = int.Parse(resultString);

                //Create packet for sending data to server
                ModPacket packet = ModContent.GetInstance<HeartAttackMod>().GetPacket();
                packet.Write(Main.myPlayer);
                packet.Write(rate);
                packet.Send();

                //Check if heart rate exceeds threshold
                checkRates(rate);
            }
            else if (rate != 0)
            {
                //Check if heart rate exceeds threshold
                checkRates(rate);
            }
        }

        private void checkRates(int hr)
        {
            //Check if the heart rate is close to the limit
            if (hr >= ServerConfig.Instance.HeartrateLimit - 5 && bloodBoiling == false && hr < ServerConfig.Instance.HeartrateLimit)
            {
                //Send message in chat and disable until below the threshold
                bloodBoiling = true;
                Main.NewText(ServerConfig.Instance.WarningText, ServerConfig.Instance.WarningTextColor);

            }
            else if (hr < ServerConfig.Instance.HeartrateLimit - 5 && bloodBoiling == true)
            {
                bloodBoiling = false;
            }

            //Check is the heartrate is lethally low.
            if (hr >= ServerConfig.Instance.HeartrateLimit && past == false)
            {
                //Prevent infinite explosions until the heartrate falls below the threshold again.
                past = true;

                //Spawn explosion on local player
                Projectile.NewProjectile(Main.LocalPlayer.GetSource_FromThis(), Main.player[Main.myPlayer].position, new Vector2(0, 0), ModContent.ProjectileType<Explosion>(), ServerConfig.Instance.Damage, 10);
            }
            else if (hr < ServerConfig.Instance.HeartrateLimit && past == true)
            {
                //Allow for explosion to happen again if the heart rate dropped enough
                past = false;
            }
        }
    }

    class StartCommand : ModCommand
    {
        public override string Command => "start";
        public override CommandType Type => CommandType.Chat;
        public override void Action(CommandCaller caller, string input, string[] args)
        {
            //Starts the server
            Task.Run(Server.SMain);
        }
    }
    class TestCommand : ModCommand
    {
        public override string Command => "test";
        public override CommandType Type => CommandType.Chat;
        public override void Action(CommandCaller caller, string input, string[] args)
        {
            Projectile.NewProjectile(Main.LocalPlayer.GetSource_FromThis(), Main.player[Main.myPlayer].position, new Vector2(0, 0), ModContent.ProjectileType<Explosion>(), ServerConfig.Instance.Damage, 10);
        }
    }

    class Modplayer : ModPlayer
    {
        private FastRandom ran = new FastRandom(); //Get random number

        public override bool PreKill(double damage, int hitDirection, bool pvp, ref bool playSound, ref bool genDust, ref PlayerDeathReason damageSource)
        {
            if (damageSource.SourceProjectileType == ModContent.ProjectileType<Explosion>())
            {
                string deathMessage = ServerConfig.Instance.DeathMessages[ran.Next(0, ServerConfig.Instance.DeathMessages.Count)]; //Get random death message

                deathMessage = deathMessage.Replace("[PLAYER]", Main.LocalPlayer.name); //Add player name if necessary
                damageSource = PlayerDeathReason.ByCustomReason(deathMessage); //display death reason
            }

            return base.PreKill(damage, hitDirection, pvp, ref playSound, ref genDust, ref damageSource);
        }
    }
}