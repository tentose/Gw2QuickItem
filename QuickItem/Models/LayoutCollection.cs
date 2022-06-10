using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.IO;
using Newtonsoft.Json;
using Blish_HUD;

namespace QuickItem
{
    public class LayoutCollection : InfoCollection<LayoutInfo>
    {
        protected override string GetInfoFileExtension()
        {
            return "layout";
        }

        protected override string GetInfoDirectory()
        {
            return QuickItemModule.Instance.LayoutsDirectory;
        }
    }
}
