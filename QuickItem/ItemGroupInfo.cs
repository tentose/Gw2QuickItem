using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickItem
{
    public class ItemGroupInfo
    {
        public Guid Guid { get; set; }

        private string _name;
        [JsonIgnore]
        public string Name { 
            get 
            {
                return _name;
            }
            set 
            {
                var oldName = _name;
                _name = value;
                OnGroupChanged(oldName);
            }
        }

        private List<ItemIconInfo> _items;
        public List<ItemIconInfo> Items
        {
            get
            {
                return _items;
            }
            set
            {
                _items = value;
                OnGroupChanged();
            }
        }

        public event EventHandler<string> GroupChanged;
        private void OnGroupChanged(string oldName = null)
        {
            if (!PauseChangedNotifications)
            {
                GroupChanged?.Invoke(this, oldName);
            }
        }

        [JsonIgnore]
        public bool PauseChangedNotifications { get; set; } = false;
    }
}
