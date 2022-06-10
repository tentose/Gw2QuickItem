using Blish_HUD.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickItem
{
    internal class InternalMenu : Menu
    {
        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            int nextY = 0;

            foreach (var child in _children.Where(c => c.Visible))
            {
                child.Location = new Microsoft.Xna.Framework.Point(0, nextY);
                child.Width = this.Width;
                child.Height = this.MenuItemHeight;

                nextY = child.Bottom;
            }
        }
    }
}
