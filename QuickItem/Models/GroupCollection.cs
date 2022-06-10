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
    public class GroupCollection : InfoCollection<ItemGroupInfo>
    {
        private static readonly Logger Logger = Logger.GetLogger<QuickItemModule>();

        protected override void InitializeNewInfo(ItemGroupInfo newInfo)
        {
            newInfo.Guid = Guid.NewGuid();
            UpdateInfo(newInfo);
        }

        public ItemGroupInfo FindGroupById(Guid id)
        {
            return this.FirstOrDefault(group => group.Guid == id);
        }

        protected override string GetInfoFileExtension()
        {
            return "group";
        }

        protected override string GetInfoDirectory()
        {
            return QuickItemModule.Instance.GroupsDirectory;
        }
    }
}
