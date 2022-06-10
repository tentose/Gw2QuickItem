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
    public class LayoutConfigView : View<LayoutConfigPresenter>
    {
        public FlowPanel LayoutSettings { get; private set; }

        private FlowPanel _viewLayout;

        public LayoutConfigView(LayoutInfo layout)
        {
            this.WithPresenter(new LayoutConfigPresenter(this, layout));
        }

        protected override void Build(Container buildPanel)
        {
            var parentSize = buildPanel.ContentRegion.Size;

            _viewLayout = new FlowPanel
            {
                Size = new Point(parentSize.X, parentSize.Y),
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.Fill,
                ControlPadding = new Vector2(10, 10),
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                Parent = buildPanel,
            };

            this.LayoutSettings = new FlowPanel
            {
                Size = new Point(parentSize.X, parentSize.Y),
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.Fill,
                Top = 0,
                CanScroll = true,
                ControlPadding = new Vector2(0, 15),
                OuterControlPadding = new Vector2(20, 5),
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                Title = "Layout Settings",
                ShowBorder = true,
                Parent = _viewLayout,
            };
        }
    }
}
