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

        public static OBSWebsocket obs = new OBSWebsocket();

        public static void ReadRequest(string requestArg)
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
                //if (!obs.StudioModeEnabled())
                    //obs.SetStudioMode(true);

                //obs.SetCurrentScene(requestArg.RemoveString("[OBS][SetScene]"));
                obs.TransitionToProgram(3000, requestArg.RemoveString("[OBS][SetScene]"));
                //obs.SetStudioMode(false);
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

        public async static void InitConnect(int port, string password)
        {
            do
            {
                var source = new CancellationTokenSource(); //original code
                source.CancelAfter(TimeSpan.FromSeconds(5)); //original code
                var completionSource = new TaskCompletionSource<object>(); //New code
                source.Token.Register(() => completionSource.TrySetCanceled()); //New code

                Task task = Task.Run(() =>
                {
                    obs.Connect("ws://" + WebServer.getIP() + ":" + port, (password ?? null));
                }, source.Token);

                await Task.WhenAny(task, completionSource.Task);
            }
            while (!obs.IsConnected);
        }
    }
}
