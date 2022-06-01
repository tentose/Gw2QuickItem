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

        public event EventHandler<ControlActivatedEventArgs> GroupSelectionChanged;

        public GroupsManagementView(GroupCollection groups)
        {
            this.WithPresenter(new GroupsManagementPresenter(this, groups));
        }

        protected override void Build(Container buildPanel)
        {
            var groupSelectionPanel = new Panel()
            {
                ShowBorder = true,
                Size = new Point(200, buildPanel.Height),
                Title = "Groups",
                Parent = buildPanel,
                CanScroll = true,
            };

            GroupsList = new Menu()
            {
                Size = groupSelectionPanel.ContentRegion.Size,
                MenuItemHeight = 40,
                Parent = groupSelectionPanel,
                CanSelect = true,
            };

            GroupsList.ItemSelected += GroupsList_ItemSelected;

            GroupConfigContainer = new ViewContainer()
            {
                FadeView = true,
                Size = new Point(buildPanel.Width - groupSelectionPanel.Width, groupSelectionPanel.Height),
                Location = new Point(groupSelectionPanel.Width, 0),
                Parent = buildPanel
            };
        }

        private void GroupsList_ItemSelected(object sender, ControlActivatedEventArgs e)
        {
            GroupSelectionChanged?.Invoke(this, e);
        }
    }
}
