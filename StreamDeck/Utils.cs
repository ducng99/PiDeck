using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Input;
using WindowsInput;
using System.Threading.Tasks;

namespace StreamDeck
{
    public static class Utils
    {
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetKeyboardState(byte[] keyState);
        private static readonly byte[] DistinctVirtualKeys = Enumerable
                                                            .Range(0, 256)
                                                            .Select(KeyInterop.KeyFromVirtualKey)
                                                            .Where(item => item != Key.None)
                                                            .Distinct()
                                                            .Select(item => (byte)KeyInterop.VirtualKeyFromKey(item))
                                                            .ToArray();

        private static readonly int modifiers = (1 << (int)Key.LeftCtrl | 1 << (int)Key.RightCtrl | 1 << (int)Key.LeftAlt | 1 << (int)Key.RightAlt | 1 << (int)Key.LeftShift | 1 << (int)Key.RightShift);
        private static readonly InputSimulator input = new InputSimulator();

        public static List<Key> GetDownKeys()
        {
            var keyboardState = new byte[256];
            GetKeyboardState(keyboardState);

            var downKeys = new List<Key>();
            for (var index = 0; index < DistinctVirtualKeys.Length; index++)
            {
                var virtualKey = DistinctVirtualKeys[index];
                if ((keyboardState[virtualKey] & 0x80) != 0)
                {
                    downKeys.Add(KeyInterop.KeyFromVirtualKey(virtualKey));
                }
            }

            return downKeys;
        }

        public static Key stringToKey(string key)
        {
            return (Key)Enum.Parse(typeof(Key), key, true);
        }

        public static byte stringToVK(string key)
        {
            return (byte)KeyInterop.VirtualKeyFromKey(stringToKey(key));
        }

        public static void ActionHandler(string requestArg)
        {
            if (requestArg.StartsWith("[PROGRAM]"))
            {
                string[] cmd = requestArg.RemoveString("[PROGRAM]").SplitString("[ARGS]");
                string path = cmd[0];
                string args = cmd[1];
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = path,
                    Arguments = args,
                    WorkingDirectory = Path.GetDirectoryName(path)
                };
                Process.Start(psi);
            }
            else if (requestArg.StartsWith("[KEYS]"))
            {
                string keys = requestArg.RemoveString("[KEYS]");
                string[] keysArray = keys.Split('+');
                List<byte> modifiersPressed = new List<byte>();

                foreach (string k in keysArray)
                {
                    input.Keyboard.KeyDown((WindowsInput.Native.VirtualKeyCode)stringToVK(k));
                    if (((1 << (int)stringToKey(k)) & modifiers) == 0)
                    {
                        input.Keyboard.KeyUp((WindowsInput.Native.VirtualKeyCode)stringToVK(k));
                    }
                    else
                    {
                        modifiersPressed.Add(stringToVK(k));
                    }
                }

                if (modifiersPressed.Count > 0)
                {
                    foreach (byte key in modifiersPressed)
                    {
                        input.Keyboard.KeyUp((WindowsInput.Native.VirtualKeyCode)key);
                    }
                }
            }
            else if (requestArg.StartsWith("[CMD]"))
            {
                string cmd = requestArg.RemoveString("[CMD]");
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    RedirectStandardInput = true,
                    CreateNoWindow = true,
                    UseShellExecute = false
                };
                using (StreamWriter stream = Process.Start(psi).StandardInput)
                {
                    stream.WriteLine(cmd);
                }
            }
            else if (requestArg.StartsWith("[OBS]"))
            {
                OBSHandler.ReadRequest(requestArg);
            }
        }

        public static string ActionFormHandler(ActionTypes type, string com = null)
        {
            switch (type)
            {
                case ActionTypes.Program:
                    return "[PROGRAM]" + com;
                case ActionTypes.Keys:
                    return "[KEYS]" + com;
                case ActionTypes.Folder:
                    return "[FOLDER]";
                case ActionTypes.Cmd:
                    return "[CMD]" + com;
                case ActionTypes.OBS_SetScence:
                    return "[OBS][SetScene]" + com;
                case ActionTypes.OBS_StartStopStreaming:
                    return "[OBS][StartStopStreaming]";
                case ActionTypes.OBS_StudioModeToggle:
                    return "[OBS][StudioModeToggle]";
                case ActionTypes.Empty:
                    return "[EMPTY]";
                default:
                    return null;
            }
        }

        public static string RemoveString(this string s, string removeString)
        {
            int index = s.IndexOf(removeString);

            if (index >= 0)
            {
                return s.Remove(index, removeString.Length);
            }
            else
                return s;
        }

        public static string[] SplitString(this string s, string splitString)
        {
            int index = 0;

            string pString = s;

            List<string> splitted = new List<string>();

            do
            {
                index = pString.IndexOf(splitString);

                if (index >= 0)
                {
                    splitted.Add(pString.Remove(index, pString.Length - index));

                    pString = pString.Substring(index + splitString.Length);
                }
                else
                {
                    splitted.Add(pString);
                }
            }
            while (index >= 0);

            return splitted.ToArray();
        }
    }
}
