using Blish_HUD.Graphics.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickItem
{
    public class GroupConfigPresenter : Presenter<GroupConfigView, ItemGroupInfo>
    {

        public GroupConfigPresenter(GroupConfigView view, ItemGroupInfo model) : base(view, model)
        {
        }

        protected override void UpdateView()
        {
            this.View.GroupEditor.ClearChildren();

            var group = new ItemIconGroup()
            {
                GroupInfo = Model,
                Parent = this.View.GroupEditor,
                AllowActivation = false,
            };
        }
    }
}
