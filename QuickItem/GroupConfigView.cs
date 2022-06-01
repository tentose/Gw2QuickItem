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
    public class GroupConfigView : View
    {
        public FlowPanel GroupSettings { get; private set; }
        public Panel GroupEditor { get; private set; }

        public GroupConfigView(ItemGroupInfo group)
        {
            this.WithPresenter(new GroupConfigPresenter(this, group));
        }

        protected override void Build(Container buildPanel)
        {
            var parentSize = buildPanel.ContentRegion.Size;

            this.GroupSettings = new FlowPanel
            {
                Size = new Point(parentSize.X, Math.Min(parentSize.Y, 150)),
                Top = 0,
                CanScroll = true,
                ControlPadding = new Vector2(0, 15),
                OuterControlPadding = new Vector2(20, 5),
                Parent = buildPanel
            };

            this.GroupEditor = new Panel
            {
                Size = new Point(parentSize.X, parentSize.Y - GroupSettings.Size.Y),
                Location = new Point(0, GroupSettings.Bottom),
                Top = 0,
                CanScroll = true,
                Parent = buildPanel
            };
        }
    }
}
