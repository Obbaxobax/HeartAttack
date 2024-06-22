using System.IO;
using Terraria.ID;
using Terraria;
using Terraria.ModLoader;

namespace HeartAttack
{
    class HeartAttackMod : Mod
    {
        public override void HandlePacket(BinaryReader reader, int whoAmI)
        {
            //Data is always sent by the one with the watch.
            //Data will never be received by the one with the watch.
            //fromWho will always be the one with the watch.

            //Read who the data is from, and what the heartrate is
            int fromWho = reader.ReadInt32();
            int heartRate = reader.ReadInt32();

            if(Main.netMode == NetmodeID.Server)
            {
                //If the receiver is the server, send the data to all clients except the one who has the watch.
                ModPacket packet = ModContent.GetInstance<HeartAttackMod>().GetPacket();
                packet.Write(fromWho);
                packet.Write(heartRate);
                packet.Send(-1, fromWho);
            }
            else
            {
                //If not the server, update the heartrate in the main script.
                ModContent.GetInstance<HeartAttack>().rate = heartRate;
            }
        }
    }
}
