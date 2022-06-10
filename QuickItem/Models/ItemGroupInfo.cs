using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickItem
{
    public class ItemGroupInfo : INamedObject, INotifyInfoPropertyChanged
    {
        public Guid Guid { get; set; } = Guid.NewGuid();

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
                OnPropertyChanged(oldName);
            }
        }

        private ObservableCollection<ItemIconInfo> _items = new ObservableCollection<ItemIconInfo>();
        public ObservableCollection<ItemIconInfo> Items
        {
            get
            {
                return _items;
            }
            set
            {
                _items.CollectionChanged -= _items_CollectionChanged;
                _items = value;
                _items.CollectionChanged += _items_CollectionChanged;
                OnPropertyChanged();
            }
        }

        public event EventHandler<string> PropertyChanged;
        private void OnPropertyChanged(string oldName = null)
        {
            if (!PauseChangedNotifications)
            {
                PropertyChanged?.Invoke(this, oldName);
            }
        }

        [JsonIgnore]
        public bool PauseChangedNotifications { get; set; } = false;

        public ItemGroupInfo()
        {
            _items.CollectionChanged += _items_CollectionChanged;
        }

        private void _items_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged();
        }
    }
}
