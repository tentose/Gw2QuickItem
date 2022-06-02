using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickItem
{
    public class GroupsManagementView : View
    {
        public Menu GroupsList { get; set; }

        public ViewContainer GroupConfigContainer { get; set; }

        private FlowPanel _layout;

        public event EventHandler<ControlActivatedEventArgs> GroupSelectionChanged;

        public event EventHandler CopySelectedItem;

        public GroupsManagementView(GroupCollection groups)
        {
            this.WithPresenter(new GroupsManagementPresenter(this, groups));
        }

        protected override void Build(Container buildPanel)
        {
            _layout = new FlowPanel
            {
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.Fill,
                ControlPadding = new Vector2(10, 10),
                FlowDirection = ControlFlowDirection.SingleLeftToRight,
                Parent = buildPanel,
            };

            var groupSelectionPanelContainer = new Panel()
            {
                Size = new Point(200, _layout.Height),
                HeightSizingMode = SizingMode.Fill,
                Parent = _layout,
            };

            var groupSelectionPanel = new Panel()
            {
                ShowBorder = true,
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.Fill,
                Title = "Groups",
                Parent = groupSelectionPanelContainer,
                CanScroll = true,
            };

            var copyGroupButton = new StandardButton()
            {
                Text = "Copy",
                Width = 95,
                Location = new Point(100, 5),
                Parent = groupSelectionPanelContainer,
            };
            copyGroupButton.Click += CopyGroupButton_Click;

            GroupsList = new InternalMenu()
            {
                WidthSizingMode = SizingMode.Fill,
                MenuItemHeight = 30,
                Parent = groupSelectionPanel,
                CanSelect = true,
            };

            GroupsList.ItemSelected += GroupsList_ItemSelected;

            GroupConfigContainer = new ViewContainer()
            {
                FadeView = true,
                //Size = new Point(buildPanel.ContentRegion.Width - groupSelectionPanel.Width, groupSelectionPanel.Height),
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.Fill,
                //Location = new Point(groupSelectionPanel.Width, 0),
                Parent = _layout
            };
        }

        private void CopyGroupButton_Click(object sender, Blish_HUD.Input.MouseEventArgs e)
        {
            CopySelectedItem?.Invoke(this, EventArgs.Empty);
        }

        private void GroupsList_ItemSelected(object sender, ControlActivatedEventArgs e)
        {
            GroupSelectionChanged?.Invoke(this, e);
        }
    }
}
