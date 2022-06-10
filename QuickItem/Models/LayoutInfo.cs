using Blish_HUD.Input;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickItem
{
    public class GroupPosition
    {
        public Guid GroupId { get; set; }
        public Point Position { get; set; }
    }

    public class LayoutInfo : INamedObject, INotifyInfoPropertyChanged
    {
        private string _name;
        [JsonIgnore]
        public string Name
        {
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

        private KeyBinding _keyBind = new KeyBinding();
        public KeyBinding KeyBind
        {
            get
            {
                return _keyBind;
            }
            set
            {
                _keyBind = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<GroupPosition> _groups = new ObservableCollection<GroupPosition>();
        public ObservableCollection<GroupPosition> Groups
        {
            get
            {
                return _groups;
            }
            set
            {
                _groups.CollectionChanged -= _groups_CollectionChanged;
                _groups = value;
                _groups.CollectionChanged += _groups_CollectionChanged;
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

        public LayoutInfo()
        {
            _groups.CollectionChanged += _groups_CollectionChanged;
        }

        private void _groups_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged();
        }
    }
}
