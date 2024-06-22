using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Terraria.ModLoader;
using Terraria;
using HeartAttack.Content.Projectiles;
using Microsoft.Xna.Framework;
using ClientSideTest;

namespace HeartAttack
{
    class Server
    {
        public static string data { get; set; }
        public static TcpListener server;
        public static bool keepRunning = true;

        public static async Task SMain()
        {
            string ip = "0.0.0.0";
            int port = ServerConfig.Instance.Port;
            server = new TcpListener(IPAddress.Parse(ip), port);

            await Task.Delay(2000);

            keepRunning = true;

            try
            {
                server.Start();
            }
            catch
            {
                ModContent.GetInstance<HeartAttack>().writeText("Restarting Server");
                server.Stop();
                await Task.Delay(1000);
                server.Start();
            }

            ModContent.GetInstance<HeartAttack>().writeText("Server Started");

            while (keepRunning)
            {
                try
                {
                    TcpClient client = await server.AcceptTcpClientAsync();
                    //Console.WriteLine("A client connected.");

                    _ = Task.Run(() => HandleClientAsync(client)); // Handle client asynchronously
                }
                catch (Exception ex)
                {
                    ModContent.GetInstance<HeartAttack>().writeText($"An error occurred while accepting client connection: {ex.Message}");
                }
            }

            server.Stop();

            ModContent.GetInstance<HeartAttack>().writeText("Script Ended");
        }

        private static async Task HandleClientAsync(TcpClient client)
        {
            try
            {
                NetworkStream stream = client.GetStream();

                while (client.Connected) // Continue processing as long as client is connected
                {
                    byte[] buffer = new byte[4096];
                    int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);

                    string receivedMessage = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                    // Process the received message
                    if (receivedMessage.Contains("{"))
                    {
                        Console.WriteLine(receivedMessage.Split('{')[1].Split('}')[0]);

                        data = receivedMessage.Split('{')[1].Split('}')[0];
                        // "data":"heartrate":"##"

                        // Send a response to indicate readiness for more data
                        byte[] okResponse = Encoding.UTF8.GetBytes("HTTP/1.1 200 OK\r\n\r\n");
                        await stream.WriteAsync(okResponse, 0, okResponse.Length);

                        client.Close();
                    }

                    // No data received, wait before checking again
                    await Task.Delay(1000);
                }
            }
            catch (Exception ex)
            {
                ModContent.GetInstance<HeartAttack>().writeText($"An error occurred: {ex.Message}");
            }
            finally
            {
                if (client != null)
                {
                    client.Close();
                    //Console.WriteLine("Client disconnected.");
                }
            }
        }
    }
}
