using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace StreamDeck
{
    public static class FileHandler
    {
        public const string decksFileName = @"decks";
        public const string obsSettingsFile = @"obs.json";

        public static string getJson(string id)
        {
            return File.ReadAllText(Path.Combine(decksFileName, decksFileName + id + ".json"));
        }

        public static void readData(out Page page, int id = 0)
        {
            string fullPath = Path.Combine(decksFileName, decksFileName + id + ".json");
            if (!File.Exists(fullPath))
                CreateData(fullPath);
            string jsonFile = File.ReadAllText(fullPath);
            page = JsonConvert.DeserializeObject<Page>(jsonFile);
        }

        public static void writeData(ref Page page)
        {
            string jsonFile = JsonConvert.SerializeObject(page, Formatting.Indented);
            File.WriteAllText(Path.Combine(decksFileName, decksFileName + page.id + ".json"), jsonFile);
        }

        private static void CreateData(string fullPath)
        {
            if (!Directory.Exists(decksFileName))
                Directory.CreateDirectory(decksFileName);
            File.Create(fullPath).Close();
            Page page = new Page();
            page.decks = new Decks[page.x * page.y];
            for (int i = 0; i < page.decks.Length; i++)
            {
                page.decks[i] = new Decks();
            }
            writeData(ref page);
        }

        public static OBSHandler.OBSSettings LoadObsSettings()
        {
            if (File.Exists(obsSettingsFile))
            {
                string json = File.ReadAllText(obsSettingsFile);

                return JsonConvert.DeserializeObject<OBSHandler.OBSSettings>(json);
            }
            else
            {
                OBSHandler.OBSSettings settings = new OBSHandler.OBSSettings
                {
                    password = "",
                    port = 4444 
                };

                string json = JsonConvert.SerializeObject(settings, Formatting.Indented);

                File.WriteAllText(obsSettingsFile, json);

                System.Windows.Forms.MessageBox.Show("Please fill in OBS-Websocket settings in " + obsSettingsFile + " file.", StreamDeckPC.appname);

                return JsonConvert.DeserializeObject<OBSHandler.OBSSettings>(json);
            }
        }
    }
}
