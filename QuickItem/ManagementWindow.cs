using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickItem
{
    public class ManagementWindow : TabbedWindow2
    {
        private GroupCollection groupCollection;

        public ManagementWindow() : base(QuickItemModule.Instance.ContentsManager.GetTexture(@"Textures\155985.png"),
                                            new Rectangle(35, 36, 930,      640),
                                            new Rectangle(95, 42, 813 + 38, 592)
                                            )
        {
            Title = "Manage";
            Parent = GameService.Graphics.SpriteScreen;
            Id = $"{nameof(ManagementWindow)}_{nameof(QuickItemModule)}_6f6df496-b40a-4155-b598-f14b6c4c5d0c";
            SavesPosition = true;

            groupCollection = new GroupCollection();
            groupCollection.InitializeFromDisk();

            Tabs.Add(new Tab(AsyncTexture2D.FromAssetId(156909), () => new GroupsManagementView(groupCollection), "stuff"));
        }
    }
}
