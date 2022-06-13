using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace QuickItem.Controls
{
    public class AutocompleteTextBox : TextBox
    {
        private const int SEARCH_DEBOUNCE_MILLIS = 500;

        private class AutocompleteItem : Label
        {
            public object Item;

            private bool _mouseInControl = false;

            public AutocompleteItem()
            {
            }

            protected override void OnMouseEntered(MouseEventArgs e)
            {
                _mouseInControl = true;
                TextColor = Color.Black;
                base.OnMouseEntered(e);
            }

            protected override void OnMouseLeft(MouseEventArgs e)
            {
                _mouseInControl = false;
                TextColor = Color.White;
                base.OnMouseLeft(e);
            }

            protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
            {
                if (_mouseInControl)
                {
                    var highlightArea = bounds;
                    highlightArea.Inflate(-2, -2);
                    spriteBatch.DrawOnCtrl(this,
                           ContentService.Textures.Pixel,
                           highlightArea,
                           Color.White);
                }
                base.Paint(spriteBatch, bounds);
            }
        }

        private class AutocompletePanel : FlowPanel
        {
            public AutocompletePanel()
            {
                Parent = Graphics.SpriteScreen;
                BackgroundColor = Color.Black;
                FlowDirection = ControlFlowDirection.SingleTopToBottom;
                HeightSizingMode = SizingMode.AutoSize;
            }
        }

        public delegate IEnumerable<object> GetAutocompleteItems(string currentInput);

        public GetAutocompleteItems AutocompleteFunction = (a) => new string[0];
        public object SelectedItem = null;

        public event EventHandler<object> SelectedItemChanged;

        private AutocompletePanel _resultsPanel;
        private Timer _searchDebounceTimer;

        public AutocompleteTextBox()
        {
            _resultsPanel = new AutocompletePanel()
            {
                Visible = false,
                ZIndex = 200,
            };
            TextChanged += HandleTextChanged;
            _searchDebounceTimer = new Timer(SEARCH_DEBOUNCE_MILLIS);
            _searchDebounceTimer.AutoReset = false;
            _searchDebounceTimer.Elapsed += _searchDebounceTimer_Elapsed;
        }

        private void _searchDebounceTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            var autocompleteItems = AutocompleteFunction(this.Text);
            _resultsPanel.ClearChildren();
            foreach (var item in autocompleteItems)
            {
                AutocompleteItem itemLabel = new AutocompleteItem()
                {
                    Text = item.ToString(),
                    Item = item,
                    Height = 20,
                    Width = this.Width,
                    Parent = _resultsPanel,
                };
                itemLabel.Click += ItemLabel_Click;
            }

            if (_resultsPanel.Children.Count > 0 && !_resultsPanel.Visible)
            {
                _resultsPanel.Location = this.AbsoluteBounds.Location + new Point(0, this.Height);
                _resultsPanel.Width = this.Width;
                _resultsPanel.Show();
            }
            else if (_resultsPanel.Children.Count == 0 && _resultsPanel.Visible)
            {
                _resultsPanel.Hide();
            }
        }

        private void ItemLabel_Click(object sender, MouseEventArgs e)
        {
            var itemLabel = sender as AutocompleteItem;
            if (itemLabel != null)
            {
                this.SelectedItem = itemLabel.Item;
                this.Text = itemLabel.Text;
                this._resultsPanel.Hide();
                SelectedItemChanged?.Invoke(this, this.SelectedItem);
            }
        }

        private void HandleTextChanged(object sender, EventArgs e)
        {
            _searchDebounceTimer.Stop();

            if (this.SelectedItem == null || this.SelectedItem.ToString() != this.Text)
            {
                _searchDebounceTimer.Start();
            }
        }

        protected override void DisposeControl()
        {
            _resultsPanel.Dispose();
            base.DisposeControl();
        }
    }
}
