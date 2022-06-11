using Blish_HUD.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickItem
{
    public interface IHasContextMenu
    {
        IEnumerable<ContextMenuStripItem> GetContextMenuItems();
    }
}
