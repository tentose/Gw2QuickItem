using Blish_HUD;
using Blish_HUD.Input;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickItem
{
    public class ItemIconInfo
    {
        private static readonly Logger Logger = Logger.GetLogger<QuickItemModule>();

        public int ItemId { get; set; }
        public int ItemAssetId { get; set; }
        public KeyBinding KeyBind { get; set; }
        public Point IconPosition { get; set; }
        public int IconSize { get; set; } = 61;

        public ItemIconInfo()
        {
        }

        public static async Task<ItemIconInfo> FromChatCode(string chatCode)
        {
            if (!Gw2Sharp.ChatLinks.Gw2ChatLink.TryParse(chatCode, out var link))
            {
                Logger.Warn("Failed to parse {0}", chatCode);
                return null;
            }

            if (link.Type != Gw2Sharp.ChatLinks.ChatLinkType.Item)
            {
                Logger.Warn("Failed to parse {0}: not an item", chatCode);
                return null;
            }

            var itemLink = link as Gw2Sharp.ChatLinks.ItemChatLink;
            if (itemLink == null)
            {
                Logger.Warn("Failed to parse {0}: cannot cast to item", chatCode);
                return null;
            }

            return new ItemIconInfo()
            {
                ItemId = itemLink.ItemId,
            };
        }

    }
}
