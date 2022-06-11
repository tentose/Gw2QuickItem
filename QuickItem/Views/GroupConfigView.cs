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

        public event EventHandler AddToLayoutClicked;

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
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.Fill,
                ControlPadding = new Vector2(10, 10),
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                Parent = buildPanel,
            };

            this.GroupSettings = new FlowPanel
            {
                Size = new Point(parentSize.X, 150),
                WidthSizingMode = SizingMode.Fill,
                Top = 0,
                CanScroll = true,
                ControlPadding = new Vector2(0, 5),
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
                Location = new Point(350, 5),
            };
            addItemButton.Click += AddItemButton_Click;

            var addToLayoutButton = new StandardButton()
            {
                Text = "Add to Layout",
                Parent = groupEditorContainer,
                Location = new Point(500, 5),
            };
            addToLayoutButton.Click += AddToLayoutButton_Click;

            var instructionLabel = new Label()
            {
                Text = "Drag items in here to move items around within the group\n" +
                       "Shift + drag the group in here to move the group around in here\n" +
                       "Right click to edit the item\n" +
                       "Drag groups in the game (outside of this window) to move them around",
                Parent = groupEditorContainer,
                Location = new Point(100, 180),
                AutoSizeWidth = true,
                AutoSizeHeight = true,
                ZIndex = -100,
            };
        }

        private void AddToLayoutButton_Click(object sender, Blish_HUD.Input.MouseEventArgs e)
        {
            AddToLayoutClicked?.Invoke(this, null);
        }

        private void AddItemButton_Click(object sender, Blish_HUD.Input.MouseEventArgs e)
        {
            AddItemClicked?.Invoke(this, null);
        }
    }
}
