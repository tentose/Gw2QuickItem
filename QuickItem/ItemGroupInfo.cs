using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickItem
{
    public class ItemGroupInfo
    {
        public Guid Guid { get; set; }

        [JsonIgnore]
        public string Name { get; set; }

        public string BoundCharacter { get; set; }

        public string BoundAccount { get; set; }

        public List<ItemIconInfo> Items { get; set; }
    }
}
