using Blish_HUD;
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
        private static readonly Logger Logger = Logger.GetLogger<QuickItemModule>();

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
                // Clear existing event registrations
                foreach (var item in _items)
                {
                    item.PropertyChanged -= Item_PropertyChanged;
                }
                _items.CollectionChanged -= _items_CollectionChanged;
                
                // Set values and register for events
                _items = value;
                _items.CollectionChanged += _items_CollectionChanged;
                foreach (var item in _items)
                {
                    item.PropertyChanged += Item_PropertyChanged;
                }
                OnPropertyChanged();
            }
        }

        private void Item_PropertyChanged(object sender, EventArgs e)
        {
            OnPropertyChanged();
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
            // Below handling of changes assumes the underlying collection never fires a multi-item change, which is true for
            // the default implementation of ObservableCollection used by GroupCollection.
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    {
                        var item = e.NewItems[0] as ItemIconInfo;
                        item.PropertyChanged += Item_PropertyChanged;
                    }
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    {
                        var item = e.OldItems[0] as ItemIconInfo;
                        item.PropertyChanged -= Item_PropertyChanged;
                    }
                    break;
                default:
                    Logger.Warn($"Unhandled collection changed event in ItemGroupInfo: {e.Action}");
                    break;
            }
            OnPropertyChanged();
        }
    }
}
