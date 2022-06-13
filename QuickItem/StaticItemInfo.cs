using Blish_HUD;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickItem
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum IconSearchMode
    {
        ToGray,
        ToGrayWithMeanHueCheck,
        RedOnly,
        BlueOnly,
        GreenOnly,
        YXorCrCb,
        HXorV,
    }

    public class StaticItemInfo
    {
        public string Name { get; set; }
        public string IconUrl { get; set; }
        public IconSearchMode SearchMode { get; set; }

        private static readonly Logger Logger = Logger.GetLogger<QuickItemModule>();

        public static async Task Initialize(string cachePath)
        {
            Logger.Info($"Initializing StaticItemInfo");
            await ReadAllFromFile(cachePath);
        }

        public static async Task ReadAllFromFile(string path)
        {
            LastUsedFilePath = path;
            await Task.Run(() =>
            {
                AllItems = JsonConvert.DeserializeObject<Dictionary<int, StaticItemInfo>>(System.IO.File.ReadAllText(path));
            });
        }

        public static async Task WriteAllToFile(string path = null)
        {
            if (path == null)
            {
                path = LastUsedFilePath;
            }
            await Task.Run(() =>
            {
                System.IO.File.WriteAllText(path, JsonConvert.SerializeObject(AllItems));
            });
        }

        public static Dictionary<int, StaticItemInfo> AllItems { get; private set; }
        public static string LastUsedFilePath = null;
    }
}
