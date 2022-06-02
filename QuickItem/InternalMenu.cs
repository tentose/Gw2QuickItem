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

            int lastBottom = 0;

            foreach (var child in _children.Where(c => c.Visible))
            {
                child.Location = new Microsoft.Xna.Framework.Point(0, lastBottom);
                child.Width = this.Width;

                lastBottom = child.Bottom;
            }
        }
    }
}
