using Newtonsoft.Json.Linq;
using OBSWebsocketDotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace StreamDeck
{
    public class OBSHandler
    {
        public struct OBSSettings
        {
            public int port;
            public string password;
        }

        public static OBSWebsocket obs;
        public static int port;
        public static string password;

        public OBSHandler(OBSSettings settings)
        {
            port = settings.port;
            password = settings.password;

            obs = new OBSWebsocket();
            obs.Disconnected += Obs_Disconnected;
        }

        private void Obs_Disconnected(object sender, EventArgs e)
        {
            try
            {
                Task.Run(() => InitConnect());
            }
            catch (TaskCanceledException)
            {
                //Yes cancel it bitch
            }
        }

        public static void ReadRequest(string requestArg)
        {
            if (obs.IsConnected)
            {
                if (requestArg.Contains("[StartStopStreaming]"))
                {
                    obs.ToggleStreaming();
                }
                else if (requestArg.Contains("[StudioModeToggle]"))
                {
                    obs.ToggleStudioMode();
                }
                else if (requestArg.Contains("[SetScene]"))
                {
                    obs.SetCurrentScene(requestArg.RemoveString("[OBS][SetScene]"));
                }
            }
        }

        public static bool CheckServerUp(int port)
        {
            try
            {
                using (var tcp = new System.Net.Sockets.TcpClient(WebServer.getIP(), port))
                {
                    return true;
                }
            }
            catch (System.Net.Sockets.SocketException)
            {
                return false;
            }
        }

        public static void InitConnect()
        {
            obs.WSTimeout = TimeSpan.FromSeconds(5);
            obs.Connect("ws://" + WebServer.getIP() + ":" + port, (password ?? null));
        }
    }
}
