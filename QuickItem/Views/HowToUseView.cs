using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickItem.Views
{
    public class HowToUseView : View
    {
        private FlowPanel _viewLayout;
        private Label _instructions;

        public HowToUseView()
        {
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
                CanScroll = true,
            };

            _instructions = new Label
            {
                Text = Strings.InfoTab_Text,
                Width = buildPanel.ContentRegion.Width,
                AutoSizeHeight = true,
                Parent = _viewLayout,
                WrapText = true,
            };
        }
    }
}
