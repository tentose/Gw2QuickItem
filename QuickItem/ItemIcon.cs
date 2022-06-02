using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.BitmapFonts;
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
    public class ItemIcon : Control
    {
        private static readonly Logger Logger = Logger.GetLogger<QuickItemModule>();

        private const int DEFAULT_ICON_SIZE = 61;

        private static Texture2D s_itemPlaceholder;

        public static async Task LoadIconResources()
        {
            await Task.Run(() =>
            {
                var contentsManager = QuickItemModule.Instance.ContentsManager;

                s_itemPlaceholder = contentsManager.GetTexture(@"Textures\ExoticBorder.png");
            });
        }

        private AsyncTexture2D m_image;
        private ContextMenuStrip m_contextMenu;
        private ItemIconInfo m_item;
        public ItemIconInfo Item
        {
            get => m_item;
        }

        private bool m_shouldLoadImage = true;

        public bool AllowActivation { get; set; } = true;

        public ItemIcon()
        {
            this.Size = new Point(DEFAULT_ICON_SIZE, DEFAULT_ICON_SIZE);
            m_image = s_itemPlaceholder;
            m_item = new ItemIconInfo();
        }

        public ItemIcon(ItemIconInfo info)
        {
            this.Size = new Point(info.IconSize, info.IconSize);
            m_image = s_itemPlaceholder;
            m_item = info;
        }

        public void LoadItemImage()
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    if (m_item.ItemId != 0)
                    {
                        if (m_item.ItemAssetId == 0)
                        {
                            m_item.ItemAssetId = await GetIconAssetId();
                        }

                        if (m_item.ItemAssetId != 0)
                        {
                            m_image = AsyncTexture2D.FromAssetId(m_item.ItemAssetId);
                        }
                        else
                        {
                            m_shouldLoadImage = true;
                        }
                    }
                }
                catch (Exception e)
                {
                    Logger.Error(e, $"Failed loading item image {m_item.ItemId}");
                }
                Invalidate();
            });
        }

        private async Task<int> GetIconAssetId()
        {
            if (m_item.ItemId == 0)
            {
                return 0;
            }

            var item = await QuickItemModule.Instance.Gw2ApiManager.Gw2ApiClient.V2.Items.GetAsync(m_item.ItemId);
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

            if (m_contextMenu == null)
            {
                BuildAndShowContextMenu();
            }
            else
            {
                m_contextMenu.Show(this);
            }
        }

        private void BuildAndShowContextMenu()
        {
            m_contextMenu = new ContextMenuStrip();
            var setItemMenuItem = m_contextMenu.AddMenuItem(Strings.ItemContextMenu_SetItem);
            setItemMenuItem.Click += SetItemMenuItem_Click;

            m_contextMenu.Show(this);
        }

        private void SetItemMenuItem_Click(object sender, MouseEventArgs e)
        {
            throw new NotImplementedException();
        }

        protected override void OnClick(MouseEventArgs e)
        {
            base.OnClick(e);

            if (AllowActivation && m_item.ItemAssetId != 0)
            {
                var iconPath = Content.DatAssetCache.GetLocalTexturePath(m_item.ItemAssetId);

                if (File.Exists(iconPath))
                {
                    Task.Run(async () =>
                    {
                        const float sizeMultiplier = 0.90625f;
                        Blish_HUD.Controls.Intern.Keyboard.Stroke(Blish_HUD.Controls.Extern.VirtualKeyShort.KEY_I);
                        await Task.Delay(300);

                        var point = ItemFinder.FindItem(iconPath);

                        if (point.HasValue)
                        {
                            var clickPoint = point.Value;//.Multiply(sizeMultiplier);
                            Blish_HUD.Controls.Intern.Mouse.DoubleClick(Blish_HUD.Controls.Intern.MouseButton.LEFT, clickPoint.X, clickPoint.Y);
                            Blish_HUD.Controls.Intern.Mouse.Release(Blish_HUD.Controls.Intern.MouseButton.LEFT, clickPoint.X, clickPoint.Y);

                            await Task.Delay(100);
                            //this.Parent.Location = new Point(clickPoint.X, clickPoint.Y);
                            Blish_HUD.Controls.Intern.Keyboard.Stroke(Blish_HUD.Controls.Extern.VirtualKeyShort.KEY_I);
                        }
                    });
                }
            }
        }


        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            if (m_shouldLoadImage)
            {
                m_shouldLoadImage = false;
                LoadItemImage();
            }
            spriteBatch.DrawOnCtrl(this, m_image, bounds);
            if (m_item != null)
            {
                //spriteBatch.DrawOnCtrl(this, s_rarityToBorder[m_itemInfo.Rarity], bounds);

                //if (m_number.Length > 0)
                //{
                //    spriteBatch.DrawStringOnCtrl(this, m_number, s_NumberFont, bounds.WithPadding(s_NumberMargin), s_NumberColor, false, true, 1, HorizontalAlignment.Right, VerticalAlignment.Top);
                //}
            }
        }
    }
}
