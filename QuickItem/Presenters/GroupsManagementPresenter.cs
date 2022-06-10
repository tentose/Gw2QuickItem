using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickItem
{
    public class GroupsManagementPresenter : InfoManagementPresenter<GroupsManagementView, GroupCollection, ItemGroupInfo>
    {
        public GroupsManagementPresenter(GroupsManagementView view, GroupCollection model) : base(view, model)
        {
        }

        protected override IView CreateConfigView(ItemGroupInfo info)
        {
            return new GroupConfigView(info);
        }
    }
}
