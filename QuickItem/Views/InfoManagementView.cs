using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Microsoft.Xna.Framework;
using QuickItem.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickItem
{
    public abstract class InfoManagementView : View
    {
        public Menu ItemsList { get; set; }

        public ViewContainer ConfigContainer { get; set; }

        private FlowPanel _viewLayout;

        public event EventHandler<ControlActivatedEventArgs> SelectionChanged;

        public event EventHandler CopySelectedItem;
        public event EventHandler DeleteSelectedItem;

        protected abstract string GetInfoSelectionPanelTitle();

        protected override void Build(Container buildPanel)
        {
            _viewLayout = new FlowPanel
            {
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.Fill,
                ControlPadding = new Vector2(10, 10),
                FlowDirection = ControlFlowDirection.SingleLeftToRight,
                Parent = buildPanel,
            };

            var layoutSelectionPanelContainer = new Panel()
            {
                Size = new Point(200, _viewLayout.Height),
                HeightSizingMode = SizingMode.Fill,
                Parent = _viewLayout,
            };

            var layoutSelectionPanel = new Panel()
            {
                ShowBorder = true,
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.Fill,
                Title = GetInfoSelectionPanelTitle(),
                Parent = layoutSelectionPanelContainer,
                CanScroll = true,
            };

            var copyLayoutButton = new IconButton()
            {
                BasicTooltipText = "Copy",
                DarkenUnlessHovered = true,
                Icon = QuickItemModule.Instance.ContentsManager.GetTexture(@"Textures\CopyButton.png"),
                Size = new Point(25, 25),
                Location = new Point(130, 5),
                Parent = layoutSelectionPanelContainer,
            };
            copyLayoutButton.Click += CopyLayoutButton_Click;

            var deleteButton = new IconButton()
            {
                BasicTooltipText = "Delete",
                DarkenUnlessHovered = true,
                Icon = QuickItemModule.Instance.ContentsManager.GetTexture(@"Textures\DeleteButton.png"),
                Size = new Point(25, 25),
                Location = new Point(160, 5),
                Parent = layoutSelectionPanelContainer,
            };
            deleteButton.Click += DeleteButton_Click;

            ItemsList = new InternalMenu()
            {
                WidthSizingMode = SizingMode.Fill,
                MenuItemHeight = 30,
                Parent = layoutSelectionPanel,
                CanSelect = true,
            };

            ItemsList.ItemSelected += LayoutsList_ItemSelected;

            ConfigContainer = new ViewContainer()
            {
                FadeView = true,
                //Size = new Point(buildPanel.ContentRegion.Width - groupSelectionPanel.Width, groupSelectionPanel.Height),
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.Fill,
                //Location = new Point(groupSelectionPanel.Width, 0),
                Parent = _viewLayout
            };
        }

        private void DeleteButton_Click(object sender, Blish_HUD.Input.MouseEventArgs e)
        {
            DeleteSelectedItem?.Invoke(this, EventArgs.Empty);
        }

        private void CopyLayoutButton_Click(object sender, Blish_HUD.Input.MouseEventArgs e)
        {
            CopySelectedItem?.Invoke(this, EventArgs.Empty);
        }

        private void LayoutsList_ItemSelected(object sender, ControlActivatedEventArgs e)
        {
            SelectionChanged?.Invoke(this, e);
        }
    }
}
