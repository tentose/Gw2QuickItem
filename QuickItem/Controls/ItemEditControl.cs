using Blish_HUD;
using Blish_HUD.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickItem.Controls
{
    public class ItemEditControl : Container
    {
        private static readonly Logger Logger = Logger.GetLogger<QuickItemModule>();

        private static ItemEditControl CurrentInstance = null;

        public class SearchResult
        {
            public StaticItemInfo ItemInfo { get; set; }
            public int ItemId { get; set; }
            public int AssetId { get; set; }

            public override string ToString()
            {
                return ItemInfo.Name;
            }
        }

        private AutocompleteTextBox _searchTextBox;
        private Image _itemImage;
        private Label _itemName;
        private StandardButton _confirmButton;
        private StandardButton _cancelButton;
        private SearchResult _result;

        public event EventHandler<SearchResult> SearchCompleted;

        public ItemEditControl()
        {
            if (CurrentInstance != null)
            {
                CurrentInstance.Cancel();
            }
            CurrentInstance = this;

            _searchTextBox = new AutocompleteTextBox()
            {
                PlaceholderText = "Search for item...",
                Parent = this,
            };
            _searchTextBox.SelectedItemChanged += _searchTextBox_SelectedItemChanged;
            _searchTextBox.AutocompleteFunction = SearchForItems;
            _searchTextBox.InputFocusChanged += _searchTextBox_InputFocusChanged;
            _searchTextBox.Focused = true;

            _itemImage = new Image()
            {
                Width = 62,
                Height = 62,
                Parent = this,
            };

            _itemName = new Label()
            {
                Parent = this,
            };

            _confirmButton = new StandardButton()
            {
                Text = "Confirm",
                Enabled = false,
                Parent = this,
            };
            _confirmButton.Click += _confirmButton_Click;

            _cancelButton = new StandardButton()
            {
                Text = "Cancel",
                Parent = this,
            };
            _cancelButton.Click += _cancelButton_Click;

            this.Width = 300;
            this.Height = 150;
            this.Parent = Graphics.SpriteScreen;
            this.Location = new Point(this.Parent.Width / 2 - this.Width / 2, this.Parent.Height / 2 - this.Height / 2);
            this.ZIndex = 100;
            var contentRegion = new Rectangle(0, 0, this.Width, this.Height);
            contentRegion.Inflate(-10, -10);
            this.ContentRegion = contentRegion;

            LayoutControls();
        }

        private void _searchTextBox_InputFocusChanged(object sender, ValueEventArgs<bool> e)
        {
            // Select all text if the user is focusing on the text box to make it easier to type a new query
            if (_searchTextBox.Focused && _searchTextBox.Text.Length > 0)
            {
                _searchTextBox.SelectionStart = 0;
                _searchTextBox.SelectionEnd = _searchTextBox.Text.Length;
                _searchTextBox.RecalculateLayout();
            }
        }

        private void _cancelButton_Click(object sender, Blish_HUD.Input.MouseEventArgs e)
        {
            Cancel();
        }

        private void _confirmButton_Click(object sender, Blish_HUD.Input.MouseEventArgs e)
        {
            SearchCompleted?.Invoke(this, _result);
            this.Hide();
            if (CurrentInstance == this)
            {
                CurrentInstance = null;
            }
        }

        public void Cancel()
        {
            SearchCompleted?.Invoke(this, null);
            this.Hide();
            if (CurrentInstance == this)
            {
                CurrentInstance = null;
            }
        }

        private IEnumerable<object> SearchForItems(string term)
        {
            if (term.Length < 3)
            {
                return Enumerable.Empty<object>();
            }

            var lowerTerm = term.ToLowerInvariant();

            return StaticItemInfo.AllItems
                .Where(item => item.Value.Name.ToLowerInvariant().Contains(term))
                .Take(5)
                .Select(item => new SearchResult()
                {
                    ItemInfo = item.Value,
                    ItemId = item.Key,
                });
        }

        private void _searchTextBox_SelectedItemChanged(object sender, object e)
        {
            var selectedItem = e as SearchResult;
            if (selectedItem != null)
            {
                _result = selectedItem;
                _result.AssetId = GetAssetIdFromUrl(_result.ItemInfo.IconUrl);
                _itemImage.Texture = ContentService.Content.DatAssetCache.GetTextureFromAssetId(_result.AssetId);
                _itemName.Text = _result.ItemInfo.Name;
                _confirmButton.Enabled = true;
            }
            else
            {
                _result = null;
                _confirmButton.Enabled = false;
            }
        }

        private int GetAssetIdFromUrl(string url)
        {
            int id = 0;
            try
            {
                Uri uri = new Uri(url);
                var lastSegment = uri.Segments[uri.Segments.Length - 1];
                var stringId = lastSegment.Substring(0, lastSegment.IndexOf('.'));
                id = int.Parse(stringId);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"Failed to get item ID from {url}");
            }
            return id;
        }

        private void LayoutControls()
        {
            var region = this.ContentRegion;

            _searchTextBox.Width = region.Width;
            _searchTextBox.Height = 20;
            _searchTextBox.Location = new Point(0, 0);


            _itemImage.Width = 62;
            _itemImage.Height = 62;
            _itemImage.Location = new Point(0, _searchTextBox.Bottom + 10);

            _itemName.Width = region.Width - _itemImage.Width - 10;
            _itemName.Height = 20;
            _itemName.Location = new Point(_itemImage.Right + 10, _searchTextBox.Bottom + 10);

            _cancelButton.Location = new Point(region.Width - _cancelButton.Width, region.Height - _cancelButton.Height);

            _confirmButton.Location = new Point(_cancelButton.Left - 10 - _confirmButton.Width, region.Height - _cancelButton.Height);
        }

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.PaintBeforeChildren(spriteBatch, bounds);

            spriteBatch.DrawOnCtrl(this,
                       ContentService.Textures.Pixel,
                       bounds,
                       new Color(20, 20, 20, 200));
        }
    }
}
