using Blish_HUD;
using Blish_HUD.Input;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickItem
{
    public class ItemIconInfo
    {
        private static readonly Logger Logger = Logger.GetLogger<QuickItemModule>();

        private int _itemId;
        public int ItemId
        {
            get { return _itemId; }
            set
            {
                _itemId = value;
                OnPropertyChanged();
            }
        }

        private int _itemAssetId;
        public int ItemAssetId
        {
            get { return _itemAssetId; }
            set
            {
                _itemAssetId = value;
                OnPropertyChanged();
            }
        }

        private KeyBinding _keyBind;
        public KeyBinding KeyBind
        {
            get { return _keyBind; }
            set
            {
                _keyBind = value;
                OnPropertyChanged();
            }
        }

        public Point IconPosition { get; set; }

        private int _iconSize = 61;
        public int IconSize
        {
            get { return _iconSize; }
            set
            {
                _iconSize = value;
                OnPropertyChanged();
            }
        }

        public ItemIconInfo()
        {
        }

        public event EventHandler PropertyChanged;
        private void OnPropertyChanged()
        {
            PropertyChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
