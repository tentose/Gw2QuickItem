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
    public class ModuleSettingsView : View
    {
        private StandardButton _manageWindowButton;
        private FlowPanel _viewLayout;
        private Label _instructions;

        private Action _openManageWindow;

        public ModuleSettingsView(Action openManageWindow)
        {
            _openManageWindow = openManageWindow;
        }

        protected override void Build(Container buildPanel)
        {
            var parentSize = buildPanel.Size;

            _viewLayout = new FlowPanel
            {
                Size = new Point(parentSize.X, parentSize.Y),
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.Fill,
                ControlPadding = new Vector2(10, 10),
                OuterControlPadding = new Vector2(5, 5),
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                Parent = buildPanel,
                CanScroll = true,
            };

            _manageWindowButton = new StandardButton()
            {
                Text = Strings.ModuleSettings_OpenManagementWindow,
                Width = 200,
                Height = 30,
                Parent = _viewLayout,
            };
            _manageWindowButton.Click += _manageWindowButton_Click;

            _instructions = new Label
            {
                Text = Strings.InfoTab_Text,
                Width = parentSize.X - 30,
                AutoSizeHeight = true,
                Parent = _viewLayout,
                WrapText = true,
            };
        }

        private void _manageWindowButton_Click(object sender, Blish_HUD.Input.MouseEventArgs e)
        {
            _openManageWindow.Invoke();
        }

        protected override void Unload()
        {
            if (_manageWindowButton != null)
            {
                _manageWindowButton.Click -= _manageWindowButton_Click;
            }
        }
    }
}
