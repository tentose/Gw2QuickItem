using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.BitmapFonts;
using QuickItem.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace QuickItem
{
    public enum InteractionInput
    {
        SingleClick,
        DoubleClick,
    }

    public class ItemIcon : Control, IHasContextMenu
    {
        private static readonly Logger Logger = Logger.GetLogger<QuickItemModule>();

        private const int DEFAULT_ICON_SIZE = ((int)IconSize.Larger);

        private static Texture2D ItemPlaceholder;
        private static Texture2D ItemNotFound;

        public static async Task LoadIconResources()
        {
            await Task.Run(() =>
            {
                var contentsManager = QuickItemModule.Instance.ContentsManager;

                ItemPlaceholder = contentsManager.GetTexture(@"Textures\ExoticBorder.png");
                ItemNotFound = contentsManager.GetTexture(@"Textures\IconNotFound.png");
            });
        }

        private AsyncTexture2D _image;
        private ContextMenuStrip m_contextMenu;
        private ItemIconInfo _item;
        public ItemIconInfo Item
        {
            get => _item;
        }

        private bool _hasBeenVisible = false;

        public bool AllowActivation { get; set; } = true;

        public bool AllowEdit { get; set; } = false;

        public event EventHandler DeleteRequested;

        public ItemIcon()
        {
            this.Size = new Point(DEFAULT_ICON_SIZE, DEFAULT_ICON_SIZE);
            _image = ItemPlaceholder;
            _item = new ItemIconInfo();
        }

        public ItemIcon(ItemIconInfo info)
        {
            this.Size = new Point(info.IconSize, info.IconSize);
            _image = ItemPlaceholder;
            _item = info;
            _item.PropertyChanged += _item_PropertyChanged;
        }

        private void _item_PropertyChanged(object sender, EventArgs e)
        {
            if (_hasBeenVisible)
            {
                LoadItemImage();
            }
        }

        public void LoadItemImage()
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    if (_item.ItemId != 0)
                    {
                        if (_item.ItemAssetId == 0)
                        {
                            _item.ItemAssetId = await GetIconAssetId();
                        }

                        if (_item.ItemAssetId != 0)
                        {
                            if (!AsyncTexture2D.TryFromAssetId(_item.ItemAssetId, out _image))
                            {
                                _image = ItemNotFound;
                            }
                        }
                        else
                        {
                            _hasBeenVisible = false;
                        }
                    }
                }
                catch (Exception e)
                {
                    Logger.Error(e, $"Failed loading item image {_item.ItemId}");
                }
                Invalidate();
            });
        }

        private async Task<int> GetIconAssetId()
        {
            if (_item.ItemId == 0)
            {
                return 0;
            }

            var item = await QuickItemModule.Instance.Gw2ApiManager.Gw2ApiClient.V2.Items.GetAsync(_item.ItemId);
            var iconUrl = item.Icon.Url.ToString();
            var iconFileName = Path.GetFileNameWithoutExtension(iconUrl);
            if (int.TryParse(iconFileName, out int assetId))
            {
                return assetId;
            }
            return 0;
        }

        protected override void OnRightMouseButtonReleased(MouseEventArgs e)
        {
            base.OnRightMouseButtonReleased(e);

            if (AllowEdit)
            {
                if (m_contextMenu == null)
                {
                    BuildAndShowContextMenu();
                }
                else
                {
                    m_contextMenu.Show(this);
                }
            }
        }

        private void BuildAndShowContextMenu()
        {
            m_contextMenu = new ContextMenuStrip();

            var menuItems = GetContextMenuItems();
            m_contextMenu.AddMenuItems(menuItems);
            var ancestorMenuItems = this.GetAncestors().OfType<IHasContextMenu>().SelectMany(ancestor => ancestor.GetContextMenuItems());
            m_contextMenu.AddMenuItems(ancestorMenuItems);

            m_contextMenu.Show(this);
        }

        protected override void OnClick(MouseEventArgs e)
        {
            base.OnClick(e);

            if (IsActivationTrigger(e) && AllowActivation && _item.ItemAssetId != 0)
            {
                var iconPath = Content.DatAssetCache.GetLocalTexturePath(_item.ItemAssetId);

                if (File.Exists(iconPath))
                {
                    Task.Run(async () =>
                    {
                        QuickItemModule.Instance.Clicker.ClickItem(_item.ItemAssetId);
                    });
                }
            }
        }

        private bool IsActivationTrigger(MouseEventArgs e)
        {
            var value = QuickItemModule.Instance.GlobalSettings.ActivationTrigger.Value;
            if (e.IsDoubleClick)
            {
                return value == InteractionInput.DoubleClick;
            }
            else
            {
                return value == InteractionInput.SingleClick;
            }
        }

        public IEnumerable<ContextMenuStripItem> GetContextMenuItems()
        {
            var deleteItem = new ContextMenuStripItem(Strings.ContextMenu_Item_Delete);
            deleteItem.Click += DeleteItem_Click;
            yield return deleteItem;

            var editItem = new ContextMenuStripItem(Strings.ContextMenu_Item_EditItem);
            editItem.Click += EditItem_Click;
            yield return editItem;
        }

        private void DeleteItem_Click(object sender, MouseEventArgs e)
        {
            DeleteRequested?.Invoke(this, EventArgs.Empty);
        }

        private void EditItem_Click(object sender, MouseEventArgs e)
        {
            ItemEditControl itemSearch = new ItemEditControl(this.Item);
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            if (!_hasBeenVisible)
            {
                _hasBeenVisible = true;
                LoadItemImage();
            }
            spriteBatch.DrawOnCtrl(this, _image, bounds);
            if (_item != null)
            {
                //spriteBatch.DrawOnCtrl(this, s_rarityToBorder[m_itemInfo.Rarity], bounds);

                //if (m_number.Length > 0)
                //{
                //    spriteBatch.DrawStringOnCtrl(this, m_number, s_NumberFont, bounds.WithPadding(s_NumberMargin), s_NumberColor, false, true, 1, HorizontalAlignment.Right, VerticalAlignment.Top);
                //}
            }
        }

        protected override void DisposeControl()
        {
            base.DisposeControl();

        }
    }
}
