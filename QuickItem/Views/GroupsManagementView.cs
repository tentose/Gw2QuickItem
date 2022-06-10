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
    public class GroupsManagementView : InfoManagementView
    {
        public GroupsManagementView(GroupCollection groups)
        {
            this.WithPresenter(new GroupsManagementPresenter(this, groups));
        }

        protected override string GetInfoSelectionPanelTitle()
        {
            return "Groups";
        }
    }
}
