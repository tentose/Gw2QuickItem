using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.IO;
using Newtonsoft.Json;
using Blish_HUD;

namespace QuickItem
{
    public class GroupCollection : ObservableCollection<ItemGroupInfo>
    {
        private static readonly Logger Logger = Logger.GetLogger<QuickItemModule>();

        public void InitializeFromDisk()
        {
            this.Clear();
            var groupFiles = Directory.EnumerateFiles(QuickItemModule.Instance.GroupsDirectory, "*.group");

            // Use a temp dictionary to ensure there are no duplicate names
            Dictionary<string, ItemGroupInfo> tempInfo = new Dictionary<string, ItemGroupInfo>();
            foreach(var groupFile in groupFiles)
            {
                try
                {
                    var groupInfo = JsonConvert.DeserializeObject<ItemGroupInfo>(File.ReadAllText(groupFile));
                    groupInfo.Name = Path.GetFileNameWithoutExtension(groupFile);
                    tempInfo.Add(groupInfo.Name, groupInfo);
                }
                catch (Exception ex)
                {
                    Logger.Warn(ex, "Failed to load group file. Skipping.");
                }
            }

            foreach (var kv in tempInfo)
            {
                this.Add(kv.Value);
            }
        }
    }
}
