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
    public class GroupConfigView : View<GroupConfigPresenter>
    {
        public FlowPanel GroupSettings { get; private set; }
        public Panel GroupEditor { get; private set; }

        private FlowPanel _layout;

        public event EventHandler AddItemClicked;

        public GroupConfigView(ItemGroupInfo group)
        {
            this.WithPresenter(new GroupConfigPresenter(this, group));
        }

        protected override void Build(Container buildPanel)
        {
            var parentSize = buildPanel.ContentRegion.Size;

            _layout = new FlowPanel
            {
                Size = new Point(parentSize.X, parentSize.Y),
                ControlPadding = new Vector2(10, 10),
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                Parent = buildPanel,
            };

            this.GroupSettings = new FlowPanel
            {
                Size = new Point(parentSize.X, Math.Min(parentSize.Y, 150)),
                Top = 0,
                CanScroll = true,
                ControlPadding = new Vector2(0, 15),
                OuterControlPadding = new Vector2(20, 5),
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                Title = "Group Settings",
                ShowBorder = true,
                Parent = _layout,
            };

            var groupEditorContainer = new Panel
            {
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.Fill,
                Parent = _layout,
            };

            this.GroupEditor = new Panel
            {
                //Size = new Point(parentSize.X, parentSize.Y - GroupSettings.Size.Y),
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.Fill,
                Location = new Point(0, GroupSettings.Bottom),
                Top = 0,
                Title = "Group Items",
                ShowBorder = true,
                Parent = groupEditorContainer,
            };

            var addItemButton = new StandardButton()
            {
                Text = "Add item",
                Parent = groupEditorContainer,
                Location = new Point(500, 5),
            };
            addItemButton.Click += AddItemButton_Click;
        }

        private void AddItemButton_Click(object sender, Blish_HUD.Input.MouseEventArgs e)
        {
            AddItemClicked?.Invoke(this, null);
        }
    }
}
