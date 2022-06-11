using Blish_HUD;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickItem
{
    public class StaticItemInfo
    {
        public string Name { get; set; }
        public string IconUrl { get; set; }

        private static readonly Logger Logger = Logger.GetLogger<QuickItemModule>();

        public static async Task Initialize(string cachePath)
        {
            Logger.Info($"Initializing StaticItemInfo");
            await ReadAllFromFile(cachePath);
        }

        public static async Task ReadAllFromFile(string path)
        {
            await Task.Run(() =>
            {
                AllItems = JsonConvert.DeserializeObject<Dictionary<int, StaticItemInfo>>(System.IO.File.ReadAllText(path));
            });
        }

        public static Dictionary<int, StaticItemInfo> AllItems { get; private set; }
    }
}
