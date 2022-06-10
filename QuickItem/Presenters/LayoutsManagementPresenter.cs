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
    public class LayoutsManagementPresenter : InfoManagementPresenter<LayoutsManagementView, LayoutCollection, LayoutInfo>
    {
        public LayoutsManagementPresenter(LayoutsManagementView view, LayoutCollection model) : base(view, model)
        {
        }

        protected override IView CreateConfigView(LayoutInfo info)
        {
            return new LayoutConfigView(info);
        }
    }
}
