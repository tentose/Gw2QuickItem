using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickItem
{
    public class ItemLayout
    {
        public List<(Guid GroupId, Point Position)> Groups { get; set; }
        public string Name { get; set; }
    }
}
