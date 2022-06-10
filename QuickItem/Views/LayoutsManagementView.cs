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
    public class LayoutsManagementView : InfoManagementView
    {

        public LayoutsManagementView(LayoutCollection layouts)
        {
            this.WithPresenter(new LayoutsManagementPresenter(this, layouts));
        }

        protected override string GetInfoSelectionPanelTitle()
        {
            return "Layouts";
        }
    }
}
